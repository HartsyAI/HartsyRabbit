namespace HartsyRabbit.Core;

public record GenericMessageEnvelope<TMessage> where TMessage : class
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string MessageType { get; init; } = typeof(TMessage).Name;
    public string SourceSite { get; init; } = string.Empty;
    public string TargetSites { get; init; } = "*";
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public int Version { get; init; } = 1;
    public string? CorrelationId { get; init; }
    public TMessage Payload { get; init; } = default!;
    public Dictionary<string, object>? Metadata { get; init; }

    public static GenericMessageEnvelope<TMessage> Create(TMessage payload, string sourceSite, string targetSites = "*", string? correlationId = null)
    {
        return new GenericMessageEnvelope<TMessage>
        {
            Payload = payload,
            SourceSite = sourceSite,
            TargetSites = targetSites,
            CorrelationId = correlationId
        };
    }

    public bool IsTargetedAt(string siteName)
    {
        if (string.IsNullOrWhiteSpace(siteName))
        {
            return false;
        }

        if (TargetSites == "*")
        {
            return true;
        }

        string[] targets = TargetSites.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return targets.Any(target => target.Trim().Equals(siteName, StringComparison.OrdinalIgnoreCase));
    }

    public GenericMessageEnvelope<TResponse> CreateResponse<TResponse>(TResponse responsePayload, string respondingSite) where TResponse : class
    {
        return new GenericMessageEnvelope<TResponse>
        {
            Payload = responsePayload,
            SourceSite = respondingSite,
            TargetSites = SourceSite,
            CorrelationId = MessageId
        };
    }
}
