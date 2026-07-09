// =============================================================================
// CountryRouteConstraint.cs - محدودیت مسیر (Route Constraint) برای کشورها
// این کلاس پیاده‌سازی IRouteConstraint را انجام می‌دهد تا فقط نام کشورهای مجاز
// در مسیرهای URL پذیرفته شوند (uk, france, monaco)
// =============================================================================

namespace Platform {
    /// <summary>
    /// CountryRouteConstraint - محدودیت سفارشی مسیر برای اعتبارسنجی نام کشورها
    /// فقط کشورهای تعریف شده در آرایه countries در مسیر URL مجاز هستند
    /// </summary>
    public class CountryRouteConstraint : IRouteConstraint {
        // لیست کشورهای مجاز - فقط این مقادیر در مسیر URL پذیرفته می‌شوند
        private static string[] countries = { "uk", "france", "monaco" };

        /// <summary>
        /// متد Match - بررسی اینکه آیا مقدار مسیر با یکی از کشورهای مجاز مطابقت دارد یا خیر
        /// </summary>
        /// <param name="httpContext">زمینه درخواست HTTP (می‌تواند null باشد)</param>
        /// <param name="route">مسیر فعلی (می‌تواند null باشد)</param>
        /// <param name="routeKey">کلید پارامتر در مسیر</param>
        /// <param name="values">دیکشنری مقادیر مسیر</param>
        /// <param name="routeDirection">جهت مسیر (درخواست ورودی یا تولید URL)</param>
        /// <returns>true اگر مقدار با یکی از کشورهای مجاز مطابقت داشته باشد</returns>
        public bool Match(HttpContext? httpContext, IRouter? route,
                string routeKey, RouteValueDictionary values,
                RouteDirection routeDirection) {
            // استخراج مقدار بخش مسیر بر اساس کلید پارامتر
            string segmentValue = values[routeKey] as string ?? "";
            // جستجوی مقدار در آرایه کشورهای مجاز (بدون حساسیت به حروف بزرگ/کوچک)
            return Array.IndexOf(countries, segmentValue.ToLower()) > -1;
        }
    }
}
