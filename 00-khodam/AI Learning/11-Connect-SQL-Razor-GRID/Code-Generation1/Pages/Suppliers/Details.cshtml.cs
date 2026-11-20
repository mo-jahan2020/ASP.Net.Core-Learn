using Code_Generation.Data;
using Code_Generation1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Code_Generation1.Pages.Suppliers
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public Supplier Supplier { get; set; } = new();

        public IActionResult OnGet(long id)
        {
            var supplier = _context.Suppliers.Find(id);
            if (supplier == null) return NotFound();
            Supplier = supplier;
            return Page();
        }
    }
}
