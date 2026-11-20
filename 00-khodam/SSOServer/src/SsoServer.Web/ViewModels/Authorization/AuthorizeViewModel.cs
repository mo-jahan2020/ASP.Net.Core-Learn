namespace SsoServer.Web.ViewModels.Authorization;

public sealed class AuthorizeViewModel
{
    public string ApplicationName { get; set; } = string.Empty;

    public string ApplicationDescription { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;

    public IReadOnlyList<string> Scopes { get; set; } = [];
}
