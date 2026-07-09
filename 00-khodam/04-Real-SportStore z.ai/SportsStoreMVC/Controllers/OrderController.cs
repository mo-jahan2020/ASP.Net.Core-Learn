using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SportsStore.Models;
using SportsStore.Models.ViewModels;

namespace SportsStore.Controllers {

    /// <summary>
    /// Controller سفارشات (Order)
    /// مسئولیت: لیست سفارشات، ثبت سفارش جدید، جزئیات سفارش، علامت‌گذاری ارسال
    ///
    /// مقایسه با Razor Pages:
    /// - در Razor Pages: پوشه Order/ با Index, Create, Details
    /// - در MVC: OrderController با اکشن‌های Index, Create, Details, MarkShipped
    ///
    /// مسیرها (با Route پیش‌فرض):
    /// - GET  /Order              → Index(page)         لیست سفارشات با Pagination
    /// - POST /Order/MarkShipped/5 → MarkShipped(5)     علامت‌گذاری ارسال (فقط Admin)
    /// - GET  /Order/Create       → Create()            فرم ثبت سفارش (با پیش‌پر آدرس)
    /// - POST /Order/Create       → Create(Order)       ذخیره سفارش
    /// - GET  /Order/Details/5    → Details(5)          جزئیات سفارش
    /// </summary>
    [Authorize]
    public class OrderController : Controller {

        private readonly IOrderRepository         _repository;
        private readonly StoreDbContext           _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly Cart                     _cart;

        // تزریق تمام وابستگی‌ها از طریق Constructor (DI)
        public OrderController(IOrderRepository repo,
                               StoreDbContext dbContext,
                               UserManager<IdentityUser> userManager,
                               Cart cart) {
            _repository = repo;
            _dbContext  = dbContext;
            _userManager = userManager;
            _cart       = cart;
        }

        // --------------------------------------------------------------------
        // INDEX (لیست سفارشات)
        // --------------------------------------------------------------------

        /// <summary>
        /// نمایش لیست سفارشات با Pagination (GET /Order یا /Order?page=2)
        /// </summary>
        /// <param name="page">شماره صفحه فعلی</param>
        public IActionResult Index(int? page) {
            const int pageSize = 5;

            int currentPage = page ?? 1;
            if (currentPage < 1) currentPage = 1;

            int totalItems = _repository.Orders.Count();
            int totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
            if (currentPage > totalPages && totalPages > 0) {
                currentPage = totalPages;
            }

            // Pagination با Skip و Take (در SQL اجرا می‌شود)
            var orders = _repository.Orders
                .OrderByDescending(o => o.OrderID)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new OrdersListViewModel {
                Orders = orders,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        // --------------------------------------------------------------------
        // MARK SHIPPED (علامت‌گذاری ارسال - فقط Admin)
        // --------------------------------------------------------------------

        /// <summary>
        /// علامت‌گذاری سفارش به عنوان "ارسال شده"
        ///
        /// نکته امنیتی (Defense in Depth):
        /// هم در UI دکمه فقط برای Admin نمایش داده می‌شود
        /// و هم در Backend با [Authorize(Roles="Admin")] محافظت می‌شود.
        ///
        /// مزیت MVC نسبت به Razor Pages:
        /// در MVC می‌توان [Authorize] را روی یک اکشن خاص گذاشت،
        /// اما در Razor Pages این امکان وجود ندارد (فقط روی کل PageModel).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkShipped(int id) {
            var order = _dbContext.Orders.FirstOrDefault(o => o.OrderID == id);
            if (order != null) {
                order.Shipped = true;
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] =
                    $"سفارش شماره {id} به عنوان ارسال شده علامت‌گذاری شد.";
            }
            return RedirectToAction(nameof(Index));
        }

        // --------------------------------------------------------------------
        // CREATE (ثبت سفارش جدید)
        // --------------------------------------------------------------------

