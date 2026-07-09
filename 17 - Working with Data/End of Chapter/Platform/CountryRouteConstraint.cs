namespace Platform {

    // یک محدودیت مسیر (Route Constraint) سفارشی که فقط اجازه می‌دهد
    // مقدار پارامتر مسیر یکی از نام‌های کشورهای مشخص‌شده باشد
    public class CountryRouteConstraint : IRouteConstraint {
        // لیست کشورهای مجاز
        private static string[] countries = { "uk", "france", "monaco" };

        // متدی که ASP.NET Core برای بررسی تطابق مقدار پارامتر مسیر فراخوانی می‌کند
        public bool Match(HttpContext? httpContext, IRouter? route,
                string routeKey, RouteValueDictionary values,
                RouteDirection routeDirection) {
            // خواندن مقدار پارامتر مسیر مربوطه
            string segmentValue = values[routeKey] as string ?? "";
            // بررسی اینکه آیا مقدار در لیست کشورهای مجاز وجود دارد یا خیر
            return Array.IndexOf(countries, segmentValue.ToLower()) > -1;
        }
    }
}
