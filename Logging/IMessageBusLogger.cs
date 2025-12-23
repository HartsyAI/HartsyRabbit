namespace HartsyRabbit.Logging;

public interface IMessageBusLogger
{
    void Debug(string message);
    void Info(string message);
    void Warning(string message);
    void Error(string message);
    void Error(string message, Exception exception);
}
