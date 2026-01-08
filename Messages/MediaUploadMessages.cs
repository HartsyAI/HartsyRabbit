using System.Text.Json.Serialization;

namespace HartsyRabbit.Messages;

/// <summary>
/// Media types supported by the upload system
/// </summary>
public enum MediaType
{
    Model,
    Image,
    Video,
    Dataset,
    Audio,
    Model3D,
    Document,
    Other
}

/// <summary>
/// Base interface for all media upload messages
/// </summary>
public interface IMediaUploadMessage
{
    string UploadId { get; }
    MediaType MediaType { get; }
    string UserId { get; }
    string SourceSite { get; }
    DateTime Timestamp { get; }
}

/// <summary>
/// Sent when a media upload is initiated
/// </summary>
public record MediaUploadStartedMessage : IMediaUploadMessage
{
    [JsonPropertyName("upload_id")]
    public required string UploadId { get; init; }

    [JsonPropertyName("media_type")]
    public required MediaType MediaType { get; init; }

    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }

    [JsonPropertyName("file_name")]
    public required string FileName { get; init; }

    [JsonPropertyName("file_size_bytes")]
    public long FileSizeBytes { get; init; }

    [JsonPropertyName("content_type")]
    public string? ContentType { get; init; }

    [JsonPropertyName("source_site")]
    public string SourceSite { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    public static MediaUploadStartedMessage Create(
        string uploadId,
        MediaType mediaType,
        string userId,
        string fileName,
        long fileSizeBytes,
        string sourceSite,
        string? contentType = null,
        Dictionary<string, object>? metadata = null)
    {
        return new MediaUploadStartedMessage
        {
            UploadId = uploadId,
            MediaType = mediaType,
            UserId = userId,
            FileName = fileName,
            FileSizeBytes = fileSizeBytes,
            SourceSite = sourceSite,
            ContentType = contentType,
            Metadata = metadata
        };
    }
}

/// <summary>
/// Sent to update progress during upload processing
/// </summary>
public record MediaUploadProgressMessage : IMediaUploadMessage
{
    [JsonPropertyName("upload_id")]
    public required string UploadId { get; init; }

