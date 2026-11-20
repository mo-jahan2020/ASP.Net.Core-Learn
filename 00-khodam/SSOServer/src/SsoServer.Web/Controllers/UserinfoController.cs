using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SsoServer.Domain.Entities;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SsoServer.Web.Controllers;

/// <summary>Userinfo endpoint: fresh profile claims for a valid access token.</summary>
[ApiExplorerSettings(IgnoreApi = true)]
public class UserinfoController : Controller
{
    private readonly UserManager<ApplicationUser> _users;

    public UserinfoController(UserManager<ApplicationUser> users) => _users = users;

    /// فلو ۶ (PDF فصل ۶) — endpoint استاندارد UserInfo: GET/POST /connect/userinfo.
    /// فقط با access token معتبر باز می‌شود (احراز هویت در اسکیم OpenIddict).
    /// کاربر از claim sub پیدا می‌شود؛ حذف/غیرفعال → خطای invalid_token.
    /// claims برگشتی بر اساس scope فیلتر می‌شوند: profile → نام، email → ایمیل،
    /// roles → نقش‌ها؛ claims سفارشی همیشه برمی‌گردند.
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo"), HttpPost("~/connect/userinfo")]
    [Produces("application/json")]
    public async Task<IActionResult> Userinfo()
    {
        var user = await _users.FindByIdAsync(User.GetClaim(Claims.Subject) ?? string.Empty);
        if (user is null || !user.IsActive)
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "توکن ارائه‌شده به حساب کاربری متصل نیست یا حساب حذف/غیرفعال شده است.",
                }));
        }

        var claims = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            [Claims.Subject] = user.Id,
        };

        if (User.HasScope(Scopes.Profile))
        {
            claims[Claims.Name] = user.DisplayName;
            claims[Claims.PreferredUsername] = user.UserName;
        }

        if (User.HasScope(Scopes.Email))
        {
            claims[Claims.Email] = user.Email;
            claims[Claims.EmailVerified] = user.EmailConfirmed;
        }

        if (User.HasScope(Scopes.Roles))
        {
            claims[Claims.Role] = await _users.GetRolesAsync(user);
        }

        foreach (var claim in await _users.GetClaimsAsync(user))
        {
            claims[claim.Type] = claim.Value;
        }

        return Ok(claims);
    }
}
