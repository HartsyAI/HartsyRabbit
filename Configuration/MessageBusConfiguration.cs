using System.ComponentModel.DataAnnotations;

namespace HartsyRabbit.Configuration;

public class MessageBusConfiguration
{
    public ConnectionSettings Connection { get; set; } = new();
    public SiteSettings Site { get; set; } = new();
    public QueueSettings Queues { get; set; } = new();
    public TrainingQueueSettings TrainingQueues { get; set; } = new();
    public RetrySettings Retry { get; set; } = new();
    public MonitoringSettings Monitoring { get; set; } = new();

    public void Validate()
    {
        Connection.Validate();
        Site.Validate();
        Queues.Validate();
        TrainingQueues.Validate();
        Retry.Validate();
        Monitoring.Validate();
    }
}

public class ConnectionSettings
{
    [Required]
    public string HostName { get; set; } = "localhost";

    [Range(1, 65535)]
    public int Port { get; set; } = 5672;

    [Required]
    public string Username { get; set; } = "guest";

    [Required]
    public string Password { get; set; } = "guest";

    [Required]
    public string VirtualHost { get; set; } = "/";

    [Range(5, 300)]
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    public bool AutomaticRecoveryEnabled { get; set; } = true;

    [Range(0, 600)]
    public int RequestedHeartbeatSeconds { get; set; } = 60;

    public bool UseTLS { get; set; } = false;

    public string? TLSServerName { get; set; }

    public void Validate()
    {
        ValidationContext validationContext = new ValidationContext(this);
        List<ValidationResult> validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(this, validationContext, validationResults, true);

        if (!isValid)
        {
            string errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
            throw new InvalidOperationException($"Connection configuration validation failed: {errors}");
        }

        if (UseTLS && Port == 5672)
        {
            throw new InvalidOperationException("TLS is enabled but port is set to non-TLS port 5672. Use port 5671 for TLS.");
        }

        if (UseTLS && string.IsNullOrEmpty(TLSServerName))
        {
            throw new InvalidOperationException("TLS is enabled but TLSServerName is not specified");
        }
    }
}

public class SiteSettings
{
    [Required]
    public string SiteName { get; set; } = string.Empty;

    [Range(1, 100)]
    public int MaxConcurrentHandlers { get; set; } = 10;

    public bool ProcessBroadcastMessages { get; set; } = true;

    /// <summary>
    /// If true, skip queue/exchange setup and assume infrastructure already exists.
    /// Use this when connecting to an external RabbitMQ server managed by another service.
    /// </summary>
    public bool SkipQueueSetup { get; set; } = false;

    public void Validate()
    {
        string[] validSiteNames = { CrossSiteQueueTopology.HARTSY, CrossSiteQueueTopology.HAWTSY, CrossSiteQueueTopology.DISCORD_BOT };

        if (!validSiteNames.Contains(SiteName))
        {
            throw new InvalidOperationException($"Invalid site name '{SiteName}'. Must be one of: {string.Join(", ", validSiteNames)}");
        }
    }
}

public class QueueSettings
{
    public int DefaultMessageTTLMs { get; set; } = 24 * 60 * 60 * 1000;
    public int MaxQueueLength { get; set; } = 10000;
    public int MaxPriority { get; set; } = 10;
    public int DeadLetterTTLMs { get; set; } = 7 * 24 * 60 * 60 * 1000;
    public bool DurableQueues { get; set; } = true;

    public void Validate()
    {
        if (DefaultMessageTTLMs <= 0)
        {
            throw new InvalidOperationException("Default message TTL must be positive");
        }

        if (MaxQueueLength <= 0)
        {
            throw new InvalidOperationException("Max queue length must be positive");
        }

        if (MaxPriority < 0 || MaxPriority > 255)
        {
            throw new InvalidOperationException("Max priority must be between 0 and 255");
        }
    }
}

public class TrainingQueueSettings
{
    public int ProgressMessageTtlMs { get; set; } = 6 * 60 * 60 * 1000;
    public int MaxTrainingQueueLength { get; set; } = 5000;

    public void Validate()
    {
        if (ProgressMessageTtlMs <= 0)
        {
            throw new InvalidOperationException("Training progress message TTL must be positive");
        }

        if (MaxTrainingQueueLength <= 0)
        {
            throw new InvalidOperationException("Training queue max length must be positive");
        }
    }
}

public class RetrySettings
{
    [Range(0, 10)]
    public int MaxRetryAttempts { get; set; } = 3;

    [Range(100, 60000)]
    public int InitialRetryDelayMs { get; set; } = 1000;

    [Range(1.0, 10.0)]
    public double RetryMultiplier { get; set; } = 2.0;

    [Range(1000, 300000)]
    public int MaxRetryDelayMs { get; set; } = 30000;

    public void Validate()
    {
        ValidationContext validationContext = new ValidationContext(this);
        List<ValidationResult> validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(this, validationContext, validationResults, true);

        if (!isValid)
        {
            string errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
            throw new InvalidOperationException($"Retry configuration validation failed: {errors}");
        }
    }
}

public class MonitoringSettings
{
    public bool EnableMetrics { get; set; } = true;
    public bool EnableMessageLogging { get; set; } = false;
    public int HealthCheckIntervalSeconds { get; set; } = 30;
    public int StatisticsIntervalSeconds { get; set; } = 60;

    public void Validate()
    {
        if (HealthCheckIntervalSeconds <= 0)
        {
            throw new InvalidOperationException("Health check interval must be positive");
        }

        if (StatisticsIntervalSeconds <= 0)
        {
            throw new InvalidOperationException("Statistics interval must be positive");
        }
    }
}
