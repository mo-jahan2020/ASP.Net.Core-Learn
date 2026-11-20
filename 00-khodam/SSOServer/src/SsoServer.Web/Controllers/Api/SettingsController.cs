using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;
using SsoServer.Web.Controllers.Api.Requests;
using SsoServer.Web.Helpers;

namespace SsoServer.Web.Controllers.Api;

/// <summary>تنظیمات با بارگذاری زنده (بدون ری‌استارت) + نمایش مقادیر مؤثر (فقط مدیر).</summary>
public class SettingsController : SsoApiController
{
    private readonly ISettingsService _settings;
    private readonly UserManager<ApplicationUser> _users;

    public SettingsController(ISettingsService settings, UserManager<ApplicationUser> users)
    {
        _settings = settings;
        _users = users;
    }

    /// <summary>مقادیر فعلی تنظیماتِ قابل بارگذاری زنده.</summary>
    [HttpGet("settings")]
    public async Task<IActionResult> Get()
    {
        var effective = await _settings.GetEffectiveAsync();
        return Ok(new
        {
            effective.CorsOrigins,
            effective.RequireConsentByDefault,
            effective.MaxFailedLoginAttempts,
            effective.LockoutMinutes,
            effective.TokenRateLimitPerMinute,
            effective.SessionCookieSliding,
            effective.LoginTitle,
            effective.LoginSubtitle,
        });
    }

    /// <summary>ذخیره تنظیمات؛ بلافاصله و بدون ری‌استارت اعمال می‌شود.</summary>
    [Authorize(Policy = SsoPolicies.Admin)]
    [HttpPut("settings")]
    public async Task<IActionResult> Update([FromBody] UpdateSettingsRequest request)
    {
        var actor = await _users.FindByIdAsync(User.FindFirst("sub")?.Value ?? string.Empty)
            ?? throw new InvalidOperationException("کاربر فراخواننده قابل تشخیص نیست.");

        var result = await _settings.UpdateAsync(new UpdateSettingsDto
        {
            CorsOrigins = request.CorsOrigins,
            RequireConsentByDefault = request.RequireConsentByDefault,
            MaxFailedLoginAttempts = request.MaxFailedLoginAttempts,
            LockoutMinutes = request.LockoutMinutes,
            TokenRateLimitPerMinute = request.TokenRateLimitPerMinute,
            SessionCookieSliding = request.SessionCookieSliding,
            LoginTitle = request.LoginTitle,
            LoginSubtitle = request.LoginSubtitle,
        }, actor.Id);

        return result.Succeeded ? Ok(new { message = "تنظیمات ذخیره و بلافاصله اعمال شد." })
            : PersianProblem(StatusCodes.Status400BadRequest, string.Join(" ", result.Errors));
    }

    /// <summary>تمام تنظیمات مؤثر، شامل مقادیر بارگذاری‌شده در راه‌اندازی (تغییرشان نیازمند ری‌استارت است).</summary>
    [HttpGet("settings/effective")]
    public async Task<IActionResult> Effective() => Ok(await _settings.GetEffectiveAsync());
}
