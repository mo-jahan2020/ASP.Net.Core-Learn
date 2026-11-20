using System.ComponentModel.DataAnnotations;

namespace SsoServer.Web.Controllers.Api.Requests;

public sealed class AssignClientUserRequest
{
    [Required] public string UserId { get; set; } = string.Empty;
}
