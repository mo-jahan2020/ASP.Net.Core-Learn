using Code_Generation.Data;
using Code_Generation1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Code_Generation1.Pages.Products
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        public SelectList Categorys { get; set; } = default!;
        public SelectList Suppliers { get; set; } = default!;

        public IActionResult OnGet(long id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null) return NotFound();

            Product = product;
            Categorys = new SelectList(_context.Categories, "CategoryId", "Name", Product.CategoryId);
            Suppliers = new SelectList(_context.Suppliers, "SupplierId", "Name", Product.SupplierId);

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Categorys = new SelectList(_context.Categories, "CategoryId", "Name", Product.CategoryId);
                Suppliers = new SelectList(_context.Suppliers, "SupplierId", "Name", Product.SupplierId);
                return Page();
            }

            _context.Update(Product);
            _context.SaveChanges();
            return RedirectToPage("/Products/Index");
        }
    }
}
