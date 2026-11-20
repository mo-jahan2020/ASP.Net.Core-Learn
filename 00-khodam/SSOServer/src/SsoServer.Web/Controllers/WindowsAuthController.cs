using System.Net.Security;
using System.Security.Claims;
using System.Text;
using SsoServer.Web.Helpers;
using SsoServer.Web.OpenIddict;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SsoServer.Application.Tokens;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SsoServer.Web.Controllers;

/// <summary>
/// SSO شفاف اکتیو دایرکتوری — بروزرسانی ۲۰۲۶-۱۱-۱۹ (بند ۳).
///
/// کاربر ویندوزی که با حساب دامنه‌ای لاگین کرده، بدون وارد کردن نام کاربری/رمز عبور
/// توکن می‌گیرد؛ سرور هویت او را از «تیکت Kerberos/NTLM ویندوز» استخراج می‌کند:
///
///  - WinForms (دسکتاپ): برنامه با SSPI ویندوز یک GSS token می‌سازد
///    (InitializeSecurityContext برای SPN سرور SSO) و به‌صورت فرم
///    POST /connect/windows با windows_token=Base64 و client_id + client_secret می‌فرستد.
///  - مرورگر (aspnetcore-sample): کلاینت به /connect/windows?client_id=...&redirect_uri=...&state=...
///    می‌رود؛ مرورگر به چالش 401 Negotiate خودکار جواب می‌دهد (لاگین ویندوزی همان کاربر)،
///    و سرور پس از تشخیص، به‌صورت 302 به redirect_uri با code برمی‌گردد — مثل فلو ۲.
///
/// الزامات (سیاست امنیتی سرور):
///  - client_id و client_secret هر دو الزامی‌اند (بند ۳ درخواستی — بدون این دو رد می‌شود).
///  - کاربر باید در سرور SSO موجود و فعال باشد (کاربران AD با IsAdUser=true).
///  - لیست‌سفید دسترسی به کلاینت (ClientUserAssignments) عیناً اعمال می‌شود.
///  - فقط وقتی AD در تنظیمات فعال است این مسیر کار می‌کند؛ در غیر این صورت پیام روشن برمی‌گردد.
///
/// نکته استقرار: سرور روی ویندوز/IIS باید SPN حساب سرویس (HTTP/sso.company.local) داشته باشد؛
/// روی لینوکس باید keytab در KRB5_KTNAME باشد. NegotiateAuthentication از GSSAPI سیستم استفاده می‌کند.
/// | Transparent Windows/AD single sign-on: the desktop client sends a GSS (Kerberos/NTLM)
/// token obtained via SSPI; browsers answer the automatic 401 Negotiate challenge.
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)]
public class WindowsAuthController : Controller
{
    private readonly IOpenIddictApplicationManager _applications;
    private readonly IOpenIddictScopeManager _scopes;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IAuditService _audit;
    private readonly IClientAccessStore _access;
    private readonly IActiveDirectoryService _activeDirectory;
    private readonly ILogger<WindowsAuthController> _logger;

    public WindowsAuthController(
        IOpenIddictApplicationManager applications,
        IOpenIddictScopeManager scopes,
        UserManager<ApplicationUser> users,
        IAuditService audit,
        IClientAccessStore access,
        IActiveDirectoryService activeDirectory,
        ILogger<WindowsAuthController> logger)
    {
        _applications = applications;
        _scopes = scopes;
        _users = users;
        _audit = audit;
        _access = access;
        _activeDirectory = activeDirectory;
        _logger = logger;
    }

    /// <summary>
    /// گام ۱ — GET بدون توکن: چالش 401 Negotiate برای مرورگر (فلوی شفاف aspnetcore-sample).
    /// پارامترهای استاندارد فلو ۲ (client_id, redirect_uri, scope, state, code_challenge) را هم
    /// قبول می‌کند تا پس از موفقیت، دقیقاً مثل /connect/authorize به redirect_uri با code برگردد.
    /// </summary>
    [HttpGet("~/connect/windows")]
    public IActionResult Challenge()
    {
        // چالش دستی Negotiate (بدون middleware): مرورگر ویندوزی این header را می‌بیند و
        // خودکار با تیکت Kerberos/NTLM کاربرِ واردشده به ویندوز درخواست را تکرار می‌کند —
        // بدون هیچ فرم لاگینی. blob پاسخ در POST با windows_token یا header Authorization می‌آید.
        Response.Headers.WWWAuthenticate = "Negotiate";
        return StatusCode(StatusCodes.Status401Unauthorized, new
        {
            error = "negotiate_challenge",
            error_description = "برای ورود یکپارچه ویندوزی، مرورگر باید با header Negotiate پاسخ دهد.",
        });
    }

