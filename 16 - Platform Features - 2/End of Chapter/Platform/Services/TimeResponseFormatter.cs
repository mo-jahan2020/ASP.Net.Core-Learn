// =============================================================================
// TimeResponseFormatter.cs - فرمت‌کننده پاسخ با زمان‌سنجی
// این کلاس محتوا را به همراه زمان فعلی (از ITimeStamper) نمایش می‌دهد
// از تزریق وابستگی برای دریافت سرویس زمان‌سنجی استفاده می‌کند
// =============================================================================

namespace Platform.Services {
    /// <summary>
    /// TimeResponseFormatter - فرمت‌کننده پاسخ با مهر زمانی
    /// در ابتدای هر پاسخ، زمان فعلی از سرویس ITimeStamper دریافت و نمایش داده می‌شود
    /// این کلاس نشان‌دهنده استفاده از تزریق وابستگی در فرمت‌کننده‌ها است
    /// </summary>
    public class TimeResponseFormatter : IResponseFormatter {
        // ارجاع به سرویس زمان‌سنجی برای دریافت زمان فعلی
        private ITimeStamper stamper;

        /// <summary>
        /// سازنده - تزریق سرویس زمان‌سنجی از طریق DI Container
        /// </summary>
        /// <param name="timeStamper">سرویس پیاده‌سازی‌کننده ITimeStamper</param>
        public TimeResponseFormatter(ITimeStamper timeStamper) {
            stamper = timeStamper; // ذخیره ارجاع به سرویس زمان‌سنجی
        }

        /// <summary>
        /// Format - فرمت‌بندی محتوا با اضافه کردن مهر زمانی
        /// خروجی به فرمت "زمان: محتوا" نوشته می‌شود
        /// </summary>
        /// <param name="context">زمینه درخواست HTTP</param>
        /// <param name="content">محتوای متنی برای نمایش</param>
        public async Task Format(HttpContext context, string content) {
            // نوشتن مهر زمانی به همراه محتوا در پاسخ
            await context.Response.WriteAsync($"{stamper.TimeStamp}: {content}");
        }
    }
}
