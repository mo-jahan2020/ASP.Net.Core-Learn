using System.DirectoryServices.Protocols;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;

namespace SsoServer.Infrastructure.ActiveDirectory;

/// <summary>Persian messages for every Active Directory failure reason.</summary>
public static class ActiveDirectoryMessages
{
    public const string InvalidCredentials = "نام کاربری یا رمز عبور نادرست است.";
    public const string AccountDisabled = "حساب کاربری شما در اکتیو دایرکتوری غیرفعال شده است.";
    public const string AccountLocked = "حساب کاربری شما در اکتیو دایرکتوری موقتاً قفل شده است؛ کمی بعد دوباره تلاش کنید یا با مدیر شبکه تماس بگیرید.";
    public const string AccountExpired = "حساب کاربری شما در اکتیو دایرکتوری منقضی شده است.";
    public const string PasswordExpired = "رمز عبور شما در اکتیو دایرکتوری منقضی شده است؛ آن را در دامنه تغییر دهید.";
    public const string PasswordMustChange = "رمز عبور شما در اکتیو دایرکتوری باید تغییر کند.";
    public const string ServerUnavailable = "اتصال به سرور اکتیو دایرکتوری برقرار نشد؛ لطفاً با مدیر شبکه تماس بگیرید.";

    public static string MessageFor(AdAuthStatus status) => status switch
    {
        AdAuthStatus.AccountDisabled => AccountDisabled,
        AdAuthStatus.AccountLocked => AccountLocked,
        AdAuthStatus.AccountExpired => AccountExpired,
        AdAuthStatus.PasswordExpired => PasswordExpired,
        AdAuthStatus.PasswordMustChange => PasswordMustChange,
        AdAuthStatus.ServerUnavailable => ServerUnavailable,
        _ => InvalidCredentials,
    };
}

/// <summary>
/// Validates credentials against Active Directory:
/// 1. binds with the configured service account and searches for the user
///    (by the configurable filter template: sAMAccountName, userPrincipalName or uid),
/// 2. performs a bind with the user's own credentials — that bind IS the password check,
/// 3. maps the outcome to a precise Persian message (wrong password / disabled / locked /
///    expired / server unreachable).
/// Works with both AuthType.Negotiate and AuthType.Basic connections.
/// </summary>
public sealed partial class ActiveDirectoryService : IActiveDirectoryService
{
    private const int LdapInvalidCredentials = 49;
    private const int UserAccountControlDisabled = 0x0002;

    private static readonly string[] UserAttributes =
    [
        "distinguishedName", "sAMAccountName", "userPrincipalName",
        "displayName", "mail", "userAccountControl", "lockoutTime", "cn",
    ];

    private readonly ActiveDirectorySettings _settings;
    private readonly ILdapConnectionFactory _connections;
    private readonly ILogger<ActiveDirectoryService> _logger;

    public ActiveDirectoryService(
        IOptions<ActiveDirectorySettings> options,
        ILdapConnectionFactory connections,
        ILogger<ActiveDirectoryService> logger)
    {
        _settings = options.Value;
        _connections = connections;
        _logger = logger;
    }

    public Task<AdAuthResult> AuthenticateAsync(string username, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult(AdAuthResult.Fail(
                AdAuthStatus.InvalidCredentials, ActiveDirectoryMessages.InvalidCredentials));
        }

        var configurationError = _settings.GetConfigurationError();
        if (configurationError is not null)
        {
            _logger.LogWarning("Active Directory login skipped for {Username}: {Error}", username, configurationError);
            return Task.FromResult(AdAuthResult.Fail(AdAuthStatus.NotConfigured, configurationError));
        }

