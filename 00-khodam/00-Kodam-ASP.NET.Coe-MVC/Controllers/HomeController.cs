using _00_Kodam_ASP.NET.Coe_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace _00_Kodam_ASP.NET.Coe_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ViewResult  RsvpForm()
        {
            return View();

        }
        [HttpPost]
        public ViewResult RsvpForm(GuestResponse guestResponse)
        {
            if(ModelState.IsValid)
            {
                Repository.AddResponse(guestResponse);
                return View("Thanks", guestResponse);
            }
            else
            return View();

        }
        public ViewResult ListResponses()
        {
            return View(Repository.Responses);
            //return View(Repository.Responses.Where(r => r.WillAttend == true));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
