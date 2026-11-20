using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SsoServer.Application.Services;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;
using SsoServer.Domain.Options;
using SsoServer.Infrastructure.ActiveDirectory;
using SsoServer.Web.OpenIddict;
using SsoServer.Web.ViewModels.Admin;

namespace SsoServer.Web.Controllers;

/// <summary>Admin console: dashboard, users, clients, settings, audit and health.</summary>
[Authorize(Roles = SsoRoles.Admin)]
[ApiExplorerSettings(IgnoreApi = true)]
public partial class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IUserAdminService _userAdmin;
    private readonly ClientAdminService _clientAdmin;
    private readonly ClientAssignmentService _clientAssignments;
    private readonly ISessionService _sessions;
    private readonly ISettingsService _settings;
    private readonly IAuditService _audit;
    private readonly ISigningCertificateProvider _signing;
    private readonly TokenPruningStatus _pruning;
    private readonly ActiveDirectorySettings _adSettings;
    private readonly ILdapConnectionFactory _ldapConnections;

    public AdminController(
        UserManager<ApplicationUser> users,
        IUserAdminService userAdmin,
        ClientAdminService clientAdmin,
        ClientAssignmentService clientAssignments,
        ISessionService sessions,
        ISettingsService settings,
        IAuditService audit,
        ISigningCertificateProvider signing,
        TokenPruningStatus pruning,
        IOptions<ActiveDirectorySettings> adSettings,
        ILdapConnectionFactory ldapConnections)
    {
        _users = users;
        _userAdmin = userAdmin;
        _clientAdmin = clientAdmin;
        _clientAssignments = clientAssignments;
        _sessions = sessions;
        _settings = settings;
        _audit = audit;
        _signing = signing;
        _pruning = pruning;
        _adSettings = adSettings.Value;
        _ldapConnections = ldapConnections;
    }

    // ---------------- Dashboard ----------------

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userList = await _userAdmin.ListAsync();
        var clients = await _clientAdmin.ListAsync();
        var audit = await _audit.QueryAsync(0, 10);
        ViewBag.Title = "داشبورد مدیریت";
        return View("Dashboard", new DashboardViewModel
        {
            UserCount = userList.Count,
            ActiveUserCount = userList.Count(u => u.IsActive),
            AdminCount = userList.Count(u => u.Roles.Contains(SsoRoles.Admin)),
            ClientCount = clients.Count,
            EnabledClientCount = clients.Count(c => c.Enabled),
            RecentAudit = audit,
        });
    }

    // ---------------- Settings ----------------

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        ViewBag.Title = "تنظیمات سامانه";
        return View(await _settings.GetEffectiveAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(UpdateSettingsViewModel model)
    {
        var actor = await _users.GetUserAsync(User);
        if (!ModelState.IsValid || actor is null)
        {
            return View((object)(await _settings.GetEffectiveAsync()));
        }

        var result = await _settings.UpdateAsync(new UpdateSettingsDto
        {
            CorsOrigins = model.CorsOrigins,
            RequireConsentByDefault = model.RequireConsentByDefault,
            MaxFailedLoginAttempts = model.MaxFailedLoginAttempts,
            LockoutMinutes = model.LockoutMinutes,
            TokenRateLimitPerMinute = model.TokenRateLimitPerMinute,
            SessionCookieSliding = model.SessionCookieSliding,
            LoginTitle = model.LoginTitle,
            LoginSubtitle = model.LoginSubtitle,
        }, actor.Id);

        if (result.Succeeded)
        {
            TempData["Success"] = "تنظیمات ذخیره و بلافاصله اعمال شد (مقادیر علامت‌دار «نیازمند ری‌استارت» بعد از راه‌اندازی مجدد اعمال می‌شوند).";
            return RedirectToAction(nameof(Settings));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View((object)(await _settings.GetEffectiveAsync()));
    }

    // ---------------- Audit ----------------

    /// <summary>
    /// گزارش رویدادهای امنیتی با فیلتر نوع رویداد و «نام کاربر».
    /// فیلتر userName روی ActorUserName (نام کاربری) اعمال می‌شود؛ اگر کاربری با آن نام
    /// یافت شود رویدادهای او و در غیر این صورت هیچ نتیجه‌ای نمایش داده می‌شود.
    /// sortKey/sortDesc برای مرتب‌سازی سمت کلاینت (کلیک روی سرستون‌ها) استفاده می‌شوند.
    /// نکته: پارامتر «eventType» است نه «action»، چون «action» با route-value هم‌نام اکشن
    /// در MVC تداخل می‌کند و مقدار query را نمی‌بیند.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Audit(int page = 1, AuditAction? eventType = null, string? userName = null, string? sortKey = null, bool sortDesc = false)
    {
        const int pageSize = 50;
        page = Math.Max(1, page);

        // فیلتر نام کاربر: نام کاربری → شناسه کاربر (برای تطبیق با ActorUserId ثبت‌شده در رویدادها)
        string? actorUserId = null;
        if (!string.IsNullOrWhiteSpace(userName))
        {
            var user = await _users.FindByNameAsync(userName.Trim());
            actorUserId = user?.Id;
        }

        var entries = await _audit.QueryAsync((page - 1) * pageSize, pageSize, eventType, actorUserId);
        entries = ApplyAuditSort(entries, sortKey, sortDesc);

        ViewBag.Title = "گزارش رویدادهای امنیتی";
        ViewBag.Page = page;
        ViewBag.Action = eventType;
        ViewBag.UserName = userName;
        ViewBag.SortKey = sortKey;
        ViewBag.SortDesc = sortDesc;
        return View(entries);
    }

    // ---------------- مدیریت نشست‌های فعال (فقط ادمین) ----------------

    /// <summary>
    /// صفحه «مدیریت نشست‌های فعال»: تمام نشست‌های فعال سامانه در یک جدول
    /// با فیلتر نام کاربری و کلاینت. انتخاب هر سطر جزئیات کامل نشست را باز می‌کند.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> AllSessions(string? userName = null, string? clientId = null, string? selected = null)
    {
        var sessions = await _sessions.ListAllAsync();

        // فیلتر نام کاربری (تطبیق دقیق نام کاربری صاحب نشست)
        if (!string.IsNullOrWhiteSpace(userName))
        {
            var filter = userName.Trim();
            sessions = [.. sessions.Where(s => string.Equals(s.UserName, filter, StringComparison.OrdinalIgnoreCase))];
        }

        // فیلتر کلاینت (تطبیق روی ClientId)
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            var filter = clientId.Trim();
            sessions = [.. sessions.Where(s => s.ClientId.Contains(filter, StringComparison.OrdinalIgnoreCase))];
        }

        // نشست انتخاب‌شده برای نمایش جزئیات (selected = AuthorizationId)
        ActiveSessionDto? selectedSession = null;
        if (!string.IsNullOrWhiteSpace(selected))
        {
            selectedSession = sessions.FirstOrDefault(s => s.AuthorizationId == selected)
                ?? sessions.Concat(await _sessions.ListAllAsync()).FirstOrDefault(s => s.AuthorizationId == selected);
        }

        ViewBag.Title = "مدیریت نشست‌های فعال";
        ViewBag.UserName = userName;
        ViewBag.ClientId = clientId;
        ViewBag.Selected = selected;
        ViewBag.SelectedSession = selectedSession;

        // آمار خلاصه بالای جدول
        ViewBag.TotalCount = sessions.Count;
        ViewBag.TotalUsers = sessions.Select(s => s.Subject).Distinct().Count();
        ViewBag.TotalClients = sessions.Select(s => s.ClientId).Distinct().Count();

        return View("AllSessions", sessions);
    }

    /// <summary>خاتمه نشست انتخاب‌شده از صفحه «مدیریت نشست‌های فعال» (فقط ادمین).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeAnySession(string authorizationId, string? userName = null, string? clientId = null)
    {
        var actor = await _users.GetUserAsync(User);
        var result = actor is null
            ? Domain.Models.ServiceResult.Fail("خطا در عملیات.")
            : await _sessions.RevokeAnyAsync(authorizationId, actor.Id);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "نشست انتخاب‌شده با موفقیت خاتمه یافت."
            : string.Join(" ", result.Errors);

        // بازگشت به همان لیست با همان فیلترها
        return RedirectToAction(nameof(AllSessions), new { userName, clientId });
    }

    /// <summary>مرتب‌سازی درون‌صفحه‌ای رویدادها بر اساس کلید انتخاب‌شده (کلیک روی سرستون گرید).</summary>
    internal static IReadOnlyList<SsoServer.Domain.Entities.AuditLog> ApplyAuditSort(
        IReadOnlyList<SsoServer.Domain.Entities.AuditLog> entries, string? sortKey, bool sortDesc)
    {
        IEnumerable<SsoServer.Domain.Entities.AuditLog> sorted = sortKey switch
        {
            "id" => entries.OrderBy(e => e.Id),
            "action" => entries.OrderBy(e => e.Action).ThenBy(e => e.Id),
            "user" => entries.OrderBy(e => e.ActorUserName ?? e.ActorUserId ?? string.Empty).ThenBy(e => e.Id),
            "client" => entries.OrderBy(e => e.ClientId ?? string.Empty).ThenBy(e => e.Id),
            "ip" => entries.OrderBy(e => e.Ip ?? string.Empty).ThenBy(e => e.Id),
            "time" => entries.OrderBy(e => e.CreatedAt).ThenBy(e => e.Id),
            _ => entries.OrderByDescending(e => e.Id),
        };

        // پیش‌فرض (بدون sortKey) همیشه جدیدترین رویدادها اول است؛ برای بقیه کلیدها
        // جهت با sortDesc برمی‌گردد.
        if (sortDesc && sortKey is not null)
        {
            sorted = sorted.Reverse();
        }

        return [.. sorted];
    }

    internal static List<ClaimDto> ParseClaims(string? text)
    {
        var claims = new List<ClaimDto>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return claims;
        }

        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            claims.Add(new ClaimDto
            {
                Type = line[..separator].Trim(),
                Value = line[(separator + 1)..].Trim(),
            });
        }

        return claims;
    }

    internal static string[] ParseLines(string? text)
        => string.IsNullOrWhiteSpace(text)
            ? []
            : text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
