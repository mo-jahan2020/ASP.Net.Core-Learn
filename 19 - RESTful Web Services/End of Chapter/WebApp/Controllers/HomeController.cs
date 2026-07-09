using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
//using WebApp.Models.ViewModels;

namespace WebApp.Controllers
{

    [ApiController]
    //[Route("api/Home1")]
    //[Route("api")]
    [Route("api/[controller]")]
    //[Route("api/_Products")]
    public class HomeController : ControllerBase
    {

        private DataContext context;

        public HomeController(DataContext ctx)
        {
            context = ctx;
        }

        [HttpGet]
        public IAsyncEnumerable<Product> GetPoroducts()
        {
            return context.Products.AsAsyncEnumerable();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(long id)
        {
            Product? p = await context.Products.FindAsync(id);
            if (p == null)
            {
                return NotFound();
            }
            return Ok(p);
        }

        [HttpPost]
        public async Task<IActionResult> SaveProduct(ProductBindingTarget target)
        {
            Product p = target.ToProduct();
            await context.Products.AddAsync(p);
            await context.SaveChangesAsync();
            return Ok(p);
        }

        [HttpPut]
        public async Task UpdateProduct(Product product)
        {
            context.Update(product);
            await context.SaveChangesAsync();
        }

        // تغییر این بخش برای ایجاد مسیر سفارشی
        [HttpDelete("Delete({id})")]
        //[HttpDelete("{id}")]
        //[HttpDelete("DeleteProduct({id})")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            Product? p = await context.Products.FindAsync(id);
            if (p == null)
            {
                return NotFound();
            }
            context.Products.Remove(p);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("redirect")]
        public IActionResult Redirect()
        {
            return RedirectToAction(nameof(GetProduct), new { Id = 1 });
        }

    }
}