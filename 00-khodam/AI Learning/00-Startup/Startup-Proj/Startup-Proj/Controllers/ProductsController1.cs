using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Startup_Proj.Models;

namespace Startup_Proj.Controllers
{
    [Controller]
    public class ProductsController : Controller
    {
        // /Products/bookid=10&isloggedin=true
        [Route("[Controller]/{bookid?}/{isloggedin?}")]
        public ContentResult Index(int? bookid, bool? isloggedin)
        {
            if (bookid.HasValue )
            {
            }
            else
            {
            }
            return Content("Call Products Controller Action Index with default value for bookid AND isLoggedin");
        }

        // GET: ProductsController1/Details/5
        [Route("[Controller]/Details")]
        public JsonResult Details()
        {

            Person person = new Person() { PersonName = "ali hadi"};
            return Json(person);
        }

        // GET: ProductsController1/Create
        [Route("/Download-File")]
        public VirtualFileResult Download()
        {
            return  File("File.pdf", "application/pdf");
        }

        // POST: ProductsController1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductsController1/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProductsController1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductsController1/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProductsController1/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
