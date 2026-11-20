using System.ComponentModel.DataAnnotations;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Models;

namespace SsoServer.Web.Controllers.Api.Requests;

public sealed class SaveClientRequest
{
    [Required] public string ClientId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    [Required] public string PersianDisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Owner { get; set; } = string.Empty;

    public ClientKind Kind { get; set; } = ClientKind.Confidential;

    public ClientFlow Flows { get; set; } = ClientFlow.AuthorizationCode | ClientFlow.RefreshToken;

    public List<string> Scopes { get; set; } = [.. SsoScopes.All];

    public List<string> RedirectUris { get; set; } = [];

    public List<string> PostLogoutRedirectUris { get; set; } = [];

    public bool? RequireConsent { get; set; }

    public bool Enabled { get; set; } = true;

    /// <summary>فقط هنگام «ساخت» استفاده می‌شود؛ اگر فراخواننده بخواهد رمز را خودش انتخاب کند (به‌جای تولید خودکار).</summary>
    public string? ClientSecret { get; set; }
}
