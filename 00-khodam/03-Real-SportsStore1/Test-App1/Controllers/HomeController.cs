using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Test_App1.Models;

namespace Test_App1.Controllers
{
    public class HomeController : Controller
    {
       

        private IStoreRepository repository;
        public HomeController(IStoreRepository repo)
        {
            repository = repo;
        }
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5) 
        {
            var Products = await repository.GetPagedProductsAsync(pageNumber, pageSize);
            var _Count = await repository.GetTotalProducesCountAsync();

            ViewBag.TotalCount = (int)Math.Ceiling(_Count / (Double)pageSize);
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;

            return View(Products);

            //Products = repository.Products.OrderBy(p => p.ProductID)
        }
    }
}
