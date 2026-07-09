// ════════════════════════════════════════════════════════════════
// فایل: Controllers/UserController.cs
// هدف: مدیریت تمام عملیات CRUD برای کاربران + Pagination
//
// 📌 Controller در MVC چیست؟
//    Controller مغز برنامه است. وقتی کاربر یک URL را باز می‌کند،
//    Controller مناسب پیدا شده، متد Action اجرا می‌شود،
//    داده‌ها از بانک خوانده می‌شوند و به View فرستاده می‌شوند.
//
// 📌 CRUD چیست؟
//    Create (ایجاد) → Read (خواندن) → Update (ویرایش) → Delete (حذف)
//    این ۴ عملیات پایه بانک اطلاعاتی هستند
//
// 📌 async/await چیست؟
//    روشی برای انجام عملیات غیرهمزمان (Asynchronous).
//    وقتی برنامه منتظر پاسخ بانک اطلاعاتی است، thread آزاد می‌ماند
//    و می‌تواند درخواست‌های دیگر را سرویس دهد. این باعث می‌شود
//    برنامه در زیر بار زیاد بهتر عمل کند.
// ════════════════════════════════════════════════════════════════

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RazorSample.Data;
using RazorSample.Models;

namespace RazorSample.Controllers
{
    // کلاس Controller باید از Controller ارث‌بری کند
    // این کلاس پایه امکاناتی مثل View(), RedirectToAction(), TempData و... را می‌دهد
    public class UserController : Controller
    {
        // ── فیلدهای Private ──────────────────────────────────────────
        // _context دسترسی به بانک اطلاعاتی را فراهم می‌کند
        // readonly یعنی بعد از مقداردهی اولیه نمی‌توان تغییرش داد
        private readonly ApplicationDbContext _context;

        // تعداد رکورد در هر صفحه — برای تغییر Pagination فقط همین عدد را عوض کنید
        private const int PageSize = 5;

        // ── سازنده (Constructor) ─────────────────────────────────────
        // DI Container به طور خودکار ApplicationDbContext را اینجا تزریق می‌کند
        // ما فقط آن را در فیلد _context ذخیره می‌کنیم
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }


        // ════════════════════════════════════════════════════════════
        // ── ACTION: Index — نمایش لیست کاربران با Pagination ────────
        // آدرس: GET /User یا GET /User/Index
        //
        // پارامتر page: شماره صفحه فعلی (پیش‌فرض ۱)
        // این پارامتر از URL می‌آید: /User?page=2
        // ════════════════════════════════════════════════════════════
        public async Task<IActionResult> Index(int page = 1)
        {
            // مرحله ۱: شمردن کل رکوردها برای محاسبه تعداد صفحات
            // CountAsync() فقط یک عدد از SQL می‌خواند (SELECT COUNT(*) FROM Users)
            var totalCount = await _context.Users.CountAsync();

            // مرحله ۲: محاسبه تعداد کل صفحات
            // Math.Ceiling گرد کردن به بالا — مثال: ۱۱ رکورد ÷ ۵ = ۲.۲ → ۳ صفحه
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            // مرحله ۳: گرفتن رکوردهای صفحه جاری از بانک اطلاعاتی
            var users = await _context.Users
                .OrderByDescending(u => u.Id)   // مرتب‌سازی: جدیدترین اول
                .Skip((page - 1) * PageSize)    // رد کردن رکوردهای صفحات قبل
                .Take(PageSize)                  // گرفتن فقط تعداد PageSize رکورد
                .ToListAsync();                  // اجرای Query و دریافت نتیجه

            // 📌 مثال Skip/Take برای page=2 و PageSize=5:
            //    Skip((2-1)*5) = Skip(5) → رکوردهای ۱ تا ۵ را رد کن
            //    Take(5) → رکوردهای ۶ تا ۱۰ را بگیر

            // مرحله ۴: ارسال اطلاعات صفحه‌بندی به View از طریق ViewBag
            // ViewBag یک شیء پویا است که داده‌های اضافی را به View می‌رساند
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            // ارسال لیست کاربران به View — این Model در View قابل دسترسی است
            return View(users);
        }


        // ════════════════════════════════════════════════════════════
        // ── ACTION: Details — نمایش جزئیات یک کاربر ─────────────────
        // آدرس: GET /User/Details/5
        // پارامتر id از URL گرفته می‌شود (طبق الگوی Route)
        // ════════════════════════════════════════════════════════════
        public async Task<IActionResult> Details(int id)
        {
            // FindAsync یک رکورد را بر اساس کلید اصلی پیدا می‌کند
            // این سریع‌ترین روش برای یافتن رکورد با Id است
            var user = await _context.Users.FindAsync(id);

            // اگر کاربر پیدا نشد → صفحه ۴۰۴ نشان بده
            if (user == null)
                return NotFound();

            return View(user);
        }


