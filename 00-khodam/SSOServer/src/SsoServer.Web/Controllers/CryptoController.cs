using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenIddict.Abstractions;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;
using SsoServer.Infrastructure.Security;
using SsoServer.Web.Services.RateLimiting;

namespace SsoServer.Web.Controllers;

/// <summary>
/// endpoint کلید عمومی رمزنگاری رمز عبور — بروزرسانی امنیتی ۲۰۲۶-۱۱-۱۹ (بند ۲).
/// کلاینت‌ها قبل از password grant این کلید را می‌خوانند، رمز عبور را با آن
/// (RSA-OAEP-SHA256) رمز می‌کنند و در پارامتر password_cipher ارسال می‌کنند.
/// کلید خصوصی فقط سمت سرور می‌ماند؛ روی شبکه‌ی بدون HTTPS، شنودکننده رمز را
/// نمی‌بیند.
///
/// بروزرسانی امنیتی ۲۰۲۶-۱۱-۱۹ (اصلاح endpoint کلید عمومی):
/// این endpoint از حالت «GET عمومی بدون احراز هویت» به «احراز هویت کلاینت» تغییر کرد.
/// دلیل: در مدل اعتماد این سرور، هر فراخوانی سمت سرور باید با جفت
/// client_id + client_secret معتبر همراه باشد (همان سیاست /connect/token و
/// /connect/windows). اگر کلید عمومی بدون احراز هویت منتشر شود:
///  - هر برنامه/اسکنر خارجی بدون ثبت‌نام می‌تواند با سرور SSO تعامل کند و
///    فرمت/اندازه کلید و پیام‌های خطا را برای حمله هدفمند برآورد کند (Recon)؛
///  - endpoint بدون احراز هویت، سطح حملات DoS/جمع‌آوری کلید را باز می‌کند.
/// توجه: کلید عمومی RSA ذاتاً محرمانه نیست (رمزگشایی ممکن نیست بدون کلید خصوصی)؛
/// این محدودیت «دفاع در عمق» و هم‌راستا کردن این endpoint با سیاست trust کل سیستم
/// است، نه ترمیم یک نشت رمزنگارانه. استفاده از TLS در محیط تولید همچنان الزامی است.
/// | Public key endpoint for client-side password encryption (RSA-OAEP).
/// Hardened on 2026-11-19: the endpoint now requires a valid client_id +
/// client_secret pair (same policy as /connect/token and /connect/windows).
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)]
public class CryptoController : Controller
{
    private readonly PasswordEncryptionService _encryption;
    private readonly IOpenIddictApplicationManager _applications;
    private readonly IAuditService _audit;

    public CryptoController(
        PasswordEncryptionService encryption,
        IOpenIddictApplicationManager applications,
        IAuditService audit)
    {
        _encryption = encryption;
        _applications = applications;
        _audit = audit;
    }

    /// <summary>
    /// GET /connect/crypto/key — کلید عمومی RSA به فرمت Base64 (SPKI).
    /// پارامترهای الزامی: client_id و client_secret (query string یا فرم —
    /// هماهنگ با الگوی /connect/windows). بدون جفت معتبر، پاسخ 401 با
    /// error=invalid_client است و کلید برگردانده نمی‌شود.
    /// </summary>
    [HttpGet("~/connect/crypto/key")]
    [Produces("application/json")]
    [EnableRateLimiting(RateLimitPolicies.Token)]
    public async Task<IActionResult> Key(
        [FromQuery(Name = "client_id")] string? clientId,
        [FromQuery(Name = "client_secret")] string? clientSecret)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        // ۱) الزام client_id + client_secret (سیاست trust سرور — بدون این دو رد می‌شود).
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            return UnauthorizedError("برای دریافت کلید عمومی رمزنگاری، ClientId و Client Secret الزامی است.");
        }

        // ۲) اعتبارسنجی جفت client_id + client_secret — همان منطق /connect/windows
        //    (FindByClientIdAsync + ValidateClientSecretAsync از OpenIddict).
        var application = await _applications.FindByClientIdAsync(clientId);
        if (application is null || !await _applications.ValidateClientSecretAsync(application, clientSecret))
        {
            await _audit.LogAsync(AuditAction.LoginFailed, actorUserName: null, clientId: clientId, ip: ip,
                data: new { Grant = "crypto-key", Reason = "invalid-client" });
            return UnauthorizedError("شناسه کلاینت یا Client Secret نادرست است.");
        }

        // ۳) کلاینت معتبر — کلید عمومی برگردانده می‌شود (بدون لاگ موفق؛ خواندن کلید
        //    یک عملیات فقط-خواندنی کم‌ریسک است و لاگ مرتبط در /connect/token ثبت می‌شود).
        return Ok(new
        {
            alg = "RSA-OAEP-256",
            use = "enc",
            public_key = _encryption.GetPublicKeyBase64(),
        });
    }

    /// <summary>
    /// پاسخ 401 استاندارد با بدنه‌ی JSON خطای OAuth — همان شکل /connect/windows،
    /// تا کلاینت‌ها بتوانند error_description فارسی را مستقیم نمایش دهند.
    /// </summary>
    private IActionResult UnauthorizedError(string description) =>
        StatusCode(StatusCodes.Status401Unauthorized, new Dictionary<string, string>
        {
            ["error"] = "invalid_client",
            ["error_description"] = description,
        });
}
