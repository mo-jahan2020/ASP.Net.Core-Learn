using SsoServer.Domain.Enums;

namespace SsoServer.Domain.Entities;

/// <summary>Persistent audit trail row for security-relevant events.</summary>
public class AuditLog
{
    public long Id { get; set; }

    public AuditAction Action { get; set; }

    public string? ActorUserId { get; set; }

    public string? ActorUserName { get; set; }

    public string? ClientId { get; set; }

    public string? Ip { get; set; }

    public string DataJson { get; set; } = "{}";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
