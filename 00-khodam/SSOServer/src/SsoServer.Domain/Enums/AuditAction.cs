namespace SsoServer.Domain.Enums;

public enum AuditAction
{
    LoginSucceeded = 1,
    LoginFailed = 2,
    Logout = 3,
    Lockout = 4,
    TokenIssued = 5,
    TokenRefreshed = 6,
    TokenRevoked = 7,
    SessionRevoked = 8,
    ConsentGranted = 9,
    ConsentDenied = 10,
    UserCreated = 11,
    UserUpdated = 12,
    UserDeleted = 13,
    ClientCreated = 14,
    ClientUpdated = 15,
    ClientDeleted = 16,
    ClientSecretRegenerated = 17,
    SettingsUpdated = 18,
    ClientAssigned = 19,
    ClientUnassigned = 20,
    ClientAccessDenied = 21,
}
