namespace HartsyRabbit.Configuration;

public static class CrossSiteQueueTopology
{
    public const string HARTSY = "Hartsy";
    public const string HAWTSY = "Hawtsy";
    public const string DISCORD_BOT = "DiscordBot";

    public static readonly string[] ALL_SITES = { HARTSY, HAWTSY, DISCORD_BOT };

    public const string DOMAIN_EVENTS_EXCHANGE = "domain.events";
    public const string TRAINING_EVENTS_EXCHANGE = "training.events";
    public const string SITE_ROUTING_EXCHANGE = "site.routing";
    public const string BROADCAST_EXCHANGE = "system.broadcast";

    public const string MODEL_EVENTS_QUEUE = "model.events";
    public const string USER_INTERACTION_EVENTS_QUEUE = "user.interaction.events";
    public const string SYSTEM_EVENTS_QUEUE = "system.events";
    public const string TRAINING_EVENTS_QUEUE = "training.events";

    public const string HARTSY_INBOX_QUEUE = "hartsy.inbox";
    public const string HAWTSY_INBOX_QUEUE = "hawtsy.inbox";
    public const string DISCORD_BOT_INBOX_QUEUE = "discord.inbox";

    public const string BROADCAST_QUEUE = "system.broadcast";
    public const string DEAD_LETTER_QUEUE = "hartsy.deadletter.queue";
    public const string MONITORING_QUEUE = "monitoring";

    public const string MODEL_UPLOAD_ROUTING_KEY = "model.upload";
    public const string MODEL_PROGRESS_ROUTING_KEY = "model.progress";
    public const string MODEL_COMPLETE_ROUTING_KEY = "model.complete";
    public const string USER_INTERACTION_ROUTING_KEY = "user.interaction";
    public const string SYSTEM_HEALTH_ROUTING_KEY = "system.health";

    public const string TRAINING_STARTED_ROUTING_KEY = "training.started";
    public const string TRAINING_PROGRESS_ROUTING_KEY = "training.progress";
    public const string TRAINING_COMPLETED_ROUTING_KEY = "training.completed";
    public const string TRAINING_FAILED_ROUTING_KEY = "training.failed";
    public const string TRAINING_TEST_IMAGE_ROUTING_KEY = "training.testimage";
    public const string TRAINING_MODEL_READY_ROUTING_KEY = "training.modelready";

    public const string HARTSY_ROUTING_KEY = "hartsy";
    public const string HAWTSY_ROUTING_KEY = "hawtsy";
    public const string DISCORD_BOT_ROUTING_KEY = "discord";

    public static string GetInboxQueueForSite(string siteName)
    {
        return siteName switch
        {
            HARTSY => HARTSY_INBOX_QUEUE,
            HAWTSY => HAWTSY_INBOX_QUEUE,
            DISCORD_BOT => DISCORD_BOT_INBOX_QUEUE,
            _ => throw new ArgumentException($"Unknown site name '{siteName}'", nameof(siteName))
        };
    }

    public static string GetRoutingKeyForSite(string siteName)
    {
        return siteName switch
        {
            HARTSY => HARTSY_ROUTING_KEY,
            HAWTSY => HAWTSY_ROUTING_KEY,
            DISCORD_BOT => DISCORD_BOT_ROUTING_KEY,
            _ => throw new ArgumentException($"Unknown site name '{siteName}'", nameof(siteName))
        };
    }

    public static string GetRoutingKeyForMessageType(string messageType)
    {
        if (string.IsNullOrWhiteSpace(messageType))
        {
            return SYSTEM_HEALTH_ROUTING_KEY;
        }

        string lower = messageType.ToLowerInvariant();

        if (lower.Contains("training"))
        {
            return TRAINING_PROGRESS_ROUTING_KEY;
        }

        if (lower.Contains("modeluploadstarted"))
        {
            return MODEL_UPLOAD_ROUTING_KEY;
        }

        if (lower.Contains("modeluploadprogress"))
        {
            return MODEL_PROGRESS_ROUTING_KEY;
        }

        if (lower.Contains("modeluploadcompleted") || lower.Contains("modeluploadcompletion"))
        {
            return MODEL_COMPLETE_ROUTING_KEY;
        }

        if (lower.Contains("user") && (lower.Contains("liked") || lower.Contains("favorited") || lower.Contains("download")))
        {
            return USER_INTERACTION_ROUTING_KEY;
        }

        return SYSTEM_HEALTH_ROUTING_KEY;
    }

    public static Dictionary<string, object?> GetStandardQueueArguments(MessageBusConfiguration config)
    {
        return new Dictionary<string, object?>
        {
            { "x-message-ttl", config.Queues.DefaultMessageTTLMs },
            { "x-dead-letter-exchange", "" },
            { "x-dead-letter-routing-key", DEAD_LETTER_QUEUE },
            { "x-max-length", config.Queues.MaxQueueLength },
            { "x-overflow", "reject-publish" }
        };
    }

    public static Dictionary<string, object?> GetPriorityQueueArguments(MessageBusConfiguration config)
    {
        Dictionary<string, object?> args = GetStandardQueueArguments(config);
        args["x-max-priority"] = config.Queues.MaxPriority;
        args["x-message-ttl"] = config.Queues.DefaultMessageTTLMs / 4;
        return args;
    }

    public static Dictionary<string, object?> GetTrainingQueueArguments(MessageBusConfiguration config)
    {
        Dictionary<string, object?> args = GetStandardQueueArguments(config);
        args["x-message-ttl"] = config.TrainingQueues.ProgressMessageTtlMs;
        args["x-max-length"] = config.TrainingQueues.MaxTrainingQueueLength;
        return args;
    }

