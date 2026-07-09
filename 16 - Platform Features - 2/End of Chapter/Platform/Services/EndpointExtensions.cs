// =============================================================================
// EndpointExtensions.cs - متد توسعه (Extension Method) برای ثبت اندپوینت‌ها
// این کلاس یک متد تعمیم‌یافته فراهم می‌کند که با استفاده از Reflection
// به صورت خودکار متد Endpoint کلاس‌ها را پیدا کرده و به عنوان اندپوینت GET ثبت می‌کند
// =============================================================================

using System.Reflection; // وارد کردن فضای نام Reflection برای کار با متادیتای کلاس‌ها

namespace Microsoft.AspNetCore.Builder {
    /// <summary>
    /// EndpointExtensions - کلاس استاتیک شامل متدهای توسعه برای IEndpointRouteBuilder
    /// این متدها امکان ثبت خودکار اندپوینت‌ها بر اساس کنوانسیون نام‌گذاری را فراهم می‌کنند
    /// </summary>
    public static class EndpointExtensions {
        /// <summary>
        /// MapEndpoint - متد تعمیم‌یافته برای ثبت خودکار اندپوینت‌ها
        /// با استفاده از Reflection متد مشخصی از یک کلاس را پیدا کرده و
        /// آن را به عنوان یک اندپوینت HTTP GET در مسیر مشخص شده ثبت می‌کند
        /// پارامترهای متد به صورت خودکار از DI Container حل می‌شوند
        /// </summary>
        /// <typeparam name="T">نوع کلاس اندپوینت</typeparam>
        /// <param name="app">بیلدر مسیر اندپوینت‌ها</param>
        /// <param name="path">مسیر URL برای ثبت اندپوینت</param>
        /// <param name="methodName">نام متد اندپوینت (پیش‌فرض: "Endpoint")</param>
        public static void MapEndpoint<T>(this IEndpointRouteBuilder app,
            string path, string methodName = "Endpoint") {

            // جستجوی متد مشخص شده در نوع T با استفاده از Reflection
            MethodInfo? methodInfo = typeof(T).GetMethod(methodName);
            // اعتبارسنجی: متد باید وجود داشته باشد و نوع بازگشتی آن Task باشد
            if (methodInfo == null || methodInfo.ReturnType != typeof(Task)) {
                throw new System.Exception("Method cannot be used");
            }

            // ایجاد نمونه‌ای از کلاس اندپوینت با استفاده از DI Container
            // (این خط برای بررسی اولیه است و در داخل MapGet دوباره ایجاد می‌شود)
            T endpointInstance =
                ActivatorUtilities.CreateInstance<T>(app.ServiceProvider);

            // استخراج اطلاعات پارامترهای متد اندپوینت
            ParameterInfo[] methodParams = methodInfo!.GetParameters();

            // ثبت اندپوینت به عنوان یک درخواست HTTP GET
            app.MapGet(path, context => {
                // ایجاد نمونه جدید از کلاس اندپوینت برای هر درخواست (محدوده Request)
                T endpointInstance =
                    ActivatorUtilities.CreateInstance<T>(context.RequestServices);
                // فراخوانی متد اندپوینت با پارامترهای حل‌شده
                // اگر پارامتر از نوع HttpContext باشد، مستقیماً context ارسال می‌شود
                // در غیر این صورت، از DI Container سرویس مربوطه حل می‌شود
                return (Task)methodInfo.Invoke(endpointInstance!,
                    methodParams.Select(p => p.ParameterType == typeof(HttpContext)
                    ? context
                    : context.RequestServices.GetService(p.ParameterType)).ToArray())!;
            });
        }
    }
}
