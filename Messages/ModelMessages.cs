using System.Text.Json.Serialization;

namespace HartsyRabbit.Messages;

public record ModelUploadStartedMessage : IModelUploadMessage
{
    public string ModelId { get; init; } = string.Empty;
    public string UploadId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public string OperationType { get; init; } = "Upload";
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; init; }

    public string ModelTitle { get; init; } = string.Empty;
    public string? ModelDescription { get; init; }
    public string? ModelArchitecture { get; init; }
    public List<string>? ModelTags { get; init; }
    public bool IsNsfw { get; init; }
    public bool IsPublic { get; init; } = true;
    public string? FileHash { get; init; }
    public int? EstimatedProcessingMinutes { get; init; }
    public string SourceSite { get; init; } = string.Empty;

    public static ModelUploadStartedMessage Create(string modelId, string uploadId, string userId, string fileName, long fileSizeBytes, string modelTitle, string sourceSite, string? modelDescription = null, string? modelArchitecture = null, List<string>? modelTags = null, bool isNsfw = false, bool isPublic = true, string? fileHash = null, Dictionary<string, object>? metadata = null)
    {
        return new ModelUploadStartedMessage
        {
            ModelId = modelId,
            UploadId = uploadId,
            UserId = userId,
            FileName = fileName,
            FileSizeBytes = fileSizeBytes,
            ModelTitle = modelTitle,
            ModelDescription = modelDescription,
            ModelArchitecture = modelArchitecture,
            ModelTags = modelTags,
            IsNsfw = isNsfw,
            IsPublic = isPublic,
            FileHash = fileHash,
            SourceSite = sourceSite,
            Metadata = metadata
        };
    }
}

public record ModelUploadProgressMessage : IModelProgressMessage
{
    public string ModelId { get; init; } = string.Empty;
    public string UploadId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string OperationType { get; init; } = "Progress";
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; init; }

    public int ProgressPercent { get; init; }
    public string CurrentStep { get; init; } = string.Empty;
    public bool IsComplete { get; init; }

    public string Status { get; init; } = "processing";
    public int? EstimatedMinutesRemaining { get; init; }
    public double? ProcessingSpeed { get; init; }
    public string? ProcessingSpeedUnit { get; init; }
    public long? ProcessedAmount { get; init; }
    public long? TotalAmount { get; init; }
    public List<string>? Warnings { get; init; }
    public string SourceSite { get; init; } = string.Empty;

    public static ModelUploadProgressMessage Create(string modelId, string uploadId, string userId, int progressPercent, string currentStep, string status = "processing", string sourceSite = "", bool isComplete = false)
    {
        return new ModelUploadProgressMessage
        {
            ModelId = modelId,
            UploadId = uploadId,
            UserId = userId,
            ProgressPercent = progressPercent,
            CurrentStep = currentStep,
            Status = status,
            SourceSite = sourceSite,
            IsComplete = isComplete
        };
    }

    public static ModelUploadProgressMessage CreateProgress(string uploadId, string userId, int progressPercent, string currentStep, string status, string sourceSite = "", double? estimatedMinutesRemaining = null, Dictionary<string, object>? metadata = null)
    {
        return new ModelUploadProgressMessage
        {
            UploadId = uploadId,
            UserId = userId,
            ProgressPercent = progressPercent,
            CurrentStep = currentStep,
            Status = status,
            SourceSite = sourceSite,
            EstimatedMinutesRemaining = estimatedMinutesRemaining.HasValue ? (int?)estimatedMinutesRemaining.Value : null,
            Metadata = metadata
        };
    }

    public static ModelUploadProgressMessage CreateCompletion(string modelId, string uploadId, string userId, string finalStep, string sourceSite, List<string>? warnings = null)
    {
        return new ModelUploadProgressMessage
        {
            ModelId = modelId,
            UploadId = uploadId,
            UserId = userId,
            ProgressPercent = 100,
            CurrentStep = finalStep,
            Status = "completed",
            SourceSite = sourceSite,
            IsComplete = true,
            Warnings = warnings
        };
    }
}

