namespace SsoServer.Infrastructure.ActiveDirectory;

/// <summary>
/// A thin, mockable LDAP connection abstraction over
/// <see cref="System.DirectoryServices.Protocols.LdapConnection"/>.
/// </summary>
public interface ILdapConnection : IDisposable
{
    /// <summary>
    /// Binds with the credentials the connection was created with.
    /// Throws <see cref="System.DirectoryServices.Protocols.LdapException"/> on failure
    /// (error 49 = rejected credentials / account problem).
    /// </summary>
    void Bind();

    /// <summary>
    /// Runs a subtree search and returns the attributes of the first matching entry,
    /// or null when nothing matched. Attribute names are case-insensitive.
    /// </summary>
    IReadOnlyDictionary<string, IReadOnlyList<string>>? SearchFirst(
        string baseDn, string ldapFilter, IReadOnlyList<string> attributes);
}

/// <summary>Creates preconfigured LDAP connections (host/port/SSL/auth type from settings).</summary>
public interface ILdapConnectionFactory
{
    /// <summary>
    /// Connection that authenticates with the configured service account —
    /// used for directory queries (finding the user's DN and account flags).
    /// </summary>
    ILdapConnection CreateServiceConnection();

    /// <summary>
    /// Connection that authenticates with the end-user's own credentials —
    /// the actual credential check is the LDAP bind itself.
    /// </summary>
    ILdapConnection CreateUserConnection(string username, string password);
}
