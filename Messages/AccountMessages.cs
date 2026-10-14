namespace HartsyRabbit.Messages;

/// <summary>Published when an account is frozen, restored, or starts or finishes a purge. Consumers drop any
/// state they hold for the user. Route it with the target site, so it reaches that site's inbox only.</summary>
public class AccountStatusChangedMessage
{
    public string UserId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public bool IsAdminAction { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
