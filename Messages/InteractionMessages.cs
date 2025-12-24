namespace HartsyRabbit.Messages;

/// <summary>Message published when a user favorites an image. Cross-site notification for user interaction tracking.</summary>
public class UserFavoritedImageMessage : IUserInteractionMessage
{
    public string UserId { get; set; } = string.Empty;
    public string ImageId { get; set; } = string.Empty;
    public string InteractionType { get; set; } = "ImageFavorited";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }

    public static UserFavoritedImageMessage CreateFavorite(string userId, string imageId, string sourceSite, string? username = null, string? interactionContext = null, long? expectedFavoriteCount = null, Dictionary<string, object>? metadata = null)
    {
        Dictionary<string, object> finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["sourceSite"] = sourceSite;
        if (!string.IsNullOrWhiteSpace(username))
        {
            finalMetadata["username"] = username;
        }
        if (!string.IsNullOrWhiteSpace(interactionContext))
        {
            finalMetadata["interactionContext"] = interactionContext;
        }
        if (expectedFavoriteCount.HasValue)
        {
            finalMetadata["expectedFavoriteCount"] = expectedFavoriteCount.Value;
        }
        return new UserFavoritedImageMessage
        {
            UserId = userId,
            ImageId = imageId,
            InteractionType = "ImageFavorited",
            Timestamp = DateTime.UtcNow,
            Metadata = finalMetadata
        };
    }

    public static UserFavoritedImageMessage CreateUnfavorite(string userId, string imageId, string sourceSite, string? username = null, string? interactionContext = null, long? expectedFavoriteCount = null, Dictionary<string, object>? metadata = null)
    {
        Dictionary<string, object> finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["sourceSite"] = sourceSite;
        if (!string.IsNullOrWhiteSpace(username))
        {
            finalMetadata["username"] = username;
        }
        if (!string.IsNullOrWhiteSpace(interactionContext))
        {
            finalMetadata["interactionContext"] = interactionContext;
        }
        if (expectedFavoriteCount.HasValue)
        {
            finalMetadata["expectedFavoriteCount"] = expectedFavoriteCount.Value;
        }
        return new UserFavoritedImageMessage
        {
            UserId = userId,
            ImageId = imageId,
            InteractionType = "ImageUnfavorited",
            Timestamp = DateTime.UtcNow,
            Metadata = finalMetadata
        };
    }
}

/// <summary>Message published when a user likes an image. Cross-site notification for user interaction tracking.</summary>
public class UserLikedImageMessage : IUserInteractionMessage
{
    public string UserId { get; set; } = string.Empty;
    public string ImageId { get; set; } = string.Empty;
    public string InteractionType { get; set; } = "ImageLiked";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }

    public static UserLikedImageMessage CreateLike(string userId, string imageId, string sourceSite, string? username = null, string? interactionContext = null, long? expectedLikeCount = null, Dictionary<string, object>? metadata = null)
    {
        Dictionary<string, object> finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["sourceSite"] = sourceSite;
        if (!string.IsNullOrWhiteSpace(username))
        {
            finalMetadata["username"] = username;
        }
        if (!string.IsNullOrWhiteSpace(interactionContext))
        {
            finalMetadata["interactionContext"] = interactionContext;
        }
        if (expectedLikeCount.HasValue)
        {
            finalMetadata["expectedLikeCount"] = expectedLikeCount.Value;
        }
        return new UserLikedImageMessage
        {
            UserId = userId,
            ImageId = imageId,
            InteractionType = "ImageLiked",
            Timestamp = DateTime.UtcNow,
            Metadata = finalMetadata
        };
    }

    public static UserLikedImageMessage CreateUnlike(string userId, string imageId, string sourceSite, string? username = null, string? interactionContext = null, long? expectedLikeCount = null, Dictionary<string, object>? metadata = null)
    {
        Dictionary<string, object> finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["sourceSite"] = sourceSite;
        if (!string.IsNullOrWhiteSpace(username))
        {
            finalMetadata["username"] = username;
        }
        if (!string.IsNullOrWhiteSpace(interactionContext))
        {
            finalMetadata["interactionContext"] = interactionContext;
        }
        if (expectedLikeCount.HasValue)
        {
            finalMetadata["expectedLikeCount"] = expectedLikeCount.Value;
        }
        return new UserLikedImageMessage
        {
            UserId = userId,
            ImageId = imageId,
            InteractionType = "ImageUnliked",
            Timestamp = DateTime.UtcNow,
            Metadata = finalMetadata
        };
    }
}

