namespace SsoServer.Application.Validation;

/// <summary>
/// Strict redirect-URI validation for client registration and request matching:
/// absolute http(s) URIs only, no wildcards, no fragments and exact matching
/// (scheme/host case-insensitive, path/query case-sensitive, explicit ports).
/// </summary>
public static class RedirectUriValidator
{
    public static bool IsWellFormed(string? uri)
    {
        if (string.IsNullOrWhiteSpace(uri) || uri.Contains('*') || uri.Contains('<') || uri.Contains('>'))
        {
            return false;
        }

        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
        {
            return false;
        }

        if (parsed.Scheme is not ("http" or "https"))
        {
            return false;
        }

        if (string.IsNullOrEmpty(parsed.Host) || parsed.UserInfo.Length > 0 || parsed.Fragment.Length > 0)
        {
            return false;
        }

        return true;
    }

    public static bool Matches(string registered, string requested)
    {
        if (!Uri.TryCreate(registered, UriKind.Absolute, out var a) ||
            !Uri.TryCreate(requested, UriKind.Absolute, out var b))
        {
            return false;
        }

        return string.Equals(a.Scheme, b.Scheme, StringComparison.OrdinalIgnoreCase)
            && string.Equals(a.Host, b.Host, StringComparison.OrdinalIgnoreCase)
            && a.Port == b.Port
            && string.Equals(a.AbsolutePath, b.AbsolutePath, StringComparison.Ordinal)
            && string.Equals(a.Query, b.Query, StringComparison.Ordinal);
    }
}
