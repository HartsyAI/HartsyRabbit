using HartsyRabbit.Configuration;
using HartsyRabbit.Infrastructure;
using HartsyRabbit.Logging;
using HartsyRabbit.Publishers;
using HartsyRabbit.Serialization;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace HartsyRabbit.Core;

public sealed class TypeSafeMessageBus : ITypeSafeMessageBus
{
    private readonly MessageBusConfiguration _configuration;
    private readonly IMessageBusLogger _logger;
    private readonly IRabbitMQConnectionLifecycleManager _connectionManager;
    private readonly IRabbitMQQueueSetupService _queueSetup;
    private readonly ITypeSafeMessagePublisher _publisher;
    private readonly MessageHandlerRegistrationService _registrations;

    private readonly ConcurrentDictionary<Type, List<object>> _directHandlers = new();

    private long _messagesPublished;
    private long _messagesProcessed;
    private long _processingErrors;

    private readonly object _statsLock = new();
    private double _avgProcessingTimeMs;
    private DateTime _startedAtUtc;
    private volatile bool _lastKnownHealthy;

    private string _siteName = string.Empty;
    private bool _started;

    public TypeSafeMessageBus(
        IOptions<MessageBusConfiguration> configuration,
        IMessageBusLogger logger,
        IRabbitMQConnectionLifecycleManager connectionManager,
        IRabbitMQQueueSetupService queueSetup,
        ITypeSafeMessagePublisher publisher,
        MessageHandlerRegistrationService registrations)
    {
        _configuration = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _queueSetup = queueSetup ?? throw new ArgumentNullException(nameof(queueSetup));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _registrations = registrations ?? throw new ArgumentNullException(nameof(registrations));
    }

    public event EventHandler<MessagePublishedEventArgs>? MessagePublished;
    public event EventHandler<MessageProcessedEventArgs>? MessageProcessed;
    public event EventHandler<MessageErrorEventArgs>? MessageError;

    public async Task PublishAsync<TMessage>(TMessage message, string targetSites = "*", string? correlationId = null, CancellationToken cancellationToken = default) where TMessage : class
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (!_started) throw new InvalidOperationException("Message bus must be started before publishing.");

        GenericMessageEnvelope<TMessage> envelope = GenericMessageEnvelope<TMessage>.Create(message, _siteName, targetSites, correlationId);

        await _publisher.PublishAsync(envelope, cancellationToken);

        Interlocked.Increment(ref _messagesPublished);

