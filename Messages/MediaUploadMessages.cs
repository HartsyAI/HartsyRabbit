using System.Text.Json.Serialization;

namespace HartsyRabbit.Messages;

/// <summary>
/// Media types supported by the upload system
/// </summary>
public enum MediaType
{
    Model = 0,
    Image = 1,
    Video = 2,
    Dataset = 3,
    Audio = 4,
    Model3D = 5,
    Document = 6,
    Other = 7,
    TrainingArtifact = 8
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

/// <summary>
/// Sent by the storage service to request that a stored blob be turned into a torrent and seeded.
/// The seeder fetches the object from blob storage using <see cref="ObjectKey"/>, runs mktorrent
/// (embedding <see cref="WebSeedUrl"/> as the HTTP web seed), starts seeding, and replies with a
/// <see cref="TorrentReadyMessage"/>.
/// </summary>
public record TorrentRequestedMessage : IMediaUploadMessage
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

    /// <summary>Blob-store object key the seeder fetches to obtain the file bytes.</summary>
    [JsonPropertyName("object_key")]
    public required string ObjectKey { get; init; }

    [JsonPropertyName("file_name")]
    public required string FileName { get; init; }

    /// <summary>Canonical HTTP download URL embedded in the torrent as the web seed (-w).</summary>
    [JsonPropertyName("web_seed_url")]
    public required string WebSeedUrl { get; init; }

    [JsonPropertyName("file_size_bytes")]
    public long FileSizeBytes { get; init; }

    public static TorrentRequestedMessage Create(
        string uploadId,
        MediaType mediaType,
        string userId,
        string sourceSite,
        string objectKey,
        string fileName,
        string webSeedUrl,
        long fileSizeBytes = 0)
    {
        return new TorrentRequestedMessage
        {
            UploadId = uploadId,
            MediaType = mediaType,
            UserId = userId,
            SourceSite = sourceSite,
            ObjectKey = objectKey,
            FileName = fileName,
            WebSeedUrl = webSeedUrl,
            FileSizeBytes = fileSizeBytes
        };
    }
}

/// <summary>
/// Sent by the seeder after a torrent has been created and seeding has started (or failed).
/// Best-effort: a failure here never invalidates the upload. Consumers patch the existing media
/// row's torrent info from <see cref="TorrentInfo"/> when <see cref="Success"/> is true.
/// </summary>
public record TorrentReadyMessage : IMediaUploadMessage
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

    [JsonPropertyName("torrent_info")]
    public TorrentInfoPayload? TorrentInfo { get; init; }

    public static TorrentReadyMessage CreateSuccess(
        string uploadId,
        MediaType mediaType,
        string userId,
        string sourceSite,
        TorrentInfoPayload torrentInfo)
    {
        return new TorrentReadyMessage
        {
            UploadId = uploadId,
            MediaType = mediaType,
            UserId = userId,
            SourceSite = sourceSite,
            Success = true,
            TorrentInfo = torrentInfo
        };
    }

    public static TorrentReadyMessage CreateFailure(
        string uploadId,
        MediaType mediaType,
        string userId,
        string sourceSite,
        string errorMessage)
    {
        return new TorrentReadyMessage
        {
            UploadId = uploadId,
            MediaType = mediaType,
            UserId = userId,
            SourceSite = sourceSite,
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Sent to HartsySeeder when a torrented upload is deleted, so it stops seeding and drops the local
/// copy. HartsyWeb knows the infohash (from the model row) and supplies it.
/// </summary>
public record TorrentRemoveRequestedMessage : IMediaUploadMessage
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

    /// <summary>Torrent infohash to remove from the seeder. May be empty — the seeder also keys cleanup off UploadId.</summary>
    [JsonPropertyName("info_hash")]
    public string? InfoHash { get; init; }

    [JsonPropertyName("delete_files")]
    public bool DeleteFiles { get; init; } = true;

    public static TorrentRemoveRequestedMessage Create(
        string uploadId, MediaType mediaType, string userId, string sourceSite, string? infoHash, bool deleteFiles = true)
        => new()
        {
            UploadId = uploadId,
            MediaType = mediaType,
            UserId = userId,
            SourceSite = sourceSite,
            InfoHash = infoHash,
            DeleteFiles = deleteFiles
        };
}
