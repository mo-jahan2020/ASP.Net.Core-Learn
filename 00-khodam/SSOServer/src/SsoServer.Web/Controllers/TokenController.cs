using Microsoft.AspNetCore;
using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SsoServer.Application.Services;
using SsoServer.Application.Tokens;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;
using SsoServer.Domain.Options;
using SsoServer.Web.Helpers;
using SsoServer.Web.OpenIddict;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SsoServer.Web.Controllers;

/// <summary>
/// The token endpoint. Authorization code and refresh token grants are also processed here
/// (rebuilt with fresh user claims) alongside the legacy password grant and client credentials.
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)]
public class TokenController : Controller
{
    private readonly IOpenIddictApplicationManager _applications;
    private readonly IOpenIddictScopeManager _scopes;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IAuditService _audit;
    private readonly ISettingsService _settings;
    private readonly IClientAccessStore _access;
    private readonly IActiveDirectoryService _activeDirectory;
    private readonly ILogger<TokenController> _logger;
    private readonly SsoServer.Infrastructure.Security.PasswordEncryptionService _passwordEncryption;

    public TokenController(
        IOpenIddictApplicationManager applications,
        IOpenIddictScopeManager scopes,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> users,
        IAuditService audit,
        ISettingsService settings,
        IClientAccessStore access,
        IActiveDirectoryService activeDirectory,
        ILogger<TokenController> logger,
        SsoServer.Infrastructure.Security.PasswordEncryptionService passwordEncryption)
    {
        _applications = applications;
        _scopes = scopes;
        _signInManager = signInManager;
        _users = users;
        _audit = audit;
        _settings = settings;
        _access = access;
        _activeDirectory = activeDirectory;
        _logger = logger;
        _passwordEncryption = passwordEncryption;
    }

    /// فلو ۴/۵ (PDF فصل ۴ و ۵) — endpoint توکن POST /connect/token؛ قلب صدور همه توکن‌ها.
    /// این اکشن فقط «مسیریاب» است: نوع grant_type را تشخیص می‌دهد و کار را به متد مربوط می‌سپارد:
    ///   password → ExchangePasswordAsync (فلو ۴)
    ///   client_credentials → ExchangeClientCredentialsAsync (فلو ۵)
    ///   authorization_code / refresh_token → ExchangeAuthorizationCodeOrRefreshAsync (فلو ۳ گام ۶-۷ و فلو ۵)
    /// پاسخ نهایی همه مسیرها یک JSON شامل access_token / id_token / refresh_token است.
    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    [EnableRateLimiting(SsoServer.Web.Services.RateLimiting.RateLimitPolicies.Token)]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("درخواست OpenID Connect قابل بازیابی نیست.");

        if (request.IsPasswordGrantType())
        {
            return await ExchangePasswordAsync(request);
        }

        if (request.IsClientCredentialsGrantType())
        {
            return await ExchangeClientCredentialsAsync(request);
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            return await ExchangeAuthorizationCodeOrRefreshAsync(request);
        }

