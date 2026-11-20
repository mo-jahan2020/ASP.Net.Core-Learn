using System.ComponentModel.DataAnnotations;
using SsoServer.Domain.Options;

namespace SsoServer.Web.Controllers.Api.Requests;

public sealed class UpdateUserRequest
{
    [Required] public string DisplayName { get; set; } = string.Empty;

    [EmailAddress] public string Email { get; set; } = string.Empty;

    [Required] public string Role { get; set; } = SsoRoles.User;

    public bool IsActive { get; set; } = true;

    /// <summary>جابه‌جایی حساب بین احراز هویت با رمز محلی و اکتیو دایرکتوری.</summary>
    public bool IsAdUser { get; set; }
}
