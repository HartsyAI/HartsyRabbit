using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HartsyRabbit.Security;

/// <summary>
/// DI registration helpers for the shared upload-token service.
/// </summary>
public static class UploadTokenServiceExtensions
{
    /// <summary>
    /// Register <see cref="IUploadTokenService"/> (HS256 JWT implementation) and bind
    /// <see cref="UploadTokenOptions"/> from the <c>UploadTokens</c> configuration section.
    /// Environment variables <c>UPLOAD_TOKEN_SECRET_CURRENT</c> and
    /// <c>UPLOAD_TOKEN_SECRET_PREVIOUS</c> override the configuration values when present.
    /// </summary>
    public static IServiceCollection AddHartsyUploadTokens(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<UploadTokenOptions>()
            .Configure(opts =>
            {
                configuration.GetSection(UploadTokenOptions.SectionName).Bind(opts);

                string? envCurrent = Environment.GetEnvironmentVariable("UPLOAD_TOKEN_SECRET_CURRENT");
                if (!string.IsNullOrWhiteSpace(envCurrent))
                {
                    opts.CurrentSecret = envCurrent;
                }

                string? envPrevious = Environment.GetEnvironmentVariable("UPLOAD_TOKEN_SECRET_PREVIOUS");
                if (!string.IsNullOrWhiteSpace(envPrevious))
                {
                    opts.PreviousSecret = envPrevious;
                }
            })
            .Validate(opts => !string.IsNullOrWhiteSpace(opts.CurrentSecret),
                "UploadTokens:CurrentSecret (or UPLOAD_TOKEN_SECRET_CURRENT env var) must be set.")
            .Validate(opts => opts.IssuerLifetimeMinutes > 0,
                "UploadTokens:IssuerLifetimeMinutes must be > 0.")
            .ValidateOnStart();

        services.AddSingleton<IUploadTokenService, HmacUploadTokenService>();
        return services;
    }
}