public class UserLikedModelMessage : IUserInteractionMessage
{
    public string UserId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string InteractionType { get; set; } = "ModelLiked";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }

    public static UserLikedModelMessage CreateLike(string userId, string modelId, string sourceSite, string? username = null, string? interactionContext = null, long? expectedLikeCount = null, Dictionary<string, object>? metadata = null)
    {
        Dictionary<string, object> finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["sourceSite"] = sourceSite;
        if (!string.IsNullOrWhiteSpace(username))
        {
            finalMetadata["username"] = username;
        }
        if (!string.IsNullOrWhiteSpace(interactionContext))
        {
            finalMetadata["interactionContext"] = interactionContext;
        }
        if (expectedLikeCount.HasValue)
        {
            finalMetadata["expectedLikeCount"] = expectedLikeCount.Value;
        }
        return new UserLikedModelMessage
        {
            UserId = userId,
            ModelId = modelId,
            InteractionType = "ModelLiked",
            Timestamp = DateTime.UtcNow,
            Metadata = finalMetadata
        };
    }

    public static UserLikedModelMessage CreateUnlike(string userId, string modelId, string sourceSite, string? username = null, string? interactionContext = null, long? expectedLikeCount = null, Dictionary<string, object>? metadata = null)
    {
        Dictionary<string, object> finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["sourceSite"] = sourceSite;
        if (!string.IsNullOrWhiteSpace(username))
        {
            finalMetadata["username"] = username;
        }
        if (!string.IsNullOrWhiteSpace(interactionContext))
        {
            finalMetadata["interactionContext"] = interactionContext;
        }
        if (expectedLikeCount.HasValue)
        {
            finalMetadata["expectedLikeCount"] = expectedLikeCount.Value;
        }
        return new UserLikedModelMessage
        {
            UserId = userId,
            ModelId = modelId,
            InteractionType = "ModelUnliked",
            Timestamp = DateTime.UtcNow,
            Metadata = finalMetadata
        };
    }
}

/// <summary>Message published when a user favorites a model. Cross-site notification for user interaction tracking.</summary>
public class UserFavoritedModelMessage : IUserInteractionMessage
{
    public string UserId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string InteractionType { get; set; } = "ModelFavorited";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }

    public static UserFavoritedModelMessage CreateFavorite(string userId, string modelId, string sourceSite, string? username = null, string? interactionContext = null, long? expectedFavoriteCount = null, Dictionary<string, object>? metadata = null)
    {
        Dictionary<string, object> finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["sourceSite"] = sourceSite;
        if (!string.IsNullOrWhiteSpace(username))
        {
            finalMetadata["username"] = username;
        }
        if (!string.IsNullOrWhiteSpace(interactionContext))
        {
            finalMetadata["interactionContext"] = interactionContext;
        }
        if (expectedFavoriteCount.HasValue)
        {
            finalMetadata["expectedFavoriteCount"] = expectedFavoriteCount.Value;
        }
        return new UserFavoritedModelMessage
        {
            UserId = userId,
            ModelId = modelId,
            InteractionType = "ModelFavorited",
            Timestamp = DateTime.UtcNow,
            Metadata = finalMetadata
        };
    }

    public static UserFavoritedModelMessage CreateUnfavorite(string userId, string modelId, string sourceSite, string? username = null, string? interactionContext = null, long? expectedFavoriteCount = null, Dictionary<string, object>? metadata = null)
    {
        Dictionary<string, object> finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["sourceSite"] = sourceSite;
        if (!string.IsNullOrWhiteSpace(username))
        {
            finalMetadata["username"] = username;
        }
        if (!string.IsNullOrWhiteSpace(interactionContext))
        {
            finalMetadata["interactionContext"] = interactionContext;
        }
        if (expectedFavoriteCount.HasValue)
        {
            finalMetadata["expectedFavoriteCount"] = expectedFavoriteCount.Value;
        }
        return new UserFavoritedModelMessage
        {
            UserId = userId,
            ModelId = modelId,
            InteractionType = "ModelUnfavorited",
            Timestamp = DateTime.UtcNow,
            Metadata = finalMetadata
        };
    }
}

