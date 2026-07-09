namespace Platform.Services {
    // اینترفیس مشترک برای تمام فرمت‌دهنده‌های پاسخ در برنامه
    public interface IResponseFormatter {

        // متدی که باید محتوای ورودی را قالب‌بندی کرده و در پاسخ HTTP بنویسد
        Task Format(HttpContext context, string content);

        // خاصیتی با مقدار پیش‌فرض که مشخص می‌کند خروجی این فرمت‌دهنده غنی (مثلاً HTML) است یا خیر
        public bool RichOutput => false;
    }
}
