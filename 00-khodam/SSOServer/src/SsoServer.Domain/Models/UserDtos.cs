namespace SsoServer.Domain.Models;

public class UserSummaryDto
{
    public required string Id { get; set; }

    public required string UserName { get; set; }

    public required string DisplayName { get; set; }

    public required string Email { get; set; }

    public bool IsActive { get; set; }

    /// <summary>True when the user authenticates against Active Directory (no local password).</summary>
    public bool IsAdUser { get; set; }

    public IReadOnlyList<string> Roles { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }
}

public class UserDetailDto : UserSummaryDto
{
    public IReadOnlyList<ClaimDto> Claims { get; set; } = [];
}

public class CreateUserDto
{
    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>Ignored when <see cref="IsAdUser"/> is true (no local password is stored).</summary>
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public bool IsActive { get; set; } = true;

    /// <summary>When true the account is validated against Active Directory instead of a local password.</summary>
    public bool IsAdUser { get; set; }
}

public class UpdateUserDto
{
    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public bool IsActive { get; set; } = true;

    /// <summary>Switches the account between local-password and Active Directory authentication.</summary>
    public bool IsAdUser { get; set; }
}

public class ClaimDto
{
    public string Type { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