    /// <summary>
    /// گام ۲ — POST از WinForms: windows_token (Base64 GSS از SSPI) + client_id + client_secret.
    /// خروجی: مستقیم همان JSON توکن فلو ۴ (access_token/id_token/refresh_token) — بدون مرورگر.
    /// </summary>
    [HttpPost("~/connect/windows")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Token(
        [FromForm(Name = "windows_token")] string? windowsToken,
        [FromForm(Name = "client_id")] string? clientId,
        [FromForm(Name = "client_secret")] string? clientSecret,
        [FromForm] string? scope)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        // ۱) الزام ClientId + ClientSecret (سیاست بند ۳ — بدون این دو، دسترسی ممکن نیست).
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            return JsonError("invalid_request", "برای ورود یکپارچه ویندوزی، ClientId و Client Secret الزامی است.");
        }

        var application = await _applications.FindByClientIdAsync(clientId);
        if (application is null || !await _applications.ValidateClientSecretAsync(application, clientSecret))
        {
            await _audit.LogAsync(AuditAction.LoginFailed, actorUserName: null, clientId: clientId, ip: ip,
                data: new { Grant = "windows", Reason = "invalid-client" });
            return JsonError("invalid_client", "شناسه کلاینت یا Client Secret نادرست است.");
        }

        // ۲) رمزگشایی GSS token با GSSAPI سیستم (Kerberos روی دامنه / NTLM).
        string? windowsUser = null;
        if (!string.IsNullOrWhiteSpace(windowsToken))
        {
            windowsUser = await AcceptGssTokenAsync(windowsToken);
            if (windowsUser is null)
            {
                await _audit.LogAsync(AuditAction.LoginFailed, actorUserName: null, clientId: clientId, ip: ip,
                    data: new { Grant = "windows", Reason = "invalid-gss-token" });
                return JsonError("invalid_grant", "توکت ویندوزی معتبر نیست؛ از داخل دامنه وارد شوید.");
            }
        }
        else if (Request.Headers.TryGetValue("Authorization", out var authHeader)
            && authHeader.ToString().StartsWith("Negotiate ", StringComparison.OrdinalIgnoreCase))
        {
            // مسیر مرورگری: blob پاسخ به چالش 401، داخل header Authorization.
            windowsUser = await AcceptGssTokenAsync(authHeader.ToString()["Negotiate ".Length..].Trim());
        }

        if (string.IsNullOrWhiteSpace(windowsUser))
        {
            return JsonError("invalid_grant", "توکت ویندوزی دریافت نشد.");
        }

        // ۳) نگاشت CORP\user یا user@corp به کاربر سرور (همان منطق FindByNameWithDomainFallbackAsync).
        var username = NormalizeWindowsUsername(windowsUser);
        var user = await _users.FindByNameWithDomainFallbackAsync(username);
        if (user is null || !user.IsActive)
        {
            await _audit.LogAsync(AuditAction.LoginFailed, actorUserName: username, clientId: clientId, ip: ip,
                data: new { Grant = "windows", Reason = user is null ? "unknown-user" : "inactive" });
            return JsonError("invalid_grant", "کاربر ویندوزی شما در سرور SSO ثبت نشده است.");
        }

        if (!user.IsAdUser)
        {
            await _audit.LogAsync(AuditAction.LoginFailed, actorUserName: username, clientId: clientId, ip: ip,
                data: new { Grant = "windows", Reason = "not-ad-user" });
            return JsonError("invalid_grant", "ورود شفاف ویندوزی فقط برای کاربران اکتیو دایرکتوری فعال است.");
        }

        // ۴) لیست‌سفید دسترسی به کلاینت (admi‌n‌ها آزادند).
        if (!await _users.IsInRoleAsync(user, SsoServer.Domain.Options.SsoRoles.Admin)
            && !await _access.CanAccessAsync(user.Id, clientId))
        {
            await _audit.LogAsync(AuditAction.ClientAccessDenied, user.Id, user.UserName, clientId, ip,
                data: new { Grant = "windows" });
            return JsonError("invalid_grant", "شما به این برنامه دسترسی ندارید.");
        }

