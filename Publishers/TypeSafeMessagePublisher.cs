using HartsyRabbit.Configuration;
using HartsyRabbit.Core;
using HartsyRabbit.Infrastructure;
using HartsyRabbit.Logging;
using HartsyRabbit.Serialization;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

namespace HartsyRabbit.Publishers;

public sealed class TypeSafeMessagePublisher : ITypeSafeMessagePublisher
{
    private readonly MessageBusConfiguration _configuration;
    private readonly IRabbitMQConnectionLifecycleManager _connectionManager;
    private readonly IMessageBusLogger _logger;

    private readonly SemaphoreSlim _publishLock = new(1, 1);

    public TypeSafeMessagePublisher(
        IOptions<MessageBusConfiguration> configuration,
        IRabbitMQConnectionLifecycleManager connectionManager,
        IMessageBusLogger logger)
    {
        _configuration = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishAsync<TMessage>(GenericMessageEnvelope<TMessage> envelope, CancellationToken cancellationToken = default) where TMessage : class
    {
        if (envelope == null) throw new ArgumentNullException(nameof(envelope));

        await _publishLock.WaitAsync(cancellationToken);
        try
        {
            string body = JsonMessageSerializer.Serialize(envelope);
            MessageRoutingInfo routing = DetermineRouting(envelope);

            IChannel channel = await _connectionManager.GetPublishChannelAsync(cancellationToken);

            BasicProperties props = new BasicProperties
            {
                MessageId = envelope.MessageId,
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                DeliveryMode = DeliveryModes.Persistent,
                Timestamp = new AmqpTimestamp(((DateTimeOffset)envelope.Timestamp).ToUnixTimeSeconds()),
                Type = envelope.MessageType,
                CorrelationId = envelope.CorrelationId,
                AppId = envelope.SourceSite
            };

            props.Headers = new Dictionary<string, object?>
            {
                ["x-source-site"] = envelope.SourceSite,
                ["x-target-sites"] = envelope.TargetSites,
                ["x-message-type"] = envelope.MessageType,
                ["x-message-version"] = envelope.Version
            };

            await channel.BasicPublishAsync(
                exchange: routing.Exchange,
                routingKey: routing.RoutingKey,
                mandatory: true,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(body),
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to publish {envelope.MessageType} ({envelope.MessageId})", ex);
            throw;
        }
        finally
        {
            _publishLock.Release();
        }
    }

    public async Task PublishDirectAsync<TMessage>(GenericMessageEnvelope<TMessage> envelope, string queueName, CancellationToken cancellationToken = default) where TMessage : class
    {
        if (envelope == null) throw new ArgumentNullException(nameof(envelope));
        if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentException("Queue name cannot be empty", nameof(queueName));

        await _publishLock.WaitAsync(cancellationToken);
        try
        {
            string body = JsonMessageSerializer.Serialize(envelope);
            IChannel channel = await _connectionManager.GetPublishChannelAsync(cancellationToken);

            BasicProperties props = new BasicProperties
            {
                MessageId = envelope.MessageId,
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                DeliveryMode = DeliveryModes.Persistent,
                Timestamp = new AmqpTimestamp(((DateTimeOffset)envelope.Timestamp).ToUnixTimeSeconds()),
                Type = envelope.MessageType,
                CorrelationId = envelope.CorrelationId,
                AppId = envelope.SourceSite
            };

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: true,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(body),
                cancellationToken: cancellationToken);
        }
        finally
        {
            _publishLock.Release();
        }
    }

    private MessageRoutingInfo DetermineRouting<TMessage>(GenericMessageEnvelope<TMessage> envelope) where TMessage : class
    {
        if (envelope.TargetSites == "*")
        {
            return new MessageRoutingInfo { Exchange = CrossSiteQueueTopology.BROADCAST_EXCHANGE, RoutingKey = "", IsBroadcast = true };
        }

        string[] targets = envelope.TargetSites.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (targets.Length == 1)
        {
            string site = targets[0].Trim();
            return new MessageRoutingInfo { Exchange = CrossSiteQueueTopology.SITE_ROUTING_EXCHANGE, RoutingKey = CrossSiteQueueTopology.GetRoutingKeyForSite(site), IsBroadcast = false };
        }

        return new MessageRoutingInfo { Exchange = CrossSiteQueueTopology.DOMAIN_EVENTS_EXCHANGE, RoutingKey = CrossSiteQueueTopology.GetRoutingKeyForMessageType(envelope.MessageType), IsBroadcast = false };
    }

    private sealed class MessageRoutingInfo
    {
        public string Exchange { get; set; } = string.Empty;
        public string RoutingKey { get; set; } = string.Empty;
        public bool IsBroadcast { get; set; }
    }
}
