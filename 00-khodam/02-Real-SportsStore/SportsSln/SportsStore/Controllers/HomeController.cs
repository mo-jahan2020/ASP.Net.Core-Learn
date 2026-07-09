using Microsoft.AspNetCore.Mvc;
using SportsStore.Models;


namespace SportsStore.Controllers
{
    public class HomeController : Controller
    {
        private IStoreRepository repository;


        public HomeController(IStoreRepository repo)
        {
            repository = repo;
        }
        public ViewResult Index() => View(new ProductsListViewModel
        {
            Products = repository.Products.OrderBy(p => p.ProductID)
        });
        //public IActionResult Index() => View(repository.Products);
    }
}
