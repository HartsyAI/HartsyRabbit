using HartsyRabbit.Configuration;
using HartsyRabbit.Core;
using HartsyRabbit.Infrastructure;
using HartsyRabbit.Publishers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace HartsyRabbit.Extensions;

public static class ServiceCollectionExtensions
{
    internal static class MessageBusEnvHelpers
    {
        public static MessageBusConfiguration BuildMessageBusConfigurationFromEnvironment()
        {
            ConnectionSettings conn = new()
            {
                HostName = GetEnv("RABBITMQ_HOSTNAME", "localhost"),
                Port = GetEnvInt("RABBITMQ_PORT", 5672),
                Username = GetEnv("RABBITMQ_USERNAME", "guest"),
                Password = GetEnv("RABBITMQ_PASSWORD", "guest"),
                VirtualHost = GetEnv("RABBITMQ_VHOST", "/"),
                ConnectionTimeoutSeconds = GetEnvInt("RABBITMQ_CONN_TIMEOUT_SECONDS", 30),
                AutomaticRecoveryEnabled = GetEnvBool("RABBITMQ_AUTO_RECOVERY", true),
                RequestedHeartbeatSeconds = GetEnvInt("RABBITMQ_HEARTBEAT_SECONDS", 60),
                UseTLS = GetEnvBool("RABBITMQ_USE_TLS", false),
                TLSServerName = Environment.GetEnvironmentVariable("RABBITMQ_TLS_SERVER_NAME")
            };

            string siteFromEnv = GetEnv("RABBITMQ_ENV_PREFIX", CrossSiteQueueTopology.HARTSY);
            string siteName = siteFromEnv.ToLowerInvariant() switch
            {
                "hartsy" => CrossSiteQueueTopology.HARTSY,
                "hawtsy" => CrossSiteQueueTopology.HAWTSY,
                "discordbot" => CrossSiteQueueTopology.DISCORD_BOT,
                "discord" => CrossSiteQueueTopology.DISCORD_BOT,
                _ => CrossSiteQueueTopology.HARTSY
            };

            SiteSettings site = new()
            {
                SiteName = siteName,
                MaxConcurrentHandlers = GetEnvInt("RABBITMQ_MAX_CONCURRENT_HANDLERS", 10),
                ProcessBroadcastMessages = GetEnvBool("RABBITMQ_PROCESS_BROADCAST", true)
            };

            QueueSettings queues = new()
            {
                DefaultMessageTTLMs = GetEnvInt("RABBITMQ_DEFAULT_TTL_MS", 24 * 60 * 60 * 1000),
                MaxQueueLength = GetEnvInt("RABBITMQ_MAX_QUEUE_LENGTH", 10000),
                MaxPriority = GetEnvInt("RABBITMQ_MAX_PRIORITY", 10),
                DeadLetterTTLMs = GetEnvInt("RABBITMQ_DLQ_TTL_MS", 7 * 24 * 60 * 60 * 1000),
                DurableQueues = GetEnvBool("RABBITMQ_DURABLE_QUEUES", true)
            };

            RetrySettings retry = new()
            {
                MaxRetryAttempts = GetEnvInt("RABBITMQ_MAX_RETRY_ATTEMPTS", 3),
                InitialRetryDelayMs = GetEnvInt("RABBITMQ_RETRY_DELAY_MS", 1000),
                RetryMultiplier = GetEnvDouble("RABBITMQ_RETRY_MULTIPLIER", 2.0),
                MaxRetryDelayMs = GetEnvInt("RABBITMQ_MAX_RETRY_DELAY_MS", 30000)
            };

            MonitoringSettings mon = new()
            {
                EnableMetrics = GetEnvBool("RABBITMQ_ENABLE_METRICS", true),
                EnableMessageLogging = GetEnvBool("RABBITMQ_ENABLE_MESSAGE_LOGGING", false),
                HealthCheckIntervalSeconds = GetEnvInt("RABBITMQ_HEALTHCHECK_INTERVAL_SECONDS", 30),
                StatisticsIntervalSeconds = GetEnvInt("RABBITMQ_STATS_INTERVAL_SECONDS", 60)
            };

            return new MessageBusConfiguration
            {
                Connection = conn,
                Site = site,
                Queues = queues,
                Retry = retry,
                Monitoring = mon
            };
        }

        private static string GetEnv(string key, string fallback)
        {
            string? v = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrWhiteSpace(v) ? fallback : v;
        }

