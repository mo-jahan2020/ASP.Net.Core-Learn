// ============================================================
// BooksController - مدیریت کامل عملیات CRUD روی کتاب‌ها
// [Authorize] => فقط کاربران واردشده اجازه دسترسی دارند
// ============================================================

using LibraryApp.Data;
using LibraryApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace LibraryApp.Controllers;

[Authorize] // <<-- فقط کاربران ثبت‌نام‌شده می‌توانند وارد شوند
public class BooksController : Controller
{
    private readonly ApplicationDbContext _context;

    // تزریق وابستگی (Dependency Injection) DbContext
    public BooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    // -------------------------------------------------------
    // GET: /Books  ->  لیست کتاب‌ها به‌صورت Grid با Pagination
    // -------------------------------------------------------
    // page: شماره صفحه فعلی (پیش‌فرض 1)
    // search: عبارت جستجو (اختیاری)
    public IActionResult Index(int? page, string? search)
    {
        int pageSize = 6; // تعداد آیتم در هر صفحه
        int pageNumber = page ?? 1;

        // ساخت Query پایه - هنوز اجرا نشده
        var query = _context.Books.AsQueryable();

        // اعمال فیلتر جستجو در صورت وجود
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(b =>
                b.Title.Contains(search) || b.Author.Contains(search));
            ViewBag.Search = search;
        }

        // مرتب‌سازی و تبدیل به لیست صفحه‌بندی‌شده
        var pagedBooks = query
            .OrderByDescending(b => b.CreatedAt)
            .ToPagedList(pageNumber, pageSize);

        return View(pagedBooks);
    }

    // -------------------------------------------------------
    // GET: /Books/Details/5  ->  نمایش جزئیات یک کتاب
    // -------------------------------------------------------
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
        if (book == null) return NotFound();

        return View(book);
    }

    // -------------------------------------------------------
    // GET: /Books/Create  ->  نمایش فرم ایجاد کتاب جدید
    // -------------------------------------------------------
    public IActionResult Create() => View();

    // -------------------------------------------------------
    // POST: /Books/Create  ->  ذخیره کتاب جدید
    // [ValidateAntiForgeryToken] => جلوگیری از حمله CSRF
    // -------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Author,ISBN,PublishYear,Description")] Book book)
    {
        if (ModelState.IsValid)
        {
            book.CreatedAt = DateTime.Now;
            _context.Add(book);
            await _context.SaveChangesAsync();
            TempData["Success"] = "کتاب با موفقیت ثبت شد.";
            return RedirectToAction(nameof(Index));
        }
        return View(book);
    }

    // -------------------------------------------------------
    // GET: /Books/Edit/5  ->  نمایش فرم ویرایش
    // -------------------------------------------------------
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var book = await _context.Books.FindAsync(id);
        if (book == null) return NotFound();
        return View(book);
    }

    // -------------------------------------------------------
    // POST: /Books/Edit/5  ->  ذخیره تغییرات
    // -------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,Title,Author,ISBN,PublishYear,Description,CreatedAt")] Book book)
    {
        if (id != book.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تغییرات ذخیره شد.";
            }
            catch (DbUpdateConcurrencyException)
            {
                // در صورت ویرایش هم‌زمان توسط کاربر دیگر
                if (!_context.Books.Any(e => e.Id == book.Id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(book);
    }

    // -------------------------------------------------------
    // GET: /Books/Delete/5  ->  صفحه تایید حذف
    // -------------------------------------------------------
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
        if (book == null) return NotFound();
        return View(book);
    }

    // -------------------------------------------------------
    // POST: /Books/Delete/5  ->  انجام حذف نهایی
    // -------------------------------------------------------
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book != null)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            TempData["Success"] = "کتاب حذف شد.";
        }
        return RedirectToAction(nameof(Index));
    }
}
