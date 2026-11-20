namespace SsoServer.Web.Middleware;

/// <summary>
/// Applies the security headers required by the hardening rules (fully offline CSP).
/// The strict policy stays in place for every human-facing page (login, admin, manage).
/// OpenIddict protocol endpoints (<c>/connect/*</c>) get a dedicated policy because
/// their authorization/logout responses are auto-submitting HTML forms:
/// OpenIddict emits an inline <c>&lt;script&gt;document.form.submit()&lt;/script&gt;</c>
/// and posts the form to the client's registered redirect URI, which is always
/// cross-origin (e.g. http://localhost:5002) and dynamic (admins register clients
/// at runtime). With the strict CSP both the script execution and the form
/// submission are blocked by the browser and the user is left on a silent blank
/// page — see OpenIddict issue #1865. These protocol pages are fully
/// server-generated (no user-controlled content), so allowing inline scripts and
/// http(s) form targets there is safe.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private const string StrictContentSecurityPolicy =
        "default-src 'self'; img-src 'self' data:; style-src 'self'; font-src 'self'; script-src 'self'; " +
        "frame-ancestors 'none'; base-uri 'self'; form-action 'self'";

    // OpenIddict protocol pages: allows the form_post auto-submit script and the
    // cross-origin form submission to each client's registered redirect URI.
    private const string ProtocolContentSecurityPolicy =
        "default-src 'self'; img-src 'self' data:; style-src 'self'; font-src 'self'; script-src 'self' 'unsafe-inline'; " +
        "frame-ancestors 'none'; base-uri 'self'; form-action http: https:";

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers.XFrameOptions = "DENY";
            headers.XContentTypeOptions = "nosniff";
            headers["Referrer-Policy"] = "no-referrer";
            headers["X-Permitted-Cross-Domain-Policies"] = "none";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            headers.ContentSecurityPolicy = IsOpenIddictProtocolEndpoint(context.Request)
                ? ProtocolContentSecurityPolicy
                : StrictContentSecurityPolicy;
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static bool IsOpenIddictProtocolEndpoint(HttpRequest request)
        => request.Path.StartsWithSegments("/connect", StringComparison.OrdinalIgnoreCase);
}
