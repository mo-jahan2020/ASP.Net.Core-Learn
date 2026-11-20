using Microsoft.AspNetCore.Identity;

namespace SsoServer.Domain.Entities;

/// <summary>
/// Role entity. The SSO uses exactly two roles: <c>Admin</c> and <c>User</c>.
/// </summary>
public class ApplicationRole : IdentityRole
{
    public string PersianDescription { get; set; } = string.Empty;

    public ApplicationRole() { }

    public ApplicationRole(string name, string persianDescription) : base(name)
        => PersianDescription = persianDescription;
}
