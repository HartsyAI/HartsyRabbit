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
