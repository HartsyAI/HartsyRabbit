namespace HartsyRabbit.Messages;

public abstract record TrainingMessageBase : ITrainingMessage
{
    public string JobId { get; init; } = string.Empty;
    public string? SessionId { get; init; }
    public string BackendId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; init; }
}

public sealed record TrainingStartedMessage : TrainingMessageBase
{
    public string? Status { get; init; }
}

public sealed record TrainingProgressMessage : TrainingMessageBase, ITrainingProgressMessage
{
    public int Progress { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? CurrentStep { get; init; }
    public int? CurrentStepNumber { get; init; }
    public int? TotalSteps { get; init; }
    public int? CurrentEpoch { get; init; }
    public int? TotalEpochs { get; init; }
    public int? EstimatedMinutesRemaining { get; init; }
    public double? Loss { get; init; }

    public static TrainingProgressMessage FromEvent(string backendId, string jobId, string? sessionId, int progress, string status, string? currentStep, int? currentStepNumber, int? totalSteps, int? currentEpoch, int? totalEpochs, int? estimatedMinutesRemaining, double? loss, object? metrics, Dictionary<string, object>? metadata)
    {
        return new TrainingProgressMessage
        {
            BackendId = backendId,
            JobId = jobId,
            SessionId = sessionId,
            EventType = "TrainingProgress",
            Progress = progress,
            Status = status,
            CurrentStep = currentStep,
            CurrentStepNumber = currentStepNumber,
            TotalSteps = totalSteps,
            CurrentEpoch = currentEpoch,
            TotalEpochs = totalEpochs,
            EstimatedMinutesRemaining = estimatedMinutesRemaining,
            Loss = loss,
            Metadata = metadata
        };
    }
}

public sealed record TrainingCompletedMessage : TrainingMessageBase, ITrainingCompletedMessage
{
    public string ModelName { get; init; } = string.Empty;
    public string NetworkVolumePath { get; init; } = string.Empty;
    public IReadOnlyList<string> OutputFiles { get; init; } = Array.Empty<string>();
    public int ImagesProcessed { get; init; }
}

public sealed record TrainingFailedMessage : TrainingMessageBase, ITrainingFailedMessage
{
    public string Error { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public sealed record TrainingTestImageMessage : TrainingMessageBase, ITrainingTestImageMessage
{
    public string ImageUrl { get; init; } = string.Empty;
    public int StepNumber { get; init; }
    public int? Epoch { get; init; }
    public string? Caption { get; init; }
}

public sealed record TrainingModelReadyMessage : TrainingMessageBase, ITrainingModelReadyMessage
{
    public IReadOnlyList<string> ModelFiles { get; init; } = Array.Empty<string>();
    public string ModelName { get; init; } = string.Empty;
    public string NetworkVolumePath { get; init; } = string.Empty;
    public long? TotalSizeBytes { get; init; }
}

/// <summary>Sent by Hartsy to Hawtsy after fetching model results from the training backend.
/// Tells Hawtsy to download the trained model file and process it through the upload pipeline
/// (storage, hashing, torrent creation, seeding) just like a regular model upload.</summary>
public sealed record TrainingModelUploadMessage : TrainingMessageBase
{
    /// <summary>Download URL for the trained model file (from FAL, RunPod, etc.)</summary>
    public string ModelDownloadUrl { get; init; } = string.Empty;

    /// <summary>Optional config file download URL</summary>
    public string? ConfigDownloadUrl { get; init; }

    /// <summary>Name of the trained model (typically the trigger word)</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>Original filename for storage (e.g., "my_lora.safetensors")</summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>Training type (flux-lora, flux-finetune, etc.)</summary>
    public string TrainingType { get; init; } = string.Empty;

    /// <summary>Trigger word used during training</summary>
    public string TriggerWord { get; init; } = string.Empty;

    /// <summary>User who initiated the training</summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>Internal DB ID of the training job on Hartsy</summary>
    public int InternalJobId { get; init; }

    /// <summary>Estimated file size in bytes (if known from backend)</summary>
    public long? FileSizeBytes { get; init; }
}
