namespace SsoServer.Domain.Enums;

/// <summary>OAuth client type: public (no secret) or confidential (secret-protected).</summary>
public enum ClientKind
{
    Public = 0,
    Confidential = 1,
}