        /// <summary>
        /// نمایش فرم ثبت سفارش (GET /Order/Create)
        ///
        /// ⚡ بهبود UX (تجربه کاربری):
        /// اگر کاربر قبلاً سفارشی ثبت کرده، اطلاعات آدرس او از آخرین سفارش
        /// در فرم پیش‌پر (Pre-fill) می‌شود. این کار باعث می‌شود کاربر برای
        /// خریدهای بعدی مجبور نباشد دوباره آدرس را وارد کند.
        ///
        /// 🔑 نکته مهم:
        /// برای تطابق سفارش با کاربر، از فیلد Order.UserId (شناسه یکتای کاربر
        /// در Identity) استفاده می‌کنیم، نه از Order.Name. دلیل: Order.Name نام
        /// گیرنده است و ممکن است در سفارش‌های مختلف متفاوت باشد.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create() {
            // اگر سبد خالی است، نمی‌توان سفارش ثبت کرد
            if (_cart.Lines.Count == 0) {
                TempData["ErrorMessage"] =
                    "سبد خرید شما خالی است. ابتدا محصولاتی را اضافه کنید.";
                return RedirectToAction("Index", "Products");
            }

            // دریافت کاربر login شده
            IdentityUser? user = await _userManager.GetUserAsync(User);

            if (user != null) {
                // جستجوی آخرین سفارش این کاربر (بر اساس UserId)
                SportsStore.Models.Order? lastOrder = _repository.Orders
                    .Where(o => o.UserId == user.Id)
                    .OrderByDescending(o => o.OrderID)
                    .FirstOrDefault();

                if (lastOrder != null) {
                    // کپی اطلاعات آدرس از آخرین سفارش به فرم جدید
                    // نکته: فیلد GiftWrap را کپی نمی‌کنیم چون هر بار باید انتخاب شود
                    var newOrder = new SportsStore.Models.Order {
                        Name    = lastOrder.Name,
                        Line1   = lastOrder.Line1,
                        Line2   = lastOrder.Line2,
                        Line3   = lastOrder.Line3,
                        City    = lastOrder.City,
                        State   = lastOrder.State,
                        Zip     = lastOrder.Zip,
                        Country = lastOrder.Country
                    };

                    // ارسال سبد خرید به View (برای خلاصه سفارش)
                    ViewBag.Cart = _cart;
                    TempData["InfoMessage"] =
                        "اطلاعات آدرس از سفارش قبلی شما پر شده است. " +
                        "در صورت نیاز، آن‌ها را ویرایش کنید.";
                    return View(newOrder);
                }
            }

            // حالت عادی: فرم خالی
            ViewBag.Cart = _cart;
            return View(new SportsStore.Models.Order());
        }

        /// <summary>
        /// پردازش فرم ثبت سفارش (POST /Order/Create)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SportsStore.Models.Order order) {
            // بررسی مجدد سبد خرید (شاید در این مدت خالی شده)
            if (_cart.Lines.Count == 0) {
                ModelState.AddModelError("", "سبد خرید شما خالی است.");
            }

            if (!ModelState.IsValid) {
                ViewBag.Cart = _cart;
                return View(order);
            }

            // دریافت کاربر فعلی و تنظیم UserId
            IdentityUser? user = await _userManager.GetUserAsync(User);
            if (user != null) {
                order.UserId = user.Id;
            }

            // کپی آیتم‌های سبد خرید به سفارش
            // نکته: یک کپی واقعی از CartLine ها می‌سازیم تا EF آن‌ها را به عنوان
            // موجودیت‌های جدید در نظر بگیرد (نه ارجاع به همان اشیاء سبد)
            order.Lines = _cart.Lines.Select(l => new CartLine {
                Product = l.Product,
                Quantity = l.Quantity
            }).ToList();

            // ذخیره سفارش در دیتابیس
            _repository.SaveOrder(order);

            // خالی کردن سبد خرید پس از ثبت موفق سفارش
            _cart.Clear();

            TempData["SuccessMessage"] =
                $"سفارش شما با شماره {order.OrderID} با موفقیت ثبت شد!";
            return RedirectToAction(nameof(Index));
        }

        // --------------------------------------------------------------------
        // DETAILS (جزئیات سفارش)
        // --------------------------------------------------------------------

        /// <summary>
        /// نمایش جزئیات یک سفارش (GET /Order/Details/5)
        /// </summary>
        public IActionResult Details(int id) {
            // بارگذاری سفارش با خطوط و محصولات (Eager Loading)
            SportsStore.Models.Order? order = _repository.Orders
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null) return NotFound();

            return View(order);
        }

        /// <summary>
        /// علامت‌گذاری سفارش به عنوان ارسال شده (از صفحه Details)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DetailsMarkShipped(int id) {
            var order = _dbContext.Orders.FirstOrDefault(o => o.OrderID == id);
            if (order != null) {
                order.Shipped = true;
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] =
                    $"سفارش شماره {id} به عنوان ارسال شده علامت‌گذاری شد.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
