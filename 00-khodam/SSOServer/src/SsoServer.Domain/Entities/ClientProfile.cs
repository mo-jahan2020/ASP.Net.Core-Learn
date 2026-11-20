namespace SsoServer.Domain.Entities;

/// <summary>
/// Extra Persian metadata for a registered OpenIddict client application.
/// <see cref="ClientId"/> mirrors the OpenIddict application client id (logical reference).
/// </summary>
public class ClientProfile
{
    public string ClientId { get; set; } = string.Empty;

    public string PersianDisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Owner { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    /// <summary>Per-client consent override. Null means: use the global setting.</summary>
    public bool? RequireConsent { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
