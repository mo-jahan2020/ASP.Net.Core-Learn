using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using SsoServer.Application.Tokens;
using AppDestinations = SsoServer.Application.Tokens.Destinations;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SsoServer.Web.OpenIddict;

/// <summary>
/// Converts the framework-independent <see cref="TokenContent"/> produced by
/// <c>ClaimFactory</c> into a ClaimsIdentity OpenIddict can serialize into tokens.
/// </summary>
public static class TokenContentExtensions
{
    /// تبدیل TokenContent مستقل از فریمورک (خروجی ClaimFactory) به ClaimsIdentity
    /// که OpenIddict بتواند آن را به توکن تبدیل کند (فلو ۳ گام ۵ و فلو ۴ گام ۶).
    public static ClaimsIdentity ToClaimsIdentity(this TokenContent content)
    {
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        foreach (var claim in content.Claims)
        {
            identity.SetClaim(claim.Type, claim.Value);
        }

        return identity;
    }

    /// <summary>
    /// Attaches destinations to every claim of an identity (mirrors the ClaimFactory rules
    /// for rebuilt principals on refresh/code exchanges, where claims come from the token).
    /// </summary>
    /// تعیین مقصد هر claim: در access_token / id_token / userinfo کدام قرار بگیرد.
    /// قواعد آینه همان قواعد ClaimFactory است و برای بازسازی Principal در refresh استفاده می‌شود.
    public static void ApplyTokenDestinations(this ClaimsIdentity identity)
    {
        identity.SetDestinations(static claim =>
        {
            return claim.Type switch
            {
                Claims.Subject => [AppDestinations.AccessToken, AppDestinations.IdentityToken, AppDestinations.UserInfo],
                Claims.Name or Claims.PreferredUsername when claim.Subject!.HasScope(Scopes.Profile)
                    => [AppDestinations.AccessToken, AppDestinations.IdentityToken, AppDestinations.UserInfo],
                Claims.Email when claim.Subject!.HasScope(Scopes.Email)
                    => [AppDestinations.AccessToken, AppDestinations.IdentityToken, AppDestinations.UserInfo],
                Claims.Role when claim.Subject!.HasScope(Scopes.Roles)
                    => [AppDestinations.AccessToken, AppDestinations.IdentityToken],
                "AspNet.Identity.SecurityStamp" => [],
                _ => [AppDestinations.AccessToken],
            };
        });
    }
}
