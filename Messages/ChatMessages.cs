using System.Text.Json.Serialization;

namespace HartsyRabbit.Messages;

public sealed record ChatMessageDispatch
{
    public string? ThreadId { get; init; }
    public string? ClientMessageId { get; init; }
    public string SenderUserId { get; init; } = string.Empty;
    public string RecipientUserId { get; init; } = string.Empty;
    public string? SenderDisplayName { get; init; }
    public string? RecipientDisplayName { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? ThreadSubject { get; init; }

    [JsonPropertyName("allowThreadCreation")]
    public bool AllowThreadCreation { get; init; } = true;

    public Dictionary<string, string>? Metadata { get; init; }
}

public sealed record ChatMessageDeleteCommand
{
    public string MessageId { get; init; } = string.Empty;
    public string RequestingUserId { get; init; } = string.Empty;
    public string? ThreadId { get; init; }

    [JsonPropertyName("deleteForEveryone")]
    public bool DeleteForEveryone { get; init; } = true;
}
