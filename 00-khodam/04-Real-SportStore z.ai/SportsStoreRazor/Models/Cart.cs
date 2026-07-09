namespace SportsStore.Models {

    /// <summary>
    /// کلاس سبد خرید (Cart)
    /// این کلاس شامل لیستی از CartLine است و عملیات افزودن/حذف/پاک کردن را انجام می‌دهد.
    /// متدها virtual هستند تا در SessionCart قابل override شوند (الگوی Strategy).
    /// </summary>
    public class Cart {

        // خطوط سبد خرید
        // از List استفاده می‌کنیم چون امکان دسترسی با index و متدهای بیشتری دارد
        public List<CartLine> Lines { get; set; } = new List<CartLine>();

        /// <summary>
        /// افزودن محصول به سبد
        /// اگر محصول از قبل وجود داشت، فقط تعداد را افزایش می‌دهد.
        /// </summary>
        public virtual void AddItem(Product product, int quantity) {
            CartLine? line = Lines
                .Where(p => p.Product.ProductID == product.ProductID)
                .FirstOrDefault();

            if (line == null) {
                Lines.Add(new CartLine {
                    Product = product,
                    Quantity = quantity
                });
            } else {
                line.Quantity += quantity;
            }
        }

        /// <summary>
        /// حذف یک محصول از سبد
        /// </summary>
        public virtual void RemoveLine(Product product) =>
            Lines.RemoveAll(l => l.Product.ProductID == product.ProductID);

        /// <summary>
        /// محاسبه مجموع ارزش سبد
        /// </summary>
        public decimal ComputeTotalValue() =>
            Lines.Sum(e => e.Product.Price * e.Quantity);

        /// <summary>
        /// پاک کردن کامل سبد
        /// </summary>
        public virtual void Clear() => Lines.Clear();
    }

    /// <summary>
    /// یک خط (آیتم) در سبد خرید
    /// هر خط شامل یک محصول و تعداد آن است.
    /// نکته: CartLineID در سبد (قبل از ثبت سفارش) همیشه 0 است.
    /// هنگام ثبت سفارش، EF Core به طور خودکار مقدار آن را تنظیم می‌کند.
    /// </summary>
    public class CartLine {
        public int CartLineID { get; set; }
        public Product Product { get; set; } = new();
        public int Quantity { get; set; }
    }
}
