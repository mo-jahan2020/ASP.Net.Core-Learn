namespace Platform 
{

    // Route Constraint سفارشی برای محدود کردن مقدار country در URL
    // فقط کشورهایی که در آرایه زیر آمده‌اند معتبر شناخته می‌شوند.
    public class CountryRouteConstraint : IRouteConstraint 
    {
        // فهرست کشورهایی که این Constraint قبول می‌کند.
        private static string[] countries = { "uk", "france", "monaco" };

        // متد Match تعیین می‌کند آیا مقدار مسیر با شرط ما سازگار هست یا نه.
        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            // مقدار segment موردنظر از RouteValues خوانده می‌شود.
            string segmentValue = values[routeKey] as string ?? "";

            // اگر مقدار در آرایه countries وجود داشته باشد، نتیجه true خواهد بود.
            return Array.IndexOf(countries, segmentValue.ToLower()) > -1;
        }
    }
}
