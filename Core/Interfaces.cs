using HartsyRabbit.Core;

namespace HartsyRabbit.Core;

public interface ITypeSafeMessageBus
{
    Task PublishAsync<TMessage>(TMessage message, string targetSites = "*", string? correlationId = null, CancellationToken cancellationToken = default) where TMessage : class;
    void RegisterHandler<TMessage>(ITypeSafeMessageHandler<TMessage> handler) where TMessage : class;
    Task StartAsync(string siteName, CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    MessageBusStatistics GetStatistics();
    event EventHandler<MessagePublishedEventArgs>? MessagePublished;
    event EventHandler<MessageProcessedEventArgs>? MessageProcessed;
    event EventHandler<MessageErrorEventArgs>? MessageError;
}

public interface ITypeSafeMessageHandler<TMessage> where TMessage : class
{
    Task<MessageHandlerResult> HandleAsync(GenericMessageEnvelope<TMessage> envelope, CancellationToken cancellationToken);
    bool CanHandle(string messageType, int version);
}

public interface IMessageHandler<TMessage> : ITypeSafeMessageHandler<TMessage> where TMessage : class
{
}

public class MessageBusStatistics
{
    public long MessagesPublished { get; set; }
    public long MessagesProcessed { get; set; }
    public long ProcessingErrors { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public double MessagesPerMinute { get; set; }
    public bool IsConnectionHealthy { get; set; }
    public int RegisteredHandlers { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}

public class MessagePublishedEventArgs : EventArgs
{
    public string MessageType { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public string TargetSites { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class MessageProcessedEventArgs : EventArgs
{
    public string MessageType { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public string HandlerType { get; set; } = string.Empty;
    public TimeSpan ProcessingTime { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class MessageErrorEventArgs : EventArgs
{
    public string MessageType { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
