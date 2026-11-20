using Microsoft.AspNetCore.Mvc;
using SsoServer.Domain.Interfaces;
using SsoServer.Infrastructure.Persistence;
using SsoServer.Web.ViewModels.Admin;

namespace SsoServer.Web.Controllers;

// Health page + live service-status JSON of the admin console (part of AdminController).
public partial class AdminController
{
    // ---------------- Health ----------------

    [HttpGet]
    public async Task<IActionResult> Health()
    {
        ViewBag.Title = "وضعیت سلامت سامانه";
        return View(new HealthViewModel
        {
            SigningKey = _signing.Describe(),
        });
    }

    /// <summary>
    /// Live service status (JSON) polled every 30 seconds by the admin health page.
    /// Deliberately a cookie-authenticated MVC action rather than a /api/v1 endpoint:
    /// the health page runs in the admin's browser and only carries the session cookie,
    /// not an OpenIddict bearer token. The public HTTP endpoints (discovery, JWKS,
    /// swagger, /health) are checked straight from the browser by health.js so the
    /// indicators reflect exactly what a client machine experiences.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> HealthServices()
    {
        Response.Headers.CacheControl = "no-store";

        // --- Database: can we actually connect? ---
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        bool databaseOk;
        string databaseDetail;
        try
        {
            var context = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            databaseOk = await context.Database.CanConnectAsync();
            databaseDetail = databaseOk
                ? context.Database.ProviderName ?? "unknown"
                : "اتصال به پایگاه داده برقرار نشد.";
        }
        catch (Exception ex)
        {
            databaseOk = false;
            databaseDetail = ex.Message;
        }
        stopwatch.Stop();
        if (databaseOk)
        {
            databaseDetail += stopwatch.ElapsedMilliseconds > 0
                ? $" — پاسخ در {stopwatch.ElapsedMilliseconds} میلی‌ثانیه"
                : " — پاسخ در کمتر از ۱ میلی‌ثانیه";
        }

        // --- Signing certificate: present / near expiry / missing ---
        var signing = _signing.Describe();
        var signingOk = _signing.GetSigningCertificate() is not null;
        var signingStatus = !signingOk ? "down"
            : signing.Status == SigningKeyStatus.NearExpiry ? "degraded" : "up";
        var signingDetail = !signingOk
            ? "گواهی امضای توکن در دسترس نیست."
            : signing.Status == SigningKeyStatus.NearExpiry
                ? $"گواهی تا {signing.NotAfter:yyyy-MM-dd} منقضی می‌شود؛ آن را جایگزین کنید."
                : $"اعتبار گواهی تا {signing.NotAfter:yyyy-MM-dd}";

        // --- Token-pruning background service ---
        var pruning = _pruning.Snapshot();
        var pruningStatus = pruning.LastError is null ? "up" : "down";
        string pruningDetail;
        if (pruning.LastError is not null)
        {
            pruningDetail = $"خطای آخرین اجرا: {pruning.LastError}";
        }
        else if (pruning.LastRunCompletedAtUtc is { } lastRun)
        {
            pruningDetail = "آخرین اجرا: " + lastRun.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                + $" ({pruning.LastPrunedTokens} توکن، {pruning.LastPrunedAuthorizations} مجوز پاک‌سازی شد)";
        }
        else
        {
            pruningDetail = "هنوز اجرا نشده؛ نخستین اجرا ۳۰ ثانیه پس از راه‌اندازی انجام می‌شود.";
        }
        if (pruning.LastError is null && pruning.NextRunExpectedAtUtc is { } next)
        {
            pruningDetail += " — اجرای بعدی: " + next.ToLocalTime().ToString("HH:mm");
        }

        // --- Active Directory / LDAP: live bind with the configured service account ---
        // "disabled" (integration switched off) and "degraded" (enabled but not fully
        // configured) never mark the whole system red; only a real connection failure does.
        var adStopwatch = System.Diagnostics.Stopwatch.StartNew();
        string adStatus;
        string adDetail;
        if (!_adSettings.Enabled)
        {
            adStatus = "disabled";
            adDetail = "ورود یکپارچه اکتیو دایرکتوری در تنظیمات فعال نیست.";
        }
        else if (_adSettings.GetConfigurationError() is { } adConfigError)
        {
            adStatus = "degraded";
            adDetail = adConfigError;
        }
        else
        {
            try
            {
                using var ldap = _ldapConnections.CreateServiceConnection();
                ldap.Bind();
                adStopwatch.Stop();
                adStatus = "up";
                adDetail = $"{_adSettings.CleanHost}:{_adSettings.EffectivePort} | نوع احراز هویت: {_adSettings.AuthType}"
                    + (adStopwatch.ElapsedMilliseconds > 0
                        ? $" — پاسخ در {adStopwatch.ElapsedMilliseconds} میلی‌ثانیه"
                        : " — پاسخ در کمتر از ۱ میلی‌ثانیه");
            }
            catch (Exception ex)
            {
                adStatus = "down";
                adDetail = $"اتصال به {_adSettings.CleanHost}:{_adSettings.EffectivePort} برقرار نشد: {ex.Message}";
            }
        }

        var results = new List<(string Key, string Status, string Detail, long DurationMs)>
        {
            ("database", databaseOk ? "up" : "down", databaseDetail, stopwatch.ElapsedMilliseconds),
            ("signing-key", signingStatus, signingDetail, 0),
            ("token-pruning", pruningStatus, pruningDetail, 0),
            ("active-directory", adStatus, adDetail, adStopwatch.ElapsedMilliseconds),
        };

        var overall = results.Any(r => r.Status == "down") ? "down"
            : results.Any(r => r.Status == "degraded") ? "degraded"
            : "up";

        return Json(new
        {
            generatedAt = DateTimeOffset.UtcNow,
            overall,
            services = results.Select(r => new
            {
                key = r.Key,
                status = r.Status,
                detail = r.Detail,
                durationMs = r.DurationMs,
            }),
        });
    }
}
