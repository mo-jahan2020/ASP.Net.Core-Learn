namespace SsoServer.Domain.Entities;

/// <summary>
/// Per-user client access whitelist ("user X may sign in to app Y").
/// A user with at least one row can only sign in to the assigned clients;
/// a user with no rows is not restricted and can access every enabled client.
/// Administrators (role <see cref="Options.SsoRoles.Admin"/>) are never restricted.
/// </summary>
public class ClientUserAssignment
{
    public int Id { get; set; }

    /// <summary>
    /// FK to <c>AspNetUsers.Id</c>. Must stay <c>nvarchar(450)</c> — exactly the same
    /// length as the ASP.NET Core Identity key column, otherwise SQL Server rejects
    /// the foreign key ("not the same length or scale").
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// FK to <see cref="ClientProfile.ClientId"/> (<c>nvarchar(100)</c>) — the OpenIddict
    /// client_id, e.g. "aspnetcore-sample".
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>User id of the admin who created the assignment (audit trail).</summary>
    public string AssignedBy { get; set; } = string.Empty;
}
