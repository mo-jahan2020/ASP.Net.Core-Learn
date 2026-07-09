namespace Platform.Services {

    // اینترفیس برای سرویسی که یک مهر زمانی (Time Stamp) به صورت رشته برمی‌گرداند
    public interface ITimeStamper {
        string TimeStamp { get; }
    }

    // پیاده‌سازی پیش‌فرض ITimeStamper که زمان جاری سیستم را برمی‌گرداند
    public class DefaultTimeStamper : ITimeStamper {

        // بازگرداندن زمان جاری به صورت رشته کوتاه (مثلاً 3:15 PM)
        public string TimeStamp {
            get => DateTime.Now.ToShortTimeString();
        }
    }
}