public record ModelUploadCompletedMessage : IModelCompletionMessage
{
    public string ModelId { get; init; } = string.Empty;
    public string UploadId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string OperationType { get; init; } = "Complete";
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; init; }

    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, string>? ModelUrls { get; init; }

    public Dictionary<string, object>? FinalMetadata { get; init; }
    public string? DirectDownloadUrl { get; init; }
    public string? MagnetLink { get; init; }
    public string? TorrentFileUrl { get; init; }
    public List<string>? WebSeedUrls { get; init; }
    public string? PreviewImageUrl { get; init; }
    public long? FinalFileSizeBytes { get; init; }
    public string? FinalFileHash { get; init; }
    public double? ProcessingTimeSeconds { get; init; }
    public string? TorrentInfoHash { get; init; }

    [JsonPropertyName("torrentInfo")]
    public TorrentInfoPayload? TorrentInfo { get; init; }

    public int? SeederCount { get; init; }
    public bool? IsPublic { get; init; }
    public bool? IsNsfw { get; init; }
    public string? DetectedArchitecture { get; init; }
    public List<string>? ProcessingTags { get; init; }
    public string SourceSite { get; init; } = string.Empty;

    public static ModelUploadCompletedMessage CreateSuccess(string modelId, string uploadId, string userId, string sourceSite, Dictionary<string, string> modelUrls, string? directDownloadUrl = null, string? magnetLink = null, string? torrentFileUrl = null, long? finalFileSizeBytes = null, string? finalFileHash = null, double? processingTimeSeconds = null, Dictionary<string, object>? finalMetadata = null)
    {
        return new ModelUploadCompletedMessage
        {
            ModelId = modelId,
            UploadId = uploadId,
            UserId = userId,
            SourceSite = sourceSite,
            IsSuccess = true,
            ModelUrls = modelUrls,
            DirectDownloadUrl = directDownloadUrl,
            MagnetLink = magnetLink,
            TorrentFileUrl = torrentFileUrl,
            FinalFileSizeBytes = finalFileSizeBytes,
            FinalFileHash = finalFileHash,
            ProcessingTimeSeconds = processingTimeSeconds,
            FinalMetadata = finalMetadata
        };
    }

    public static ModelUploadCompletedMessage CreateFailure(string modelId, string uploadId, string userId, string sourceSite, string errorMessage, double? processingTimeSeconds = null, Dictionary<string, object>? metadata = null)
    {
        return new ModelUploadCompletedMessage
        {
            ModelId = modelId,
            UploadId = uploadId,
            UserId = userId,
            SourceSite = sourceSite,
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ProcessingTimeSeconds = processingTimeSeconds,
            Metadata = metadata
        };
    }

    public record TorrentInfoPayload
    {
        [JsonPropertyName("infohash")] public string? InfoHash { get; init; }
        [JsonPropertyName("magnet_link")] public string? MagnetLink { get; init; }
        [JsonPropertyName("torrent_url")] public string? TorrentUrl { get; init; }
        [JsonPropertyName("web_seed_url")] public string? WebSeedUrl { get; init; }
        [JsonPropertyName("piece_length_bytes")] public int? PieceLengthBytes { get; init; }
        [JsonPropertyName("torrent_created_at")] public DateTime? TorrentCreatedAt { get; init; }
        [JsonPropertyName("is_single_file")] public bool? IsSingleFile { get; init; }
        [JsonPropertyName("files")] public List<TorrentFilePayload>? Files { get; init; }
    }

    public record TorrentFilePayload
    {
        [JsonPropertyName("path")] public string? Path { get; init; }
        [JsonPropertyName("size_bytes")] public long? SizeBytes { get; init; }
    }
}

public record ModelDeletedMessage : IModelMessage
{
    public string ModelId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string? FileName { get; init; }
    public string? HawtsyUploadId { get; init; }
    public string? DirectUrl { get; init; }
    public string? TorrentUrl { get; init; }
    public string? MagnetLink { get; init; }
    public string OperationType { get; init; } = "Delete";
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? Reason { get; init; }
    public bool IsAdminAction { get; init; }
    public string SourceSite { get; init; } = "Hartsy";
    public Dictionary<string, object>? Metadata { get; init; }

    public static ModelDeletedMessage Create(string modelId, string userId, string? fileName = null, string? hawtsyUploadId = null, string? directUrl = null, string? torrentUrl = null, string? magnetLink = null, string? reason = null, bool isAdminAction = false, string sourceSite = "Hartsy", Dictionary<string, object>? metadata = null)
    {
        return new ModelDeletedMessage
        {
            ModelId = modelId,
            UserId = userId,
            FileName = fileName,
            HawtsyUploadId = hawtsyUploadId,
            DirectUrl = directUrl,
            TorrentUrl = torrentUrl,
            MagnetLink = magnetLink,
            Reason = reason,
            IsAdminAction = isAdminAction,
            SourceSite = sourceSite,
            Metadata = metadata
        };
    }
}
