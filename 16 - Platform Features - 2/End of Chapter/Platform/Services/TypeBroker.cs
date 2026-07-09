// =============================================================================
// TypeBroker.cs - بروکر استاتیک برای دسترسی به فرمت‌کننده پاسخ
// این کلاس یک الگوی Service Locator ساده را پیاده‌سازی می‌کند
// یک نمونه ثابت از HtmlResponseFormatter را نگهداری و از طریق ویژگی Formatter
// در دسترس قرار می‌دهد
// =============================================================================

namespace Platform.Services {
    /// <summary>
    /// TypeBroker - بروکر نوع (Type Broker) استاتیک
    /// این کلاس یک راه جایگزین برای DI Container فراهم می‌کند
    /// با استفاده از یک نمونه ثابت، دسترسی سراسری به فرمت‌کننده پاسخ را ممکن می‌سازد
    /// </summary>
    public static class TypeBroker {
        // نمونه ثابت از HtmlResponseFormatter - در زمان بارگذاری کلاس ایجاد می‌شود
        // این الگوی مشابه Singleton اما ساده‌تر است
        private static IResponseFormatter formatter = new HtmlResponseFormatter();

        /// <summary>
        /// Formatter - دسترسی به نمونه فرمت‌کننده پاسخ
        /// هر بار فراخوانی، همان نمونه ثابت HtmlResponseFormatter را برمی‌گرداند
        /// </summary>
        public static IResponseFormatter Formatter => formatter;
    }
}
