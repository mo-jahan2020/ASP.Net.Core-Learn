using SsoServer.Domain.Enums;

namespace SsoServer.Domain.Models;

public class ClientDto
{
    public required string ClientId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string PersianDisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Owner { get; set; } = string.Empty;

    public ClientKind Kind { get; set; }

    public ClientFlow Flows { get; set; }

    public IReadOnlyList<string> Scopes { get; set; } = [];

    public IReadOnlyList<string> RedirectUris { get; set; } = [];

    public IReadOnlyList<string> PostLogoutRedirectUris { get; set; } = [];

    public bool? RequireConsent { get; set; }

    public bool Enabled { get; set; } = true;

    public bool HasSecret { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>Payload used to create or update a client application.</summary>
public class SaveClientDto
{
    public string ClientId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PersianDisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Owner { get; set; } = string.Empty;

    public ClientKind Kind { get; set; } = ClientKind.Confidential;

    public ClientFlow Flows { get; set; } = ClientFlow.AuthorizationCode | ClientFlow.RefreshToken;

    public IReadOnlyList<string> Scopes { get; set; } = [.. SsoScopes.All];

    public IReadOnlyList<string> RedirectUris { get; set; } = [];

    public IReadOnlyList<string> PostLogoutRedirectUris { get; set; } = [];

    public bool? RequireConsent { get; set; }

    public bool Enabled { get; set; } = true;
}

public class NewClientSecretDto
{
    public required string ClientId { get; set; }

    public required string ClientSecret { get; set; }
}

/// <summary>
/// One row of the per-user client access whitelist:
/// "user <see cref="UserId"/> may sign in to client <see cref="ClientId"/>".
/// Returned by the admin UI and the management API
/// (<c>GET /api/v1/clients/{id}/assignments</c>).
/// </summary>
public class ClientAssignmentDto
{
    /// <summary>OpenIddict client_id, e.g. "aspnetcore-sample".</summary>
    public required string ClientId { get; set; }

    /// <summary>Human-friendly client name (falls back to ClientId when unknown).</summary>
    public string ClientDisplayName { get; set; } = string.Empty;

    /// <summary>ASP.NET Core Identity user id.</summary>
    public required string UserId { get; set; }

    /// <summary>Login name of the assigned user ("-" when the user was deleted).</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Display name of the assigned user ("-" when the user was deleted).</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>When the assignment was created (UTC).</summary>
    public DateTimeOffset AssignedAt { get; set; }

    /// <summary>User id of the admin who created the assignment (audit trail).</summary>
    public string AssignedBy { get; set; } = string.Empty;
}
