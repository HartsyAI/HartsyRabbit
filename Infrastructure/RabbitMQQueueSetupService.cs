using HartsyRabbit.Configuration;
using HartsyRabbit.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace HartsyRabbit.Infrastructure;

public sealed class RabbitMQQueueSetupService : IRabbitMQQueueSetupService
{
    private readonly MessageBusConfiguration _configuration;
    private readonly IRabbitMQConnectionLifecycleManager _connectionManager;
    private readonly IMessageBusLogger _logger;

    private bool _isSetup;
    private readonly object _lock = new();

    public RabbitMQQueueSetupService(
        IOptions<MessageBusConfiguration> configuration,
        IRabbitMQConnectionLifecycleManager connectionManager,
        IMessageBusLogger logger)
    {
        _configuration = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SetupInfrastructureAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_isSetup) return;
        }

        using IChannel channel = await _connectionManager.GetPublishChannelAsync(cancellationToken);

        List<ExchangeDefinition> exchanges = CrossSiteQueueTopology.GetAllExchangeDefinitions();
        foreach (ExchangeDefinition exchange in exchanges)
        {
            await channel.ExchangeDeclareAsync(
                exchange: exchange.Name,
                type: exchange.Type,
                durable: exchange.Durable,
                autoDelete: exchange.AutoDelete,
                arguments: exchange.Arguments,
                cancellationToken: cancellationToken);
        }

        List<QueueDefinition> queues = CrossSiteQueueTopology.GetAllQueueDefinitions(_configuration);

        QueueDefinition? deadLetter = queues.FirstOrDefault(q => q.Name == CrossSiteQueueTopology.DEAD_LETTER_QUEUE);
        if (deadLetter != null)
        {
            await DeclareQueueAsync(channel, deadLetter, cancellationToken);
        }

        foreach (QueueDefinition queue in queues.Where(q => q.Name != CrossSiteQueueTopology.DEAD_LETTER_QUEUE))
        {
            await DeclareQueueAsync(channel, queue, cancellationToken);
        }

        List<QueueBinding> bindings = CrossSiteQueueTopology.GetAllQueueBindings();
        foreach (QueueBinding binding in bindings)
        {
            await channel.QueueBindAsync(
                queue: binding.QueueName,
                exchange: binding.ExchangeName,
                routingKey: binding.RoutingKey,
                arguments: null,
                cancellationToken: cancellationToken);
        }

        lock (_lock)
        {
            _isSetup = true;
        }

        _logger.Info("RabbitMQ infrastructure setup complete");
    }

    private async Task DeclareQueueAsync(IChannel channel, QueueDefinition queue, CancellationToken cancellationToken)
    {
        try
        {
            await channel.QueueDeclareAsync(
                queue: queue.Name,
                durable: queue.Durable,
                exclusive: queue.Exclusive,
                autoDelete: queue.AutoDelete,
                arguments: queue.Arguments,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to declare queue '{queue.Name}'", ex);
            throw;
        }
    }
}
