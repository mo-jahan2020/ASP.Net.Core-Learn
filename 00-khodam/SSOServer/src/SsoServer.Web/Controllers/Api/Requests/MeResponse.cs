namespace SsoServer.Web.Controllers.Api.Requests;

public sealed class MeResponse
{
    public required string Sub { get; set; }

    public required string UserName { get; set; }

    public required string DisplayName { get; set; }

    public string? Email { get; set; }

    public IReadOnlyList<string> Roles { get; set; } = [];

    public Dictionary<string, string> Claims { get; set; } = [];
}
