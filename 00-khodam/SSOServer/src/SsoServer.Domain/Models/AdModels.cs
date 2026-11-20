namespace SsoServer.Domain.Models;

/// <summary>Possible outcomes of an Active Directory credential check.</summary>
public enum AdAuthStatus
{
    /// <summary>Username + password validated successfully against Active Directory.</summary>
    Success = 0,

    /// <summary>Wrong username or password.</summary>
    InvalidCredentials = 1,

    /// <summary>The account is disabled in Active Directory.</summary>
    AccountDisabled = 2,

    /// <summary>The account is locked out in Active Directory.</summary>
    AccountLocked = 3,

    /// <summary>The account itself has expired in Active Directory.</summary>
    AccountExpired = 4,

    /// <summary>The domain password has expired and must be changed.</summary>
    PasswordExpired = 5,

    /// <summary>The user must change the password at next logon.</summary>
    PasswordMustChange = 6,

    /// <summary>The Active Directory server could not be reached or the query failed.</summary>
    ServerUnavailable = 7,

    /// <summary>Active Directory integration is disabled or not fully configured.</summary>
    NotConfigured = 8,
}

/// <summary>
/// Result of an Active Directory credential check. <see cref="Message"/> carries a
/// ready-to-display Persian message for every failure reason (wrong password,
/// disabled account, locked account, unreachable server, ...).
/// </summary>
public sealed record AdAuthResult(AdAuthStatus Status, string Message, string? DisplayName = null, string? Email = null)
{
    public bool IsSuccess => Status == AdAuthStatus.Success;

    public static AdAuthResult Ok(string? displayName, string? email)
        => new(AdAuthStatus.Success, string.Empty, displayName, email);

    public static AdAuthResult Fail(AdAuthStatus status, string message)
        => new(status, message);
}
