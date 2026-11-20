using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Primitives;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SsoServer.Application.Tokens;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;
using SsoServer.Domain.Options;
using SsoServer.Web.Helpers;
using SsoServer.Web.OpenIddict;
using SsoServer.Web.ViewModels.Authorization;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SsoServer.Web.Controllers;

/// <summary>
/// Interactive authorization: login-challenge, consent, sign-in and end-session.
/// The "one login, all apps" behavior: with a valid SSO session cookie this endpoint
/// returns the authorization code without showing any UI.
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)]
public class AuthorizationController : Controller
{
    private readonly IOpenIddictApplicationManager _applications;
    private readonly IOpenIddictAuthorizationManager _authorizations;
    private readonly IOpenIddictScopeManager _scopes;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ISettingsService _settings;
    private readonly IAuditService _audit;
    private readonly IClientAccessStore _access;

    public AuthorizationController(
        IOpenIddictApplicationManager applications,
        IOpenIddictAuthorizationManager authorizations,
        IOpenIddictScopeManager scopes,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> users,
        ISettingsService settings,
        IAuditService audit,
        IClientAccessStore access)
    {
        _applications = applications;
        _authorizations = authorizations;
        _scopes = scopes;
        _signInManager = signInManager;
        _users = users;
        _settings = settings;
        _audit = audit;
        _access = access;
    }

    /// فلو ۳ (PDF فصل ۳) — ورود تعاملی مرورگری (Authorization Code) از اینجا شروع می‌شود.
    /// گام ۱: کلاینت کاربر را با client_id/redirect_uri/scope به این اکشن می‌فرستد.
    /// گام ۲: با کوکی نشست SSO چک می‌شود؛ اگر نشست معتبر باشد هیچ صفحه‌ای نمایش داده نمی‌شود
    ///         (رفتار «یک بار ورود، همه‌جا»)، وگرنه کاربر با Challenge به فرم لاگین هدایت می‌شود.
    /// گام ۳: IsClientAccessAllowedAsync دسترسی کاربر به این کلاینت را چک می‌کند (لیست‌سفید).
    /// گام ۴: در صورت نیاز، صفحه رضایت (Consent) نمایش داده می‌شود (اکشن‌های Accept/Deny).
    /// گام ۵: در نهایت SignInWithFreshPrincipalAsync کد مجوز را صادر و به redirect_uri برمی‌گرداند.
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    [EnableRateLimiting(SsoServer.Web.Services.RateLimiting.RateLimitPolicies.Auth)]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("درخواست OpenID Connect قابل بازیابی نیست.");

        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        // Read unconditionally so the one-shot flag is consumed exactly on the round-trip
        // it was meant for (authorize -> login -> authorize), regardless of short-circuiting.
        var ignoreChallenge = TempData["IgnoreAuthenticationChallenge"] is true;

