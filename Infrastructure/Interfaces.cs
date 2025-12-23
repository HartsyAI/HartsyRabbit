namespace HartsyRabbit.Infrastructure;

public interface IRabbitMQConnectionLifecycleManager
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);

    Task<RabbitMQ.Client.IChannel> GetPublishChannelAsync(CancellationToken cancellationToken = default);

    Task StartConsumingAsync(
        string queueName,
        Func<string, Dictionary<string, object?>, Task<bool>> messageHandler,
        CancellationToken cancellationToken = default);
}

public interface IRabbitMQQueueSetupService
{
    Task SetupInfrastructureAsync(CancellationToken cancellationToken = default);
}