        return Task.FromResult(Authenticate(username, password));
    }

    private AdAuthResult Authenticate(string username, string password)
    {
        AdUserEntry? entry = null;
        try
        {
            using (var query = _connections.CreateServiceConnection())
            {
                query.Bind();
                entry = FindUser(query, username);
            }

            if (entry is null || string.IsNullOrWhiteSpace(entry.DistinguishedName))
            {
                _logger.LogInformation("Active Directory login for {Username}: no matching entry in the search base", username);
                return AdAuthResult.Fail(AdAuthStatus.InvalidCredentials, ActiveDirectoryMessages.InvalidCredentials);
            }

            using var userConnection = _connections.CreateUserConnection(BuildUserBindName(entry, username), password);
            userConnection.Bind();
        }
        catch (LdapException ex) when (ex.ErrorCode == LdapInvalidCredentials)
        {
            // The bind itself was rejected. The sub-code inside ServerErrorMessage (when present)
            // is the most precise signal; otherwise fall back to the account flags from the search.
            var fromServer = TryMapAccountState(ex.ServerErrorMessage);
            var status = fromServer
                ?? (entry is { Disabled: true } ? AdAuthStatus.AccountDisabled
                : entry is { Locked: true } ? AdAuthStatus.AccountLocked
                : AdAuthStatus.InvalidCredentials);

            _logger.LogWarning("Active Directory bind rejected for {Username}: {Status}", username, status);
            return AdAuthResult.Fail(status, ActiveDirectoryMessages.MessageFor(status));
        }
        catch (Exception ex) when (ex is LdapException or DirectoryOperationException or TlsOperationException or ArgumentException)
        {
            _logger.LogWarning(ex, "Active Directory is unreachable or the query failed for {Username}", username);
            return AdAuthResult.Fail(AdAuthStatus.ServerUnavailable, ActiveDirectoryMessages.ServerUnavailable);
        }

        // The password itself is correct. Final safety gate on the account flags captured
        // during the search (a truly disabled account cannot reach this point on real AD).
        if (entry.Disabled)
        {
            return AdAuthResult.Fail(AdAuthStatus.AccountDisabled, ActiveDirectoryMessages.AccountDisabled);
        }

        _logger.LogInformation("Active Directory login succeeded for {Username}", username);
        return AdAuthResult.Ok(entry.DisplayName, entry.Email);
    }

    private AdUserEntry? FindUser(ILdapConnection connection, string username)
    {
        var filter = BuildUserFilter(username);
        var attributes = connection.SearchFirst(_settings.SearchBase, filter, UserAttributes);
        return attributes is null ? null : AdUserEntry.From(attributes);
    }

    /// <summary>
    /// Username used for the user's own bind:
    /// Basic  -> the distinguished name (most reliable for simple binds);
    /// Negotiate -> DOMAIN\user / UPN as typed, else the directory's UPN,
    ///              else sAMAccountName@domain derived from the DN.
    /// </summary>
    internal string BuildUserBindName(AdUserEntry entry, string typedUsername)
    {
        if (_settings.IsBasic)
        {
            return entry.DistinguishedName;
        }

        if (typedUsername.Contains('\\') || typedUsername.Contains('@'))
        {
            return typedUsername;
        }

        if (!string.IsNullOrWhiteSpace(entry.UserPrincipalName))
        {
            return entry.UserPrincipalName;
        }

        var domain = DomainFromDistinguishedName(entry.DistinguishedName);
        if (domain is not null && !string.IsNullOrWhiteSpace(entry.SamAccountName))
        {
            return $"{entry.SamAccountName}@{domain}";
        }

        return entry.DistinguishedName;
    }

    /// <summary>
    /// Builds the user search filter from the configured template
    /// (ActiveDirectory:UserSearchFilter; {0} is replaced with the LDAP-escaped username).
    /// Input variants are all covered: as typed, the bare name from DOMAIN\user and the
    /// local part of user@domain. Every candidate is RFC 4515-escaped (no LDAP injection).
    /// </summary>
    internal string BuildUserFilter(string username)
    {
        var template = _settings.UserSearchFilterOrDefault;
        var clauses = UsernameCandidates(username)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(candidate => template.Replace("{0}", EscapeLdapFilterValue(candidate)));

        return $"(|{string.Concat(clauses)})";
    }

    private static IEnumerable<string> UsernameCandidates(string username)
    {
        yield return username;

        var backslash = username.LastIndexOf('\\');
        if (backslash >= 0 && backslash < username.Length - 1)
        {
            yield return username[(backslash + 1)..];
        }

        var at = username.IndexOf('@');
        if (at > 0 && at < username.Length - 1)
        {
            yield return username[..at];
        }
    }

    /// <summary>Escapes RFC 4515 filter metacharacters: * ( ) \ NUL.</summary>
    internal static string EscapeLdapFilterValue(string value)
    {
        var escaped = value.Replace(@"\", @"\5c")
            .Replace("*", @"\2a")
            .Replace("(", @"\28")
            .Replace(")", @"\29")
            .Replace("\0", @"\00");
        return escaped;
    }

    /// <summary>
    /// Maps the "data 52e / 533 / 775 / ..." sub-code inside an LDAP error 49
    /// ServerErrorMessage to the matching account state (null when absent/unknown).
    /// </summary>
    internal static AdAuthStatus? TryMapAccountState(string? serverErrorMessage)
    {
        if (string.IsNullOrEmpty(serverErrorMessage))
        {
            return null;
        }

        var match = DataSubCodeRegex().Match(serverErrorMessage);
        if (!match.Success)
        {
            return null;
        }

        return match.Groups[1].Value.ToLowerInvariant() switch
        {
            "532" => AdAuthStatus.PasswordExpired,
            "533" => AdAuthStatus.AccountDisabled,
            "701" => AdAuthStatus.AccountExpired,
            "773" => AdAuthStatus.PasswordMustChange,
            "775" => AdAuthStatus.AccountLocked,
            _ => null, // 52e (bad password), 525 (no such user), 530/531 (restrictions), ...
        };
    }

    /// <summary>DC=corp,DC=local (anywhere inside a DN) -> corp.local.</summary>
    internal static string? DomainFromDistinguishedName(string distinguishedName)
    {
        var parts = distinguishedName
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(p => p.StartsWith("DC=", StringComparison.OrdinalIgnoreCase))
            .Select(p => p[3..]);

        var domain = string.Join(".", parts);
        return string.IsNullOrEmpty(domain) ? null : domain;
    }

    [GeneratedRegex(@"data\s+([0-9a-fA-F]{3})")]
    private static partial Regex DataSubCodeRegex();

    /// <summary>Account data captured from the directory entry during the search.</summary>
    internal sealed record AdUserEntry(
        string DistinguishedName,
        string? SamAccountName,
        string? UserPrincipalName,
        string? DisplayName,
        string? Email,
        bool Disabled,
        bool Locked)
    {
        public static AdUserEntry From(IReadOnlyDictionary<string, IReadOnlyList<string>> attributes)
        {
            string? First(string name) =>
                attributes.TryGetValue(name, out var values) && values.Count > 0 ? values[0] : null;

            var distinguishedName = First("distinguishedName") ?? string.Empty;
            var userAccountControl = First("userAccountControl");
            var lockoutTime = First("lockoutTime");

            return new AdUserEntry(
                DistinguishedName: distinguishedName,
                SamAccountName: First("sAMAccountName"),
                UserPrincipalName: First("userPrincipalName"),
                // OpenLDAP-style directories (e.g. ldap.forumsys.com) have no displayName —
                // fall back to cn so the local profile still gets a readable name.
                DisplayName: First("displayName") ?? First("cn"),
                Email: First("mail"),
                Disabled: int.TryParse(userAccountControl, out var uac) && (uac & UserAccountControlDisabled) != 0,
                // lockoutTime is a FILETIME stamp; non-zero means the account has been locked
                // (it stays set after the lockout window passes, so it is only used as a hint
                // when the bind itself already failed — never to block a successful bind).
                Locked: long.TryParse(lockoutTime, out var locked) && locked != 0);
        }
    }
}
