using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementMvc.Repositories;
using UserManagementMvc.ViewModels;

namespace UserManagementMvc.Controllers;

// Controller مسئول دریافت درخواست‌های کاربر و هماهنگی بین View و Model است.
// در این پروژه تمام عملیات مربوط به کاربران در این کنترلر انجام می‌شود.
[Authorize]
public class UsersController : Controller
{
    // به‌جای DbContext مستقیم، از Repository استفاده می‌کنیم.
    private readonly IUserRepository _userRepository;

    // وابستگی Repository از طریق سازنده تزریق می‌شود.
    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // اکشن Index لیست کاربران را نمایش می‌دهد.
    // این اکشن از جستجو، فیلتر، مرتب‌سازی و Pagination پشتیبانی می‌کند.
    public async Task<IActionResult> Index(UserFilterViewModel filter)
    {
        var model = await _userRepository.GetPagedUsersAsync(filter);
        return View(model);
    }

    // نمایش جزئیات یک کاربر
    public async Task<IActionResult> Details(int? id)
    {
        // اگر شناسه ارسال نشده باشد، نتیجه NotFound برگردانده می‌شود.
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userRepository.GetDetailsAsync(id.Value);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // نمایش فرم ایجاد کاربر جدید
    public IActionResult Create()
    {
        return View();
    }

    // ثبت اطلاعات کاربر جدید
    // HttpPost یعنی این اکشن زمانی اجرا می‌شود که فرم ارسال شود.
    [HttpPost]
    // این ویژگی از حمله CSRF جلوگیری می‌کند.
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel user)
    {
        // اگر اعتبارسنجی مدل شکست بخورد، دوباره همان فرم با خطاها نمایش داده می‌شود.
        if (!ModelState.IsValid)
        {
            return View(user);
        }

        // افزودن کاربر جدید از طریق Repository
        await _userRepository.CreateAsync(user);
        TempData["SuccessMessage"] = "کاربر جدید با موفقیت ثبت شد.";

        // بعد از ثبت موفق به لیست کاربران برمی‌گردیم.
        return RedirectToAction(nameof(Index));
    }

    // نمایش فرم ویرایش اطلاعات یک کاربر
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userRepository.GetForEditAsync(id.Value);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // ذخیره تغییرات فرم ویرایش
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserEditViewModel user)
    {
        // بررسی می‌کنیم شناسه موجود در URL و مدل یکی باشد.
        if (id != user.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(user);
        }

        var isUpdated = await _userRepository.UpdateAsync(user);
        if (!isUpdated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "اطلاعات کاربر با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Index));
    }

    // نمایش صفحه تأیید حذف
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userRepository.GetForDeleteAsync(id.Value);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // حذف نهایی کاربر پس از تأیید
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var isDeleted = await _userRepository.DeleteAsync(id);
        if (!isDeleted)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "کاربر با موفقیت حذف شد.";
        return RedirectToAction(nameof(Index));
    }

    // صفحه خطا
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View();
    }
}
