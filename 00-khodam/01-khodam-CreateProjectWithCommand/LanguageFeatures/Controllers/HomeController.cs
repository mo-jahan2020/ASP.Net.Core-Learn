using Microsoft.AspNetCore.Mvc;
using LanguageFeatures.Models;
using System.Diagnostics;

namespace LanguageFeatures.Controllers
{
    public class HomeController : Controller
    {
        public ViewResult Index()
        {

            Dictionary<string, Product> products = new Dictionary<string, Product>
            {
                { "Kayak", new Product { Name = "Kayak", Price = 275M } },
                { "Lifejacket", new Product{ Name = "Lifejacket", Price = 48.95M } }
            };
            return View("Dictionary", products);
            //Product[] products = Product.GetProducts();
            //return View(new string[] { products[0].Name });  //kayak

            //List<Product> products = Product.GetProducts().ToList();
            // return View("List" , products );

            //return View(new string[] { "C#", "Language", "Features" });
        }
    }
}
