using SsoServer.Domain.Enums;
using Microsoft.Extensions.Primitives;
using SsoServer.Domain.Interfaces;

namespace SsoServer.Web.Middleware;

/// <summary>
/// Protocol-level gate: rejects every /connect/* request whose client has been disabled by
/// an administrator (authorize, token, introspection, revocation, end-session) and audits
/// revocation-endpoint calls. Runs before the OpenIddict authentication handlers.
/// </summary>
public sealed class DisabledClientMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DisabledClientMiddleware> _logger;

    public DisabledClientMiddleware(RequestDelegate next, ILogger<DisabledClientMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/connect"))
        {
            var clientId = await ResolveClientIdAsync(context);
            if (!string.IsNullOrEmpty(clientId))
            {
                var clients = context.RequestServices.GetRequiredService<IClientRepository>();
                if (!await clients.IsEnabledAsync(clientId, context.RequestAborted))
                {
                    _logger.LogWarning("Rejected request for disabled client {ClientId} at {Path}", clientId, context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        """{"error":"invalid_client","error_description":"این برنامه توسط مدیر سامانه غیرفعال شده است."}""",
                        context.RequestAborted);
                    return;
                }
            }

            if (context.Request.Path.StartsWithSegments("/connect/revoke") &&
                HttpMethods.IsPost(context.Request.Method))
            {
                var audit = context.RequestServices.GetRequiredService<IAuditService>();
                await audit.LogAsync(
                    AuditAction.TokenRevoked,
                    clientId: clientId,
                    ip: context.Connection.RemoteIpAddress?.ToString(),
                    data: new { Endpoint = "revocation" },
                    ct: context.RequestAborted);
            }
        }

        await _next(context);
    }

    private static async Task<string?> ResolveClientIdAsync(HttpContext context)
    {
        var request = context.Request;
        if (request.HasFormContentType)
        {
            if (request.Form.TryGetValue("client_id", out var fromForm) && !StringValues.IsNullOrEmpty(fromForm))
            {
                return fromForm.ToString();
            }
        }
        else if (request.Query.TryGetValue("client_id", out var fromQuery) && !StringValues.IsNullOrEmpty(fromQuery))
        {
            return fromQuery.ToString();
        }

        if (request.Query.TryGetValue("client_id", out var fallback) && !StringValues.IsNullOrEmpty(fallback))
        {
            return fallback.ToString();
        }

        return null;
    }
}
