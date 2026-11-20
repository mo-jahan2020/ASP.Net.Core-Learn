using SsoServer.Domain.Models;

namespace SsoServer.Web.Controllers.Api.Requests;

public sealed class SetClaimsRequest
{
    public List<ClaimDto> Claims { get; set; } = [];
}