        if (result is not { Succeeded: true } ||
            ((request.HasPromptValue(PromptValues.Login) || request.MaxAge is 0 ||
             (request.MaxAge is not null && result.Properties?.IssuedUtc is not null &&
              DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value))) &&
            !ignoreChallenge))
        {
            if (request.HasPromptValue(PromptValues.None))
            {
                return ForbidError(Errors.LoginRequired, "کاربر وارد نشده است.");
            }

            TempData["IgnoreAuthenticationChallenge"] = true;
            return Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form : Request.Query),
                },
                IdentityConstants.ApplicationScheme);
        }

        var user = await _users.GetUserAsync(result.Principal!) ??
            throw new InvalidOperationException("اطلاعات کاربر واردشده قابل بازیابی نیست.");

        if (!user.IsActive)
        {
            await _signInManager.SignOutAsync();
            TempData["IgnoreAuthenticationChallenge"] = true;
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Request.PathBase + Request.Path + QueryString.Create(Request.Query),
            }, IdentityConstants.ApplicationScheme);
        }

        var application = await _applications.FindByClientIdAsync(request.ClientId!) ??
            throw new InvalidOperationException("اطلاعات کلاینت درخواست‌کننده یافت نشد.");

        // Per-user client access whitelist: a non-admin user with at least one
        // assignment row can only sign in to the assigned clients.
        if (!await IsClientAccessAllowedAsync(user, request.ClientId!))
        {
            await _audit.LogAsync(AuditAction.ClientAccessDenied, user.Id, user.UserName, request.ClientId,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return ForbidError(Errors.AccessDenied, "شما به این برنامه دسترسی ندارید.");
        }

        var clients = HttpContext.RequestServices.GetRequiredService<IClientRepository>();
        var profile = await clients.FindAsync(request.ClientId!);
        var requireConsent = profile?.RequireConsent ?? _settings.Current.RequireConsentByDefault;

        var authorizations = await _authorizations.FindAsync(
            subject: await _users.GetUserIdAsync(user),
            client: (await _applications.GetIdAsync(application))!,
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        var mustShowConsent = (requireConsent && authorizations.Count == 0) || request.HasPromptValue(PromptValues.Consent);
        if (mustShowConsent)
        {
            if (request.HasPromptValue(PromptValues.None))
            {
                return ForbidError(Errors.ConsentRequired, "تأیید کاربر برای این برنامه الزامی است.");
            }

            return View("Authorize", new AuthorizeViewModel
            {
                ApplicationName = profile?.PersianDisplayName ?? request.ClientId!,
                ApplicationDescription = profile?.Description ?? string.Empty,
                Scope = request.Scope ?? string.Empty,
                Scopes = [.. request.GetScopes()],
            });
        }

        return await SignInWithFreshPrincipalAsync(user, request, application, authorizations);
    }

    /// فلو ۳ — اکشن دکمه «قبول» صفحه رضایت (Consent).
    /// بعد از کلیک کاربر روی دکمه قبول، رضایت دائمی (authorization) ثبت شده و
    /// همان مسیر SignInWithFreshPrincipalAsync کد مجوز را صادر می‌کند.
    /// نکته: فرم رضایت نمی‌تواند محدودیت دسترسی کاربر به کلاینت را دور بزند.
    [Authorize]
    [FormValueRequired("submit.Accept")]
    [HttpPost("~/connect/authorize")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("درخواست OpenID Connect قابل بازیابی نیست.");

        var user = await _users.GetUserAsync(User) ??
            throw new InvalidOperationException("اطلاعات کاربر واردشده قابل بازیابی نیست.");

        var application = await _applications.FindByClientIdAsync(request.ClientId!) ??
            throw new InvalidOperationException("اطلاعات کلاینت درخواست‌کننده یافت نشد.");

        // The consent form must not bypass the per-user client access whitelist.
        if (!await IsClientAccessAllowedAsync(user, request.ClientId!))
        {
            await _audit.LogAsync(AuditAction.ClientAccessDenied, user.Id, user.UserName, request.ClientId,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return ForbidError(Errors.AccessDenied, "شما به این برنامه دسترسی ندارید.");
        }

        var authorizations = await _authorizations.FindAsync(
            subject: await _users.GetUserIdAsync(user),
            client: (await _applications.GetIdAsync(application))!,
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        await _audit.LogAsync(AuditAction.ConsentGranted, user.Id, user.UserName, request.ClientId,
            HttpContext.Connection.RemoteIpAddress?.ToString(), data: new { Scopes = request.GetScopes().ToArray() });

        return await SignInWithFreshPrincipalAsync(user, request, application, authorizations);
    }

    /// فلو ۳ — اکشن دکمه «رد» صفحه رضایت.
    /// با Forbid در اسکیم OpenIddict، خطای استاندارد access_denied به کلاینت برمی‌گردد.
    [Authorize]
    [FormValueRequired("submit.Deny")]
    [HttpPost("~/connect/authorize")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deny()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request?.ClientId is not null)
        {
            var user = await _users.GetUserAsync(User);
            await _audit.LogAsync(AuditAction.ConsentDenied, user?.Id, user?.UserName, request.ClientId,
                HttpContext.Connection.RemoteIpAddress?.ToString());
        }

        return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("~/connect/logout")]
    public IActionResult Logout() => View("Logout");

    /// فلو ۷ — پایان نشست SSO از سمت کلاینت (End-Session).
    /// هم کوکی ورود کاربر و هم نشست OpenIddict بسته می‌شود؛ یعنی خروج از همه برنامه‌ها.
    /// GET مربوطه (Logout) فقط صفحه تأیید خروج را نشان می‌دهد.
    [ActionName(nameof(Logout))]
    [HttpPost("~/connect/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutPost()
    {
        await _signInManager.SignOutAsync();

        var user = await _users.GetUserAsync(User);
        await _audit.LogAsync(AuditAction.Logout, user?.Id, user?.UserName,
            HttpContext.Request.Query.TryGetValue("client_id", out StringValues clientId) ? clientId.ToString() : null,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        return SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            IdentityConstants.ApplicationScheme);
    }

    /// <summary>
    /// Whitelist policy: admins are never restricted; a user with no assignment rows
    /// can access every client; a user with at least one row only the assigned ones.
    /// </summary>
    /// فلو ۳ گام ۳ — سیاست لیست‌سفید دسترسی کاربر به کلاینت‌ها.
    /// مدیران هیچ محدودیتی ندارند؛ کاربر بدون هیچ ردیف تخصیصی به همه کلاینت‌ها دسترسی دارد؛
    /// کاربر دارای حداقل یک تخصیص فقط به همان کلاینت‌های تخصیص‌یافته.
    private async Task<bool> IsClientAccessAllowedAsync(ApplicationUser user, string clientId)
    {
        if (await _users.IsInRoleAsync(user, SsoRoles.Admin))
        {
            return true;
        }

        return await _access.CanAccessAsync(user.Id, clientId);
    }

    /// فلو ۳ گام ۵ — ساخت Principal تازه و صدور کد مجوز.
    /// ClaimFactory نقش‌ها و claims سفارشی را به محتوای توکن تبدیل می‌کند،
    /// authorization-id نشست روی identity تنظیم می‌شود و SignIn در اسکیم OpenIddict
    /// باعث صدور کد مجوز و ریدایرکت به redirect_uri کلاینت می‌شود.
    /// همین متد در اکشن Accept هم بعد از رضایت کاربر فراخوانی می‌شود.
    private async Task<IActionResult> SignInWithFreshPrincipalAsync(
        ApplicationUser user,
        OpenIddictRequest request,
        object application,
        List<object> authorizations)
    {
        var roles = await _users.GetRolesAsync(user);
        var customClaims = (await _users.GetClaimsAsync(user))
            .Select(c => new ClaimDto { Type = c.Type, Value = c.Value })
            .ToList();

        var content = ClaimFactory.Create(user, roles, customClaims, request.GetScopes());
        var identity = content.ToClaimsIdentity();
        identity.SetScopes(content.Scopes);
        identity.SetResources(await _scopes.ListResourcesAsync([.. content.Scopes]).ToListAsync());

        var authorization = authorizations.LastOrDefault();
        authorization ??= await _authorizations.CreateAsync(
            identity: identity,
            subject: user.Id,
            client: (await _applications.GetIdAsync(application))!,
            type: AuthorizationTypes.Permanent,
            scopes: identity.GetScopes());

        identity.SetAuthorizationId(await _authorizations.GetIdAsync(authorization));
        identity.ApplyTokenDestinations();

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private ForbidResult ForbidError(string error, string description) => Forbid(
        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
        properties: new AuthenticationProperties(new Dictionary<string, string?>
        {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description,
        }));
}
