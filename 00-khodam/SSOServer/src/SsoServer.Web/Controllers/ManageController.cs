using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SsoServer.Application.Services;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;
using SsoServer.Web.ViewModels.Manage;

namespace SsoServer.Web.Controllers;

[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class ManageController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ISessionService _sessions;
    private readonly ISettingsService _settings;

    public ManageController(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signInManager,
        ISessionService sessions,
        ISettingsService settings)
    {
        _users = users;
        _signInManager = signInManager;
        _sessions = sessions;
        _settings = settings;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _users.GetUserAsync(User) ?? throw new InvalidOperationException("کاربر یافت نشد.");
        ViewBag.Title = "پروفایل کاربری";
        return View(new ProfileViewModel
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty,
            Roles = [.. await _users.GetRolesAsync(user)],
            Claims = [.. (await _users.GetClaimsAsync(user)).Select(c => new ClaimDto { Type = c.Type, Value = c.Value })],
            CreatedAt = user.CreatedAt,
            Sessions = await _sessions.ListOwnAsync(user.Id),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var user = await _users.GetUserAsync(User);
        if (user is null || !ModelState.IsValid)
        {
            TempData["Error"] = "اطلاعات واردشده معتبر نیست.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _users.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(" ", PersianIdentityErrors.Map(result.Errors));
            return RedirectToAction(nameof(Index));
        }

        // Re-sign so the security stamp refreshes and other devices re-authenticate.
        await _signInManager.RefreshSignInAsync(user);
        TempData["Success"] = "رمز عبور با موفقیت تغییر کرد.";
        return RedirectToAction(nameof(Index));
    }

    /// فلو ۷ — صفحه نشست‌های فعال کاربر (پنل مدیریت).
    /// لیست نشست‌ها از SessionService.ListOwnAsync می‌آید (هر نشست = یک authorization فعال).
    [HttpGet]
    public async Task<IActionResult> Sessions()
    {
        var user = await _users.GetUserAsync(User) ?? throw new InvalidOperationException("کاربر یافت نشد.");
        var sessions = await _sessions.ListOwnAsync(user.Id);
        ViewBag.Title = "نشست‌های فعال من";
        return View(sessions);
    }

    /// فلو ۷ — ابطال یک نشست مشخص.
    /// SessionService.RevokeOwnAsync ردیف authorization را باطل می‌کند؛
    /// چون EnableAuthorizationEntryValidation فعال است، access tokenهای آن نشست هم بلافاصله رد می‌شوند.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeSession(string authorizationId)
    {
        var user = await _users.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _sessions.RevokeOwnAsync(user.Id, authorizationId);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "نشست انتخاب‌شده باطل شد."
            : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(Sessions));
    }

    /// فلو ۷ — ابطال همه نشست‌ها در تمام دستگاه‌ها (خروج همه‌جا).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeAllSessions()
    {
        var user = await _users.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction("Login", "Account");
        }

        await _sessions.RevokeAllOwnAsync(user.Id);
        TempData["Success"] = "همه نشست‌های شما در تمام دستگاه‌ها باطل شدند.";
        return RedirectToAction(nameof(Sessions));
    }
}
