using System.Collections;
using System.DirectoryServices.Protocols;
using System.Net;
using Microsoft.Extensions.Options;

namespace SsoServer.Infrastructure.ActiveDirectory;

/// <summary>
/// Creates real LDAP connections to the configured Active Directory.
/// Host, port, SSL and auth type (Negotiate/Basic) come from <see cref="ActiveDirectorySettings"/>.
/// </summary>
public sealed class LdapConnectionFactory : ILdapConnectionFactory
{
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(5);

    private readonly ActiveDirectorySettings _settings;

    public LdapConnectionFactory(IOptions<ActiveDirectorySettings> options)
    {
        _settings = options.Value;
    }

    public ILdapConnection CreateServiceConnection()
        => Create(_settings.Username, _settings.Password);

    public ILdapConnection CreateUserConnection(string username, string password)
        => Create(username, password);

    private LdapConnectionWrapper Create(string username, string password)
    {
        // CleanHost/EffectivePort strip a possible ldap:// or ldaps:// prefix from the
        // configured Host — LdapDirectoryIdentifier expects a bare host name, not a URL.
        var identifier = new LdapDirectoryIdentifier(_settings.CleanHost, _settings.EffectivePort);
        var credential = new NetworkCredential(username, password);
        var connection = new LdapConnection(identifier, credential, _settings.LdapAuthType)
        {
            Timeout = ConnectTimeout,
        };

        // LDAPv3 explicitly: the library default (LDAPv2) is rejected by modern servers —
        // e.g. ldap.forumsys.com answers a v2 bind with "protocol error", which surfaces
        // as the generic "server unavailable" message. Active Directory also fully supports v3.
        connection.SessionOptions.ProtocolVersion = 3;

        if (_settings.EffectiveUseSsl)
        {
            // Must be set before Bind(); encrypts the whole session including Basic binds.
            connection.SessionOptions.SecureSocketLayer = true;
        }

        if (_settings.LdapAuthType == AuthType.Negotiate && OperatingSystem.IsWindows())
        {
            // Domain controllers require signed/sealed Negotiate binds by default (2020 hardening).
            connection.SessionOptions.Signing = true;
            connection.SessionOptions.Sealing = true;
        }

        return new LdapConnectionWrapper(connection);
    }

    private sealed class LdapConnectionWrapper(LdapConnection connection) : ILdapConnection
    {
        public void Bind() => connection.Bind();

        public IReadOnlyDictionary<string, IReadOnlyList<string>>? SearchFirst(
            string baseDn, string ldapFilter, IReadOnlyList<string> attributes)
        {
            var request = new SearchRequest(baseDn, ldapFilter, SearchScope.Subtree, [.. attributes]);
            var response = (SearchResponse)connection.SendRequest(request);
            if (response.Entries.Count == 0)
            {
                return null;
            }

            var entry = response.Entries[0];
            var result = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

            // Windows yields DirectoryAttribute items directly; the Unix implementation
            // exposes the attributes collection as a name -> DirectoryAttribute map
            // (DictionaryEntry items), so both shapes are handled here.
            foreach (var item in entry.Attributes)
            {
                var attribute = item switch
                {
                    DirectoryAttribute direct => direct,
                    DictionaryEntry { Value: DirectoryAttribute fromMap } => fromMap,
                    _ => null,
                };

                if (attribute is null)
                {
                    continue;
                }

                result[attribute.Name] = [.. attribute.GetValues(typeof(string)).Cast<string>()];
            }

            // OpenLDAP-style servers (e.g. ldap.forumsys.com) expose the DN only on the
            // entry itself, not as a returned attribute — provide it so the caller can
            // always rely on "distinguishedName" being present.
            if (!result.ContainsKey("distinguishedName") &&
                !string.IsNullOrEmpty(entry.DistinguishedName))
            {
                result["distinguishedName"] = [entry.DistinguishedName];
            }

            return result;
        }

        public void Dispose() => connection.Dispose();
    }
}
