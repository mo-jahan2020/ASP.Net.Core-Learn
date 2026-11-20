using Microsoft.AspNetCore.Mvc;
using Startup_Proj.Models;

namespace Startup_Proj.Controllers
{
    [Controller]
    public class HomeController : Controller
    {
        [Route("/")]
        [Route("/Index")]
        [Route("/Home")]
        public IActionResult Index([FromForm] Person person)
        {
            return View();
            
        }
        [Route("/About")]
        public string About()
        {
            return "Hellow Form Home Controller Action About";

        }
    }
}
