using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using SsoServer.Application.Services;
using SsoServer.Domain.Entities;
using SsoServer.Web.Controllers.Api.Requests;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SsoServer.Web.Controllers.Api;

/// <summary>عملیات حساب شخصیِ صاحب توکن (مشاهده اطلاعات و تغییر رمز).</summary>
public class MeController : SsoApiController
{
    private readonly UserManager<ApplicationUser> _users;

    public MeController(UserManager<ApplicationUser> users) => _users = users;

    /// <summary>اطلاعات حساب صاحب توکن: نام، نقش‌ها و claims سفارشی.</summary>
    /// فلو ۶ — GET /api/v1/me : اطلاعات حساب صاحب توکن.
    /// subject از claims توکن خوانده می‌شود؛ پاسخ شامل نام، نقش‌ها و claims سفارشی است.
    /// مثل UserInfo اما با خروجی ساخت‌یافته برای برنامه‌های کلاینت.
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _users.FindByIdAsync(User.GetClaim(Claims.Subject) ?? string.Empty);
        if (user is null)
        {
            return PersianProblem(StatusCodes.Status401Unauthorized, "کاربر مرتبط با این توکن یافت نشد.");
        }

        var customClaims = (await _users.GetClaimsAsync(user))
            .GroupBy(c => c.Type, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => string.Join(",", g.Select(c => c.Value)), StringComparer.Ordinal);

        return Ok(new MeResponse
        {
            Sub = user.Id,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Roles = [.. await _users.GetRolesAsync(user)],
            Claims = customClaims,
        });
    }

    /// <summary>تغییر رمز عبور حساب خود؛ نیازمند رمز فعلی.</summary>
    /// فلو ۶ — PUT /api/v1/me/password : تغییر رمز با ارائه رمز فعلی.
    /// خطاهای Identity به پیام‌های فارسی ترجمه می‌شوند (PersianIdentityErrors).
    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _users.FindByIdAsync(User.GetClaim(Claims.Subject) ?? string.Empty);
        if (user is null)
        {
            return PersianProblem(StatusCodes.Status401Unauthorized, "کاربر مرتبط با این توکن یافت نشد.");
        }

        var result = await _users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return PersianProblem(StatusCodes.Status400BadRequest, string.Join(" ", PersianIdentityErrors.Map(result.Errors)));
        }

        return Ok(new { message = "رمز عبور با موفقیت تغییر کرد." });
    }
}
