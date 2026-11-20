using Microsoft.AspNetCore.Cors.Infrastructure;
using SsoServer.Domain.Interfaces;

namespace SsoServer.Web.Services;

/// <summary>
/// Serves the CORS policy from the hot-reloadable settings: only origins saved by the
/// admin are allowed (any header/method, no credentials needed by the samples).
/// </summary>
public sealed class DynamicCorsPolicyProvider : ICorsPolicyProvider
{
    private readonly ISettingsService _settings;

    public DynamicCorsPolicyProvider(ISettingsService settings) => _settings = settings;

    public Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        var origins = _settings.Current.CorsOrigins;
        if (origins.Count == 0)
        {
            return Task.FromResult<CorsPolicy?>(null);
        }

        var policy = new CorsPolicyBuilder()
            .WithOrigins([.. origins])
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("Authorization", "Content-Type", "Accept")
            .WithExposedHeaders("Content-Type", "X-Correlation-Id")
            .Build();

        return Task.FromResult<CorsPolicy?>(policy);
    }
}
