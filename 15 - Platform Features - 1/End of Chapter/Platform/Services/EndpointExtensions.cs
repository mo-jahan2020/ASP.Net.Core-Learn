using System.Reflection;

namespace Microsoft.AspNetCore.Builder {

    // متدهای الحاقی (Extension Methods) برای IEndpointRouteBuilder
    // که امکان نگاشت یک متد از یک کلاس عمومی (Generic) به یک مسیر HTTP GET را فراهم می‌کند
    public static class EndpointExtensions {

        // متد MapEndpoint که با استفاده از Reflection متد مورد نظر (پیش‌فرض "Endpoint") از نوع T را پیدا کرده
        // و آن را به عنوان handler مسیر داده‌شده ثبت می‌کند
        public static void MapEndpoint<T>(this IEndpointRouteBuilder app,
            string path, string methodName = "Endpoint") {

            // پیدا کردن اطلاعات متد مورد نظر از طریق Reflection
            MethodInfo? methodInfo = typeof(T).GetMethod(methodName);
            // بررسی اینکه متد وجود دارد و نوع بازگشتی آن Task است
            if (methodInfo == null || methodInfo.ReturnType != typeof(Task)) {
                throw new System.Exception("Method cannot be used");
            }
            // ساخت یک نمونه اولیه از T (در اینجا صرفاً برای اعتبارسنجی استفاده شده و مستقیماً استفاده نمی‌شود)
            T endpointInstance =
                ActivatorUtilities.CreateInstance<T>(app.ServiceProvider);

            // دریافت پارامترهای متد برای استفاده در زمان فراخوانی
            ParameterInfo[] methodParams = methodInfo!.GetParameters();

            // ثبت یک Endpoint از نوع GET برای مسیر مشخص‌شده
            app.MapGet(path, context => {
                // برای هر درخواست یک نمونه جدید از T با استفاده از سرویس‌های درخواست ساخته می‌شود
                T endpointInstance =
                    ActivatorUtilities.CreateInstance<T>(context.RequestServices);
                // فراخوانی متد مورد نظر با تزریق پارامترهای لازم
                // (HttpContext مستقیماً و بقیه پارامترها از طریق DI تامین می‌شوند)
                return (Task)methodInfo.Invoke(endpointInstance!,
                    methodParams.Select(p => p.ParameterType == typeof(HttpContext)
                    ? context
                    : context.RequestServices.GetService(p.ParameterType)).ToArray())!;
            });
        }
    }
}
