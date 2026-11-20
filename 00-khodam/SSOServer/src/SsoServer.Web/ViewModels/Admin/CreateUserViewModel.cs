using System.ComponentModel.DataAnnotations;
using SsoServer.Domain.Options;

namespace SsoServer.Web.ViewModels.Admin;

public sealed class CreateUserViewModel
{
    [Required(ErrorMessage = "نام کاربری الزامی است.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام نمایشی الزامی است.")]
    public string DisplayName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "ایمیل نامعتبر است.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Only required for non-AD users (validated conditionally in the action).
    /// Ignored when <see cref="IsAdUser"/> is checked.
    /// </summary>
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = SsoRoles.User;

    public bool IsActive { get; set; } = true;

    /// <summary>Tick = the account authenticates against Active Directory (no local password).</summary>
    public bool IsAdUser { get; set; }

    /// <summary>
    /// Client ids ticked in the «دسترسی به برنامه‌ها» checkboxes.
    /// Empty = no restriction (the user may access every enabled client).
    /// </summary>
    public List<string> SelectedClientIds { get; set; } = [];
}
