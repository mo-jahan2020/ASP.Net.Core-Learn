using Microsoft.AspNetCore.Mvc;

namespace RazorSample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
            //return RedirectToAction("Index", "User");
        }
    }
}
