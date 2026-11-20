using Microsoft.AspNetCore.Mvc;
using SsoServer.Domain.Models;
using SsoServer.Domain.Options;
using SsoServer.Web.ViewModels.Admin;

namespace SsoServer.Web.Controllers;

// User management actions of the admin console (part of AdminController).
public partial class AdminController
{
    // ---------------- Users ----------------

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        ViewBag.Title = "مدیریت کاربران";
        return View(await _userAdmin.ListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> UserCreate()
    {
        ViewBag.Title = "ایجاد کاربر جدید";
        await PopulateClientAccessViewBagAsync();
        return View(new CreateUserViewModel { IsActive = true, Role = SsoRoles.User });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserCreate(CreateUserViewModel model)
    {
        var actor = await _users.GetUserAsync(User);
        ValidateCreateUserPassword(model);

        if (!ModelState.IsValid || actor is null)
        {
            await PopulateClientAccessViewBagAsync();
            return View(model);
        }

        var result = await _userAdmin.CreateAsync(new CreateUserDto
        {
            UserName = model.UserName,
            DisplayName = model.DisplayName,
            Email = model.Email,
            Password = model.Password,
            Role = model.Role,
            IsActive = model.IsActive,
            IsAdUser = model.IsAdUser,
        }, actor.Id);

        if (result.Succeeded && result.Value is not null)
        {
            var assignmentResult = await _clientAssignments.SyncUserAssignmentsAsync(
                result.Value.Id, model.SelectedClientIds, actor.Id);
            TempData[assignmentResult.Succeeded ? "Success" : "Error"] = assignmentResult.Succeeded
                ? $"کاربر «{model.UserName}» ایجاد شد."
                : $"کاربر «{model.UserName}» ایجاد شد؛ اما ذخیره دسترسی برنامه‌ها با خطا مواجه شد: {string.Join(" ", assignmentResult.Errors)}";
            return RedirectToAction(nameof(Users));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        await PopulateClientAccessViewBagAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> UserEdit(string id)
    {
        var user = await _userAdmin.GetAsync(id);
        if (user is null)
        {
            TempData["Error"] = "کاربر یافت نشد.";
            return RedirectToAction(nameof(Users));
        }

        ViewBag.Title = $"ویرایش کاربر {user.UserName}";
        await PopulateClientAccessViewBagAsync();
        return View(new EditUserViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Roles.FirstOrDefault() ?? SsoRoles.User,
            IsActive = user.IsActive,
            IsAdUser = user.IsAdUser,
            ClaimsText = string.Join("\n", user.Claims.Select(c => $"{c.Type}={c.Value}")),
            SelectedClientIds = [.. await _clientAssignments.ListClientIdsByUserAsync(user.Id)],
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserEdit(EditUserViewModel model)
    {
        var actor = await _users.GetUserAsync(User);
        if (!ModelState.IsValid || actor is null)
        {
            await PopulateClientAccessViewBagAsync();
            return View(model);
        }

        var result = await _userAdmin.UpdateAsync(model.Id, new UpdateUserDto
        {
            DisplayName = model.DisplayName,
            Email = model.Email,
            Role = model.Role,
            IsActive = model.IsActive,
            IsAdUser = model.IsAdUser,
        }, actor.Id);

        if (result.Succeeded)
        {
            var assignmentResult = await _clientAssignments.SyncUserAssignmentsAsync(
                model.Id, model.SelectedClientIds, actor.Id);
            TempData[assignmentResult.Succeeded ? "Success" : "Error"] = assignmentResult.Succeeded
                ? "اطلاعات کاربر و دسترسی برنامه‌های او ذخیره شد."
                : $"اطلاعات کاربر ذخیره شد؛ اما ذخیره دسترسی برنامه‌ها با خطا مواجه شد: {string.Join(" ", assignmentResult.Errors)}";
            return RedirectToAction(nameof(Users));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        await PopulateClientAccessViewBagAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserSetPassword(string id, string newPassword)
    {
        var actor = await _users.GetUserAsync(User);
        var result = actor is null
            ? Domain.Models.ServiceResult.Fail("خطا در عملیات.")
            : await _userAdmin.SetPasswordAsync(id, newPassword, actor.Id);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "رمز عبور کاربر بازنشانی شد."
            : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(UserEdit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserSetClaims(string id, string claimsText)
    {
        var actor = await _users.GetUserAsync(User);
        var claims = ParseClaims(claimsText);
        var result = actor is null
            ? Domain.Models.ServiceResult.Fail("خطا در عملیات.")
            : await _userAdmin.SetClaimsAsync(id, claims, actor.Id);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "کلیم‌های سفارشی کاربر ذخیره شد."
            : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(UserEdit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserSetStatus(string id, bool isActive)
    {
        var actor = await _users.GetUserAsync(User);
        var result = actor is null
            ? Domain.Models.ServiceResult.Fail("خطا در عملیات.")
            : await _userAdmin.SetStatusAsync(id, isActive, actor.Id);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? (isActive ? "کاربر فعال شد." : "کاربر غیرفعال و نشست‌هایش باطل شدند.")
            : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserDelete(string id)
    {
        var actor = await _users.GetUserAsync(User);
        var result = actor is null
            ? Domain.Models.ServiceResult.Fail("خطا در عملیات.")
            : await _userAdmin.DeleteAsync(id, actor.Id);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "کاربر حذف شد."
            : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> UserSessions(string id)
    {
        var user = await _userAdmin.GetAsync(id);
        if (user is null)
        {
            TempData["Error"] = "کاربر یافت نشد.";
            return RedirectToAction(nameof(Users));
        }

        ViewBag.Title = $"نشست‌های کاربر {user.UserName}";
        ViewBag.TargetUser = user;
        return View(await _sessions.ListForUserAsync(id));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserRevokeSession(string id, string authorizationId)
    {
        var actor = await _users.GetUserAsync(User);
        var result = actor is null
            ? Domain.Models.ServiceResult.Fail("خطا در عملیات.")
            : await _sessions.RevokeForUserAsync(id, authorizationId, actor.Id);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "نشست باطل شد."
            : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(UserSessions), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserRevokeAllSessions(string id)
    {
        var actor = await _users.GetUserAsync(User);
        var result = actor is null
            ? Domain.Models.ServiceResult.Fail("خطا در عملیات.")
            : await _sessions.RevokeAllForUserAsync(id, actor.Id);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "همه نشست‌های این کاربر باطل شدند."
            : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(UserSessions), new { id });
    }

    /// <summary>Loads every client for the user-form access checkboxes (UserCreate/UserEdit).</summary>
    private async Task PopulateClientAccessViewBagAsync()
        => ViewBag.AllClients = await _clientAdmin.ListAsync();

    /// <summary>
    /// Local (non-AD) users must have a password; Active Directory users must not have one
    /// (their password lives in the domain), so the field is validated conditionally.
    /// </summary>
    private void ValidateCreateUserPassword(CreateUserViewModel model)
    {
        if (model.IsAdUser)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(CreateUserViewModel.Password), "رمز عبور الزامی است.");
        }
        else if (model.Password.Length < 8)
        {
            ModelState.AddModelError(nameof(CreateUserViewModel.Password), "رمز عبور باید حداقل ۸ کاراکتر باشد.");
        }
    }
}
