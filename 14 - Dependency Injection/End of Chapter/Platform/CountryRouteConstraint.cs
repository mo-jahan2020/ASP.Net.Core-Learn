// این کلاس یک "محدودیت مسیر سفارشی" (Custom Route Constraint) است.
// Route Constraint ها به ASP.NET Core می‌گویند که یک بخش از URL (مثل {country})
// چه مقادیری می‌تواند داشته باشد تا مسیر معتبر شناخته شود؛ در غیر این صورت
// درخواست با کد 404 مواجه می‌شود، بدون این‌که حتی وارد بدنه‌ی Endpoint شود.
﻿namespace Platform {

    // پیاده‌سازی اینترفیس IRouteConstraint که بخشی از سیستم Routing در ASP.NET Core است.
    public class CountryRouteConstraint : IRouteConstraint {
        // لیست ثابت (Whitelist) کشورهای مجاز برای این بخش از مسیر.
        private static string[] countries = { "uk", "france", "monaco" };

        // متد Match در زمان تطبیق مسیر (Routing) فراخوانی می‌شود.
        // اگر true برگرداند، یعنی مقدار موجود در URL با این محدودیت سازگار است.
        public bool Match(HttpContext? httpContext, IRouter? route,
                string routeKey, RouteValueDictionary values,
                RouteDirection routeDirection) {
            // استخراج مقدار سگمنت مسیر (مثلاً مقدار {country} در URL).
            string segmentValue = values[routeKey] as string ?? "";
            // بررسی این‌که آیا مقدار (با نادیده گرفتن بزرگی/کوچکی حروف) در لیست کشورهای مجاز وجود دارد یا خیر.
            return Array.IndexOf(countries, segmentValue.ToLower()) > -1;
        }
    }
}
