namespace Platform 
{
    // این کلاس جمعیت شهرها را برمی‌گرداند.
    // ساختار آن شبیه Capital است اما روی شهر تمرکز دارد.
    public class Population 
    {

        // این Endpoint نام شهر را از مسیر می‌خواند و جمعیت را برمی‌گرداند.
        public static async Task Endpoint(HttpContext context) 
        {
            // اگر مقدار city در مسیر وجود نداشته باشد، london به‌عنوان پیش‌فرض انتخاب می‌شود.
            string city = context.Request.RouteValues["city"] as string ?? "london";
            int? pop = null;

            // تعیین جمعیت با استفاده از switch
            switch (city.ToLower()) 
            {
                case "london":
                    pop = 8_136_000;
                    break;
                case "paris":
                    pop = 2_141_000;
                    break;
                case "monaco":
                    pop = 39_000;
                    break;
            }

            // اگر جمعیت مشخص شد، در پاسخ نمایش داده می‌شود.
            if (pop.HasValue) 
            {
                await context.Response.WriteAsync($"City: {city}, Population: {pop}");
            } 
            else 
            {
                // اگر شهر شناخته نشود، پاسخ 404 برمی‌گردد.
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }
    }
}
