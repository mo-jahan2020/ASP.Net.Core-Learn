// این فایل شامل تعریف یک اینترفیس ساده (ITimeStamper) و یک پیاده‌سازی پیش‌فرض آن است.
// هدف از این طراحی، امکان تعویض منبع زمان (مثلاً برای تست‌نویسی با Mock کردن ITimeStamper)
// بدون نیاز به تغییر کلاس‌هایی است که به آن وابسته‌اند (مثل TimeResponseFormatter).
﻿namespace Platform.Services {

    // قرارداد ساده برای هر چیزی که بتواند یک "برچسب زمانی" ارائه دهد.
    public interface ITimeStamper {
        string TimeStamp { get; }
    }

    // پیاده‌سازی پیش‌فرض که از ساعت سیستم برای تولید برچسب زمانی استفاده می‌کند.
    public class DefaultTimeStamper : ITimeStamper {

        // هر بار که این پراپرتی خوانده شود، زمان فعلی (به‌صورت کوتاه) محاسبه و برگردانده می‌شود.
        public string TimeStamp {
            get => DateTime.Now.ToShortTimeString();
        }
    }
}