    public static Dictionary<string, object?> GetDeadLetterQueueArguments(MessageBusConfiguration config)
    {
        return new Dictionary<string, object?>
        {
            { "x-message-ttl", config.Queues.DeadLetterTTLMs },
            { "x-max-length", 1000 }
        };
    }

    public static Dictionary<string, object?> GetBroadcastQueueArguments(MessageBusConfiguration config)
    {
        return new Dictionary<string, object?>
        {
            { "x-message-ttl", 10 * 60 * 1000 },
            { "x-max-length", 5000 }
        };
    }

    public static List<ExchangeDefinition> GetAllExchangeDefinitions()
    {
        return new List<ExchangeDefinition>
        {
            new ExchangeDefinition { Name = DOMAIN_EVENTS_EXCHANGE, Type = "topic", Durable = true, AutoDelete = false },
            new ExchangeDefinition { Name = TRAINING_EVENTS_EXCHANGE, Type = "topic", Durable = true, AutoDelete = false },
            new ExchangeDefinition { Name = SITE_ROUTING_EXCHANGE, Type = "direct", Durable = true, AutoDelete = false },
            new ExchangeDefinition { Name = BROADCAST_EXCHANGE, Type = "fanout", Durable = true, AutoDelete = false }
        };
    }

    public static List<QueueDefinition> GetAllQueueDefinitions(MessageBusConfiguration config)
    {
        List<QueueDefinition> queues = new List<QueueDefinition>
        {
            new QueueDefinition { Name = MODEL_EVENTS_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetStandardQueueArguments(config) },
            new QueueDefinition { Name = USER_INTERACTION_EVENTS_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetStandardQueueArguments(config) },
            new QueueDefinition { Name = SYSTEM_EVENTS_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetPriorityQueueArguments(config) },
            new QueueDefinition { Name = TRAINING_EVENTS_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetTrainingQueueArguments(config) },
            new QueueDefinition { Name = HARTSY_INBOX_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetStandardQueueArguments(config) },
            new QueueDefinition { Name = HAWTSY_INBOX_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetStandardQueueArguments(config) },
            new QueueDefinition { Name = DISCORD_BOT_INBOX_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetStandardQueueArguments(config) },
            new QueueDefinition { Name = BROADCAST_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetBroadcastQueueArguments(config) },
            new QueueDefinition { Name = DEAD_LETTER_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetDeadLetterQueueArguments(config) },
            new QueueDefinition { Name = MONITORING_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetStandardQueueArguments(config) }
        };

        return queues;
    }

    public static List<QueueBinding> GetAllQueueBindings()
    {
        return new List<QueueBinding>
        {
            new QueueBinding(DOMAIN_EVENTS_EXCHANGE, MODEL_EVENTS_QUEUE, MODEL_UPLOAD_ROUTING_KEY),
            new QueueBinding(DOMAIN_EVENTS_EXCHANGE, MODEL_EVENTS_QUEUE, MODEL_PROGRESS_ROUTING_KEY),
            new QueueBinding(DOMAIN_EVENTS_EXCHANGE, MODEL_EVENTS_QUEUE, MODEL_COMPLETE_ROUTING_KEY),
            new QueueBinding(DOMAIN_EVENTS_EXCHANGE, USER_INTERACTION_EVENTS_QUEUE, USER_INTERACTION_ROUTING_KEY),
            new QueueBinding(DOMAIN_EVENTS_EXCHANGE, SYSTEM_EVENTS_QUEUE, SYSTEM_HEALTH_ROUTING_KEY),
            new QueueBinding(TRAINING_EVENTS_EXCHANGE, TRAINING_EVENTS_QUEUE, TRAINING_STARTED_ROUTING_KEY),
            new QueueBinding(TRAINING_EVENTS_EXCHANGE, TRAINING_EVENTS_QUEUE, TRAINING_PROGRESS_ROUTING_KEY),
            new QueueBinding(TRAINING_EVENTS_EXCHANGE, TRAINING_EVENTS_QUEUE, TRAINING_COMPLETED_ROUTING_KEY),
            new QueueBinding(TRAINING_EVENTS_EXCHANGE, TRAINING_EVENTS_QUEUE, TRAINING_FAILED_ROUTING_KEY),
            new QueueBinding(TRAINING_EVENTS_EXCHANGE, TRAINING_EVENTS_QUEUE, TRAINING_TEST_IMAGE_ROUTING_KEY),
            new QueueBinding(TRAINING_EVENTS_EXCHANGE, TRAINING_EVENTS_QUEUE, TRAINING_MODEL_READY_ROUTING_KEY),
            new QueueBinding(SITE_ROUTING_EXCHANGE, HARTSY_INBOX_QUEUE, HARTSY_ROUTING_KEY),
            new QueueBinding(SITE_ROUTING_EXCHANGE, HAWTSY_INBOX_QUEUE, HAWTSY_ROUTING_KEY),
            new QueueBinding(SITE_ROUTING_EXCHANGE, DISCORD_BOT_INBOX_QUEUE, DISCORD_BOT_ROUTING_KEY)
        };
    }
}

public class ExchangeDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
    public Dictionary<string, object?>? Arguments { get; set; }
}

public class QueueDefinition
{
    public string Name { get; set; } = string.Empty;
    public bool Durable { get; set; } = true;
    public bool Exclusive { get; set; } = false;
    public bool AutoDelete { get; set; } = false;
    public Dictionary<string, object?>? Arguments { get; set; }
}

public record QueueBinding(string ExchangeName, string QueueName, string RoutingKey);
