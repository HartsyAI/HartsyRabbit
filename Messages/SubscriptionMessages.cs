namespace HartsyRabbit.Messages;

public class SubscriptionActivatedMessage : IPaymentMessage
{
    public string PaymentEventType { get; set; } = "SubscriptionActivated";
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }
    public string SubscriptionId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public DateTime CurrentPeriodStart { get; set; } = DateTime.UtcNow;
    public DateTime CurrentPeriodEnd { get; set; } = DateTime.UtcNow;
    public string PaymentProvider { get; set; } = string.Empty;
}

public class SubscriptionCancelledMessage : IPaymentMessage
{
    public string PaymentEventType { get; set; } = "SubscriptionCancelled";
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }
    public string SubscriptionId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime EndsAt { get; set; } = DateTime.UtcNow;
    public string? CancellationReason { get; set; }
    public string PaymentProvider { get; set; } = string.Empty;
}
