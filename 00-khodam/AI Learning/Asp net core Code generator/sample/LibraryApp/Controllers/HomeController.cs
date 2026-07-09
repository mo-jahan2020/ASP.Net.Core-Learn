// ============================================================
// HomeController - کنترلر صفحه اصلی
// ============================================================

using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Controllers;

public class HomeController : Controller
{
    // صفحه اصلی - برای همه قابل دسترسی است
    public IActionResult Index() => View();

    public IActionResult Error() => View();
}
