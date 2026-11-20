using System.ComponentModel.DataAnnotations;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Models;

namespace SsoServer.Web.ViewModels.Admin;

public sealed class SaveClientViewModel
{
    [Required(ErrorMessage = "شناسه کلاینت الزامی است.")]
    public string ClientId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام نمایشی فارسی الزامی است.")]
    public string PersianDisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Owner { get; set; } = string.Empty;

    public ClientKind Kind { get; set; }

    /// <summary>Bound from the flow checkboxes (multi-select).</summary>
    public List<int> FlowValues { get; set; } = [];

    /// <summary>Computed flags for display/POST composition.</summary>
    public ClientFlow Flows { get; set; }

    /// <summary>Bound from the scope checkboxes.</summary>
    public List<string> ScopeValues { get; set; } = [];

    public IReadOnlyList<string>? Scopes { get; set; }

    [Display(Name = "Redirect URIها (هر خط یک مورد)")]
    public string RedirectUris { get; set; } = string.Empty;

    [Display(Name = "Post-Logout URIها (هر خط یک مورد)")]
    public string PostLogoutRedirectUris { get; set; } = string.Empty;

    public bool? RequireConsent { get; set; }

    public bool Enabled { get; set; } = true;

    [StringLength(200, MinimumLength = 16, ErrorMessage = "Client Secret باید حداقل ۱۶ کاراکتر باشد.")]
    public string? ClientSecret { get; set; }

    public bool HasFlow(ClientFlow flow) => Flows.HasFlag(flow);

    public bool HasScope(string scope) => Scopes?.Contains(scope, StringComparer.Ordinal) ?? false;

    /// <summary>Composes the checkbox selections into the enum flags/scope list (after POST binding).</summary>
    public void NormalizeSelections()
    {
        Flows = (ClientFlow)FlowValues.Sum(v => v);
        Scopes = [.. ScopeValues];
    }

    public static SaveClientViewModel FromDto(ClientDto client) => new()
    {
        ClientId = client.ClientId,
        DisplayName = client.DisplayName,
        PersianDisplayName = client.PersianDisplayName,
        Description = client.Description,
        Owner = client.Owner,
        // سیاست امنیتی ۲۰۲۶-۱۱-۱۹: کلاینت‌های Public قدیمی هنگام ویرایش به Confidential
        // ارتقا می‌یابند؛ ادمین باید Secret جدید بگیرد (بازتولید) و به برنامه بدهد.
        Kind = ClientKind.Confidential,
        Flows = client.Flows,
        FlowValues = [.. Enum.GetValues<ClientFlow>().Where(f => f != ClientFlow.None && client.Flows.HasFlag(f)).Select(f => (int)f)],
        Scopes = client.Scopes,
        ScopeValues = [.. client.Scopes],
        RedirectUris = string.Join("\n", client.RedirectUris),
        PostLogoutRedirectUris = string.Join("\n", client.PostLogoutRedirectUris),
        RequireConsent = client.RequireConsent,
        Enabled = client.Enabled,
    };
}
