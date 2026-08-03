using HartsyRabbit.Messages;

namespace HartsyRabbit.Security;

/// <summary>
/// Strongly-typed upload-token claims shared by the issuer (HartsyWeb) and the validator (Hawtsy).
/// Encodes every piece of information Hawtsy needs to authorize a TUS upload without a DB round-trip.
/// </summary>
public sealed class UploadTokenClaims
{
    /// <summary>Supabase user id (JWT `sub`).</summary>
    public required string UserId { get; init; }

    /// <summary>Upload/row id that this token is bound to (JWT `uid`). Must equal the TUS metadata `uploadId`.</summary>
    public required string UploadId { get; init; }

    /// <summary>Media type that this token authorizes (JWT `mt`).</summary>
    public required MediaType MediaType { get; init; }

    /// <summary>Maximum total bytes the upload is allowed to be (JWT `max`). Enforced on TUS create AND on every write.</summary>
    public required long MaxBytes { get; init; }

    /// <summary>Store the object under the non-public `private/` key prefix (JWT `prv`). In the token
    /// rather than TUS metadata because the browser supplies metadata, and a client that cleared this
    /// would get its private image written to a CDN-servable key.</summary>
    public bool IsPrivate { get; init; }

    /// <summary>Token issue time (JWT `iat`).</summary>
    public DateTimeOffset IssuedAt { get; init; }

    /// <summary>Token expiry time (JWT `exp`).</summary>
    public DateTimeOffset ExpiresAt { get; init; }

    /// <summary>Unique token id (JWT `jti`). Useful for revocation/audit.</summary>
    public string Jti { get; init; } = Guid.NewGuid().ToString("N");
}
