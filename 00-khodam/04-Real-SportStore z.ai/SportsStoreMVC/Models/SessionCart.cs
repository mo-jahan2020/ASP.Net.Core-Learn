using System.Text.Json.Serialization;
using SportsStore.Infrastructure;

namespace SportsStore.Models {

    /// <summary>
    /// پیاده‌سازی سبد خرید مبتنی بر Session
    /// این کلاس از Cart ارث‌بری می‌کند و متدها را طوری Override می‌کند
    /// که هر تغییری در سبد، در Session ذخیره شود.
    /// از الگوی Singleton-per-Session استفاده می‌شود (یک سبد به ازای هر Session).
    /// </summary>
    public class SessionCart : Cart {

        /// <summary>
        /// متد کارخانه (Factory Method) برای گرفتن سبد فعلی کاربر
        /// اگر سبدی در Session وجود نداشت، یک سبد جدید می‌سازد.
        /// </summary>
        /// <param name="services">ServiceProvider برای دسترسی به HttpContext</param>
        public static Cart GetCart(IServiceProvider services) {
            // دریافت Session از HttpContext
            ISession? session = services.GetRequiredService<IHttpContextAccessor>()
                .HttpContext?.Session;

            // تلاش برای بازیابی سبد از Session
            // اگر وجود نداشت، یک سبد جدید خالی می‌سازیم
            SessionCart cart = session?.GetJson<SessionCart>("Cart")
                ?? new SessionCart();

            // تنظیم Session روی سبد (برای ذخیره بعدی)
            cart.Session = session;
            return cart;
        }

        // Session جاری - با JsonIgnore چون قابل سریالایز شدن نیست
        [JsonIgnore]
        public ISession? Session { get; set; }

        // متد کمکی برای ذخیره سبد در Session
        private void SaveSession() {
            Session?.SetJson("Cart", this);
        }

        // Override متدها برای ذخیره خودکار در Session

        /// <summary>
        /// افزودن محصول به سبد و ذخیره در Session
        /// </summary>
        public override void AddItem(Product product, int quantity) {
            base.AddItem(product, quantity);
            SaveSession();
        }

        /// <summary>
        /// حذف محصول از سبد و ذخیره در Session
        /// </summary>
        public override void RemoveLine(Product product) {
            base.RemoveLine(product);
            SaveSession();
        }

        /// <summary>
        /// پاک کردن کامل سبد و حذف کلید از Session
        /// </summary>
        public override void Clear() {
            base.Clear();
            Session?.Remove("Cart");
        }
    }
}
