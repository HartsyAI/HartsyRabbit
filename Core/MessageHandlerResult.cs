namespace HartsyRabbit.Core;

public class MessageHandlerResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public bool ShouldRetry { get; set; }

    public static MessageHandlerResult Success()
    {
        return new MessageHandlerResult { IsSuccess = true, ShouldRetry = false };
    }

    public static MessageHandlerResult Failure(string errorMessage, Exception? exception = null, bool shouldRetry = true)
    {
        return new MessageHandlerResult { IsSuccess = false, ErrorMessage = errorMessage, Exception = exception, ShouldRetry = shouldRetry };
    }
}
