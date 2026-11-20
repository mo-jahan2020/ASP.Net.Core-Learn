using Code_Generation.Data;
using Code_Generation1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Code_Generation1.Pages.Suppliers
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Supplier Supplier { get; set; } = new();

        public IActionResult OnGet(long id)
        {
            var supplier = _context.Suppliers.Find(id);
            if (supplier == null) return NotFound();
            Supplier = supplier;
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            var existing = _context.Suppliers.Find(Supplier.SupplierId);
            if (existing == null) return NotFound();

            existing.Name = Supplier.Name;
            existing.City = Supplier.City;
            _context.SaveChanges();
            return RedirectToPage("/Suppliers/Index");
        }
    }
}
