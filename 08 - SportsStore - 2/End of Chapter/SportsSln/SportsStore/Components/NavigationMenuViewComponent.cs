using Microsoft.AspNetCore.Mvc;
using SportsStore.Models;

namespace SportsStore.Components {

    public class NavigationMenuViewComponent : ViewComponent {
        private IStoreRepository repository;

        public NavigationMenuViewComponent(IStoreRepository repo) {
            repository = repo;
        }

        public IViewComponentResult Invoke() {
            ViewBag.SelectedCategory = RouteData?.Values["category"];
            //ViewBag.SelectedController = RouteData?.Values["controller"];
            //ViewBag.SelectedAction = RouteData?.Values["action"];

            //if (ViewBag.SelectedCategory != null)
            //{
            //    Console.WriteLine("SelectedCategory:" + RouteData?.Values["category"]);//Masalan Chess
            //    Console.WriteLine("controller:" + RouteData?.Values["controller"]);//home
            //    Console.WriteLine("action:" + RouteData?.Values["action"]);//Index
            //}
            return View(repository.Products
                .Select(x => x.Category)
                .Distinct()
                .OrderBy(x => x));
        }
    }
}
