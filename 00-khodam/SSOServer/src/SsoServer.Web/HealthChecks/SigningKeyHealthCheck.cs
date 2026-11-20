using Microsoft.Extensions.Diagnostics.HealthChecks;
using SsoServer.Domain.Interfaces;

namespace SsoServer.Web.HealthChecks;

/// <summary>Signing-key readiness: certificate present, valid and not near expiry.</summary>
public sealed class SigningKeyHealthCheck : IHealthCheck
{
    private readonly ISigningCertificateProvider _provider;

    public SigningKeyHealthCheck(ISigningCertificateProvider provider) => _provider = provider;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var info = _provider.Describe();
        if (_provider.GetSigningCertificate() is null)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("گواهی امضای توکن در دسترس نیست."));
        }

        if (info.Status == SigningKeyStatus.NearExpiry)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"گواهی امضا تا {info.NotAfter:yyyy-MM-dd} منقضی می‌شود؛ به‌زودی آن را جایگزین کنید."));
        }

        return Task.FromResult(HealthCheckResult.Healthy($"گواهی امضا ({info.Status}): {info.Subject}، اعتبار تا {info.NotAfter:yyyy-MM-dd}."));
    }
}
