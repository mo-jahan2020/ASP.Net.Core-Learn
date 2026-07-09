// این کلاس یک الگوی ساده‌ی "Service Locator" را نشان می‌دهد:
// به‌جای گرفتن سرویس از طریق تزریق وابستگی (DI)، یک نمونه‌ی ثابت و از پیش تعیین‌شده
// از IResponseFormatter را در سطح استاتیک (Static) نگه می‌دارد و در دسترس قرار می‌دهد.
// این الگو معمولاً برای مقایسه با روش صحیح‌تر Dependency Injection در آموزش استفاده می‌شود،
// چون وابستگی به این شکل، تست‌پذیری و انعطاف کد را کاهش می‌دهد.
﻿namespace Platform.Services {
    public static class TypeBroker {
        // یک نمونه‌ی ثابت از HtmlResponseFormatter که به‌صورت مستقیم (نه از طریق DI) ساخته شده است.
        private static IResponseFormatter formatter = new HtmlResponseFormatter();

        // دسترسی عمومی و فقط-خواندنی (Read-Only) به نمونه‌ی فرمت‌دهنده‌ی بالا.
        public static IResponseFormatter Formatter => formatter;
    }
}
