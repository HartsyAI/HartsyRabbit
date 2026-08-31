using HartsyRabbit.Messages;

namespace HartsyRabbit.Configuration;

public static class CrossSiteQueueTopology
{
    public const string HARTSY = "Hartsy";
    public const string HAWTSY = "Hawtsy";
    public const string DISCORD_BOT = "DiscordBot";
    public const string HARTSY_STORAGE = "HartsyStorage";
    public const string HARTSY_SEEDER = "HartsySeeder";

    public static readonly string[] ALL_SITES = { HARTSY, HAWTSY, DISCORD_BOT, HARTSY_STORAGE, HARTSY_SEEDER };

    public const string DOMAIN_EVENTS_EXCHANGE = "domain.events";
    public const string TRAINING_EVENTS_EXCHANGE = "training.events";
    public const string SITE_ROUTING_EXCHANGE = "site.routing";
    public const string BROADCAST_EXCHANGE = "system.broadcast";

    public const string MODEL_EVENTS_QUEUE = "model.events";
    public const string MEDIA_EVENTS_QUEUE = "media.events";
    public const string USER_INTERACTION_EVENTS_QUEUE = "user.interaction.events";
    public const string SYSTEM_EVENTS_QUEUE = "system.events";

    public const string HARTSY_INBOX_QUEUE = "hartsy.inbox";
    public const string HAWTSY_INBOX_QUEUE = "hawtsy.inbox";
    public const string DISCORD_BOT_INBOX_QUEUE = "discord.inbox";
    public const string HARTSY_STORAGE_INBOX_QUEUE = "hartsystorage.inbox";
    public const string HARTSY_SEEDER_INBOX_QUEUE = "hartsyseeder.inbox";

    public const string HARTSY_BROADCAST_QUEUE = "hartsy.broadcast";
    public const string HAWTSY_BROADCAST_QUEUE = "hawtsy.broadcast";
    public const string DISCORD_BOT_BROADCAST_QUEUE = "discord.broadcast";
    public const string HARTSY_STORAGE_BROADCAST_QUEUE = "hartsystorage.broadcast";
    public const string HARTSY_SEEDER_BROADCAST_QUEUE = "hartsyseeder.broadcast";

    public const string DEAD_LETTER_QUEUE = "hartsy.deadletter.queue";
    public const string MONITORING_QUEUE = "monitoring";

    public const string MODEL_UPLOAD_ROUTING_KEY = "model.upload";
    public const string MODEL_PROGRESS_ROUTING_KEY = "model.progress";
    public const string MODEL_COMPLETE_ROUTING_KEY = "model.complete";
    public const string MEDIA_UPLOAD_ROUTING_KEY = "media.upload";
    public const string MEDIA_PROGRESS_ROUTING_KEY = "media.progress";
    public const string MEDIA_COMPLETE_ROUTING_KEY = "media.complete";
    public const string MEDIA_DELETED_ROUTING_KEY = "media.deleted";
    public const string TORRENT_REQUESTED_ROUTING_KEY = "torrent.requested";
    public const string TORRENT_READY_ROUTING_KEY = "torrent.ready";
    public const string USER_INTERACTION_ROUTING_KEY = "user.interaction";
    public const string SYSTEM_HEALTH_ROUTING_KEY = "system.health";

    public const string TRAINING_STARTED_ROUTING_KEY = "training.started";
    public const string TRAINING_PROGRESS_ROUTING_KEY = "training.progress";
    public const string TRAINING_COMPLETED_ROUTING_KEY = "training.completed";
    public const string TRAINING_FAILED_ROUTING_KEY = "training.failed";
    public const string TRAINING_TEST_IMAGE_ROUTING_KEY = "training.testimage";
    public const string TRAINING_MODEL_READY_ROUTING_KEY = "training.modelready";

    public const string HAWTSY_TRAINING_EVENTS_QUEUE = "training.events.hawtsy";
    public const string HARTSY_TRAINING_EVENTS_QUEUE_PREFIX = "training.events.hartsy";

