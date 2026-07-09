using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Razor_sample.Models;

namespace RazorForm.Pages;

public class UserInfoModel : PageModel
{
    // -------------------------------------------------------
    // BindProperty: مقادیر فرم را به این کلاس وصل می‌کند
    // -------------------------------------------------------
    [BindProperty]
    public UserInputModel Input { get; set; } = new();

    // نشان می‌دهد آیا ذخیره موفق بوده یا نه
    public bool IsSaved { get; set; } = false;

    // -------------------------------------------------------
    // GET: وقتی صفحه برای اولین بار باز می‌شود
    // -------------------------------------------------------
    public void OnGet()
    {
        // در صورت نیاز می‌توان مقادیر پیش‌فرض را اینجا تنظیم کرد
        // مثال: Input.City = "تهران";
    }

    // -------------------------------------------------------
    // POST: وقتی دکمه "ذخیره" کلیک می‌شود
    // -------------------------------------------------------
    public IActionResult OnPost()
    {
        // اعتبارسنجی سمت سرور
        if (!ModelState.IsValid)
        {
            // فرم را دوباره نمایش بده با پیام‌های خطا
            return Page();
        }

        // ✅ اینجا می‌توانید داده را ذخیره کنید:
        // - در دیتابیس (DbContext)
        // - در فایل
        // - ارسال ایمیل
        // مثال ذخیره در دیتابیس:
        // await _dbContext.Users.AddAsync(new User { Name = Input.Name, ... });
        // await _dbContext.SaveChangesAsync();

        // فعلاً فقط پیام موفقیت نشان می‌دهیم
        IsSaved = true;

        // ریست فرم بعد از ذخیره موفق
        Input = new UserInputModel();
        ModelState.Clear();

        return Page();

        // اگر می‌خواهید به صفحه دیگری هدایت کنید:
        // return RedirectToPage("/Index");
    }
}

// -------------------------------------------------------
// مدل داده‌های ورودی با اعتبارسنجی
// -------------------------------------------------------

