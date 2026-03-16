using HartsyRabbit.Configuration;
using HartsyRabbit.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;

namespace HartsyRabbit.Infrastructure;

public sealed class RabbitMQConnectionLifecycleManager : IRabbitMQConnectionLifecycleManager, IAsyncDisposable, IDisposable
{
    private readonly MessageBusConfiguration _configuration;
    private readonly IMessageBusLogger _logger;

    private IConnection? _connection;
    private IChannel? _publishChannel;

    private readonly ConcurrentDictionary<string, IChannel> _consumerChannels = new();
    private readonly ConcurrentDictionary<string, string> _consumerTags = new();

    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private bool _started;
    private bool _disposed;

    public RabbitMQConnectionLifecycleManager(IOptions<MessageBusConfiguration> configuration, IMessageBusLogger logger)
    {
        _configuration = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RabbitMQConnectionLifecycleManager));
        if (_started) return;

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_started) return;
            await ConnectInternalAsync(cancellationToken);
            _started = true;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            _started = false;

            foreach ((string queue, IChannel ch) in _consumerChannels)
            {
                try
                {
                    if (_consumerTags.TryGetValue(queue, out string? tag) && !string.IsNullOrWhiteSpace(tag))
                    {
                        await ch.BasicCancelAsync(tag, cancellationToken: cancellationToken);
                    }
                }
                catch
                {
                }

                try
                {
                    await ch.CloseAsync(cancellationToken: cancellationToken);
                }
                catch
                {
                }

                ch.Dispose();
            }

            _consumerChannels.Clear();
            _consumerTags.Clear();

            if (_publishChannel != null)
            {
                try { await _publishChannel.CloseAsync(cancellationToken: cancellationToken); } catch { }
                _publishChannel.Dispose();
                _publishChannel = null;
            }

            if (_connection != null)
            {
                try { await _connection.CloseAsync(cancellationToken: cancellationToken); } catch { }
                _connection.Dispose();
                _connection = null;
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>Checks if the RabbitMQ connection is healthy.</summary>
    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        if (!_started || _disposed) return false;
        if (_connection?.IsOpen != true) return false;

        if (_publishChannel?.IsOpen == true) return true;

        try
        {
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            IChannel test = await _connection.CreateChannelAsync(cancellationToken: cts.Token);
            await test.CloseAsync(cts.Token);
            test.Dispose();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IChannel> GetPublishChannelAsync(CancellationToken cancellationToken = default)
    {
        if (!_started) throw new InvalidOperationException("Connection manager must be started before publishing.");

        if (_publishChannel != null && _publishChannel.IsOpen && _connection?.IsOpen == true)
        {
            return _publishChannel;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            // Reconnect if needed (lock is already held, so call internal directly)
            if (_connection?.IsOpen != true)
            {
                await ConnectInternalAsync(cancellationToken);
            }

            if (_publishChannel != null && _publishChannel.IsOpen)
            {
                return _publishChannel;
            }

            _publishChannel = await _connection!.CreateChannelAsync(cancellationToken: cancellationToken);
            _publishChannel.BasicReturnAsync += (sender, args) =>
            {
                _logger.Warning($"RabbitMQ returned message (unroutable). ReplyCode={args.ReplyCode} ReplyText={args.ReplyText} Exchange={args.Exchange} RoutingKey={args.RoutingKey}");
                return Task.CompletedTask;
            };

            return _publishChannel;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task StartConsumingAsync(
        string queueName,
        Func<string, Dictionary<string, object?>, Task<bool>> messageHandler,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentException("Queue name cannot be null or empty", nameof(queueName));
        if (messageHandler == null) throw new ArgumentNullException(nameof(messageHandler));
        if (!_started) throw new InvalidOperationException("Connection manager must be started before consuming.");

        if (_connection?.IsOpen != true)
        {
            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                if (_connection?.IsOpen != true)
                {
                    await ConnectInternalAsync(cancellationToken);
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        if (_consumerChannels.TryGetValue(queueName, out IChannel? existing) && existing.IsOpen)
        {
            return;
        }

        IChannel channel = await _connection!.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: (ushort)Math.Clamp(_configuration.Site.MaxConcurrentHandlers, 1, 1000),
            global: false,
            cancellationToken: cancellationToken);

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            bool ok = false;
            try
            {
                string body = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                Dictionary<string, object?> headers = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                if (eventArgs.BasicProperties?.Headers != null)
                {
                    foreach (KeyValuePair<string, object?> kvp in eventArgs.BasicProperties.Headers)
                    {
                        headers[kvp.Key] = kvp.Value;
                    }
                }

                ok = await messageHandler(body, headers);
            }
            catch (Exception ex)
            {
                _logger.Error($"Unhandled exception in consumer for {queueName}", ex);
                ok = false;
            }

            try
            {
                if (ok)
                {
                    await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: CancellationToken.None);
                }
                else
                {
                    await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, cancellationToken: CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to ack/nack message from {queueName}", ex);
            }
        };

        string tag = await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);

        _consumerChannels[queueName] = channel;
        _consumerTags[queueName] = tag;

        _logger.Info($"Started consumer for queue '{queueName}' tag '{tag}'");
    }

    /// <summary>
    /// Creates the RabbitMQ connection. Caller MUST hold _connectionLock.
    /// </summary>
    private async Task ConnectInternalAsync(CancellationToken cancellationToken)
    {
        if (_connection?.IsOpen == true) return;

        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = _configuration.Connection.HostName,
            Port = _configuration.Connection.Port,
            UserName = _configuration.Connection.Username,
            Password = _configuration.Connection.Password,
            VirtualHost = _configuration.Connection.VirtualHost,
            RequestedHeartbeat = TimeSpan.FromSeconds(_configuration.Connection.RequestedHeartbeatSeconds),
            RequestedConnectionTimeout = TimeSpan.FromSeconds(_configuration.Connection.ConnectionTimeoutSeconds),
            AutomaticRecoveryEnabled = _configuration.Connection.AutomaticRecoveryEnabled,
            TopologyRecoveryEnabled = _configuration.Connection.AutomaticRecoveryEnabled
        };

        if (_configuration.Connection.UseTLS)
        {
            factory.Ssl.Enabled = true;
            if (!string.IsNullOrWhiteSpace(_configuration.Connection.TLSServerName))
            {
                factory.Ssl.ServerName = _configuration.Connection.TLSServerName;
            }
        }

        try
        {
            _logger.Info($"[RABBITMQ-CONN] Attempting connection to {_configuration.Connection.HostName}:{_configuration.Connection.Port}...");
            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _logger.Info($"[RABBITMQ-CONN] ✓ Connection established successfully!");

            _connection.ConnectionShutdownAsync += (sender, args) =>
            {
                _logger.Warning($"RabbitMQ connection shutdown: {args.ReplyText}");
                return Task.CompletedTask;
            };
        }
        catch (Exception ex)
        {
            _logger.Error($"[RABBITMQ-CONN] ✗ Connection failed: {ex.GetType().Name} - {ex.Message}");
            _logger.Error($"[RABBITMQ-CONN] Stack: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>Async disposal with graceful shutdown.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
            await StopAsync(cts.Token);
        }
        catch { }

        _connectionLock.Dispose();
    }

    /// <summary>Sync disposal — releases resources without async graceful shutdown.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            foreach ((string _, IChannel ch) in _consumerChannels)
            {
                try { ch.Dispose(); } catch { }
            }
            _consumerChannels.Clear();
            _consumerTags.Clear();
            _publishChannel?.Dispose();
            _publishChannel = null;
            _connection?.Dispose();
            _connection = null;
        }
        catch { }

        _connectionLock.Dispose();
    }
}
