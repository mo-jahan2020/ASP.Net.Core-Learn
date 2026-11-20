using System.ComponentModel.DataAnnotations;
using SsoServer.Domain.Options;

namespace SsoServer.Web.ViewModels.Admin;

public sealed class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام نمایشی الزامی است.")]
    public string DisplayName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "ایمیل نامعتبر است.")]
    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = SsoRoles.User;

    public bool IsActive { get; set; } = true;

    /// <summary>Tick = the account authenticates against Active Directory (no local password).</summary>
    public bool IsAdUser { get; set; }

    [Display(Name = "کلیم‌ها (هر خط: نوع=مقدار)")]
    public string ClaimsText { get; set; } = string.Empty;

    /// <summary>
    /// Client ids ticked in the «دسترسی به برنامه‌ها» checkboxes.
    /// Empty = no restriction (the user may access every enabled client).
    /// </summary>
    public List<string> SelectedClientIds { get; set; } = [];
}
