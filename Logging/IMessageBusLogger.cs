namespace HartsyRabbit.Logging;

public interface IMessageBusLogger
{
    void Verbose(string message);
    void Debug(string message);
    void Info(string message);
    void Warning(string message);
    void Error(string message);
    void Error(string message, Exception exception);
}
