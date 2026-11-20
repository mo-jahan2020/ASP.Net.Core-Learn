using SsoServer.Domain.Entities;
using SsoServer.Domain.Models;

namespace SsoServer.Application.Tokens;

/// <summary>Token claim destination names (identical to the OpenIddict constants).</summary>
public static class Destinations
{
    public const string AccessToken = "access_token";
    public const string IdentityToken = "id_token";
    public const string UserInfo = "userinfo";
}

/// <summary>A claim with its token destinations, computed without any framework dependency.</summary>
public sealed record TokenClaim(string Type, string Value, IReadOnlyList<string> Destinations);

/// <summary>The full content used to build an OpenIddict sign-in principal.</summary>
public sealed class TokenContent
{
    public required string Subject { get; init; }

    public required IReadOnlyList<TokenClaim> Claims { get; init; }

    public required IReadOnlyList<string> Scopes { get; init; }
}

/// <summary>
/// Builds the claim set embedded into access/identity tokens: subject, display name,
/// preferred username, email, roles (multi-value so <c>[Authorize(Roles=...)]</c> works in
/// client apps) and the admin-managed custom claims. Pure and unit-testable.
/// </summary>
public static class ClaimFactory
{
    /// قلب ساخت محتوای توکن (در همه فلوهای توکن استفاده می‌شود).
    /// ورودی: کاربر + نقش‌ها + claims سفارشی + درخواست scopeها.
    /// خروجی TokenContent شامل: sub (شناسه کاربر)، name، preferred_username، email،
    /// role (چندمقداری تا [Authorize(Roles=...)] در برنامه‌های کلاینت کار کند)
    /// و claims سفارشی مدیر (به‌جز موارد رزروشده).
    /// برای هر claim مشخص می‌شود در کدام توکن‌ها قرار بگیرد (destinations).
    public static TokenContent Create(
        ApplicationUser user,
        IEnumerable<string> roles,
        IEnumerable<ClaimDto> customClaims,
        IEnumerable<string> requestedScopes)
    {
        ArgumentNullException.ThrowIfNull(user);

        var scopes = requestedScopes
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Intersect(SsoScopes.All, StringComparer.Ordinal)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var claims = new List<TokenClaim>
        {
            new("sub", user.Id, [Destinations.AccessToken, Destinations.IdentityToken, Destinations.UserInfo]),
            new("name", user.DisplayName, BuildDestinations(hasProfile: scopes.Contains(SsoScopes.Profile), includeUserInfo: true)),
            new("preferred_username", user.UserName, BuildDestinations(hasProfile: scopes.Contains(SsoScopes.Profile), includeUserInfo: true)),
            new("email", user.Email ?? string.Empty, BuildDestinations(scopes.Contains(SsoScopes.Email), includeUserInfo: true)),
        };

        claims.AddRange(roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(role => new TokenClaim("role", role, BuildDestinations(hasProfile: scopes.Contains(SsoScopes.Roles), includeUserInfo: false))));

        var reserved = new HashSet<string>(StringComparer.Ordinal) { "sub", "name", "preferred_username", "email", "role" };
        foreach (var claim in customClaims)
        {
            if (string.IsNullOrWhiteSpace(claim.Type) || reserved.Contains(claim.Type))
            {
                continue;
            }

            claims.Add(new TokenClaim(claim.Type, claim.Value, [Destinations.AccessToken, Destinations.IdentityToken]));
        }

        return new TokenContent { Subject = user.Id, Claims = claims, Scopes = scopes };
    }

    private static IReadOnlyList<string> BuildDestinations(bool hasProfile, bool includeUserInfo)
    {
        var destinations = new List<string> { Destinations.AccessToken };
        if (hasProfile)
        {
            destinations.Add(Destinations.IdentityToken);
        }

        if (includeUserInfo && hasProfile)
        {
            destinations.Add(Destinations.UserInfo);
        }

        return destinations;
    }
}
