using System.DirectoryServices.Protocols;

namespace SsoServer.Infrastructure.ActiveDirectory;

/// <summary>
/// Active Directory / LDAP connection settings (appsettings.json section "ActiveDirectory").
/// Supports both Windows-integrated authentication (AuthType.Negotiate) and simple
/// binds (AuthType.Basic, e.g. DN or DOMAIN\user + password).
/// </summary>
public sealed class ActiveDirectorySettings
{
    public const string SectionName = "ActiveDirectory";

    /// <summary>
    /// Default user lookup filter: matches Active Directory entries (sAMAccountName /
    /// userPrincipalName) AND standard LDAP directories such as OpenLDAP (uid) —
    /// e.g. the public test server ldap.forumsys.com only exposes "uid"/"cn".
    /// </summary>
    internal const string DefaultUserSearchFilter = "(|(sAMAccountName={0})(userPrincipalName={0})(uid={0}))";

    /// <summary>Master switch: when false, AD logins are rejected with a clear message.</summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Domain controller host name or IP (e.g. dc01.corp.local). Full LDAP URLs are also
    /// accepted and normalized automatically: "ldap://host" (plain, port 389) and
    /// "ldaps://host" (SSL + port 636). A port inside the URL ("ldaps://host:10636")
    /// or a "host:port" pair wins over <see cref="Port"/>.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>LDAP port (389 plain / 636 LDAPS).</summary>
    public int Port { get; set; } = 389;

    /// <summary>Use LDAPS (SSL/TLS). Recommended for Basic auth so passwords are encrypted.</summary>
    public bool UseSsl { get; set; }

    /// <summary>"Negotiate" (Kerberos/NTLM, SSPI/GSSAPI) or "Basic" (simple bind).</summary>
    public string AuthType { get; set; } = "Negotiate";

    /// <summary>
    /// Service account used for directory queries. Accepted formats:
    /// DOMAIN\user, user@domain (UPN) or a full DN (CN=svc,CN=Users,DC=corp,DC=local).
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Password of the query service account (use env vars / secrets in production).</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Search scope, e.g. "DC=corp,DC=local" or "OU=Staff,DC=corp,DC=local".</summary>
    public string SearchBase { get; set; } = string.Empty;

    /// <summary>
    /// Optional custom search filter for locating the user entry; every "{0}" is replaced
    /// with the LDAP-escaped username (as typed plus the DOMAIN\user and user@domain
    /// variants). Example for OpenLDAP-style test servers: "(|(uid={0})(cn={0}))".
    /// Empty = <see cref="DefaultUserSearchFilter"/>.
    /// </summary>
    public string UserSearchFilter { get; set; } = string.Empty;

    internal bool IsBasic => string.Equals(AuthType?.Trim(), "Basic", StringComparison.OrdinalIgnoreCase);

    internal bool IsNegotiate => string.Equals(AuthType?.Trim(), "Negotiate", StringComparison.OrdinalIgnoreCase);

    /// <summary>The parsed LDAP auth type (defaults to Negotiate; use <see cref="IsValidAuthType"/> first).</summary>
    internal System.DirectoryServices.Protocols.AuthType LdapAuthType
        => IsBasic
            ? System.DirectoryServices.Protocols.AuthType.Basic
            : System.DirectoryServices.Protocols.AuthType.Negotiate;

    internal bool IsValidAuthType => IsBasic || IsNegotiate;

    // ----- Normalized connection values (a scheme/URL in Host is tolerated) -----

    /// <summary>Host name with any ldap:// or ldaps:// prefix removed — safe for LdapDirectoryIdentifier.</summary>
    public string CleanHost => ParseHost(Host).Host;

    /// <summary>Effective port: explicit port from the Host URL &gt; ldaps:// default 636 &gt; <see cref="Port"/>.</summary>
    public int EffectivePort
    {
        get
        {
            var parsed = ParseHost(Host);
            if (parsed.PortFromUrl is int urlPort)
            {
                return urlPort;
            }

            return parsed.Ldaps && Port == 389 ? 636 : Port;
        }
    }

    /// <summary>SSL is implied by an ldaps:// URL even when <see cref="UseSsl"/> is false.</summary>
    public bool EffectiveUseSsl => UseSsl || ParseHost(Host).Ldaps;

    /// <summary>The configured filter template, or the default when empty/unset.</summary>
    internal string UserSearchFilterOrDefault =>
        string.IsNullOrWhiteSpace(UserSearchFilter) ? DefaultUserSearchFilter : UserSearchFilter.Trim();

