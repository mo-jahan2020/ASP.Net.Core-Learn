// این کلاس یک "Extension Method" برای IEndpointRouteBuilder تعریف می‌کند
// تا امکان تعریف Endpoint ها به‌صورت کلاس (Class-Based Endpoints) فراهم شود،
// به‌جای نوشتن مستقیم لامبدا در Program.cs (مثل app.MapGet("path", async context => {...})).
// این کار با استفاده از Reflection (بازتاب) انجام می‌شود تا متد "Endpoint" هر کلاسی
// به‌صورت پویا (Dynamic) پیدا و فراخوانی شود.
﻿using System.Reflection;

// این Extension Method عمداً در namespace خودِ ASP.NET Core (Microsoft.AspNetCore.Builder)
// تعریف شده تا بدون نیاز به using اضافی، مستقیماً روی app در Program.cs قابل استفاده باشد.
namespace Microsoft.AspNetCore.Builder {

    public static class EndpointExtensions {

        // متد Generic که نوع T را می‌گیرد (کلاسی که شامل متد Endpoint است)
        // و آن را به یک مسیر (path) با متد GET متصل می‌کند.
        // methodName پیش‌فرض "Endpoint" است اما در صورت نیاز قابل تغییر است.
        public static void MapEndpoint<T>(this IEndpointRouteBuilder app,
            string path, string methodName = "Endpoint") {

            // با استفاده از Reflection، متد مورد نظر (مثلاً "Endpoint") از نوع T پیدا می‌شود.
            MethodInfo? methodInfo = typeof(T).GetMethod(methodName);
            // اعتبارسنجی: متد باید وجود داشته باشد و نوع بازگشتی آن Task باشد،
            // در غیر این صورت یک Exception پرتاب می‌شود (این بررسی در زمان راه‌اندازی برنامه انجام می‌شود، نه در هر درخواست).
            if (methodInfo == null || methodInfo.ReturnType != typeof(Task)) {
                throw new System.Exception("Method cannot be used");
            }
            // این نمونه فقط برای اعتبارسنجی اولیه ساخته می‌شود (استفاده‌ی مستقیم دیگری ندارد)
            // و نشان می‌دهد که ActivatorUtilities می‌تواند وابستگی‌های سازنده را از DI Container تامین کند.
            T endpointInstance =
                ActivatorUtilities.CreateInstance<T>(app.ServiceProvider);

            // دریافت لیست پارامترهای متد Endpoint، تا در زمان هر درخواست بدانیم
            // باید چه مقادیری (HttpContext یا سرویس‌های تزریقی) به آن پاس داده شود.
            ParameterInfo[] methodParams = methodInfo!.GetParameters();

            // تعریف واقعی مسیر با متد GET؛ این بخش برای هر درخواست HTTP اجرا می‌شود.
            app.MapGet(path, context => {
                // برای هر درخواست، یک نمونه‌ی جدید از کلاس T ساخته می‌شود
                // (با استفاده از RequestServices مخصوص همان درخواست، نه ServiceProvider سراسری بالا).
                T endpointInstance =
                    ActivatorUtilities.CreateInstance<T>(context.RequestServices);
                // فراخوانی متد Endpoint از طریق Reflection:
                // برای هر پارامتر متد، اگر نوع آن HttpContext باشد، همان context فعلی پاس داده می‌شود،
                // در غیر این صورت مقدار آن پارامتر از DI Container (RequestServices) گرفته می‌شود.
                return (Task)methodInfo.Invoke(endpointInstance!,
                    methodParams.Select(p => p.ParameterType == typeof(HttpContext)
                    ? context
                    : context.RequestServices.GetService(p.ParameterType)).ToArray())!;
            });
        }
    }
}
