using Microsoft.AspNetCore.Mvc;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Models;
using SsoServer.Domain.Options;
using SsoServer.Web.ViewModels.Admin;

namespace SsoServer.Web.Controllers;

// Client (application) management actions of the admin console (part of AdminController).
public partial class AdminController
{
    // ---------------- Clients ----------------

    [HttpGet]
    public async Task<IActionResult> Clients()
    {
        ViewBag.Title = "مدیریت کلاینت‌ها";
        return View(await _clientAdmin.ListAsync());
    }

    [HttpGet]
    public IActionResult ClientCreate()
    {
        ViewBag.Title = "ثبت کلاینت جدید";
        return View(new SaveClientViewModel
        {
            Kind = ClientKind.Confidential,
            Flows = ClientFlow.AuthorizationCode | ClientFlow.RefreshToken,
            Scopes = [.. SsoScopes.All],
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClientCreate(SaveClientViewModel model)
    {
        var actor = await _users.GetUserAsync(User);
        model.NormalizeSelections();
        if (!ModelState.IsValid || actor is null)
        {
            return View(model);
        }

        var dto = ToDto(model);
        var result = await _clientAdmin.CreateAsync(dto, model.ClientSecret, actor.Id);
        if (result.Succeeded && result.Value is not null)
        {
            TempData["Success"] = string.IsNullOrEmpty(result.Value.ClientSecret)
                ? $"کلاینت «{dto.ClientId}» ثبت شد."
                : $"کلاینت «{dto.ClientId}» ثبت شد. Client Secret (فقط همین بار نمایش داده می‌شود): <code dir=\"ltr\">{result.Value.ClientSecret}</code>";
            return RedirectToAction(nameof(Clients));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ClientEdit(string id)
    {
        var client = await _clientAdmin.GetAsync(id);
        if (client is null)
        {
            TempData["Error"] = "کلاینت یافت نشد.";
            return RedirectToAction(nameof(Clients));
        }

        ViewBag.Title = $"ویرایش کلاینت {client.ClientId}";
        await PopulateClientAssignmentViewBagAsync(client.ClientId);
        return View(SaveClientViewModel.FromDto(client));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClientEdit(SaveClientViewModel model)
    {
        var actor = await _users.GetUserAsync(User);
        model.NormalizeSelections();
        if (!ModelState.IsValid || actor is null)
        {
            await PopulateClientAssignmentViewBagAsync(model.ClientId);
            return View(model);
        }

        var result = await _clientAdmin.UpdateAsync(ToDto(model), actor.Id);
        if (result.Succeeded)
        {
            TempData["Success"] = "تنظیمات کلاینت ذخیره شد.";
            return RedirectToAction(nameof(Clients));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        await PopulateClientAssignmentViewBagAsync(model.ClientId);
        return View(model);
    }

    /// <summary>Adds one user to the client's access whitelist (from the ClientEdit page).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClientAssignUser(string id, string? userId)
    {
        var actor = await _users.GetUserAsync(User);
        ServiceResult result;
        if (string.IsNullOrWhiteSpace(userId))
        {
            result = ServiceResult.Fail("ابتدا یک کاربر برای افزودن انتخاب کنید.");
        }
        else if (actor is null)
        {
            result = ServiceResult.Fail("خطا در عملیات.");
        }
        else
        {
            result = await _clientAssignments.AssignAsync(id, userId, actor.Id);
        }

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "کاربر به کاربران مجاز این کلاینت اضافه شد."
            : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(ClientEdit), new { id });
    }

    /// <summary>Removes one user from the client's access whitelist (from the ClientEdit page).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClientUnassignUser(string id, string userId)
    {
        var actor = await _users.GetUserAsync(User);
        var result = actor is null
            ? ServiceResult.Fail("خطا در عملیات.")
            : await _clientAssignments.UnassignAsync(id, userId, actor.Id);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "دسترسی کاربر به این کلاینت حذف شد."
            : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(ClientEdit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClientRegenerateSecret(string id)
    {
        var actor = await _users.GetUserAsync(User);
        var result = actor is null
            ? Domain.Models.ServiceResult<NewClientSecretDto>.Fail("خطا در عملیات.")
            : await _clientAdmin.RegenerateSecretAsync(id, actor.Id);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? $"Client Secret جدید: <code dir=\"ltr\">{result.Value!.ClientSecret}</code> — فقط همین بار نمایش داده می‌شود."
            : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(ClientEdit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClientSetStatus(string id, bool enabled)
    {
        var actor = await _users.GetUserAsync(User);
        var result = actor is null
            ? Domain.Models.ServiceResult.Fail("خطا در عملیات.")
            : await _clientAdmin.SetStatusAsync(id, enabled, actor.Id);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? (enabled ? "کلاینت فعال شد." : "کلاینت غیرفعال شد؛ همه درخواست‌های آن رد می‌شوند.")
            : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(Clients));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClientDelete(string id)
    {
        var actor = await _users.GetUserAsync(User);
        var result = actor is null
            ? Domain.Models.ServiceResult.Fail("خطا در عملیات.")
            : await _clientAdmin.DeleteAsync(id, actor.Id);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "کلاینت حذف شد."
            : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(Clients));
    }

    /// <summary>Loads the assigned + assignable users for the ClientEdit whitelist card.</summary>
    private async Task PopulateClientAssignmentViewBagAsync(string clientId)
    {
        var assignments = await _clientAssignments.ListByClientAsync(clientId);
        var users = await _userAdmin.ListAsync();
        var byId = users.ToDictionary(u => u.Id);

        var assigned = new List<ClientAssignmentRowViewModel>();
        foreach (var assignment in assignments.OrderBy(a => a.AssignedAt))
        {
            byId.TryGetValue(assignment.UserId, out var user);
            assigned.Add(new ClientAssignmentRowViewModel
            {
                UserId = assignment.UserId,
                UserName = user?.UserName ?? "(کاربر حذف‌شده)",
                DisplayName = user?.DisplayName ?? "-",
                IsAdmin = user?.Roles.Contains(SsoRoles.Admin) ?? false,
                AssignedAt = assignment.AssignedAt,
            });
        }

        ViewBag.AssignedUsers = assigned;
        ViewBag.AssignableUsers = users
            .Where(u => u.IsActive
                && !assigned.Any(a => a.UserId == u.Id)
                && !u.Roles.Contains(SsoRoles.Admin))
            .OrderBy(u => u.UserName, StringComparer.Ordinal)
            .ToList();
    }

    private static SaveClientDto ToDto(SaveClientViewModel model) => new()
    {
        ClientId = model.ClientId.Trim(),
        DisplayName = model.DisplayName,
        PersianDisplayName = model.PersianDisplayName,
        Description = model.Description,
        Owner = model.Owner,
        Kind = model.Kind,
        Flows = model.Flows,
        Scopes = model.Scopes ?? [],
        RedirectUris = ParseLines(model.RedirectUris),
        PostLogoutRedirectUris = ParseLines(model.PostLogoutRedirectUris),
        RequireConsent = model.RequireConsent,
        Enabled = model.Enabled,
    };
}
