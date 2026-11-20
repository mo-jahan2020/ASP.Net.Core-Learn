using Code_Generation.Data;
using Code_Generation1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Code_Generation1.Pages.Products
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        public IActionResult OnGet(long id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null) return NotFound();

            Product = product;
            return Page();
        }

        public IActionResult OnPost()
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductID == Product.ProductID);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToPage("/Products/Index");
        }
    }
}
