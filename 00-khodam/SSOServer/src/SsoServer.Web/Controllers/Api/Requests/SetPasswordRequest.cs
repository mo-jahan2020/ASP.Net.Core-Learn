using System.ComponentModel.DataAnnotations;

namespace SsoServer.Web.Controllers.Api.Requests;

public sealed class SetPasswordRequest
{
    [Required] [StringLength(100, MinimumLength = 8)] public string NewPassword { get; set; } = string.Empty;
}
