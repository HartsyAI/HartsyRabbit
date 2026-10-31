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

/// <summary>Sent by the Discord bot to HartsyWeb on a timer, so the admin dashboard can tell a live bot from a silent one.</summary>
/// <remarks>The bot has no inbound port, so it cannot be polled; silence is the signal. HartsyWeb treats the bot as
/// down once no heartbeat has arrived for a few multiples of <see cref="IntervalSeconds"/>, which the sender states
/// rather than the receiver guessing. The broker is shared by every environment, so <see cref="HartsyOrigin"/> names
/// the site the bot works against and a receiver ignores heartbeats for any other.</remarks>
public sealed record BotHeartbeatMessage : ISystemMessage
{
    public string SystemEventType { get; init; } = "BotHeartbeat";
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>The site the bot talks to (its <c>BASE_URL</c>), without a trailing slash.</summary>
    public required string HartsyOrigin { get; init; }

    /// <summary>True when the Discord gateway session is connected, which is not the same as the process running.</summary>
    public bool GatewayConnected { get; init; }

    /// <summary>The estimated round trip to the Discord gateway, in milliseconds.</summary>
    public int GatewayLatencyMs { get; init; }

    public int GuildCount { get; init; }

    /// <summary>When this bot process started, UTC.</summary>
    public DateTime StartedAt { get; init; }

    public string Version { get; init; } = string.Empty;

    /// <summary>How often the sender intends to send this, in seconds.</summary>
    public int IntervalSeconds { get; init; }

    public static BotHeartbeatMessage Create(string hartsyOrigin, bool gatewayConnected, int gatewayLatencyMs, int guildCount, DateTime startedAt, string version, int intervalSeconds)
        => new()
        {
            HartsyOrigin = hartsyOrigin.TrimEnd('/'),
            GatewayConnected = gatewayConnected,
            GatewayLatencyMs = gatewayLatencyMs,
            GuildCount = guildCount,
            StartedAt = startedAt,
            Version = version,
            IntervalSeconds = intervalSeconds
        };
}
