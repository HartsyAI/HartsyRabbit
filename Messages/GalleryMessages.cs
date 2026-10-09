namespace HartsyRabbit.Messages;

/// <summary>Asks the site to file a picture from Discord into a member's Hartsy gallery.</summary>
/// <remarks>
/// Sent by the Discord bot to the site's inbox (<c>TargetSites = "Hartsy"</c>). The site fetches the picture itself
/// from Discord's CDN, so the bytes never pass through the bot. The site runs the same upload pipeline as a browser
/// upload and resolves the member from <see cref="DiscordUserId"/>.
///
/// The site must treat (<see cref="DiscordUserId"/>, <see cref="RequestId"/>) as one save: a redelivered or
/// double-clicked request files the picture once and answers the same way.
/// </remarks>
public sealed record GalleryImageSaveRequestedMessage
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Idempotency key chosen by the bot: the Discord message id and the picture's position, e.g. <c>123:0</c>.</summary>
    public string RequestId { get; init; } = string.Empty;

    /// <summary>The member the picture is saved for, as a Discord snowflake string.</summary>
    public string DiscordUserId { get; init; } = string.Empty;

    /// <summary>The picture's link. The site accepts only https on Discord's CDN hosts (cdn.discordapp.com, media.discordapp.net).</summary>
    public string SourceUrl { get; init; } = string.Empty;

    /// <summary>The picture's name as Discord has it. The site takes the extension from the bytes it fetches.</summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>True when the member uploaded the picture; false when the bot made it.</summary>
    public bool MemberUpload { get; init; }

    /// <summary>The generation settings, in the site's SwarmUI metadata shape, when the bot knows them. Null means the
    /// site reads what it can from the file.</summary>
    public string? MetadataJson { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>The outcome of a <see cref="GalleryImageSaveRequestedMessage"/>, sent back to the bot.</summary>
public sealed record GalleryImageSaveCompletedMessage
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>The <see cref="GalleryImageSaveRequestedMessage.RequestId"/> this answers.</summary>
    public string RequestId { get; init; } = string.Empty;

    public string DiscordUserId { get; init; } = string.Empty;

    public bool Saved { get; init; }

    /// <summary>The gallery image's id when <see cref="Saved"/> is true.</summary>
    public string? ImageId { get; init; }

    /// <summary>Why it was not saved: <c>link_required</c>, <c>fetch_failed</c>, <c>too_large</c>, <c>unsupported_type</c>,
    /// <c>gallery_full</c>, <c>processing_failed</c> or <c>rejected</c>. Null when saved.</summary>
    public string? FailureCode { get; init; }

    /// <summary>A line fit to show a member. Null when saved.</summary>
    public string? FailureMessage { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