    [JsonPropertyName("media_type")]
    public required MediaType MediaType { get; init; }

    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }

    [JsonPropertyName("source_site")]
    public string SourceSite { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    [JsonPropertyName("progress_percent")]
    public int ProgressPercent { get; init; }

    [JsonPropertyName("current_step")]
    public required string CurrentStep { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = "processing";

    [JsonPropertyName("estimated_minutes_remaining")]
    public double? EstimatedMinutesRemaining { get; init; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    public static MediaUploadProgressMessage Create(
        string uploadId,
        MediaType mediaType,
        string userId,
        int progressPercent,
        string currentStep,
        string status,
        string sourceSite,
        double? estimatedMinutesRemaining = null,
        Dictionary<string, object>? metadata = null)
    {
        return new MediaUploadProgressMessage
        {
            UploadId = uploadId,
            MediaType = mediaType,
            UserId = userId,
            ProgressPercent = progressPercent,
            CurrentStep = currentStep,
            Status = status,
            SourceSite = sourceSite,
            EstimatedMinutesRemaining = estimatedMinutesRemaining,
            Metadata = metadata
        };
    }
}

/// <summary>
/// Sent when upload processing is complete (success or failure)
/// </summary>
public record MediaUploadCompletedMessage : IMediaUploadMessage
{
    [JsonPropertyName("upload_id")]
    public required string UploadId { get; init; }

    [JsonPropertyName("media_type")]
    public required MediaType MediaType { get; init; }

    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }

    [JsonPropertyName("source_site")]
    public string SourceSite { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; init; }

    [JsonPropertyName("direct_url")]
    public string? DirectUrl { get; init; }

    [JsonPropertyName("torrent_info")]
    public TorrentInfoPayload? TorrentInfo { get; init; }

    [JsonPropertyName("thumbnail_urls")]
    public List<string>? ThumbnailUrls { get; init; }

    [JsonPropertyName("preview_url")]
    public string? PreviewUrl { get; init; }

    [JsonPropertyName("file_size_bytes")]
    public long? FileSizeBytes { get; init; }

    [JsonPropertyName("file_hash")]
    public string? FileHash { get; init; }

    [JsonPropertyName("processing_time_seconds")]
    public double? ProcessingTimeSeconds { get; init; }

    [JsonPropertyName("extracted_metadata")]
    public Dictionary<string, object>? ExtractedMetadata { get; init; }

    [JsonPropertyName("warnings")]
    public List<string>? Warnings { get; init; }

    public static MediaUploadCompletedMessage CreateSuccess(
        string uploadId,
        MediaType mediaType,
        string userId,
        string sourceSite,
        string? directUrl = null,
        TorrentInfoPayload? torrentInfo = null,
        List<string>? thumbnailUrls = null,
        string? previewUrl = null,
        long? fileSizeBytes = null,
        string? fileHash = null,
        double? processingTimeSeconds = null,
        Dictionary<string, object>? extractedMetadata = null,
        List<string>? warnings = null)
    {
        return new MediaUploadCompletedMessage
        {
            UploadId = uploadId,
            MediaType = mediaType,
            UserId = userId,
            SourceSite = sourceSite,
            Success = true,
            DirectUrl = directUrl,
            TorrentInfo = torrentInfo,
            ThumbnailUrls = thumbnailUrls,
            PreviewUrl = previewUrl,
            FileSizeBytes = fileSizeBytes,
            FileHash = fileHash,
            ProcessingTimeSeconds = processingTimeSeconds,
            ExtractedMetadata = extractedMetadata,
            Warnings = warnings
        };
    }

    public static MediaUploadCompletedMessage CreateFailure(
        string uploadId,
        MediaType mediaType,
        string userId,
        string sourceSite,
        string errorMessage,
        double? processingTimeSeconds = null)
    {
        return new MediaUploadCompletedMessage
        {
            UploadId = uploadId,
            MediaType = mediaType,
            UserId = userId,
            SourceSite = sourceSite,
            Success = false,
            ErrorMessage = errorMessage,
            ProcessingTimeSeconds = processingTimeSeconds
        };
    }
}

/// <summary>
/// Torrent information payload
/// </summary>
public record TorrentInfoPayload
{
    [JsonPropertyName("infohash")]
    public string? InfoHash { get; init; }

    [JsonPropertyName("magnet_link")]
    public string? MagnetLink { get; init; }

    [JsonPropertyName("torrent_url")]
    public string? TorrentUrl { get; init; }

    [JsonPropertyName("web_seed_url")]
    public string? WebSeedUrl { get; init; }

    [JsonPropertyName("piece_length_bytes")]
    public int? PieceLengthBytes { get; init; }

    [JsonPropertyName("torrent_created_at")]
    public DateTime? TorrentCreatedAt { get; init; }

    [JsonPropertyName("seeder_count")]
    public int? SeederCount { get; init; }

    [JsonPropertyName("leecher_count")]
    public int? LeecherCount { get; init; }
}

/// <summary>
/// Sent when a media upload is deleted
/// </summary>
public record MediaUploadDeletedMessage : IMediaUploadMessage
{
    [JsonPropertyName("upload_id")]
    public required string UploadId { get; init; }

    [JsonPropertyName("media_type")]
    public required MediaType MediaType { get; init; }

    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }

    [JsonPropertyName("source_site")]
    public string SourceSite { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    [JsonPropertyName("is_admin_action")]
    public bool IsAdminAction { get; init; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    public static MediaUploadDeletedMessage Create(
        string uploadId,
        MediaType mediaType,
        string userId,
        string sourceSite,
        string? reason = null,
        bool isAdminAction = false,
        Dictionary<string, object>? metadata = null)
    {
        return new MediaUploadDeletedMessage
        {
            UploadId = uploadId,
            MediaType = mediaType,
            UserId = userId,
            SourceSite = sourceSite,
            Reason = reason,
            IsAdminAction = isAdminAction,
            Metadata = metadata
        };
    }
}
