using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SsoServer.Application.Services;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;
using SsoServer.Web.Controllers.Api.Requests;
using SsoServer.Web.Helpers;

namespace SsoServer.Web.Controllers.Api;

/// <summary>مدیریت کاربران (فقط مدیر): ساخت، ویرایش، نقش، claims، رمز و وضعیت حساب‌ها.</summary>
[Authorize(Policy = SsoPolicies.Admin)]
public class UsersController : SsoApiController
{
    private readonly IUserAdminService _users;
    private readonly ClientAssignmentService _assignments;
    private readonly UserManager<ApplicationUser> _userLookup;

    public UsersController(IUserAdminService users, ClientAssignmentService assignments, UserManager<ApplicationUser> userLookup)
    {
        _users = users;
        _assignments = assignments;
        _userLookup = userLookup;
    }

    /// <summary>فهرست همه کاربران.</summary>
    [HttpGet("users")]
    public async Task<IActionResult> List()
        => Ok(await _users.ListAsync());

    /// <summary>ساخت کاربر جدید (محلی یا اکتیو دایرکتوری).</summary>
    [HttpPost("users")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var actor = await ActorAsync();
        var result = await _users.CreateAsync(new CreateUserDto
        {
            UserName = request.UserName,
            DisplayName = request.DisplayName,
            Email = request.Email,
            Password = request.Password,
            Role = request.Role,
            IsActive = request.IsActive,
            IsAdUser = request.IsAdUser,
        }, actor.Id);

        return result.Succeeded
            ? CreatedAtAction(nameof(List), result.Value)
            : PersianProblem(StatusCodes.Status400BadRequest, string.Join(" ", result.Errors));
    }

    /// <summary>جزئیات یک کاربر.</summary>
    [HttpGet("users/{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var user = await _users.GetAsync(id);
        return user is null ? PersianProblem(StatusCodes.Status404NotFound, "کاربر یافت نشد.") : Ok(user);
    }

    /// <summary>ویرایش اطلاعات کاربر.</summary>
    [HttpPut("users/{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request)
    {
        var actor = await ActorAsync();
        var result = await _users.UpdateAsync(id, new UpdateUserDto
        {
            DisplayName = request.DisplayName,
            Email = request.Email,
            Role = request.Role,
            IsActive = request.IsActive,
            IsAdUser = request.IsAdUser,
        }, actor.Id);

        return result.Succeeded ? Ok(new { message = "اطلاعات کاربر ذخیره شد." })
            : PersianProblem(StatusCodes.Status400BadRequest, string.Join(" ", result.Errors));
    }

    /// <summary>بازنشانی رمز عبور توسط مدیر.</summary>
    [HttpPut("users/{id}/password")]
    public async Task<IActionResult> SetPassword(string id, [FromBody] SetPasswordRequest request)
    {
        var actor = await ActorAsync();
        var result = await _users.SetPasswordAsync(id, request.NewPassword, actor.Id);
        return result.Succeeded ? Ok(new { message = "رمز عبور بازنشانی شد." })
            : PersianProblem(StatusCodes.Status400BadRequest, string.Join(" ", result.Errors));
    }

    /// <summary>فعال/غیرفعال‌کردن حساب کاربر.</summary>
    [HttpPut("users/{id}/status")]
    public async Task<IActionResult> SetStatus(string id, [FromBody] UserStatusRequest request)
    {
        var actor = await ActorAsync();
        var result = await _users.SetStatusAsync(id, request.IsActive, actor.Id);
        return result.Succeeded ? Ok(new { message = "وضعیت کاربر تغییر کرد." })
            : PersianProblem(StatusCodes.Status400BadRequest, string.Join(" ", result.Errors));
    }

    /// <summary>ثبت claims سفارشی کاربر (این claims وارد توکن‌ها می‌شود).</summary>
    [HttpPut("users/{id}/claims")]
    public async Task<IActionResult> SetClaims(string id, [FromBody] SetClaimsRequest request)
    {
        var actor = await ActorAsync();
        var result = await _users.SetClaimsAsync(id, request.Claims, actor.Id);
        return result.Succeeded ? Ok(new { message = "کلیم‌های کاربر ذخیره شد." })
            : PersianProblem(StatusCodes.Status400BadRequest, string.Join(" ", result.Errors));
    }

    /// <summary>حذف کاربر.</summary>
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var actor = await ActorAsync();
        var result = await _users.DeleteAsync(id, actor.Id);
        return result.Succeeded ? Ok(new { message = "کاربر حذف شد." })
            : PersianProblem(StatusCodes.Status400BadRequest, string.Join(" ", result.Errors));
    }

    /// <summary>
    /// کلاینت‌هایی که این کاربر اجازه ورود به آن‌ها را دارد. فهرست خالی یعنی بدون محدودیت
    /// (دسترسی به همه کلاینت‌های فعال).
    /// </summary>
    [HttpGet("users/{id}/clients")]
    public async Task<IActionResult> AssignedClients(string id)
        => Ok(await _assignments.ListClientIdsByUserAsync(id));

    private async Task<ApplicationUser> ActorAsync()
        => await _userLookup.FindByIdAsync(User.FindFirst("sub")?.Value ?? string.Empty)
           ?? throw new InvalidOperationException("کاربر فراخواننده قابل تشخیص نیست.");
}