    /// <summary>Maps each training-lifecycle message type to its topic-exchange routing key. This is
    /// the single source of truth used both by the publisher (to route these six message types onto
    /// TRAINING_EVENTS_EXCHANGE by type, bypassing TargetSites) and by GetRoutingKeyForMessageType's
    /// fallback. TrainingModelUploadMessage deliberately does NOT appear here — it stays point-to-point
    /// via SITE_ROUTING_EXCHANGE/TargetSites="HartsyStorage", unrelated to this topic-exchange scheme.</summary>
    private static readonly Dictionary<string, string> TrainingMessageTypeRoutingKeys = new(StringComparer.Ordinal)
    {
        [nameof(TrainingStartedMessage)] = TRAINING_STARTED_ROUTING_KEY,
        [nameof(TrainingProgressMessage)] = TRAINING_PROGRESS_ROUTING_KEY,
        [nameof(TrainingCompletedMessage)] = TRAINING_COMPLETED_ROUTING_KEY,
        [nameof(TrainingFailedMessage)] = TRAINING_FAILED_ROUTING_KEY,
        [nameof(TrainingTestImageMessage)] = TRAINING_TEST_IMAGE_ROUTING_KEY,
        [nameof(TrainingModelReadyMessage)] = TRAINING_MODEL_READY_ROUTING_KEY
    };

    /// <summary>Looks up the topic-exchange routing key for one of the six training-lifecycle message
    /// types. Returns false for anything else, including TrainingModelUploadMessage.</summary>
    public static bool TryGetTrainingRoutingKey(string messageType, out string routingKey)
    {
        if (string.IsNullOrWhiteSpace(messageType))
        {
            routingKey = string.Empty;
            return false;
        }

        return TrainingMessageTypeRoutingKeys.TryGetValue(messageType, out routingKey!);
    }

    /// <summary>Which training-event routing keys a site should bind its own queue to. Empty means the
    /// site doesn't consume training events at all (its training-adjacent traffic, e.g.
    /// TrainingModelUploadMessage, flows over SITE_ROUTING_EXCHANGE instead and is untouched by this).</summary>
    public static string[] GetTrainingRoutingKeysForSite(string siteName)
    {
        return siteName switch
        {
            HARTSY => new[]
            {
                TRAINING_STARTED_ROUTING_KEY,
                TRAINING_PROGRESS_ROUTING_KEY,
                TRAINING_COMPLETED_ROUTING_KEY,
                TRAINING_FAILED_ROUTING_KEY,
                TRAINING_TEST_IMAGE_ROUTING_KEY,
                TRAINING_MODEL_READY_ROUTING_KEY
            },
            HAWTSY => new[]
            {
                TRAINING_PROGRESS_ROUTING_KEY,
                TRAINING_COMPLETED_ROUTING_KEY,
                TRAINING_TEST_IMAGE_ROUTING_KEY
            },
            DISCORD_BOT => Array.Empty<string>(),
            HARTSY_STORAGE => Array.Empty<string>(),
            HARTSY_SEEDER => Array.Empty<string>(),
            _ => throw new ArgumentException($"Unknown site name '{siteName}'", nameof(siteName))
        };
    }

    public const string HARTSY_ROUTING_KEY = "hartsy";
    public const string HAWTSY_ROUTING_KEY = "hawtsy";
    public const string DISCORD_BOT_ROUTING_KEY = "discord";
    public const string HARTSY_STORAGE_ROUTING_KEY = "hartsystorage";
    public const string HARTSY_SEEDER_ROUTING_KEY = "hartsyseeder";

    public static string GetInboxQueueForSite(string siteName)
    {
        return siteName switch
        {
            HARTSY => HARTSY_INBOX_QUEUE,
            HAWTSY => HAWTSY_INBOX_QUEUE,
            DISCORD_BOT => DISCORD_BOT_INBOX_QUEUE,
            HARTSY_STORAGE => HARTSY_STORAGE_INBOX_QUEUE,
            HARTSY_SEEDER => HARTSY_SEEDER_INBOX_QUEUE,
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
            HARTSY_STORAGE => HARTSY_STORAGE_ROUTING_KEY,
            HARTSY_SEEDER => HARTSY_SEEDER_ROUTING_KEY,
            _ => throw new ArgumentException($"Unknown site name '{siteName}'", nameof(siteName))
        };
    }

