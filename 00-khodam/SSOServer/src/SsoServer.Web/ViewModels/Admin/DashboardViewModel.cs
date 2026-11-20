using SsoServer.Domain.Entities;

namespace SsoServer.Web.ViewModels.Admin;

public sealed class DashboardViewModel
{
    public int UserCount { get; set; }

    public int ActiveUserCount { get; set; }

    public int AdminCount { get; set; }

    public int ClientCount { get; set; }

    public int EnabledClientCount { get; set; }

    public IReadOnlyList<AuditLog> RecentAudit { get; set; } = [];
}