/// <summary>Message published when a user favorites a preset. Cross-site notification for user interaction tracking.</summary>
public class UserFavoritedPresetMessage : IUserInteractionMessage
{
    public string UserId { get; set; } = string.Empty;
    public string PresetId { get; set; } = string.Empty;
    public string InteractionType { get; set; } = "PresetFavorited";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }

    public static UserFavoritedPresetMessage CreateFavorite(string userId, string presetId, string sourceSite, string? username = null, string? interactionContext = null, long? expectedFavoriteCount = null, Dictionary<string, object>? metadata = null)
    {
        Dictionary<string, object> finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["sourceSite"] = sourceSite;
        if (!string.IsNullOrWhiteSpace(username))
        {
            finalMetadata["username"] = username;
        }
        if (!string.IsNullOrWhiteSpace(interactionContext))
        {
            finalMetadata["interactionContext"] = interactionContext;
        }
        if (expectedFavoriteCount.HasValue)
        {
            finalMetadata["expectedFavoriteCount"] = expectedFavoriteCount.Value;
        }
        return new UserFavoritedPresetMessage
        {
            UserId = userId,
            PresetId = presetId,
            InteractionType = "PresetFavorited",
            Timestamp = DateTime.UtcNow,
            Metadata = finalMetadata
        };
    }

    public static UserFavoritedPresetMessage CreateUnfavorite(string userId, string presetId, string sourceSite, string? username = null, string? interactionContext = null, long? expectedFavoriteCount = null, Dictionary<string, object>? metadata = null)
    {
        Dictionary<string, object> finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["sourceSite"] = sourceSite;
        if (!string.IsNullOrWhiteSpace(username))
        {
            finalMetadata["username"] = username;
        }
        if (!string.IsNullOrWhiteSpace(interactionContext))
        {
            finalMetadata["interactionContext"] = interactionContext;
        }
        if (expectedFavoriteCount.HasValue)
        {
            finalMetadata["expectedFavoriteCount"] = expectedFavoriteCount.Value;
        }
        return new UserFavoritedPresetMessage
        {
            UserId = userId,
            PresetId = presetId,
            InteractionType = "PresetUnfavorited",
            Timestamp = DateTime.UtcNow,
            Metadata = finalMetadata
        };
    }
}

/// <summary>Message published when a user likes a preset. Cross-site notification for user interaction tracking.</summary>
public class UserLikedPresetMessage : IUserInteractionMessage
{
    public string UserId { get; set; } = string.Empty;
    public string PresetId { get; set; } = string.Empty;
    public string InteractionType { get; set; } = "PresetLiked";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }

    public static UserLikedPresetMessage CreateLike(string userId, string presetId, string sourceSite, string? username = null, string? interactionContext = null, long? expectedLikeCount = null, Dictionary<string, object>? metadata = null)
    {
        Dictionary<string, object> finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["sourceSite"] = sourceSite;
        if (!string.IsNullOrWhiteSpace(username))
        {
            finalMetadata["username"] = username;
        }
        if (!string.IsNullOrWhiteSpace(interactionContext))
        {
            finalMetadata["interactionContext"] = interactionContext;
        }
        if (expectedLikeCount.HasValue)
        {
            finalMetadata["expectedLikeCount"] = expectedLikeCount.Value;
        }
        return new UserLikedPresetMessage
        {
            UserId = userId,
            PresetId = presetId,
            InteractionType = "PresetLiked",
            Timestamp = DateTime.UtcNow,
            Metadata = finalMetadata
        };
    }

    public static UserLikedPresetMessage CreateUnlike(string userId, string presetId, string sourceSite, string? username = null, string? interactionContext = null, long? expectedLikeCount = null, Dictionary<string, object>? metadata = null)
    {
        Dictionary<string, object> finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["sourceSite"] = sourceSite;
        if (!string.IsNullOrWhiteSpace(username))
        {
            finalMetadata["username"] = username;
        }
        if (!string.IsNullOrWhiteSpace(interactionContext))
        {
            finalMetadata["interactionContext"] = interactionContext;
        }
        if (expectedLikeCount.HasValue)
        {
            finalMetadata["expectedLikeCount"] = expectedLikeCount.Value;
        }
        return new UserLikedPresetMessage
        {
            UserId = userId,
            PresetId = presetId,
            InteractionType = "PresetUnliked",
            Timestamp = DateTime.UtcNow,
            Metadata = finalMetadata
        };
    }
}