        MessagePublished?.Invoke(this, new MessagePublishedEventArgs
        {
            MessageId = envelope.MessageId,
            MessageType = envelope.MessageType,
            TargetSites = envelope.TargetSites,
            Timestamp = envelope.Timestamp
        });
    }

    public void RegisterHandler<TMessage>(ITypeSafeMessageHandler<TMessage> handler) where TMessage : class
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        _directHandlers.AddOrUpdate(typeof(TMessage), _ => new List<object> { handler }, (_, list) => { list.Add(handler); return list; });
    }

    public async Task StartAsync(string siteName, CancellationToken cancellationToken = default)
    {
        if (_started) return;
        if (string.IsNullOrWhiteSpace(siteName)) throw new ArgumentException("Site name cannot be empty", nameof(siteName));

        _siteName = siteName;
        _startedAtUtc = DateTime.UtcNow;

        _configuration.Site.SiteName = siteName;
        _configuration.Validate();

        try
        {
            await _connectionManager.StartAsync(cancellationToken);

            // Only setup queues/exchanges if we're managing our own infrastructure
            if (!_configuration.Site.SkipQueueSetup)
            {
                _logger.Info($"Setting up RabbitMQ infrastructure for '{siteName}'...");
                await _queueSetup.SetupInfrastructureAsync(cancellationToken);
            }
            else
            {
                _logger.Info($"Skipping queue setup - using existing RabbitMQ infrastructure");
            }

            // Unconditional (runs even with SkipQueueSetup=true, e.g. Hawtsy) and re-run on every
            // reconnect: the static setup above is latched to run once and can't express "one
            // auto-delete queue per running instance", so this site's training-events queue is
            // declared+bound here instead of via GetAllQueueDefinitions/GetAllQueueBindings.
            string? trainingQueueName = await DeclareTrainingQueueForSiteAsync(siteName, cancellationToken);

            foreach (string queue in GetQueuesToConsume(siteName, trainingQueueName))
            {
                await _connectionManager.StartConsumingAsync(queue, HandleIncomingMessageAsync, cancellationToken);
            }

            _started = true;
            _logger.Info($"TypeSafeMessageBus started for site '{siteName}'");
        }
        catch
        {
            _started = false;
            await _connectionManager.StopAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_started) return;
        _started = false;
        await _connectionManager.StopAsync(cancellationToken);
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        return await _connectionManager.IsHealthyAsync(cancellationToken);
    }

    public MessageBusStatistics GetStatistics()
    {
        double msgsPerMinute;
        lock (_statsLock)
        {
            double mins = Math.Max(0.0001, (DateTime.UtcNow - _startedAtUtc).TotalMinutes);
            msgsPerMinute = _messagesProcessed / mins;
        }

        return new MessageBusStatistics
        {
            MessagesPublished = Interlocked.Read(ref _messagesPublished),
            MessagesProcessed = Interlocked.Read(ref _messagesProcessed),
            ProcessingErrors = Interlocked.Read(ref _processingErrors),
            AverageProcessingTimeMs = _avgProcessingTimeMs,
            MessagesPerMinute = msgsPerMinute,
            IsConnectionHealthy = _lastKnownHealthy,
            RegisteredHandlers = _registrations.GetRegistrations().Count,
            CollectedAt = DateTime.UtcNow
        };
    }

    public async Task<MessageBusStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        double msgsPerMinute;
        lock (_statsLock)
        {
            double mins = Math.Max(0.0001, (DateTime.UtcNow - _startedAtUtc).TotalMinutes);
            msgsPerMinute = _messagesProcessed / mins;
        }

        try
        {
            _lastKnownHealthy = await _connectionManager.IsHealthyAsync(cancellationToken);
        }
        catch
        {
        }

        return new MessageBusStatistics
        {
            MessagesPublished = Interlocked.Read(ref _messagesPublished),
            MessagesProcessed = Interlocked.Read(ref _messagesProcessed),
            ProcessingErrors = Interlocked.Read(ref _processingErrors),
            AverageProcessingTimeMs = _avgProcessingTimeMs,
            MessagesPerMinute = msgsPerMinute,
            IsConnectionHealthy = _lastKnownHealthy,
            RegisteredHandlers = _registrations.GetRegistrations().Count,
            CollectedAt = DateTime.UtcNow
        };
    }

    private IEnumerable<string> GetQueuesToConsume(string siteName, string? trainingQueueName)
    {
        List<string> queues = new List<string>
        {
            CrossSiteQueueTopology.MODEL_EVENTS_QUEUE,
            CrossSiteQueueTopology.MEDIA_EVENTS_QUEUE,
            CrossSiteQueueTopology.USER_INTERACTION_EVENTS_QUEUE,
            CrossSiteQueueTopology.SYSTEM_EVENTS_QUEUE,
            CrossSiteQueueTopology.GetInboxQueueForSite(siteName)
        };

        if (!string.IsNullOrWhiteSpace(trainingQueueName))
        {
            queues.Add(trainingQueueName);
        }

        if (_configuration.Site.ProcessBroadcastMessages)
        {
            queues.Add(CrossSiteQueueTopology.GetBroadcastQueueForSite(siteName));
        }

        return queues.Distinct(StringComparer.Ordinal);
    }

    /// <summary>Declares (and binds) this site's own training-events queue, per the site's routing-key
    /// map, and returns its name — or null if the site doesn't consume training events at all. Two queue
    /// shapes: Hawtsy gets one durable named queue shared across its instances (competing consumers is
    /// fine there — it does simple fan-in work); HartsyWeb gets one auto-delete queue per running
    /// instance (non-exclusive, x-expires-backed) so every instance sees every training event addressed
    /// to it, matching the pre-existing broadcast-based behavior.</summary>
    private async Task<string?> DeclareTrainingQueueForSiteAsync(string siteName, CancellationToken cancellationToken)
    {
        string[] routingKeys = CrossSiteQueueTopology.GetTrainingRoutingKeysForSite(siteName);
        if (routingKeys.Length == 0)
        {
            return null;
        }

        string queueName;
        Dictionary<string, object?> arguments = CrossSiteQueueTopology.GetTrainingQueueArguments(_configuration);
        QueueDefinition queueDefinition;

        if (siteName == CrossSiteQueueTopology.HAWTSY)
        {
            queueName = CrossSiteQueueTopology.HAWTSY_TRAINING_EVENTS_QUEUE;
            queueDefinition = new QueueDefinition
            {
                Name = queueName,
                Durable = true,
                Exclusive = false,
                AutoDelete = false,
                Arguments = arguments
            };
        }
        else
        {
            // One auto-delete queue per running instance. Non-exclusive (NOT exclusive=true — that pins
            // the queue to the declaring connection and would deadlock against
            // RabbitMQConnectionLifecycleManager's automatic reconnect, which opens a new connection).
            // x-expires makes an orphaned queue (instance hard-killed before it could clean up) self-reap.
            //
            // Durable despite being an ephemeral per-instance queue. RabbitMQ 4 denies the
            // transient + non-exclusive combination outright (the transient_nonexcl_queues
            // deprecated feature, state "denied" on 4.3.2), so declaring it transient failed with
            // a 541 INTERNAL_ERROR, took the whole message bus down with it, and left the site
            // retrying the declaration every 30 seconds forever. Exclusive is not the way out, per
            // the note above. Durable only means the definition survives a broker restart; the
            // queue is still removed by auto-delete when the last consumer goes away, and still
            // self-reaps via x-expires if the instance dies, so the shape is unchanged.
            queueName = $"{CrossSiteQueueTopology.HARTSY_TRAINING_EVENTS_QUEUE_PREFIX}.{Environment.MachineName}";
            arguments["x-expires"] = 10 * 60 * 1000;
            queueDefinition = new QueueDefinition
            {
                Name = queueName,
                Durable = true,
                Exclusive = false,
                AutoDelete = true,
                Arguments = arguments
            };
        }

        List<QueueBinding> bindings = routingKeys
            .Select(key => new QueueBinding(CrossSiteQueueTopology.TRAINING_EVENTS_EXCHANGE, queueName, key))
            .ToList();

        await _queueSetup.DeclareAndBindQueueAsync(queueDefinition, bindings, cancellationToken);

        return queueName;
    }

    private async Task<MessageConsumeResult> HandleIncomingMessageAsync(string body, Dictionary<string, object?> headers)
    {
        Stopwatch sw = Stopwatch.StartNew();
        _logger.Verbose($"[MSG-BUS] ===== INCOMING MESSAGE =====");
        _logger.Verbose($"[MSG-BUS] Body length: {body.Length} chars");
        string messageType;
        int version;
        string messageId;
        string sourceSite;
        string targetSites;
        try
        {
            using JsonDocument doc = JsonMessageSerializer.Parse(body);
            JsonElement root = doc.RootElement;
            messageType = root.TryGetProperty("MessageType", out JsonElement mt) ? mt.GetString() ?? string.Empty : string.Empty;
            version = root.TryGetProperty("Version", out JsonElement v) && v.TryGetInt32(out int vi) ? vi : 1;
            messageId = root.TryGetProperty("MessageId", out JsonElement mi) ? mi.GetString() ?? string.Empty : string.Empty;
            sourceSite = root.TryGetProperty("SourceSite", out JsonElement ss) ? ss.GetString() ?? string.Empty : string.Empty;
            targetSites = root.TryGetProperty("TargetSites", out JsonElement ts) ? ts.GetString() ?? "*" : "*";
            _logger.Verbose($"[MSG-BUS] Parsed: Type={messageType}, Id={messageId}, Source={sourceSite}, Target={targetSites}");
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to parse incoming message JSON", ex);
            Interlocked.Increment(ref _processingErrors);
            // Unparseable message can never succeed on retry — dead-letter it.
            return MessageConsumeResult.Reject;
        }
        if (!ShouldProcessTarget(targetSites, _siteName))
        {
            _logger.Verbose($"[MSG-BUS] Skipping message {messageId} - target '{targetSites}' doesn't match site '{_siteName}'");
            Interlocked.Increment(ref _messagesProcessed);
            return MessageConsumeResult.Ack;
        }
        _logger.Verbose($"[MSG-BUS] Processing message {messageId} of type {messageType}");
        try
        {
            bool anyHandler = false;

            List<MessageHandlerRegistration> handlers = _registrations.GetHandlersForMessageType(messageType).ToList();
            _logger.Verbose($"[MSG-BUS] Found {handlers.Count} handler(s) for message type '{messageType}'");
            foreach (MessageHandlerRegistration reg in handlers)
            {
                anyHandler = true;
                _logger.Verbose($"[MSG-BUS] Invoking handler: {reg.HandlerType.Name}");
                _ = version;

                Type envelopeType = typeof(GenericMessageEnvelope<>).MakeGenericType(reg.MessageType);
                object? envelopeObj = JsonMessageSerializer.Deserialize(body, envelopeType);

                if (envelopeObj == null)
                {
                    throw new InvalidOperationException($"Failed to deserialize envelope for {messageType}");
                }

                object handlerObj = _registrations.CreateHandler(reg);

                System.Reflection.MethodInfo? handleMethod = handlerObj.GetType().GetMethod("HandleAsync");
                if (handleMethod == null)
                {
                    throw new InvalidOperationException($"Handler {reg.HandlerType.Name} missing HandleAsync");
                }

                Task<MessageHandlerResult> task = (Task<MessageHandlerResult>)handleMethod.Invoke(handlerObj, new object?[] { envelopeObj, CancellationToken.None })!;
                MessageHandlerResult result = await task;

                if (!result.IsSuccess)
                {
                    Interlocked.Increment(ref _processingErrors);

                    MessageError?.Invoke(this, new MessageErrorEventArgs
                    {
                        MessageId = messageId,
                        MessageType = messageType,
                        ErrorMessage = result.ErrorMessage ?? "Handler failed",
                        Exception = result.Exception,
                        Timestamp = DateTime.UtcNow
                    });

                    // Retryable → requeue (capped by redelivery in the consumer); otherwise dead-letter.
                    return result.ShouldRetry ? MessageConsumeResult.Requeue : MessageConsumeResult.Reject;
                }

                MessageProcessed?.Invoke(this, new MessageProcessedEventArgs
                {
                    MessageId = messageId,
                    MessageType = messageType,
                    HandlerType = reg.HandlerType.FullName ?? reg.HandlerType.Name,
                    ProcessingTime = sw.Elapsed,
                    Timestamp = DateTime.UtcNow
                });
            }

            // Directly-registered handlers (via RegisterHandler). HartsyStorage and HartsySeeder wire their
            // handlers this way instead of DI AddMessageHandler, so they MUST be dispatched here too —
            // otherwise those services silently consume and drop every message (handler never runs).
            Type? directType = _directHandlers.Keys.FirstOrDefault(t => t.Name == messageType);
            if (directType != null && _directHandlers.TryGetValue(directType, out List<object>? directList) && directList.Count > 0)
            {
                Type envelopeType = typeof(GenericMessageEnvelope<>).MakeGenericType(directType);
                object? envelopeObj = JsonMessageSerializer.Deserialize(body, envelopeType)
                    ?? throw new InvalidOperationException($"Failed to deserialize envelope for {messageType}");

                foreach (object handler in directList.ToList())
                {
                    anyHandler = true;
                    _logger.Verbose($"[MSG-BUS] Invoking direct handler: {handler.GetType().Name}");

                    System.Reflection.MethodInfo? handleMethod = handler.GetType().GetMethod("HandleAsync");
                    if (handleMethod == null)
                    {
                        throw new InvalidOperationException($"Direct handler {handler.GetType().Name} missing HandleAsync");
                    }

                    Task<MessageHandlerResult> task = (Task<MessageHandlerResult>)handleMethod.Invoke(handler, new object?[] { envelopeObj, CancellationToken.None })!;
                    MessageHandlerResult result = await task;

                    if (!result.IsSuccess)
                    {
                        Interlocked.Increment(ref _processingErrors);
                        MessageError?.Invoke(this, new MessageErrorEventArgs
                        {
                            MessageId = messageId,
                            MessageType = messageType,
                            ErrorMessage = result.ErrorMessage ?? "Handler failed",
                            Exception = result.Exception,
                            Timestamp = DateTime.UtcNow
                        });
                        return result.ShouldRetry ? MessageConsumeResult.Requeue : MessageConsumeResult.Reject;
                    }

                    MessageProcessed?.Invoke(this, new MessageProcessedEventArgs
                    {
                        MessageId = messageId,
                        MessageType = messageType,
                        HandlerType = handler.GetType().FullName ?? handler.GetType().Name,
                        ProcessingTime = sw.Elapsed,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            if (!anyHandler)
            {
                _logger.Warning($"No handlers registered for messageType '{messageType}'");
            }

            Interlocked.Increment(ref _messagesProcessed);

            lock (_statsLock)
            {
                if (_messagesProcessed == 1)
                {
                    _avgProcessingTimeMs = sw.Elapsed.TotalMilliseconds;
                }
                else
                {
                    _avgProcessingTimeMs = (_avgProcessingTimeMs * 0.95) + (sw.Elapsed.TotalMilliseconds * 0.05);
                }
            }

            return MessageConsumeResult.Ack;
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _processingErrors);

            _logger.Error($"Unhandled exception processing messageType '{messageType}'", ex);

            MessageError?.Invoke(this, new MessageErrorEventArgs
            {
                MessageId = messageId,
                MessageType = messageType,
                ErrorMessage = ex.Message,
                Exception = ex,
                Timestamp = DateTime.UtcNow
            });

            // Unhandled exception may be transient (broker/DB blip): requeue once, then dead-letter.
            return MessageConsumeResult.Requeue;
        }
        finally
        {
            sw.Stop();
        }
    }

    private static bool ShouldProcessTarget(string targetSites, string siteName)
    {
        if (targetSites == "*") return true;
        if (string.IsNullOrWhiteSpace(siteName)) return false;

        string[] targets = targetSites.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return targets.Any(t => t.Trim().Equals(siteName, StringComparison.OrdinalIgnoreCase));
    }
}
