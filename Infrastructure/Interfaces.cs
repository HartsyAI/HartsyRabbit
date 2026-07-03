namespace HartsyRabbit.Infrastructure;

/// <summary>How the broker should dispose of a delivered message after the handler runs.
/// Ack = handled, remove. Requeue = transient failure, put back for another attempt (capped by
/// redelivery so a poison message can't hot-loop). Reject = permanent failure / unparseable,
/// dead-letter it (never requeue).</summary>
public enum MessageConsumeResult
{
    Ack,
    Requeue,
    Reject
}

public interface IRabbitMQConnectionLifecycleManager
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);

    Task<RabbitMQ.Client.IChannel> GetPublishChannelAsync(CancellationToken cancellationToken = default);

    Task StartConsumingAsync(
        string queueName,
        Func<string, Dictionary<string, object?>, Task<MessageConsumeResult>> messageHandler,
        CancellationToken cancellationToken = default);
}

public interface IRabbitMQQueueSetupService
{
    Task SetupInfrastructureAsync(CancellationToken cancellationToken = default);
}
