using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using SsoServer.Domain.Interfaces;

namespace SsoServer.Web.Services.RateLimiting;

public static class RateLimitPolicies
{
    public const string Auth = "sso-auth";
    public const string Token = "sso-token";
    public const string Api = "sso-api";
}

/// <summary>
/// Single fixed-window policy whose limit is read live from the hot-reloadable settings,
/// partitioned per policy: auth = IP+username, token = IP+client_id, api = IP+subject.
/// </summary>
public sealed class SsoRateLimitPolicy : IRateLimiterPolicy<string>
{
    private readonly string _name;

    public SsoRateLimitPolicy(string name) => _name = name;

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; }

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var settings = httpContext.RequestServices.GetRequiredService<ISettingsService>().Current;
        var permitLimit = Math.Max(1, settings.TokenRateLimitPerMinute);
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            PartitionKey(httpContext, ip),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            });
    }

    private string PartitionKey(HttpContext context, string ip) => _name switch
    {
        RateLimitPolicies.Auth => $"auth:{ip}:{FormValue(context, "Username")}",
        RateLimitPolicies.Token => $"token:{ip}:{FormValue(context, "client_id")}",
        _ => $"api:{ip}:{context.User.FindFirst("sub")?.Value ?? "anonymous"}",
    };

    private static string FormValue(HttpContext context, string key)
        => context.Request.HasFormContentType && context.Request.Form.TryGetValue(key, out var value)
            ? value.ToString()
            : "anonymous";
}
