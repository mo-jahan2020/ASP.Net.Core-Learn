namespace SsoServer.Web.ViewModels.Admin;

/// <summary>One row of the per-client whitelist table on the ClientEdit page.</summary>
public sealed class ClientAssignmentRowViewModel
{
    public string UserId { get; init; } = string.Empty;

    public string UserName { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public bool IsAdmin { get; init; }

    public DateTimeOffset AssignedAt { get; init; }
}
