using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SsoServer.Application.Services;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Models;
using SsoServer.Web.Controllers.Api.Requests;
using SsoServer.Web.Helpers;

namespace SsoServer.Web.Controllers.Api;

/// <summary>مدیریت کلاینت‌ها/برنامه‌های متصل (فقط مدیر): ساخت، ویرایش، رمز، وضعیت و تخصیص کاربران.</summary>
[Authorize(Policy = SsoPolicies.Admin)]
public class ClientsController : SsoApiController
{
    private readonly ClientAdminService _clients;
    private readonly ClientAssignmentService _assignments;
    private readonly UserManager<ApplicationUser> _userLookup;

    public ClientsController(ClientAdminService clients, ClientAssignmentService assignments, UserManager<ApplicationUser> userLookup)
    {
        _clients = clients;
        _assignments = assignments;
        _userLookup = userLookup;
    }

    /// <summary>فهرست همه کلاینت‌ها.</summary>
    [HttpGet("clients")]
    public async Task<IActionResult> List() => Ok(await _clients.ListAsync());

    /// <summary>جزئیات یک کلاینت.</summary>
    [HttpGet("clients/{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var client = await _clients.GetAsync(id);
        return client is null ? PersianProblem(StatusCodes.Status404NotFound, "کلاینت یافت نشد.") : Ok(client);
    }

    /// <summary>ساخت کلاینت جدید (در صورت نداشتن ClientSecret، رمز خودکار تولید و فقط یک بار نمایش داده می‌شود).</summary>
    [HttpPost("clients")]
    public async Task<IActionResult> Create([FromBody] SaveClientRequest request)
    {
        var actor = await ActorAsync();
        var result = await _clients.CreateAsync(ToDto(request), request.ClientSecret, actor.Id);
        return result.Succeeded ? Ok(result.Value)
            : PersianProblem(StatusCodes.Status400BadRequest, string.Join(" ", result.Errors));
    }

    /// <summary>ویرایش تنظیمات کلاینت.</summary>
    [HttpPut("clients/{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] SaveClientRequest request)
    {
        var actor = await ActorAsync();
        request.ClientId = id;
        var result = await _clients.UpdateAsync(ToDto(request), actor.Id);
        return result.Succeeded ? Ok(new { message = "تنظیمات کلاینت ذخیره شد." })
            : PersianProblem(StatusCodes.Status400BadRequest, string.Join(" ", result.Errors));
    }

    /// <summary>تولید رمز جدید برای کلاینت (رمز فقط همین یک بار برگردانده می‌شود).</summary>
    [HttpPost("clients/{id}/secret")]
    public async Task<IActionResult> RegenerateSecret(string id)
    {
        var actor = await ActorAsync();
        var result = await _clients.RegenerateSecretAsync(id, actor.Id);
        return result.Succeeded ? Ok(result.Value)
            : PersianProblem(StatusCodes.Status400BadRequest, string.Join(" ", result.Errors));
    }

    /// <summary>فعال/غیرفعال‌کردن کلاینت (کلاینت غیرفعال از پروتکل ورود مسدود می‌شود).</summary>
    [HttpPut("clients/{id}/status")]
    public async Task<IActionResult> SetStatus(string id, [FromBody] ClientStatusRequest request)
    {
        var actor = await ActorAsync();
        var result = await _clients.SetStatusAsync(id, request.Enabled, actor.Id);
        return result.Succeeded ? Ok(new { message = "وضعیت کلاینت تغییر کرد." })
            : PersianProblem(StatusCodes.Status400BadRequest, string.Join(" ", result.Errors));
    }

    /// <summary>حذف کلاینت.</summary>
    [HttpDelete("clients/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var actor = await ActorAsync();
        var result = await _clients.DeleteAsync(id, actor.Id);
        return result.Succeeded ? Ok(new { message = "کلاینت حذف شد." })
            : PersianProblem(StatusCodes.Status400BadRequest, string.Join(" ", result.Errors));
    }

    /// <summary>کاربران تخصیص‌یافته (مجاز به ورود) به این کلاینت. فهرست خالی یعنی بدون محدودیت.</summary>
    [HttpGet("clients/{id}/assignments")]
    public async Task<IActionResult> ListAssignments(string id)
    {
        var rows = await _assignments.ListByClientAsync(id);
        var result = new List<ClientAssignmentDto>();
        foreach (var row in rows)
        {
            var user = await _userLookup.FindByIdAsync(row.UserId);
            result.Add(new ClientAssignmentDto
            {
                ClientId = id,
                UserId = row.UserId,
                UserName = user?.UserName ?? "-",
                DisplayName = user?.DisplayName ?? "-",
                AssignedAt = row.AssignedAt,
                AssignedBy = row.AssignedBy,
            });
        }

        return Ok(result);
    }

    /// <summary>تخصیص کاربر به کلاینت (فهرست سفید دسترسی).</summary>
    [HttpPost("clients/{id}/assignments")]
    public async Task<IActionResult> AssignUser(string id, [FromBody] AssignClientUserRequest request)
    {
        var actor = await ActorAsync();
        var result = await _assignments.AssignAsync(id, request.UserId, actor.Id);
        return result.Succeeded ? Ok(new { message = "کاربر به این کلاینت تخصیص یافت." })
            : PersianProblem(StatusCodes.Status400BadRequest, string.Join(" ", result.Errors));
    }

    /// <summary>حذف تخصیص کاربر از کلاینت.</summary>
    [HttpDelete("clients/{id}/assignments/{userId}")]
    public async Task<IActionResult> UnassignUser(string id, string userId)
    {
        var actor = await ActorAsync();
        var result = await _assignments.UnassignAsync(id, userId, actor.Id);
        return result.Succeeded ? Ok(new { message = "تخصیص کاربر حذف شد." })
            : PersianProblem(StatusCodes.Status400BadRequest, string.Join(" ", result.Errors));
    }

    private static SaveClientDto ToDto(SaveClientRequest request) => new()
    {
        ClientId = request.ClientId,
        DisplayName = request.DisplayName,
        PersianDisplayName = request.PersianDisplayName,
        Description = request.Description,
        Owner = request.Owner,
        Kind = request.Kind,
        Flows = request.Flows,
        Scopes = request.Scopes,
        RedirectUris = request.RedirectUris,
        PostLogoutRedirectUris = request.PostLogoutRedirectUris,
        RequireConsent = request.RequireConsent,
        Enabled = request.Enabled,
    };

    private async Task<ApplicationUser> ActorAsync()
        => await _userLookup.FindByIdAsync(User.FindFirst("sub")?.Value ?? string.Empty)
           ?? throw new InvalidOperationException("کاربر فراخواننده قابل تشخیص نیست.");
}
