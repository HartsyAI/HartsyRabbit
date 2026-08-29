using HartsyRabbit.Configuration;
using HartsyRabbit.Core;
using HartsyRabbit.Infrastructure;
using HartsyRabbit.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HartsyRabbit.Hosting;

/// <summary>
/// Owns the complete message-bus lifecycle for a process. Startup and health failures are retried
/// until application shutdown so container ordering or a broker restart cannot leave a healthy-looking
/// process without consumers.
/// </summary>
public sealed class ResilientMessageBusHostedService(
    ITypeSafeMessageBus messageBus,
    IRabbitMQConnectionLifecycleManager connectionManager,
    IMessageBusLogger logger,
    IOptions<MessageBusConfiguration> options) : BackgroundService
{
    private readonly MessageBusConfiguration _configuration = options.Value;
    private readonly TimeSpan _initialRetryDelay = TimeSpan.FromMilliseconds(Math.Max(100, options.Value.Retry.InitialRetryDelayMs));
    private readonly TimeSpan _maxRetryDelay = TimeSpan.FromMilliseconds(Math.Max(options.Value.Retry.InitialRetryDelayMs, options.Value.Retry.MaxRetryDelayMs));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        TimeSpan retryDelay = _initialRetryDelay;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                string siteName = _configuration.Site.SiteName;
                logger.Info($"[RABBITMQ] Starting resilient message bus for site '{siteName}' at {_configuration.Connection.HostName}:{_configuration.Connection.Port}...");
                await messageBus.StartAsync(siteName, stoppingToken);
                logger.Info($"[RABBITMQ] Message bus is healthy and consuming for site '{siteName}'");
                retryDelay = _initialRetryDelay;

                await MonitorHealthAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.Error($"[RABBITMQ] Message bus unavailable; retrying in {retryDelay.TotalSeconds:F0}s", ex);
                await StopBusSafelyAsync();
                await Task.Delay(retryDelay, stoppingToken);
                retryDelay = NextRetryDelay(retryDelay);
            }
        }
    }

    private async Task MonitorHealthAsync(CancellationToken stoppingToken)
    {
        TimeSpan interval = TimeSpan.FromSeconds(Math.Max(5, _configuration.Monitoring.HealthCheckIntervalSeconds));
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(interval, stoppingToken);
            if (!await messageBus.IsHealthyAsync(stoppingToken))
            {
                throw new InvalidOperationException("RabbitMQ message bus health check failed.");
            }
        }
    }

    private TimeSpan NextRetryDelay(TimeSpan current)
    {
        double multiplier = Math.Max(1, _configuration.Retry.RetryMultiplier);
        return TimeSpan.FromMilliseconds(Math.Min(current.TotalMilliseconds * multiplier, _maxRetryDelay.TotalMilliseconds));
    }

    private async Task StopBusSafelyAsync()
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
        try
        {
            await messageBus.StopAsync(timeout.Token);
        }
        catch (Exception ex)
        {
            logger.Warning($"[RABBITMQ] Error while stopping message bus: {ex.Message}");
        }

        try
        {
            await connectionManager.StopAsync(timeout.Token);
        }
        catch (Exception ex)
        {
            logger.Warning($"[RABBITMQ] Error while resetting connection manager: {ex.Message}");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await StopBusSafelyAsync();
        await base.StopAsync(cancellationToken);
    }
}