    /// <summary>
    /// Parses <see cref="Host"/>: plain host names pass through (trailing '/' trimmed);
    /// "host:port" without a scheme and ldap:// or ldaps:// URLs are decomposed into
    /// host/port/SSL. <see cref="ParsedHostInfo.Valid"/> is false for empty input or
    /// unsupported schemes (e.g. http://).
    /// </summary>
    internal static ParsedHostInfo ParseHost(string? host)
    {
        var trimmed = (host ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            return new ParsedHostInfo(string.Empty, null, Ldaps: false, Valid: false);
        }

        var schemeSeparator = trimmed.IndexOf("://", StringComparison.Ordinal);
        if (schemeSeparator < 0)
        {
            // "host:port" without scheme (e.g. "ldap.forumsys.com:389").
            var lastColon = trimmed.LastIndexOf(':');
            if (lastColon > 0 && lastColon < trimmed.Length - 1 &&
                int.TryParse(trimmed[(lastColon + 1)..], out var explicitPort))
            {
                return new ParsedHostInfo(trimmed[..lastColon].TrimEnd('/'), explicitPort, Ldaps: false, Valid: true);
            }

            return new ParsedHostInfo(trimmed.TrimEnd('/'), null, Ldaps: false, Valid: true);
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || uri.Host.Length == 0 ||
            (uri.Scheme is not ("ldap" or "ldaps")))
        {
            return new ParsedHostInfo(trimmed, null, Ldaps: false, Valid: false);
        }

        var isLdaps = uri.Scheme.Equals("ldaps", StringComparison.OrdinalIgnoreCase);

        // Uri canonicalization hides an explicit default port (uri.Authority for
        // "ldap://host:389" is just "host") and uri.Port invents 389 when no port is
        // present — so read the explicit port straight from the raw authority text.
        var rest = trimmed[(schemeSeparator + 3)..];
        var pathStart = rest.IndexOfAny(['/', '?']);
        if (pathStart >= 0)
        {
            rest = rest[..pathStart];
        }

        var atSign = rest.LastIndexOf('@');
        if (atSign >= 0)
        {
            rest = rest[(atSign + 1)..];
        }

        int? portFromUrl = null;
        var portSeparator = rest.LastIndexOf(':');
        if (portSeparator >= 0 && portSeparator < rest.Length - 1 &&
            int.TryParse(rest[(portSeparator + 1)..], out var urlPort))
        {
            portFromUrl = urlPort;
        }

        return new ParsedHostInfo(uri.Host, portFromUrl, isLdaps, Valid: true);
    }

    internal readonly record struct ParsedHostInfo(string Host, int? PortFromUrl, bool Ldaps, bool Valid);

    /// <summary>Returns a Persian configuration error, or null when the section is ready to use.</summary>
    public string? GetConfigurationError()
    {
        if (!Enabled)
        {
            return "ورود از طریق اکتیو دایرکتوری در تنظیمات فعال نیست (ActiveDirectory:Enabled).";
        }

        if (!IsValidAuthType)
        {
            return "نوع احراز هویت اکتیو دایرکتوری نامعتبر است؛ فقط Negotiate یا Basic مجاز است.";
        }

        if (string.IsNullOrWhiteSpace(Host))
        {
            return "آدرس سرور اکتیو دایرکتوری (ActiveDirectory:Host) تنظیم نشده است.";
        }

        if (!ParseHost(Host).Valid)
        {
            return "آدرس سرور اکتیو دایرکتوری (ActiveDirectory:Host) نامعتبر است؛ فقط نام میزبان، IP یا آدرس ldap:// یا ldaps:// مجاز است.";
        }

        if (Port is < 1 or > 65535)
        {
            return "پورت اکتیو دایرکتوری (ActiveDirectory:Port) نامعتبر است.";
        }

        if (string.IsNullOrWhiteSpace(Username))
        {
            return "نام کاربری حساب پرس‌وجوی اکتیو دایرکتوری (ActiveDirectory:Username) تنظیم نشده است.";
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            return "کلمه عبور حساب پرس‌وجوی اکتیو دایرکتوری (ActiveDirectory:Password) تنظیم نشده است.";
        }

        if (string.IsNullOrWhiteSpace(SearchBase))
        {
            return "محدوده جستجوی اکتیو دایرکتوری (ActiveDirectory:SearchBase) تنظیم نشده است.";
        }

        return null;
    }
}