        // ════════════════════════════════════════════════════════════
        // ── ACTION: Create (GET) — نمایش فرم ایجاد کاربر جدید ───────
        // آدرس: GET /User/Create
        //
        // 📌 چرا دو متد Create داریم؟
        //    یکی برای GET (نمایش فرم خالی) و یکی برای POST (ذخیره داده)
        //    این الگو در MVC بسیار رایج است
        // ════════════════════════════════════════════════════════════
        public IActionResult Create()
        {
            // یک مدل خالی به View می‌فرستیم تا فرم با مقادیر پیش‌فرض نمایش داده شود
            return View(new UserInputModel());
        }

        // ── ACTION: Create (POST) — دریافت و ذخیره داده‌های فرم ─────
        // آدرس: POST /User/Create
        //
        // [HttpPost] → این متد فقط درخواست‌های POST را می‌پذیرد
        // [ValidateAntiForgeryToken] → از حمله CSRF جلوگیری می‌کند
        //
        // 📌 CSRF چیست؟ Cross-Site Request Forgery
        //    نوعی حمله که در آن یک سایت مخرب، کاربر را وادار می‌کند
        //    بدون اطلاع خودش درخواستی به سایت شما ارسال کند.
        //    Token یک رشته تصادفی است که هر بار تولید می‌شود.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserInputModel model)
        {
            // ModelState.IsValid → بررسی می‌کند آیا همه Data Annotations رعایت شده‌اند
            // اگر کاربر نام را خالی گذاشته، IsValid = false می‌شود
            if (ModelState.IsValid)
            {
                // اضافه کردن رکورد جدید به DbContext (هنوز در SQL ذخیره نشده)
                _context.Users.Add(model);

                // SaveChangesAsync → تمام تغییرات را در SQL اعمال می‌کند
                // معادل دستور INSERT INTO در SQL
                await _context.SaveChangesAsync();

                // TempData برای ارسال پیام به صفحه بعدی استفاده می‌شود
                // (بر خلاف ViewBag که فقط در همان Request زنده است)
                TempData["SuccessMessage"] = "کاربر با موفقیت ایجاد شد.";

                // Redirect: کاربر را به صفحه لیست بفرست
                // nameof(Index) = رشته "Index" — از نوشتن رشته‌های Magic جلوگیری می‌کند
                return RedirectToAction(nameof(Index));
            }

            // اگر Validation شکست خورد، فرم را با خطاها دوباره نشان بده
            // model را برمی‌گردانیم تا کاربر مقادیری که وارد کرده از دست ندهد
            return View(model);
        }


        // ════════════════════════════════════════════════════════════
        // ── ACTION: Edit (GET) — نمایش فرم ویرایش ───────────────────
        // آدرس: GET /User/Edit/5
        // ════════════════════════════════════════════════════════════
        public async Task<IActionResult> Edit(int id)
        {
            // پیدا کردن کاربر برای پر کردن فرم با داده‌های فعلی
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // فرم را با داده‌های کاربر فعلی نشان می‌دهیم
            return View(user);
        }

        // ── ACTION: Edit (POST) — ذخیره تغییرات ─────────────────────
        // آدرس: POST /User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserInputModel model)
        {
            // بررسی امنیتی: Id در URL باید با Id در فرم یکسان باشد
            // این جلوگیری می‌کند کاربر Id را دستکاری کند
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Update → EF Core را مطلع می‌کند که این رکورد تغییر کرده
                    // معادل دستور UPDATE در SQL
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "اطلاعات کاربر با موفقیت ویرایش شد.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    // 📌 Concurrency Exception چیست؟
                    //    وقتی دو کاربر همزمان یک رکورد را ویرایش می‌کنند
                    //    و رکورد در فاصله بین خواندن و نوشتن حذف شده باشد

                    // بررسی می‌کنیم آیا رکورد اصلاً وجود دارد یا نه
                    if (!_context.Users.Any(u => u.Id == id))
                        return NotFound();

                    // اگر رکورد وجود دارد ولی مشکل دیگری بود، Exception را بالا می‌اندازیم
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }


        // ════════════════════════════════════════════════════════════
        // ── ACTION: Delete (GET) — نمایش صفحه تأیید حذف ─────────────
        // آدرس: GET /User/Delete/5
        //
        // 📌 چرا برای حذف هم GET داریم؟
        //    برای نمایش صفحه تأیید — کاربر اول مطمئن می‌شود که
        //    واقعاً می‌خواهد حذف کند، سپس فرم را Submit می‌کند
        // ════════════════════════════════════════════════════════════
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // اطلاعات کاربر را نشان می‌دهیم تا کاربر بداند چه چیزی حذف می‌شود
            return View(user);
        }

        // ── ACTION: Delete (POST) — حذف واقعی از بانک اطلاعاتی ──────
        // آدرس: POST /User/Delete/5
        //
        // [ActionName("Delete")] → نام این Action را "Delete" می‌گذارد
        // این لازم است چون نمی‌توانیم دو متد Delete با یک امضا داشته باشیم
        // (یکی برای GET و یکی برای POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // ابتدا رکورد را از بانک پیدا می‌کنیم
            var user = await _context.Users.FindAsync(id);

            if (user != null)
            {
                // Remove → رکورد را از DbContext حذف می‌کند
                _context.Users.Remove(user);

                // اعمال حذف در SQL → معادل DELETE FROM Users WHERE Id = @id
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "کاربر با موفقیت حذف شد.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
