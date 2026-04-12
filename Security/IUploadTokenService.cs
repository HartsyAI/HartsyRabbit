namespace HartsyRabbit.Security;

/// <summary>
/// Issues and validates upload tokens used to authorize direct browser→Hawtsy TUS uploads.
/// Implementations must be thread-safe and side-effect free.
/// </summary>
public interface IUploadTokenService
{
    /// <summary>
    /// Issue a signed token for the given claims. Populates <c>IssuedAt</c>/<c>ExpiresAt</c>/<c>Jti</c>
    /// on the returned claims using the configured lifetime.
    /// </summary>
    string Issue(UploadTokenClaims claims);

    /// <summary>
    /// Validate a token. Returns <c>true</c> and populates <paramref name="claims"/> on success.
    /// Returns <c>false</c> and populates <paramref name="error"/> with a short reason on failure.
    /// </summary>
    bool TryValidate(string token, out UploadTokenClaims? claims, out string? error);
}
