namespace HartsyRabbit.Messages;

/// <summary>Message published when system health metrics are updated.
/// Used for live admin dashboard updates across services.</summary>
public sealed record SystemHealthUpdatedMessage : ISystemMessage
{
    public string SystemEventType { get; init; } = "SystemHealthUpdated";
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; init; }

    public string OverallHealth { get; init; } = "Unknown";
    public string DatabaseHealth { get; init; } = "Unknown";
    public string StorageHealth { get; init; } = "Unknown";
    public string ApiHealth { get; init; } = "Unknown";
    public string BotHealth { get; init; } = "Unknown";

    public int DatabaseResponseTime { get; init; }
    public int StorageResponseTime { get; init; }
    public int ApiResponseTime { get; init; }
    public int BotResponseTime { get; init; }
    public double StorageCapacityRemaining { get; init; }
    public long DatabaseUptimeSeconds { get; init; }
}
