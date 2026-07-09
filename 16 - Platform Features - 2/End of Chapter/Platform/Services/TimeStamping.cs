// =============================================================================
// TimeStamping.cs - اینترفیس و پیاده‌سازی پیش‌فرض زمان‌سنجی
// شامل اینترفیس ITimeStamper و پیاده‌سازی پیش‌فرض DefaultTimeStamper
// از این الگو برای انتزاع منبع زمان و امکان جایگزینی آسان استفاده می‌شود
// =============================================================================

namespace Platform.Services {
    /// <summary>
    /// ITimeStamper - اینترفیس زمان‌سنجی
    /// این اینترفیس یک قرارداد برای دریافت مهر زمانی (Timestamp) تعریف می‌کند
    /// پیاده‌سازی‌های مختلف می‌توانند منابع زمان متفاوتی استفاده کنند
    /// </summary>
    public interface ITimeStamper {
        /// <summary>
        /// TimeStamp - دریافت مهر زمانی فعلی به صورت رشته
        /// </summary>
        string TimeStamp { get; }
    }

    /// <summary>
    /// DefaultTimeStamper - پیاده‌سازی پیش‌فرض زمان‌سنجی
    /// از DateTime.Now برای دریافت زمان فعلی سیستم استفاده می‌کند
    /// زمان را به فرمت کوتاه (ShortTimeString) نمایش می‌دهد
    /// </summary>
    public class DefaultTimeStamper : ITimeStamper {
        /// <summary>
        /// TimeStamp - دریافت زمان فعلی سیستم به فرمت کوتاه
        /// مثال خروجی: "2:30 PM"
        /// </summary>
        public string TimeStamp {
            get => DateTime.Now.ToShortTimeString(); // تبدیل زمان فعلی به فرمت کوتاه
        }
    }
}
