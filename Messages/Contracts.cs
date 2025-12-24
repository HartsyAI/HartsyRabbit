namespace HartsyRabbit.Messages;

public interface IModelMessage
{
    string ModelId { get; }
    string UserId { get; }
    string OperationType { get; }
    DateTime Timestamp { get; }
    Dictionary<string, object>? Metadata { get; }
}

public interface IModelUploadMessage : IModelMessage
{
    string UploadId { get; }
    string FileName { get; }
    long FileSizeBytes { get; }
}

public interface IModelProgressMessage : IModelMessage
{
    int ProgressPercent { get; }
    string CurrentStep { get; }
    bool IsComplete { get; }
}

public interface IModelCompletionMessage : IModelMessage
{
    bool IsSuccess { get; }
    string? ErrorMessage { get; }
    Dictionary<string, string>? ModelUrls { get; }
}

public interface ITrainingMessage
{
    string JobId { get; }
    string? SessionId { get; }
    string BackendId { get; }
    string EventType { get; }
    DateTime Timestamp { get; }
    Dictionary<string, object>? Metadata { get; }
}

public interface ITrainingProgressMessage : ITrainingMessage
{
    int Progress { get; }
    string Status { get; }
    string? CurrentStep { get; }
    int? CurrentStepNumber { get; }
    int? TotalSteps { get; }
    int? CurrentEpoch { get; }
    int? TotalEpochs { get; }
    int? EstimatedMinutesRemaining { get; }
    double? Loss { get; }
}

public interface ITrainingCompletedMessage : ITrainingMessage
{
    string ModelName { get; }
    string NetworkVolumePath { get; }
    IReadOnlyList<string> OutputFiles { get; }
    int ImagesProcessed { get; }
}

public interface ITrainingFailedMessage : ITrainingMessage
{
    string Error { get; }
    string Status { get; }
}

public interface ITrainingTestImageMessage : ITrainingMessage
{
    string ImageUrl { get; }
    int StepNumber { get; }
    int? Epoch { get; }
    string? Caption { get; }
}

public interface ITrainingModelReadyMessage : ITrainingMessage
{
    IReadOnlyList<string> ModelFiles { get; }
    string ModelName { get; }
    string NetworkVolumePath { get; }
    long? TotalSizeBytes { get; }
}

public interface IUserInteractionMessage
{
    string UserId { get; }
    string InteractionType { get; }
    DateTime Timestamp { get; }
    Dictionary<string, object>? Metadata { get; }
}

public interface ISystemMessage
{
    string SystemEventType { get; }
    DateTime Timestamp { get; }
    Dictionary<string, object>? Metadata { get; }
}

public interface IPaymentMessage
{
    string PaymentEventType { get; }
    string UserId { get; }
    DateTime Timestamp { get; }
    Dictionary<string, object>? Metadata { get; }
}

public interface IPayoutProcessedMessage : IPaymentMessage
{
    string PayoutId { get; }
    decimal Amount { get; }
    string Currency { get; }
    string Status { get; }
    string PayoutMethod { get; }
    string? TransactionReference { get; }
    DateTime ProcessedAt { get; }
}
