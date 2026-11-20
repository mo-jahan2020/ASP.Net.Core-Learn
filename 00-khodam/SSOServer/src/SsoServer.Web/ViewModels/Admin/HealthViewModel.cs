using SsoServer.Domain.Interfaces;

namespace SsoServer.Web.ViewModels.Admin;

public sealed class HealthViewModel
{
    public SigningCertificateInfo SigningKey { get; set; } =
        new(SigningKeyStatus.NotConfigured, string.Empty, DateTimeOffset.MinValue, string.Empty);
}