    public static string GetBroadcastQueueForSite(string siteName)
    {
        return siteName switch
        {
            HARTSY => HARTSY_BROADCAST_QUEUE,
            HAWTSY => HAWTSY_BROADCAST_QUEUE,
            DISCORD_BOT => DISCORD_BOT_BROADCAST_QUEUE,
            HARTSY_STORAGE => HARTSY_STORAGE_BROADCAST_QUEUE,
            HARTSY_SEEDER => HARTSY_SEEDER_BROADCAST_QUEUE,
            _ => throw new ArgumentException($"Unknown site name '{siteName}'", nameof(siteName))
        };
    }

    public static string GetRoutingKeyForMessageType(string messageType)
    {
        if (string.IsNullOrWhiteSpace(messageType))
        {
            return SYSTEM_HEALTH_ROUTING_KEY;
        }

        // Training lifecycle events (exact type match — see TryGetTrainingRoutingKey). Deliberately
        // NOT a substring match: TrainingModelUploadMessage must never be caught here.
        if (TryGetTrainingRoutingKey(messageType, out string trainingKey))
        {
            return trainingKey;
        }

        string lower = messageType.ToLowerInvariant();

        // Generic media upload messages (new system)
        if (lower.Contains("mediauploadstarted"))
        {
            return MEDIA_UPLOAD_ROUTING_KEY;
        }

        if (lower.Contains("mediauploadprogress"))
        {
            return MEDIA_PROGRESS_ROUTING_KEY;
        }

        if (lower.Contains("mediauploadcompleted") || lower.Contains("mediauploadcompletion"))
        {
            return MEDIA_COMPLETE_ROUTING_KEY;
        }

        if (lower.Contains("uploaddelete") || lower.Contains("mediauploaddeleted") || lower.Contains("mediadeleted"))
        {
            return MEDIA_DELETED_ROUTING_KEY;
        }

        // Torrent lifecycle (storage <-> seeder)
        if (lower.Contains("torrentrequested"))
        {
            return TORRENT_REQUESTED_ROUTING_KEY;
        }

        if (lower.Contains("torrentready"))
        {
            return TORRENT_READY_ROUTING_KEY;
        }

        if (lower.Contains("torrentremove"))
        {
            return TORRENT_REQUESTED_ROUTING_KEY; // routed to the seeder like other torrent requests
        }

        // Legacy model upload messages (backward compatibility)
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

        if (lower.Contains("modeldeleted"))
        {
            return MEDIA_DELETED_ROUTING_KEY; // Route to same place as media deletions
        }

        // User interactions
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
            new QueueDefinition { Name = MEDIA_EVENTS_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetStandardQueueArguments(config) },
            new QueueDefinition { Name = USER_INTERACTION_EVENTS_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetStandardQueueArguments(config) },
            new QueueDefinition { Name = SYSTEM_EVENTS_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetPriorityQueueArguments(config) },
            new QueueDefinition { Name = HARTSY_INBOX_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetStandardQueueArguments(config) },
            new QueueDefinition { Name = HAWTSY_INBOX_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetStandardQueueArguments(config) },
            new QueueDefinition { Name = DISCORD_BOT_INBOX_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetStandardQueueArguments(config) },
            new QueueDefinition { Name = HARTSY_STORAGE_INBOX_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetStandardQueueArguments(config) },
            new QueueDefinition { Name = HARTSY_SEEDER_INBOX_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetStandardQueueArguments(config) },
            new QueueDefinition { Name = HARTSY_BROADCAST_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetBroadcastQueueArguments(config) },
            new QueueDefinition { Name = HAWTSY_BROADCAST_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetBroadcastQueueArguments(config) },
            new QueueDefinition { Name = DISCORD_BOT_BROADCAST_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetBroadcastQueueArguments(config) },
            new QueueDefinition { Name = HARTSY_STORAGE_BROADCAST_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetBroadcastQueueArguments(config) },
            new QueueDefinition { Name = HARTSY_SEEDER_BROADCAST_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetBroadcastQueueArguments(config) },
            new QueueDefinition { Name = DEAD_LETTER_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetDeadLetterQueueArguments(config) },
            new QueueDefinition { Name = MONITORING_QUEUE, Durable = config.Queues.DurableQueues, Exclusive = false, AutoDelete = false, Arguments = GetStandardQueueArguments(config) }
        };

