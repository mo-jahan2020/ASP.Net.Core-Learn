using System.Text.Json;

namespace SsoServer.Web.Middleware;

/// <summary>
/// Global exception handler: ProblemDetails JSON for API/OIDC paths, friendly redirect for MVC pages.
/// Outside production the ProblemDetails carries the exception type and message to ease debugging.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            var correlationId = context.Items[CorrelationIdMiddleware.HeaderName]?.ToString();
            var detail = _environment.IsProduction()
                ? null
                : $"{ex.GetType().FullName}: {ex.Message}";

            if (context.Request.Path.StartsWithSegments("/api") ||
                context.Request.Path.StartsWithSegments("/connect") ||
                context.Request.Path.StartsWithSegments("/health"))
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";
                var payload = new Dictionary<string, object?>
                {
                    ["title"] = "خطای داخلی سرور رخ داده است.",
                    ["status"] = 500,
                    ["correlationId"] = correlationId,
                };
                if (detail is not null)
                {
                    payload["detail"] = detail;
                }

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(
                "<html dir=\"rtl\" lang=\"fa\"><head><meta charset=\"utf-8\"/><title>خطا</title></head>" +
                "<body style=\"font-family:Tahoma;text-align:center;padding-top:60px\">" +
                "<h2>خطای غیرمنتظره‌ای رخ داد. لطفاً دوباره تلاش کنید.</h2>" +
                $"<p dir=\"ltr\">Correlation ID: {correlationId}</p><a href=\"/\">بازگشت</a></body></html>");
        }
    }
}
