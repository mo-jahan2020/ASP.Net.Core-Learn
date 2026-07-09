// =============================================================================
// IResponseFormatter.cs - اینترفیس فرمت‌کننده پاسخ
// این اینترفیس یک قرارداد (Contract) برای تمام فرمت‌کننده‌های پاسخ تعریف می‌کند
// هر کلاسی که این اینترفیس را پیاده‌سازی کند باید متد Format را داشته باشد
// =============================================================================

namespace Platform.Services {
    /// <summary>
    /// IResponseFormatter - اینترفیس فرمت‌کننده پاسخ
    /// این اینترفیس امکان تعویض و تزریق وابستگی فرمت‌کننده‌های مختلف را فراهم می‌کند
    /// فرمت‌کننده‌های مختلف (HTML، متن ساده، زمان‌دار و ...) این اینترفیس را پیاده‌سازی می‌کنند
    /// </summary>
    public interface IResponseFormatter {
        /// <summary>
        /// Format - فرمت‌بندی و نوشتن محتوا در پاسخ HTTP
        /// </summary>
        /// <param name="context">زمینه درخواست HTTP جاری برای دسترسی به Response</param>
        /// <param name="content">محتوای متنی که باید فرمت‌بندی و نمایش داده شود</param>
        /// <returns>Task ناهمگام برای نوشتن در پاسخ</returns>
        Task Format(HttpContext context, string content);

        /// <summary>
        /// RichOutput - مشخص می‌کند آیا این فرمت‌کننده خروجی غنی (مانند HTML) تولید می‌کند
        /// مقدار پیش‌فرض false است (خروجی ساده/متنی)
        /// فرمت‌کننده‌هایی مانند HtmlResponseFormatter این مقدار را به true تغییر می‌دهند
        /// </summary>
        public bool RichOutput => false;
    }
}
