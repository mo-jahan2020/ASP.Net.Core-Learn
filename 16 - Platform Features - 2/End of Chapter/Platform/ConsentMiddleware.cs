// =============================================================================
// ConsentMiddleware.cs - میان‌افزار مدیریت رضایت ردیابی (Tracking Consent)
// این میان‌افزار برای تغییر وضعیت رضایت کاربر به ردیابی استفاده می‌شود
// با درخواست به مسیر /consent وضعیت رضایت تغییر می‌کند
// =============================================================================

using Microsoft.AspNetCore.Http.Features; // وارد کردن ویژگی‌های HTTP شامل ITrackingConsentFeature

namespace Platform {
    /// <summary>
    /// میان‌افزار ConsentMiddleware - مدیریت رضایت ردیابی کاربر
    /// این کلاس با استفاده از ITrackingConsentFeature قابلیت اعطا یا لغو رضایت
    /// ردیابی کوکی‌ها را فراهم می‌کند
    /// </summary>
    public class ConsentMiddleware {
        // ارجاع به میان‌افزار بعدی در زنجیره پردازش
        private RequestDelegate next;

        /// <summary>
        /// سازنده کلاس - دریافت ارجاع میان‌افزار بعدی
        /// </summary>
        /// <param name="nextDelgate">دلیگیت میان‌افزار بعدی در خط لوله</param>
        public ConsentMiddleware(RequestDelegate nextDelgate) {
            next = nextDelgate;
        }

        /// <summary>
        /// متد Invoke - پردازش درخواست HTTP
        /// اگر مسیر /consent باشد، وضعیت رضایت تغییر می‌کند
        /// در غیر این صورت، درخواست به میان‌افزار بعدی ارجاع داده می‌شود
        /// </summary>
        /// <param name="context">زمینه درخواست HTTP جاری</param>
        public async Task Invoke(HttpContext context) {
            if (context.Request.Path == "/consent") {
                // استخراج ویژگی رضایت ردیابی از ویژگی‌های درخواست
                ITrackingConsentFeature? consentFeature
                    = context.Features.Get<ITrackingConsentFeature>();
                if (consentFeature != null) {
                    // تغییر وضعیت رضایت: اگر رضایت ندارد، آن را اعطا کند و بالعکس
                    if (!consentFeature.HasConsent) {
                        consentFeature.GrantConsent(); // اعطای رضایت ردیابی
                    } else {
                        consentFeature.WithdrawConsent(); // لغو رضایت ردیابی
                    }
                    // نمایش وضعیت نهایی رضایت در پاسخ
                    await context.Response.WriteAsync(consentFeature.HasConsent
                        ? "Consent Granted \n" : "Consent Withdrawn\n");
                }
            } else {
                // ارجاع درخواست به میان‌افزار بعدی در زنجیره
                await next(context);
            }
        }
    }
}
