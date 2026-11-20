namespace SsoServer.Domain.Enums;

/// <summary>Allowed OAuth 2.0 flows for a registered client application.</summary>
[Flags]
public enum ClientFlow
{
    None = 0,

    /// <summary>Authorization Code flow; PKCE (S256) is always required.</summary>
    AuthorizationCode = 1,

    RefreshToken = 2,

    /// <summary>Legacy resource-owner password grant; confidential clients only.</summary>
    Password = 4,

    /// <summary>Machine-to-machine; confidential clients only.</summary>
    ClientCredentials = 8,
}
