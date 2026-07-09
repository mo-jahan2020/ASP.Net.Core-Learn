using Code_Generation.Data;
using Code_Generation1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Code_Generation1.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        public SelectList Categorys { get; set; } = default!;
        public SelectList Suppliers { get; set; } = default!;

        public void OnGet()
        {
            Categorys = new SelectList(_context.Categories, "CategoryId", "Name");
            Suppliers = new SelectList(_context.Suppliers, "SupplierId", "Name");
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Categorys = new SelectList(_context.Categories, "CategoryId", "Name", Product.CategoryId);
                Suppliers = new SelectList(_context.Suppliers, "SupplierId", "Name", Product.SupplierId);
                return Page();
            }

            _context.Products.Add(Product);
            _context.SaveChanges();
            return RedirectToPage("/Products/Index");
        }
    }
}