        return ForbidError(Errors.UnsupportedGrantType, "این نوع اعطای توکن پشتیبانی نمی‌شود.");
    }

    /// فلو ۴ — دریافت مستقیم توکن با نام کاربری/رمز (Password Grant).
    /// گام ۳: کاربر با FindByNameWithDomainFallbackAsync پیدا می‌شود (پشتیبانی از CORP\user و user@domain).
    /// گام ۴: احراز هویت — کاربر AD با LDAP (ActiveDirectoryService.AuthenticateAsync)،
    ///         کاربر محلی با CheckPasswordSignInAsync (قفل‌شدن حساب در تلاش ناموفق فعال است).
    /// گام ۵: چک لیست‌سفید دسترسی به کلاینت (CanAccessAsync).
    /// گام ۶: BuildUserPrincipalAsync کلaims تازه می‌سازد و پاسخ JSON توکن صادر می‌شود.
    private async Task<IActionResult> ExchangePasswordAsync(OpenIddictRequest request)
    {
        // بروزرسانی امنیتی ۲۰۲۶-۱۱-۱۹: کلاینت می‌تواند به‌جای password خام، رمز عبور را
        // با کلید عمومی سرور (RSA-OAEP) رمز کرده و در password_cipher بفرستد؛ روی شبکه‌ی
        // بدون HTTPS، رمز عبور هرگز به‌صورت متن خام منتقل نمی‌شود. پارامتر password
        // قدیمی برای سازگاری با کلاینت‌های موجود باقی مانده (در تولید TLS + cipher توصیه است).
        // فیلد password یا خالی است یا فقط جای‌نگه‌دار ("RSA-OAEP-CIPHER") —
        // وقتی password_cipher موجود باشد همیشه همان ملاک است و متن خام هرگز استفاده نمی‌شود.
        var cipher = HttpContext.Request.HasFormContentType
            ? HttpContext.Request.Form["password_cipher"].ToString()
            : HttpContext.Request.Query["password_cipher"].ToString();
        var password = !string.IsNullOrEmpty(cipher)
            ? _passwordEncryption.Decrypt(cipher)
            : request.Password;

        var user = await _users.FindByNameWithDomainFallbackAsync(request.Username ?? string.Empty);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        if (user is null || !user.IsActive)
        {
            await _audit.LogAsync(AuditAction.LoginFailed, actorUserName: request.Username, clientId: request.ClientId, ip: ip,
                data: new { Grant = "password", Reason = user is null ? "unknown-user" : "inactive" });
            return ForbidError(Errors.InvalidGrant, "نام کاربری یا رمز عبور نادرست است.");
        }

        // Active Directory users: validate the password against the domain instead of a local hash.
        if (user.IsAdUser)
        {
            var ad = await _activeDirectory.AuthenticateAsync(request.Username ?? string.Empty, password ?? string.Empty);
            if (!ad.IsSuccess)
            {
                // Only genuinely wrong passwords count toward the local lockout.
                if (ad.Status == AdAuthStatus.InvalidCredentials)
                {
                    await _users.AccessFailedAsync(user);
                }

                await _audit.LogAsync(AuditAction.LoginFailed, user.Id, user.UserName, request.ClientId, ip,
                    data: new { Grant = "password", Auth = "active-directory", Reason = ad.Status.ToString() });
                return ForbidError(Errors.InvalidGrant, ad.Message);
            }

            await _users.ResetAccessFailedCountAsync(user);
            await _users.SyncProfileFromAdAsync(_logger, user, ad);
        }
        else
        {
            var result = await _signInManager.CheckPasswordSignInAsync(user, password ?? string.Empty, lockoutOnFailure: true);
            if (result.IsLockedOut)
            {
                await _audit.LogAsync(AuditAction.Lockout, user.Id, user.UserName, request.ClientId, ip, data: new { Grant = "password" });
                return ForbidError(Errors.InvalidGrant,
                    $"حساب شما به دلیل تلاش‌های ناموفق موقتاً قفل شده است. حدود {_settings.Current.LockoutMinutes} دقیقه دیگر دوباره تلاش کنید.");
            }

            if (!result.Succeeded)
            {
                await _audit.LogAsync(AuditAction.LoginFailed, user.Id, user.UserName, request.ClientId, ip, data: new { Grant = "password" });
                return ForbidError(Errors.InvalidGrant, "نام کاربری یا رمز عبور نادرست است.");
            }
        }

        // Per-user client access whitelist also applies to the password grant:
        // a restricted user must not receive tokens for unassigned clients.
        if (!await _users.IsInRoleAsync(user, SsoRoles.Admin))
        {
            if (request.ClientId is not null && !await _access.CanAccessAsync(user.Id, request.ClientId))
            {
                await _audit.LogAsync(AuditAction.ClientAccessDenied, user.Id, user.UserName, request.ClientId, ip,
                    data: new { Grant = "password" });
                return ForbidError(Errors.InvalidGrant, "شما به این برنامه دسترسی ندارید.");
            }
        }

        var principal = await BuildUserPrincipalAsync(user, request.GetScopes());
        await _audit.LogAsync(AuditAction.LoginSucceeded, user.Id, user.UserName, request.ClientId, ip, data: new { Grant = "password" });
        await _audit.LogAsync(AuditAction.TokenIssued, user.Id, user.UserName, request.ClientId, ip,
            data: new { Grant = "password", Scopes = request.GetScopes().ToArray() });

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// فلو ۵ — توکن ماشین‌به‌ماشین (Client Credentials).
    /// بدون کاربر انسانی: sub همان client_id است؛ فقط کلاینت با secret خودش معرفی می‌شود.
    /// هیچ claims کاربری (نام/ایمیل/نقش) صادر نمی‌شود.
    private async Task<IActionResult> ExchangeClientCredentialsAsync(OpenIddictRequest request)
    {
        var application = await _applications.FindByClientIdAsync(request.ClientId!) ??
            throw new InvalidOperationException("کلاینت درخواست‌کننده یافت نشد.");

        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, request.ClientId);
        identity.SetClaim(Claims.Name, await _applications.GetDisplayNameAsync(application));
        identity.SetScopes(request.GetScopes());
        identity.SetResources(await _scopes.ListResourcesAsync(request.GetScopes()).ToListAsync());
        identity.ApplyTokenDestinations();

        await _audit.LogAsync(AuditAction.TokenIssued, clientId: request.ClientId, ip: HttpContext.Connection.RemoteIpAddress?.ToString(),
            data: new { Grant = "client_credentials", Scopes = request.GetScopes().ToArray() });

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// فلو ۳ گام ۶-۷ و فلو ۵ — تبدیل کد مجوز به توکن و نوسازی توکن (Refresh).
    /// کد/توکن قبلی توسط میان‌افزار OpenIddict اعتبارسنجی می‌شود (کد یک‌بارمصرف است؛
    /// refresh با چرخش سخت — هر توکن فقط یک‌بار).
    /// سپس Principal با claims تازه از دیتابیس بازسازی می‌شود (تغییر نقش/claims در توکن جدید اعمال می‌شود)
    /// و authorization-id نشست حفظ می‌شود تا ابطال نشست، این توکن‌ها را هم باطل کند.
    private async Task<IActionResult> ExchangeAuthorizationCodeOrRefreshAsync(OpenIddictRequest request)
    {
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (result is not { Succeeded: true } || result.Principal is null)
        {
            return ForbidError(Errors.InvalidGrant, "توکن یا کد ارائه‌شده دیگر معتبر نیست.");
        }

        var subject = result.Principal.GetClaim(Claims.Subject);
        var user = subject is null ? null : await _users.FindByIdAsync(subject);
        if (user is null)
        {
            return ForbidError(Errors.InvalidGrant, "کاربر مرتبط با این توکن یافت نشد.");
        }

        if (!user.IsActive || !await _signInManager.CanSignInAsync(user))
        {
            return ForbidError(Errors.InvalidGrant, "این کاربر مجاز به ورود نیست.");
        }

        // Rebuild the principal with FRESH claims (role/custom-claim changes apply on refresh)
        // while preserving the granted scopes and the authorization (session) linkage.
        var principal = await BuildUserPrincipalAsync(user, result.Principal.GetScopes());
        var authorizationId = result.Principal.GetAuthorizationId();
        if (!string.IsNullOrEmpty(authorizationId))
        {
            ((ClaimsIdentity)principal.Identity!).SetAuthorizationId(authorizationId);
        }

        await _audit.LogAsync(
            request.IsRefreshTokenGrantType() ? AuditAction.TokenRefreshed : AuditAction.TokenIssued,
            user.Id, user.UserName, request.ClientId, HttpContext.Connection.RemoteIpAddress?.ToString(),
            data: new { Grant = request.GrantType, Scopes = request.GetScopes().ToArray() });

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// گام ۶ مشترک فلوهای توکن — ساخت Principal با claims تازه.
    /// ClaimFactory.Create محتوای توکن (sub/name/roles/claims سفارشی + scopes مجاز) را می‌سازد،
    /// ToClaimsIdentity آن را به ClaimsIdentity تبدیل می‌کند و ApplyTokenDestinations
    /// تعیین می‌کند هر claim در کدام توکن (access/id/userinfo) قرار بگیرد.
    private async Task<ClaimsPrincipal> BuildUserPrincipalAsync(ApplicationUser user, IEnumerable<string> scopes)
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

    private ForbidResult ForbidError(string error, string description) => Forbid(
        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
        properties: new AuthenticationProperties(new Dictionary<string, string?>
        {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description,
        }));
}