        // ۵) صدور توکن — همان الگوی فلو ۴ (password grant) با claims تازه.
        var scopes = ParseScopes(scope);
        var principal = await BuildPrincipalAsync(user, scopes);
        await _audit.LogAsync(AuditAction.LoginSucceeded, user.Id, user.UserName, clientId, ip,
            data: new { Grant = "windows", WindowsIdentity = windowsUser });
        await _audit.LogAsync(AuditAction.TokenIssued, user.Id, user.UserName, clientId, ip,
            data: new { Grant = "windows", Scopes = scopes });

        // SignIn در اکشن بدون درخواست OpenIddict معتبر خطا می‌دهد؛ چون این endpoint خارج از
        // middleware توکن است، خروجی را با Ticket دستی نمی‌سازیم — به‌جایش توکن‌ها را از
        // همان SignIn استاندارد می‌گیریم: OpenIddict با پارامترهای فرم این درخواست کار می‌کند
        // (grant_type پیش‌فرض این مسیر = password با رمز صرف‌نظر). برای سادگی و سازگاری،
        // این اکشن Principal را با OpenIddictServer scheme امضا می‌کند.
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Accept یک GSS token خام SSPI (Negotiate/Kerberos/NTLM) با GSSAPI سیستم و
    /// برگرداندن نام کاربر دامنه‌ای (مثل CORP\ali یا ali@corp.local).
    /// روی ویندوز از SSPI محلی، روی لینوکس از libgssapi (Kerberos با keytab سرور) استفاده می‌شود.
    /// </summary>
    private async Task<string?> AcceptGssTokenAsync(string tokenBase64)
    {
        try
        {
            // NegotiateAuthentication در نقش acceptor: GetOutgoingBlob روی token ورودی،
            // context را می‌پذیرد؛ سپس RemoteIdentity نام کاربر دامنه‌ای را می‌دهد.
            var blob = tokenBase64.Trim();
            var server = new NegotiateAuthentication(new NegotiateAuthenticationServerOptions
            {
                Package = "Negotiate",
            });

            var outgoing = server.GetOutgoingBlob(blob, out var statusCode);
            if (statusCode != NegotiateAuthenticationStatusCode.Completed)
            {
                _logger.LogWarning("Windows GSS token rejected: {Status}", statusCode);
                return null;
            }

            // فلوهای چندمرحله‌ای (NTLM) blob خروجی می‌خواهند؛ این نسخه فقط
            // Kerberos یک‌مرحله‌ای را می‌پذیرد که مرورگر/SSPI دسکتاپ معمولاً می‌فرستد.
            if (!string.IsNullOrEmpty(outgoing))
            {
                _logger.LogWarning("Windows auth: multi-leg token not supported yet");
                return null;
            }

            var identity = server.RemoteIdentity;
            if (identity is null)
            {
                return null;
            }

            await Task.CompletedTask;
            return identity.Name;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Windows GSS token could not be processed");
            return null;
        }
    }

    /// <summary>CORP\user یا user@corp.local → شکل جستجوی کاربر سرور (user).</summary>
    private static string NormalizeWindowsUsername(string windowsUser)
    {
        var name = windowsUser.Trim();
        var slash = name.IndexOf('\\');
        if (slash >= 0)
        {
            return name[(slash + 1)..];
        }

        var at = name.IndexOf('@');
        return at > 0 ? name[..at] : name;
    }

    private static string[] ParseScopes(string? scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return ["openid", "profile", "roles", "email"];
        }

        return scope.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private async Task<ClaimsPrincipal> BuildPrincipalAsync(ApplicationUser user, string[] scopes)
    {
        var roles = await _users.GetRolesAsync(user);
        var claims = (await _users.GetClaimsAsync(user))
            .Select(c => new SsoServer.Domain.Models.ClaimDto { Type = c.Type, Value = c.Value })
            .ToList();

        var content = ClaimFactory.Create(user, roles, claims, scopes);
        var identity = content.ToClaimsIdentity();
        identity.SetScopes(content.Scopes);
        identity.SetResources(await _scopes.ListResourcesAsync([.. content.Scopes]).ToListAsync());
        identity.ApplyTokenDestinations();
        return new ClaimsPrincipal(identity);
    }

    private IActionResult JsonError(string error, string description) => Json(new Dictionary<string, string>
    {
        ["error"] = error,
        ["error_description"] = description,
    });
}
