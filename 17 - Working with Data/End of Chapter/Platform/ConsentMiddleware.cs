using Microsoft.AspNetCore.Http.Features;

namespace Platform {
    // میان‌افزاری که برای مسیر "/consent" وضعیت رضایت (Consent) کاربر نسبت به کوکی‌ها را
    // تغییر می‌دهد (اعطا یا لغو رضایت)
    public class ConsentMiddleware {
        // مرجع به Middleware بعدی در زنجیره پردازش
        private RequestDelegate next;

        // سازنده‌ای که Middleware بعدی را دریافت می‌کند
        public ConsentMiddleware(RequestDelegate nextDelgate) {
            next = nextDelgate;
        }

        // متدی که برای هر درخواست فراخوانی می‌شود
        public async Task Invoke(HttpContext context) {
            // اگر مسیر درخواست دقیقا "/consent" باشد
            if (context.Request.Path == "/consent") {
                // دریافت ویژگی (Feature) مربوط به ردیابی رضایت کاربر از HttpContext
                ITrackingConsentFeature? consentFeature
                    = context.Features.Get<ITrackingConsentFeature>();
                if (consentFeature != null) {
                    // اگر کاربر هنوز رضایت نداده، رضایت را اعطا کن
                    if (!consentFeature.HasConsent) {
                        consentFeature.GrantConsent();
                    } else {
                        // در غیر این صورت (رضایت قبلاً داده شده)، آن را لغو کن
                        consentFeature.WithdrawConsent();
                    }
                    // نمایش وضعیت فعلی رضایت در پاسخ
                    await context.Response.WriteAsync(consentFeature.HasConsent
                        ? "Consent Granted \n" : "Consent Withdrawn\n");
                }
            } else {
                // در غیر این صورت درخواست را به Middleware بعدی پاس بده
                await next(context);
            }
        }
    }
}
