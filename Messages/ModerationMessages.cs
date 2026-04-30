namespace HartsyRabbit.Messages;

/// <summary>Published when a user submits a new content report.</summary>
public class ReportSubmittedMessage
{
    public string ReportId { get; set; } = string.Empty;
    public short Severity { get; set; }
    public string CategoryKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string ContentId { get; set; } = string.Empty;
    public bool AutoEscalate { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>Published when an admin resolves or dismisses a report.</summary>
public class ReportResolvedMessage
{
    public string ReportId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ResolvedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>Published when a user blocks another user.</summary>
public class UserBlockedMessage
{
    public string BlockerId { get; set; } = string.Empty;
    public string BlockedId { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Published when a user unblocks another user.</summary>
public class UserUnblockedMessage
{
    public string BlockerId { get; set; } = string.Empty;
    public string BlockedId { get; set; } = string.Empty;
    public DateTime UnblockedAt { get; set; } = DateTime.UtcNow;
}
