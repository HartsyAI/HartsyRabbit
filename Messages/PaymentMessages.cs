namespace HartsyRabbit.Messages;

public class PayoutProcessedMessage : IPayoutProcessedMessage
{
    public string PaymentEventType { get; set; } = "PayoutProcessed";
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }
    public string PayoutId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
    public string PayoutMethod { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
