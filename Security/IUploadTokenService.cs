namespace HartsyRabbit.Security;

/// <summary>
/// Issues and validates upload tokens used to authorize direct browser→Hawtsy TUS uploads.
/// Implementations must be thread-safe and side-effect free.
/// </summary>
/// <summary>
/// Checks the token an upload was created with, at the moment its last byte arrives.
/// </summary>
/// <remarks>The token is checked in full, lifetime included, when the upload is created. A transfer can outlast the token: it is
/// sent once, with the first request, and a renewed token never reaches the server. Checking the lifetime again when the
/// transfer finishes therefore refuses exactly the uploads that took longest, after every byte has been received.
///
/// This is a separate interface so that ignoring the lifetime is never an option on the ordinary check. It proves the token
/// was issued by the site and has not been altered; it does not prove it is current. Its caller must already hold the upload
/// the token was accepted for, and must check that the claims match that upload.</remarks>
public interface IAcceptedUploadTokenValidator
{
    /// <summary>Validates signature, issuer, audience and claims, and not the lifetime.</summary>
    bool TryValidateForAcceptedUpload(string token, out UploadTokenClaims? claims, out string? error);
}

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
