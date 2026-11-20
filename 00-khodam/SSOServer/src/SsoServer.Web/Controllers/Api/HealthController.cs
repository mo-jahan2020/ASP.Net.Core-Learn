using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SsoServer.Domain.Interfaces;
using SsoServer.Infrastructure.ActiveDirectory;
using SsoServer.Infrastructure.Persistence;
using SsoServer.Web.Helpers;

namespace SsoServer.Web.Controllers.Api;

/// <summary>
/// اطلاعات تفصیلی سلامت (فقط مدیر): پایگاه داده، گواهی امضا و اتصال اکتیو دایرکتوری/LDAP.
/// حالت‌های Disabled و NotConfigured صرفاً اطلاع‌رسان‌اند و هرگز سلامت کلی را خراب نمی‌کنند.
/// </summary>
[Authorize(Policy = SsoPolicies.Admin)]
public class HealthController : SsoApiController
{
    private readonly ISigningCertificateProvider _signing;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ActiveDirectorySettings _adSettings;
    private readonly ILdapConnectionFactory _ldapConnections;

    public HealthController(
        ISigningCertificateProvider signing,
        IServiceScopeFactory scopeFactory,
        IOptions<ActiveDirectorySettings> adSettings,
        ILdapConnectionFactory ldapConnections)
    {
        _signing = signing;
        _scopeFactory = scopeFactory;
        _adSettings = adSettings.Value;
        _ldapConnections = ldapConnections;
    }

    /// <summary>گزارش تفصیلی سلامت: پایگاه داده، گواهی امضا و اکتیو دایرکتوری.</summary>
    [HttpGet("health/detail")]
    public async Task<IActionResult> Detail()
    {
        bool databaseOk;
        string databaseError = string.Empty;
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            databaseOk = await context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            databaseOk = false;
            databaseError = ex.Message;
        }

        var signing = _signing.Describe();
        var signingOk = _signing.GetSigningCertificate() is not null;

        var ad = CheckActiveDirectory();
        var overallOk = databaseOk && signingOk && ad.Status != "Unhealthy";

        return Ok(new
        {
            status = overallOk ? "Healthy" : "Unhealthy",
            checks = new object[]
            {
                new { name = "database", status = databaseOk ? "Healthy" : "Unhealthy", detail = databaseError },
                new
                {
                    name = "signing-key",
                    status = signingOk ? signing.Status.ToString() : "NotConfigured",
                    detail = signingOk
                        ? $"Subject: {signing.Subject} | NotAfter: {signing.NotAfter:yyyy-MM-dd} | Source: {signing.Source}"
                        : "گواهی امضا در دسترس نیست.",
                },
                new { name = "active-directory", status = ad.Status, detail = ad.Detail },
            },
            generatedAt = DateTimeOffset.UtcNow,
        });
    }

    /// <summary>
    /// آزمایش زندهٔ LDAP: bind با حساب سرویسِ تنظیم‌شده. حالت‌های Disabled و
    /// NotConfigured اطلاع‌رسان‌اند و هرگز سلامت کلی را خراب نمی‌کنند.
    /// </summary>
    private (string Status, string Detail) CheckActiveDirectory()
    {
        if (!_adSettings.Enabled)
        {
            return ("Disabled", "ورود یکپارچه اکتیو دایرکتوری در تنظیمات فعال نیست.");
        }

        var configurationError = _adSettings.GetConfigurationError();
        if (configurationError is not null)
        {
            return ("NotConfigured", configurationError);
        }

        try
        {
            using var connection = _ldapConnections.CreateServiceConnection();
            connection.Bind();
            return ("Healthy",
                $"{_adSettings.CleanHost}:{_adSettings.EffectivePort} | AuthType: {_adSettings.AuthType}");
        }
        catch (Exception ex)
        {
            return ("Unhealthy",
                $"اتصال به {_adSettings.CleanHost}:{_adSettings.EffectivePort} برقرار نشد: {ex.Message}");
        }
    }
}