        private static int GetEnvInt(string key, int fallback)
        {
            string? v = Environment.GetEnvironmentVariable(key);
            return int.TryParse(v, out int i) ? i : fallback;
        }

        private static double GetEnvDouble(string key, double fallback)
        {
            string? v = Environment.GetEnvironmentVariable(key);
            return double.TryParse(v, out double d) ? d : fallback;
        }

        private static bool GetEnvBool(string key, bool fallback)
        {
            string? v = Environment.GetEnvironmentVariable(key);
            return bool.TryParse(v, out bool b) ? b : fallback;
        }
    }

    public static IServiceCollection AddTypeSafeMessageBus(this IServiceCollection services, IConfiguration configuration, string configurationSection = "MessageBus")
    {
        MessageBusConfiguration? config = configuration.GetSection(configurationSection).Get<MessageBusConfiguration>();

        if (config != null)
        {
            services.Configure<MessageBusConfiguration>(configuration.GetSection(configurationSection));
            config.Validate();
        }
        else
        {
            config = MessageBusEnvHelpers.BuildMessageBusConfigurationFromEnvironment();
            config.Validate();
            services.AddSingleton<IOptions<MessageBusConfiguration>>(Options.Create(config));
        }

        services.AddSingleton<ITypeSafeMessageBus, TypeSafeMessageBus>();
        services.AddSingleton<IRabbitMQConnectionLifecycleManager, RabbitMQConnectionLifecycleManager>();
        services.AddSingleton<IRabbitMQQueueSetupService, RabbitMQQueueSetupService>();
        services.AddSingleton<ITypeSafeMessagePublisher, TypeSafeMessagePublisher>();
        services.AddSingleton<MessageHandlerRegistrationService>();

        return services;
    }

    public static IServiceCollection AddMessageHandler<TMessage, THandler>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped, int priority = 0)
        where TMessage : class
        where THandler : class, ITypeSafeMessageHandler<TMessage>
    {
        services.Add(new ServiceDescriptor(typeof(THandler), typeof(THandler), lifetime));
        services.Add(new ServiceDescriptor(typeof(ITypeSafeMessageHandler<TMessage>), sp => sp.GetRequiredService<THandler>(), lifetime));

        services.AddSingleton(_ => new MessageHandlerRegistration
        {
            MessageType = typeof(TMessage),
            HandlerType = typeof(THandler),
            ServiceLifetime = lifetime,
            Priority = priority
        });

        return services;
    }

    public static IServiceCollection AddMessageHandlersFromAssembly(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        foreach (Assembly assembly in assemblies)
        {
            Type[] handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypeSafeMessageHandler<>)))
                .ToArray();

            foreach (Type handlerType in handlerTypes)
            {
                Type? handlerInterface = handlerType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypeSafeMessageHandler<>));

                if (handlerInterface == null)
                {
                    continue;
                }

                Type messageType = handlerInterface.GetGenericArguments()[0];

                services.AddScoped(handlerType);
                services.AddScoped(handlerInterface, handlerType);

                services.AddSingleton(_ => new MessageHandlerRegistration
                {
                    MessageType = messageType,
                    HandlerType = handlerType,
                    ServiceLifetime = ServiceLifetime.Scoped,
                    Priority = 0
                });
            }
        }

        return services;
    }

    public static IServiceCollection AddMessageBusHealthChecks(this IServiceCollection services, string name = "rabbitmq-messagebus", params string[] tags)
    {
        services.AddHealthChecks().AddCheck<MessageBusHealthCheck>(name, tags: tags);
        return services;
    }
}

public sealed class MessageBusHealthCheck : IHealthCheck
{
    private readonly ITypeSafeMessageBus _bus;

    public MessageBusHealthCheck(ITypeSafeMessageBus bus)
    {
        _bus = bus;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            bool ok = await _bus.IsHealthyAsync(cancellationToken);
            if (!ok)
            {
                return HealthCheckResult.Unhealthy("RabbitMQ message bus is not healthy");
            }

            MessageBusStatistics stats = _bus.GetStatistics();
            Dictionary<string, object> data = new()
            {
                { "MessagesPublished", stats.MessagesPublished },
                { "MessagesProcessed", stats.MessagesProcessed },
                { "ProcessingErrors", stats.ProcessingErrors },
                { "RegisteredHandlers", stats.RegisteredHandlers },
                { "IsConnectionHealthy", stats.IsConnectionHealthy }
            };

            return HealthCheckResult.Healthy("RabbitMQ message bus is healthy", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ message bus health check failed", ex);
        }
    }
}
