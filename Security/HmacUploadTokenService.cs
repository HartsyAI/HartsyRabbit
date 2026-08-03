using HartsyRabbit.Messages;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HartsyRabbit.Security;

/// <summary>
/// Options binding for <see cref="HmacUploadTokenService"/>.
/// </summary>
public sealed class UploadTokenOptions
{
    public const string SectionName = "UploadTokens";

    /// <summary>Active HS256 secret. REQUIRED. Should be 32+ random bytes, base64 or hex-encoded.</summary>
    public string CurrentSecret { get; set; } = string.Empty;

    /// <summary>Previous secret — accepted for validation only, not issuance. Enables zero-downtime rotation.</summary>
    public string? PreviousSecret { get; set; }

    /// <summary>Token lifetime in minutes. Kept short to bound replay of a leaked token. Defaults to 15.</summary>
    public int IssuerLifetimeMinutes { get; set; } = 15;

    /// <summary>JWT issuer. Must match on both sides.</summary>
    public string Issuer { get; set; } = "hartsy";

    /// <summary>JWT audience. Must match on both sides. Configure explicitly on both sides;
    /// this default only exists so a missing config fails closed with an obvious mismatch.</summary>
    public string Audience { get; set; } = "hartsystorage";

    /// <summary>Clock skew tolerated on validation, in seconds.</summary>
    public int ClockSkewSeconds { get; set; } = 60;
}

/// <summary>
/// HS256 JWT implementation of <see cref="IUploadTokenService"/>. Supports dual-secret rotation:
/// tokens are signed with <c>CurrentSecret</c> and validated against both <c>CurrentSecret</c>
/// and <c>PreviousSecret</c>.
/// </summary>
public sealed class HmacUploadTokenService : IUploadTokenService
{
    // Claim types (short names keep the payload compact).
    public const string ClaimUploadId = "uid";
    public const string ClaimMediaType = "mt";
    public const string ClaimMaxBytes = "max";
    public const string ClaimIsPrivate = "prv";

    public readonly UploadTokenOptions _options;
    public readonly JwtSecurityTokenHandler _handler = new() { MapInboundClaims = false };
    public readonly SigningCredentials _currentSigningCreds;
    public readonly List<SecurityKey> _validationKeys;

    public HmacUploadTokenService(IOptions<UploadTokenOptions> options)
    {
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.CurrentSecret))
        {
            throw new InvalidOperationException($"{UploadTokenOptions.SectionName}:CurrentSecret is required.");
        }

        SymmetricSecurityKey currentKey = BuildKey(_options.CurrentSecret, "current");
        _currentSigningCreds = new SigningCredentials(currentKey, SecurityAlgorithms.HmacSha256);

        _validationKeys = new List<SecurityKey> { currentKey };
        if (!string.IsNullOrWhiteSpace(_options.PreviousSecret))
        {
            _validationKeys.Add(BuildKey(_options.PreviousSecret!, "previous"));
        }
    }

    public string Issue(UploadTokenClaims claims)
    {
        DateTime now = DateTime.UtcNow;
        DateTime exp = now.AddMinutes(_options.IssuerLifetimeMinutes);

        JwtSecurityToken jwt = new(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, claims.UserId),
                new Claim(JwtRegisteredClaimNames.Jti, claims.Jti),
                new Claim(ClaimUploadId, claims.UploadId),
                new Claim(ClaimMediaType, claims.MediaType.ToString()),
                new Claim(ClaimMaxBytes, claims.MaxBytes.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                new Claim(ClaimIsPrivate, claims.IsPrivate ? "1" : "0"),
            },
            notBefore: now,
            expires: exp,
            signingCredentials: _currentSigningCreds);

        return _handler.WriteToken(jwt);
    }

    public bool TryValidate(string token, out UploadTokenClaims? claims, out string? error)
    {
        claims = null;
        error = null;

        if (string.IsNullOrWhiteSpace(token))
        {
            error = "Token is empty";
            return false;
        }

        TokenValidationParameters parameters = new()
        {
            ValidIssuer = _options.Issuer,
            ValidateIssuer = true,
            ValidAudience = _options.Audience,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = _validationKeys,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromSeconds(_options.ClockSkewSeconds),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
        };

        try
        {
            ClaimsPrincipal principal = _handler.ValidateToken(token, parameters, out SecurityToken validated);
            if (validated is not JwtSecurityToken jwt)
            {
                error = "Token is not a JWT";
                return false;
            }

            string? userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            string? uploadId = principal.FindFirst(ClaimUploadId)?.Value;
            string? mediaTypeRaw = principal.FindFirst(ClaimMediaType)?.Value;
            string? maxBytesRaw = principal.FindFirst(ClaimMaxBytes)?.Value;
            string? jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(userId) ||
                string.IsNullOrEmpty(uploadId) ||
                string.IsNullOrEmpty(mediaTypeRaw) ||
                string.IsNullOrEmpty(maxBytesRaw))
            {
                error = "Token missing required claims";
                return false;
            }

            if (!Enum.TryParse(mediaTypeRaw, ignoreCase: false, out MediaType mediaType))
            {
                error = $"Invalid media type claim '{mediaTypeRaw}'";
                return false;
            }

            if (!long.TryParse(maxBytesRaw, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out long maxBytes) || maxBytes <= 0)
            {
                error = "Invalid max-bytes claim";
                return false;
            }

            claims = new UploadTokenClaims
            {
                UserId = userId,
                UploadId = uploadId,
                MediaType = mediaType,
                MaxBytes = maxBytes,
                // Absent reads as public, so tokens minted before this claim existed stay valid.
                IsPrivate = principal.FindFirst(ClaimIsPrivate)?.Value == "1",
                IssuedAt = jwt.ValidFrom,
                ExpiresAt = jwt.ValidTo,
                Jti = jti ?? string.Empty
            };
            return true;
        }
        catch (SecurityTokenExpiredException)
        {
            error = "Token expired";
            return false;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            error = "Invalid signature";
            return false;
        }
        catch (SecurityTokenException ex)
        {
            error = $"Token validation failed: {ex.Message}";
            return false;
        }
    }

    public static SymmetricSecurityKey BuildKey(string secret, string label)
    {
        // Accept base64 first (recommended), fall back to UTF-8 bytes so dev setups with plain text still work.
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(secret);
        }
        catch (FormatException)
        {
            bytes = Encoding.UTF8.GetBytes(secret);
        }

        if (bytes.Length < 32)
        {
            throw new InvalidOperationException($"UploadTokens:{label} secret must be at least 32 bytes (got {bytes.Length}).");
        }

        return new SymmetricSecurityKey(bytes) { KeyId = label };
    }
}
