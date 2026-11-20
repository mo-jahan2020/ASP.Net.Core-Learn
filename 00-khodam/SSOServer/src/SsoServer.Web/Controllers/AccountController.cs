using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;
using SsoServer.Infrastructure.ActiveDirectory;
using SsoServer.Web.Helpers;
using SsoServer.Web.ViewModels.Account;

namespace SsoServer.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IAuditService _audit;
    private readonly ISettingsService _settings;
    private readonly IActiveDirectoryService _activeDirectory;
    private readonly ActiveDirectorySettings _adSettings;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> users,
        IAuditService audit,
        ISettingsService settings,
        IActiveDirectoryService activeDirectory,
        IOptions<ActiveDirectorySettings> adSettings,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _users = users;
        _audit = audit;
        _settings = settings;
        _activeDirectory = activeDirectory;
        _adSettings = adSettings.Value;
        _logger = logger;
    }

    /// فلو ۲ (PDF فصل ۲) — GET فرم لاگین پنل مدیریت.
    /// اگر کاربر قبلاً وارد شده باشد مستقیماً به returnUrl (یا /Manage) می‌رود.
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(Url.SafeLocal(returnUrl, "/Manage"));
        }

        var settings = await _settings.GetEffectiveAsync();

        // A subtle hint for domain users once the Active Directory integration is fully configured.
        ViewBag.ShowAdHint = _adSettings.GetConfigurationError() is null;

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl,
        });
    }

    /// فلو ۲ — POST فرم لاگین؛ نقطه شروع واقعی ورود.
    /// کاربر AD → LoginViaActiveDirectoryAsync (اعتبارسنجی رمز با LDAP در دامنه).
    /// کاربر محلی → PasswordSignInAsync (مقایسه hash + قفل‌شدن).
    /// ورود موفق → کوکی SsoServer.Session صادر و View واسط LoginRedirect
    /// با meta-refresh برمی‌گردد (رفع فریز مرورگر در زنجیره ریدایرکت‌ها).
    /// returnUrl همان آدرس /connect/authorize است که کاربر را به فلو ۳ برمی‌گرداند.
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(SsoServer.Web.Services.RateLimiting.RateLimitPolicies.Auth)]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        model.ReturnUrl = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var user = await _users.FindByNameWithDomainFallbackAsync(model.Username);

        if (user is not null && !user.IsActive)
        {
            await _audit.LogAsync(AuditAction.LoginFailed, user.Id, user.UserName, ip: ip, data: new { Reason = "inactive" });
            ModelState.AddModelError(string.Empty, "حساب کاربری شما غیرفعال شده است. با مدیر سامانه تماس بگیرید.");
            return View(model);
        }

        // Active Directory users: the entered password is validated by the domain (LDAP bind),
        // never against a local hash (they have none). Distinct Persian messages cover
        // wrong password / disabled / locked / expired / unreachable directory cases.
        if (user is not null && user.IsAdUser)
        {
            return await LoginViaActiveDirectoryAsync(user, model, returnUrl, ip);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Username, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            await _audit.LogAsync(AuditAction.LoginSucceeded, user?.Id, model.Username, ip: ip, data: new { Via = "ui" });
            _logger.LogInformation("UI login succeeded for {Username}", model.Username);
            // به‌جای 302 مستقیم، صفحه‌ی واسط با redirect سمت کلاینت برمی‌گردانیم:
            // برخی مرورگرها زنجیره‌ی POST → 302 → 302 → origin دیگر را کامل دنبال
            // نمی‌کنند و روی همین صفحه «فریز» می‌شوند تا F5. با GET خالصِ سمت
            // کلاینت، زنجیره‌ی authorize برای همه‌ی مرورگرها کامل می‌شود.
            return View("LoginRedirect", Url.SafeLocal(returnUrl, "/Manage"));
        }

        if (result.IsLockedOut)
        {
            await _audit.LogAsync(AuditAction.Lockout, user?.Id, model.Username, ip: ip, data: new { Via = "ui" });
            ModelState.AddModelError(string.Empty,
                $"حساب شما به دلیل تلاش‌های ناموفق موقتاً قفل شده است. حدود {_settings.Current.LockoutMinutes} دقیقه دیگر دوباره تلاش کنید.");
            return View(model);
        }

        await _audit.LogAsync(AuditAction.LoginFailed, user?.Id, model.Username, ip: ip, data: new { Via = "ui" });
        ModelState.AddModelError(string.Empty, "نام کاربری یا رمز عبور نادرست است.");
        return View(model);
    }

    /// <summary>Password-less local flow for users flagged as Active Directory accounts.</summary>
    /// فلو ۲ — مسیر ورود بدون-رمزِمحلی برای کاربران Active Directory.
    /// رمز با bind به LDAP در دامنه اعتبارسنجی می‌شود؛ فقط رمز اشتباه شمارنده قفل را بالا می‌برد
    /// (خطای زیرساخت مثل در دسترس نبودن دامنه نباید حساب را قفل کند).
    /// پروفایل (نام/ایمیل) بعد از ورود موفق از AD همگام‌سازی می‌شود.
    private async Task<IActionResult> LoginViaActiveDirectoryAsync(
        ApplicationUser user, LoginViewModel model, string? returnUrl, string? ip)
    {
        if (await _users.IsLockedOutAsync(user))
        {
            await _audit.LogAsync(AuditAction.Lockout, user.Id, user.UserName, ip: ip, data: new { Via = "ui", Auth = "active-directory" });
            ModelState.AddModelError(string.Empty,
                $"حساب شما به دلیل تلاش‌های ناموفق موقتاً قفل شده است. حدود {_settings.Current.LockoutMinutes} دقیقه دیگر دوباره تلاش کنید.");
            return View(model);
        }

        var ad = await _activeDirectory.AuthenticateAsync(model.Username, model.Password);
        if (ad.IsSuccess)
        {
            await _users.ResetAccessFailedCountAsync(user);
            await _users.SyncProfileFromAdAsync(_logger, user, ad);
            await _signInManager.SignInAsync(user, model.RememberMe);

            await _audit.LogAsync(AuditAction.LoginSucceeded, user.Id, user.UserName, ip: ip,
                data: new { Via = "ui", Auth = "active-directory" });
            _logger.LogInformation("UI login via Active Directory succeeded for {Username}", model.Username);
            // همان اصلاح فریزِ مرورگر: redirect سمت کلاینت به‌جای 302 بعد از POST.
            return View("LoginRedirect", Url.SafeLocal(returnUrl, "/Manage"));
        }

        // Only genuinely wrong passwords count toward the local lockout; infrastructure
        // problems (unreachable directory) must not lock the account.
        if (ad.Status == AdAuthStatus.InvalidCredentials)
        {
            await _users.AccessFailedAsync(user);
        }

        await _audit.LogAsync(AuditAction.LoginFailed, user.Id, user.UserName, ip: ip,
            data: new { Via = "ui", Auth = "active-directory", Reason = ad.Status.ToString() });
        _logger.LogWarning("Active Directory login failed for {Username}: {Status}", model.Username, ad.Status);

        ModelState.AddModelError(string.Empty, ad.Message);
        return View(model);
    }

    /// فلو ۷ — خروج از پنل مدیریت.
    /// کوکی نشست پاک می‌شود؛ توکن‌های صادرشده قبلی تا انقضایشان معتبر می‌مانند
    /// (خروج کامل SSO با ابطال نشست: AuthorizationController.LogoutPost یا فصل ۷).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var user = await _users.GetUserAsync(User);
        await _signInManager.SignOutAsync();
        await _audit.LogAsync(AuditAction.Logout, user?.Id, user?.UserName, ip: HttpContext.Connection.RemoteIpAddress?.ToString());
        return RedirectToAction(nameof(LoggedOut));
    }

    [HttpGet]
    public IActionResult LoggedOut() => View();

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
