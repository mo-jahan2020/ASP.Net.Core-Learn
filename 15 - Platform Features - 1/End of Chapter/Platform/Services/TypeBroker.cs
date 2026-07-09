namespace Platform.Services {
    // یک کلاس ایستا (Static) که به عنوان یک واسطه ساده برای دسترسی به یک نمونه ثابت
    // از IResponseFormatter عمل می‌کند (نوعی الگوی Service Locator ساده)
    public static class TypeBroker {
        // نمونه ثابت از HtmlResponseFormatter که همیشه استفاده می‌شود
        private static IResponseFormatter formatter = new HtmlResponseFormatter();

        // خاصیت فقط-خواندنی برای دسترسی به فرمت‌دهنده
        public static IResponseFormatter Formatter => formatter;
    }
}
