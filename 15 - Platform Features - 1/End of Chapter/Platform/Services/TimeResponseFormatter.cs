namespace Platform.Services {
    // پیاده‌سازی IResponseFormatter که پیش از محتوا، زمان جاری را نیز به پاسخ اضافه می‌کند
    public class TimeResponseFormatter : IResponseFormatter {
        // وابستگی به سرویس تولید مهر زمانی (Time Stamp)
        private ITimeStamper stamper;

        // دریافت ITimeStamper از طریق تزریق وابستگی
        public TimeResponseFormatter(ITimeStamper timeStamper) {
            stamper = timeStamper;
        }

        // نوشتن مهر زمانی به همراه محتوای ورودی در پاسخ
        public async Task Format(HttpContext context, string content) {
            await context.Response.WriteAsync($"{stamper.TimeStamp}: {content}");
        }
    }
}