/// <summary>Message published when a user likes a community post. Cross-site notification for user interaction tracking.</summary>
public class UserLikedCommunityPostMessage : IUserInteractionMessage
{
    public string UserId { get; set; } = string.Empty;
    public string PostId { get; set; } = string.Empty;
    public string InteractionType { get; set; } = "CommunityPostLiked";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }

    public static UserLikedCommunityPostMessage CreateLike(string userId, string communityPostId, string sourceSite, string? username = null, string? interactionContext = null, long? expectedLikeCount = null, Dictionary<string, object>? metadata = null)
    {
        Dictionary<string, object> finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["sourceSite"] = sourceSite;
        if (!string.IsNullOrWhiteSpace(username))
        {
            finalMetadata["username"] = username;
        }
        if (!string.IsNullOrWhiteSpace(interactionContext))
        {
            finalMetadata["interactionContext"] = interactionContext;
        }
        if (expectedLikeCount.HasValue)
        {
            finalMetadata["expectedLikeCount"] = expectedLikeCount.Value;
        }
        return new UserLikedCommunityPostMessage
        {
            UserId = userId,
            PostId = communityPostId,
            InteractionType = "CommunityPostLiked",
            Timestamp = DateTime.UtcNow,
            Metadata = finalMetadata
        };
    }

    public static UserLikedCommunityPostMessage CreateUnlike(string userId, string communityPostId, string sourceSite, string? username = null, string? interactionContext = null, long? expectedLikeCount = null, Dictionary<string, object>? metadata = null)
    {
        Dictionary<string, object> finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["sourceSite"] = sourceSite;
        if (!string.IsNullOrWhiteSpace(username))
        {
            finalMetadata["username"] = username;
        }
        if (!string.IsNullOrWhiteSpace(interactionContext))
        {
            finalMetadata["interactionContext"] = interactionContext;
        }
        if (expectedLikeCount.HasValue)
        {
            finalMetadata["expectedLikeCount"] = expectedLikeCount.Value;
        }
        return new UserLikedCommunityPostMessage
        {
            UserId = userId,
            PostId = communityPostId,
            InteractionType = "CommunityPostUnliked",
            Timestamp = DateTime.UtcNow,
            Metadata = finalMetadata
        };
    }
}

/// <summary>Message published when a user downloads content (model, preset, etc). Cross-site notification for analytics and user interaction tracking.</summary>
public class UserDownloadedContentMessage : IUserInteractionMessage
{
    public string UserId { get; set; } = string.Empty;
    public string ContentId { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty; // "model", "preset", "image", etc.
    public string InteractionType { get; set; } = "ContentDownloaded";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public long? FileSizeBytes { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }

    public static UserDownloadedContentMessage CreateDownload(string userId, string contentId, string contentType, string sourceSite, string? username = null, string? interactionContext = null, Dictionary<string, object>? metadata = null)
    {
        Dictionary<string, object> finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["sourceSite"] = sourceSite;
        if (!string.IsNullOrWhiteSpace(username))
        {
            finalMetadata["username"] = username;
        }
        if (!string.IsNullOrWhiteSpace(interactionContext))
        {
            finalMetadata["interactionContext"] = interactionContext;
        }
        return new UserDownloadedContentMessage
        {
            UserId = userId,
            ContentId = contentId,
            ContentType = contentType,
            InteractionType = "ContentDownloaded",
            Timestamp = DateTime.UtcNow,
            Metadata = finalMetadata
        };
    }
}
