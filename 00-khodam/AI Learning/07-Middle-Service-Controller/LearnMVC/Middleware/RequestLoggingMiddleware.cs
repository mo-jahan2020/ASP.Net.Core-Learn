namespace LearnMVC.Middleware;

/// <summary>
/// Middleware: بین Browser و Controller قرار می‌گیرد
/// هر Request از این "دروازه" عبور می‌کند
/// 
/// Pipeline: Browser → Middleware1 → Middleware2 → Controller → Middleware2 → Middleware1 → Browser
/// </summary>
public class RequestLoggingMiddleware
{
    // _next = "دروازه بعدی" در pipeline
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        Console.WriteLine("Constrator Of RequestLoggingMiddleware RUN!!!");
        _next = next;
        _logger = logger;
    }

    // این متد برای هر Request یک بار اجرا می‌شود
    public async Task InvokeAsync(HttpContext context)
    {
        var requestTime = DateTime.Now;

        // ⬇️ قبل از رسیدن به Controller
        _logger.LogInformation(
            "📥 REQUEST آمد | مسیر: {Path} | زمان: {Time}",
            context.Request.Path,
            requestTime.ToString("HH:mm:ss.fff"));

        // اطلاعاتی به Request اضافه می‌کنیم (مثل هدر سفارشی)
        context.Items["RequestStartTime"] = requestTime;
        context.Response.Headers["X-Request-Logged"] = "true";

        // ✅ درخواست را به middleware/controller بعدی پاس می‌دهیم
        await _next(context);

        // ⬆️ بعد از برگشت از Controller (Response آماده شده)
        var duration = DateTime.Now - requestTime;
        _logger.LogInformation(
            "📤 RESPONSE رفت | Status: {Status} | مدت: {Duration}ms",
            context.Response.StatusCode,
            duration.TotalMilliseconds.ToString("F2"));
    }
}

/// <summary>
/// Extension Method برای ثبت راحت‌تر Middleware در Program.cs
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}
