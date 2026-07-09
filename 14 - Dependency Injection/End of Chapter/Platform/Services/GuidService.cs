// این کلاس یکی از پیاده‌سازی‌های IResponseFormatter است و معمولاً برای نمایش
// تفاوت طول عمر (Lifetime) سرویس‌ها در DI (مثل Singleton، Scoped، Transient) استفاده می‌شود.
// هر بار که یک نمونه‌ی جدید از GuidService ساخته شود، یک GUID جدید تولید می‌کند؛
// اگر Lifetime آن Singleton باشد، این GUID برای همیشه ثابت می‌ماند، اما اگر Transient
// باشد، هر بار GUID جدیدی خواهیم دید.
﻿namespace Platform.Services {

    public class GuidService : IResponseFormatter {
        // GUID فقط یک‌بار، در لحظه‌ی ساخت نمونه (Constructor ضمنی)، تولید می‌شود.
        private Guid guid = Guid.NewGuid();

        public async Task Format(HttpContext context, string content) {
            // نمایش GUID تولیدشده به همراه محتوای اضافی (content) در پاسخ.
            await context.Response.WriteAsync($"Guid: {guid}\n{content}");
        }
    }
}
