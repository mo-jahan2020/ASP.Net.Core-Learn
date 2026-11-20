using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Interfaces;
using SsoServer.Web.Helpers;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SsoServer.Web.Controllers.Api;

/// <summary>نشست‌های فعال: هر کاربر نشست‌های خودش را می‌بیند/ابطال می‌کند؛ مدیر به نشست‌های هر کاربر دسترسی دارد.</summary>
public class SessionsController : SsoApiController
{
    private readonly ISessionService _sessions;
    private readonly UserManager<ApplicationUser> _users;

    public SessionsController(ISessionService sessions, UserManager<ApplicationUser> users)
    {
        _sessions = sessions;
        _users = users;
    }

    /// <summary>فهرست نشست‌های فعال خودِ صاحب توکن.</summary>
    [HttpGet("sessions")]
    public async Task<IActionResult> ListOwn()
    {
        var subject = User.GetClaim(Claims.Subject) ?? string.Empty;
        return Ok(await _sessions.ListOwnAsync(subject));
    }

    /// <summary>ابطال یکی از نشست‌های خود.</summary>
    [HttpDelete("sessions/{id}")]
    public async Task<IActionResult> RevokeOwn(string id)
    {
        var subject = User.GetClaim(Claims.Subject) ?? string.Empty;
        var result = await _sessions.RevokeOwnAsync(subject, id);
        return result.Succeeded ? Ok(new { message = "نشست باطل شد." })
            : PersianProblem(StatusCodes.Status404NotFound, "نشست یافت نشد یا متعلق به شما نیست.");
    }

    /// <summary>ابطال همه نشست‌های خود.</summary>
    [HttpDelete("sessions")]
    public async Task<IActionResult> RevokeAllOwn()
    {
        var subject = User.GetClaim(Claims.Subject) ?? string.Empty;
        await _sessions.RevokeAllOwnAsync(subject);
        return Ok(new { message = "همه نشست‌های شما باطل شدند." });
    }

    /// <summary>(مدیر) فهرست نشست‌های فعال یک کاربر.</summary>
    [Authorize(Policy = SsoPolicies.Admin)]
    [HttpGet("admin/sessions")]
    public async Task<IActionResult> ListForUser([FromQuery] string? userId)
    {
        var target = await ResolveTargetAsync(userId);
        if (target is null)
        {
            return PersianProblem(StatusCodes.Status404NotFound, "کاربر یافت نشد.");
        }

        return Ok(await _sessions.ListForUserAsync(target.Id));
    }

    /// <summary>(مدیر) ابطال یکی از نشست‌های کاربر.</summary>
    [Authorize(Policy = SsoPolicies.Admin)]
    [HttpDelete("admin/sessions/{id}")]
    public async Task<IActionResult> RevokeForUser([FromQuery] string? userId, string id)
    {
        var target = await ResolveTargetAsync(userId);
        var actor = await _users.GetUserAsync(User);
        if (target is null || actor is null)
        {
            return PersianProblem(StatusCodes.Status404NotFound, "کاربر یافت نشد.");
        }

        var result = await _sessions.RevokeForUserAsync(target.Id, id, actor.Id);
        return result.Succeeded ? Ok(new { message = "نشست باطل شد." })
            : PersianProblem(StatusCodes.Status404NotFound, "نشست یافت نشد.");
    }

    /// <summary>(مدیر) ابطال همه نشست‌های کاربر.</summary>
    [Authorize(Policy = SsoPolicies.Admin)]
    [HttpDelete("admin/sessions")]
    public async Task<IActionResult> RevokeAllForUser([FromQuery] string? userId)
    {
        var target = await ResolveTargetAsync(userId);
        var actor = await _users.GetUserAsync(User);
        if (target is null || actor is null)
        {
            return PersianProblem(StatusCodes.Status404NotFound, "کاربر یافت نشد.");
        }

        await _sessions.RevokeAllForUserAsync(target.Id, actor.Id);
        return Ok(new { message = "همه نشست‌های کاربر باطل شدند." });
    }

    /// <summary>userId اختیاری است؛ در صورت نبود، از subject توکنِ فراخوان استفاده می‌شود.</summary>
    private async Task<ApplicationUser?> ResolveTargetAsync(string? userId)
    {
        var id = string.IsNullOrWhiteSpace(userId) ? User.GetClaim(Claims.Subject) : userId;
        return await _users.FindByIdAsync(id ?? string.Empty);
    }
}
