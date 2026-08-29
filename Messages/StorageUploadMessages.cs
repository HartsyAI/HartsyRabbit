namespace HartsyRabbit.Messages;

/// <summary>Advisory derivative/status event emitted by the storage authority. The authoritative
/// terminal lifecycle is carried by <see cref="MediaUploadCompletedMessage"/>; this event lets the
/// owning site expose thumbnails or previews that finish independently.</summary>
public sealed record UploadStatusChangedMessage
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public string UploadId { get; init; } = string.Empty;
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public List<string>? ThumbnailUrls { get; init; }
    public string? DominantColor { get; init; }
    public string? VideoPreviewUrl { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
