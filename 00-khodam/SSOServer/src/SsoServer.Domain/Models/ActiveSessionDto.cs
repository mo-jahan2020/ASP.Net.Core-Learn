namespace SsoServer.Domain.Models;

/// <summary>A human-readable active session: one OpenIddict authorization and its tokens.</summary>
public class ActiveSessionDto
{
    public required string AuthorizationId { get; set; }

    public required string Subject { get; set; }

    /// <summary>نام کاربری صاحب نشست (برای نمایش در صفحه «مدیریت نشست‌های فعال» ادمین).</summary>
    public string UserName { get; set; } = string.Empty;

    public required string ClientId { get; set; }

    public string ClientDisplayName { get; set; } = string.Empty;

    /// <summary>permanent = consented via UI; adhoc = API grants (password etc.).</summary>
    public string Type { get; set; } = "permanent";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? LastUsedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public IReadOnlyList<string> Scopes { get; set; } = [];
}