        return queues;
    }

    public static List<QueueBinding> GetAllQueueBindings()
    {
        return new List<QueueBinding>
        {
            // Legacy model events (backward compatibility)
            new QueueBinding(DOMAIN_EVENTS_EXCHANGE, MODEL_EVENTS_QUEUE, MODEL_UPLOAD_ROUTING_KEY),
            new QueueBinding(DOMAIN_EVENTS_EXCHANGE, MODEL_EVENTS_QUEUE, MODEL_PROGRESS_ROUTING_KEY),
            new QueueBinding(DOMAIN_EVENTS_EXCHANGE, MODEL_EVENTS_QUEUE, MODEL_COMPLETE_ROUTING_KEY),

            // Generic media events (new system)
            new QueueBinding(DOMAIN_EVENTS_EXCHANGE, MEDIA_EVENTS_QUEUE, MEDIA_UPLOAD_ROUTING_KEY),
            new QueueBinding(DOMAIN_EVENTS_EXCHANGE, MEDIA_EVENTS_QUEUE, MEDIA_PROGRESS_ROUTING_KEY),
            new QueueBinding(DOMAIN_EVENTS_EXCHANGE, MEDIA_EVENTS_QUEUE, MEDIA_COMPLETE_ROUTING_KEY),
            new QueueBinding(DOMAIN_EVENTS_EXCHANGE, MEDIA_EVENTS_QUEUE, MEDIA_DELETED_ROUTING_KEY),

            // Other domain events
            new QueueBinding(DOMAIN_EVENTS_EXCHANGE, USER_INTERACTION_EVENTS_QUEUE, USER_INTERACTION_ROUTING_KEY),
            new QueueBinding(DOMAIN_EVENTS_EXCHANGE, SYSTEM_EVENTS_QUEUE, SYSTEM_HEALTH_ROUTING_KEY),

            // Site-specific routing
            new QueueBinding(SITE_ROUTING_EXCHANGE, HARTSY_INBOX_QUEUE, HARTSY_ROUTING_KEY),
            new QueueBinding(SITE_ROUTING_EXCHANGE, HAWTSY_INBOX_QUEUE, HAWTSY_ROUTING_KEY),
            new QueueBinding(SITE_ROUTING_EXCHANGE, DISCORD_BOT_INBOX_QUEUE, DISCORD_BOT_ROUTING_KEY),
            new QueueBinding(SITE_ROUTING_EXCHANGE, HARTSY_STORAGE_INBOX_QUEUE, HARTSY_STORAGE_ROUTING_KEY),
            new QueueBinding(SITE_ROUTING_EXCHANGE, HARTSY_SEEDER_INBOX_QUEUE, HARTSY_SEEDER_ROUTING_KEY),

            // Broadcast routing
            new QueueBinding(BROADCAST_EXCHANGE, HARTSY_BROADCAST_QUEUE, string.Empty),
            new QueueBinding(BROADCAST_EXCHANGE, HAWTSY_BROADCAST_QUEUE, string.Empty),
            new QueueBinding(BROADCAST_EXCHANGE, DISCORD_BOT_BROADCAST_QUEUE, string.Empty),
            new QueueBinding(BROADCAST_EXCHANGE, HARTSY_STORAGE_BROADCAST_QUEUE, string.Empty),
            new QueueBinding(BROADCAST_EXCHANGE, HARTSY_SEEDER_BROADCAST_QUEUE, string.Empty)
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
