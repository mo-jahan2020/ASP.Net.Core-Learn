using Platform.Services;

namespace Platform {
    // کلاسی که یک Endpoint مربوط به اطلاعات آب و هوا را پیاده‌سازی می‌کند
    public class WeatherEndpoint {
        //private IResponseFormatter formatter;

        //public WeatherEndpoint(IResponseFormatter responseFormatter) {
        //    formatter = responseFormatter;
        //}

        // متد Endpoint که با استفاده از تزریق وابستگی، یک IResponseFormatter دریافت می‌کند
        // و پیام آب و هوا را با استفاده از آن قالب‌بندی و در پاسخ می‌نویسد
        public async Task Endpoint(HttpContext context,
                IResponseFormatter formatter) {
            await formatter.Format(context, "Endpoint Class: It is cloudy in Milan");
        }
    }
}
