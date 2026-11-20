using Microsoft.AspNetCore.Identity;

namespace SsoServer.Domain.Entities;

/// <summary>
/// Application user with SSO-specific profile fields, backed by ASP.NET Core Identity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Persian-friendly display name shown on the UI and inside tokens (claim: name).</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Soft enable/disable switch. Deactivated users cannot sign in.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// True when this account authenticates against Active Directory (LDAP bind).
    /// AD users have no local password; their credentials are validated by the domain.
    /// </summary>
    public bool IsAdUser { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
