// =============================================================================
// WeatherEndpoint.cs - اندپوینت اطلاعات آب و هوا
// این کلاس یک اندپوینت برای نمایش اطلاعات آب و هوا فراهم می‌کند
// از الگوی IResponseFormatter برای قالب‌بندی پاسخ استفاده می‌شود
// =============================================================================

using Platform.Services; // وارد کردن سرویس‌های قالب‌بندی پاسخ

namespace Platform {
    /// <summary>
    /// WeatherEndpoint - کلاس اندپوینت آب و هوا
    /// این کلاس اطلاعات آب و هوا را با استفاده از فرمت‌کننده تزریق‌شده نمایش می‌دهد
    /// </summary>
    public class WeatherEndpoint {
        //private IResponseFormatter formatter;

        // سازنده با تزریق وابستگی - در حال حاضر غیرفعال شده
        // به جای آن، formatter مستقیماً در متد Endpoint تزریق می‌شود
        //public WeatherEndpoint(IResponseFormatter responseFormatter) {
        //    formatter = responseFormatter;
        //}

        /// <summary>
        /// متد اندپوینت - نمایش اطلاعات آب و هوا با استفاده از فرمت‌کننده
        /// فرمت‌کننده پاسخ (IResponseFormatter) به صورت مستقیم در پارامترها تزریق می‌شود
        /// </summary>
        /// <param name="context">زمینه درخواست HTTP جاری</param>
        /// <param name="formatter">فرمت‌کننده پاسخ برای قالب‌بندی خروجی</param>
        public async Task Endpoint(HttpContext context,
                IResponseFormatter formatter) {
            // فرمت‌بندی و نوشتن پیام آب و هوا در پاسخ با استفاده از فرمت‌کننده تزریق‌شده
            await formatter.Format(context, "Endpoint Class: It is cloudy in Milan");
        }
    }
}
