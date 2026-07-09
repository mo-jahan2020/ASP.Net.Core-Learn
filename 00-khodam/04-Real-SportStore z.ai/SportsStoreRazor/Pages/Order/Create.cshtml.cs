using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SportsStore.Models;

namespace SportsStore.Pages.Order {

    /// <summary>
    /// PageModel صفحه ثبت سفارش جدید (Create)
    ///
    /// این صفحه:
    /// - نیاز به ورود کاربر دارد
    /// - اطلاعات گیرنده را از فرم دریافت می‌کند
    /// - محتوای سبد خرید را به عنوان یک سفارش ذخیره می‌کند
    /// - پس از ثبت، سبد خرید را خالی می‌کند
    ///
    /// ⚡ بهبود UX (تجربه کاربری):
    /// در متد OnGet، آخرین سفارش کاربر را از دیتابیس می‌خوانیم و اطلاعات
    /// آدرس او را در فرم پیش‌پر (Pre-fill) می‌کنیم. این کار باعث می‌شود
    /// کاربر برای خریدهای بعدی مجبور نباشد دوباره آدرس را وارد کند.
    /// کاربر می‌تواند اطلاعات را ویرایش کند یا همان را قبول کند.
    ///
    /// 🔑 نکته مهم:
    /// برای تطابق سفارش با کاربر، از فیلد Order.UserId (شناسه یکتای کاربر در Identity)
    /// استفاده می‌کنیم، نه از Order.Name. دلیل: Order.Name نام گیرنده است و ممکن است
    /// در سفارش‌های مختلف متفاوت باشد (مثلاً کاربر برای فرد دیگری سفارش می‌دهد).
    /// اما UserId همیشه ثابت است و به حساب کاربر login شده اشاره می‌کند.
    /// </summary>
    [Authorize]
    public class CreateModel : PageModel {

        private readonly IOrderRepository   _repository;
        private readonly UserManager<IdentityUser> _userManager;

        // UserManager برای دسترسی به اطلاعات کاربر login شده
        public CreateModel(IOrderRepository repo,
                           Cart cartService,
                           UserManager<IdentityUser> userManager) {
            _repository = repo;
            Cart = cartService;
            _userManager = userManager;
        }

        // سبد خرید فعلی کاربر (تزریق از DI)
        public Cart Cart { get; set; }

        // سفارشی که از فرم پر می‌شود
        // از نام کامل استفاده می‌کنیم چون namespace فعلی SportsStore.Pages.Order است
        // و کلاس Order در SportsStore.Models قرار دارد (تداخل نام)
        [BindProperty]
        public SportsStore.Models.Order Order { get; set; } = new();

        /// <summary>
        /// نمایش فرم ثبت سفارش
        ///
        /// مراحل:
        /// 1) اگر سبد خرید خالی است، به صفحه محصولات هدایت می‌کند
        /// 2) شناسه کاربر فعلی را از Identity می‌گیریم
        /// 3) آخرین سفارش این کاربر را از دیتابیس می‌خواند (با تطابق UserId)
        /// 4) اگر سفارش قبلی وجود داشت، اطلاعات آدرس را در فرم پیش‌پر می‌کند
        ///    (به جز فیلد GiftWrap که هر بار باید انتخاب شود)
        /// </summary>
        public async Task<IActionResult> OnGetAsync() {
            // اگر سبد خالی است، نمی‌توان سفارش ثبت کرد
            if (Cart.Lines.Count == 0) {
                TempData["ErrorMessage"] = "سبد خرید شما خالی است. ابتدا محصولاتی را اضافه کنید.";
                return RedirectToPage("/Products/Index");
            }

            // --- دریافت کاربر login شده ---
            // نکته: User.Identity.Name نام کاربری است، اما ما به Id (GUID) نیاز داریم
            // چون Id یکتاست و هرگز تغییر نمی‌کند (برخلاف Username که قابل تغییر است)
            IdentityUser? user = await _userManager.GetUserAsync(User);

            if (user != null) {
                // --- جستجوی آخرین سفارش این کاربر (بر اساس UserId) ---
                // Where: فیلتر روی UserId
                // OrderByDescending: جدیدترین سفارش اول
                // FirstOrDefault: گرفتن اولین نتیجه (یا null اگر وجود ندارد)
                SportsStore.Models.Order? lastOrder = _repository.Orders
                    .Where(o => o.UserId == user.Id)
                    .OrderByDescending(o => o.OrderID)
                    .FirstOrDefault();

                if (lastOrder != null) {
                    // --- کپی اطلاعات آدرس از آخرین سفارش به فرم جدید ---
                    // این کار باعث می‌شود کاربر مجبور نباشد دوباره آدرس را وارد کند
                    // نکته ۱: فیلد GiftWrap را کپی نمی‌کنیم چون هر بار باید انتخاب شود
                    // نکته ۲: فیلد Name (نام گیرنده) را هم کپی می‌کنیم چون معمولاً ثابت است
                    //         اما کاربر می‌تواند آن را ویرایش کند
                    Order.Name    = lastOrder.Name;
                    Order.Line1   = lastOrder.Line1;
                    Order.Line2   = lastOrder.Line2;
                    Order.Line3   = lastOrder.Line3;
                    Order.City    = lastOrder.City;
                    Order.State   = lastOrder.State;
                    Order.Zip     = lastOrder.Zip;
                    Order.Country = lastOrder.Country;

                    // پیام اطلاع‌رسانی به کاربر
                    TempData["InfoMessage"] =
                        "اطلاعات آدرس از سفارش قبلی شما پر شده است. " +
                        "در صورت نیاز، آن‌ها را ویرایش کنید.";
                }
            }

            return Page();
        }

        /// <summary>
        /// پردازش فرم ثبت سفارش
        /// </summary>
        public async Task<IActionResult> OnPostAsync() {
            // اعتبارسنجی فرم
            if (!ModelState.IsValid) {
                return Page();
            }

            // --- ثبت UserId کاربر فعلی روی سفارش ---
            // این کار برای تطابق سفارش با کاربر در خریدهای بعدی استفاده می‌شود
            IdentityUser? user = await _userManager.GetUserAsync(User);
            if (user != null) {
                Order.UserId = user.Id;
            }

            // --- کپی کردن آیتم‌های سبد خرید به سفارش ---
            // نکته ۱: از ToList استفاده می‌کنیم چون Order.Lines از نوع List است
            // نکته ۲: یک کپی واقعی از CartLine ها می‌سازیم تا EF آن‌ها را به عنوان
            //         موجودیت‌های جدید در نظر بگیرد (نه ارجاع به همان اشیاء سبد)
            Order.Lines = Cart.Lines.Select(l => new CartLine {
                Product = l.Product,
                Quantity = l.Quantity
            }).ToList();

            // --- ذخیره سفارش در دیتابیس ---
            // در این مرحله، EF Core محصولات را Attach می‌کند (تکرار نشوند)
            // و سپس Order را با تمام CartLine های آن Insert می‌کند
            _repository.SaveOrder(Order);

            // --- خالی کردن سبد خرید پس از ثبت موفق سفارش ---
            Cart.Clear();

            // نمایش پیام موفقیت
            TempData["SuccessMessage"] =
                $"سفارش شما با شماره {Order.OrderID} با موفقیت ثبت شد!";

            // هدایت به صفحه لیست سفارشات
            return RedirectToPage("Index");
        }
    }
}
