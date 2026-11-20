using SsoServer.Domain.Models;

namespace SsoServer.Web.ViewModels.Manage;

public sealed class ProfileViewModel
{
    public string Id { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public IReadOnlyList<string> Roles { get; set; } = [];

    public IReadOnlyList<ClaimDto> Claims { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }

    public IReadOnlyList<ActiveSessionDto> Sessions { get; set; } = [];
}
