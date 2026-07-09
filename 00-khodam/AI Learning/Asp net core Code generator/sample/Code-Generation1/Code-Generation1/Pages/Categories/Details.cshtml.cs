using Code_Generation.Data;
using Code_Generation1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Code_Generation1.Pages.Categories
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public Category _Category { get; set; } = new();

        public IActionResult OnGet(long id)
        {
            var category1 = _context.Categories.Find(id);
            if (category1 == null) return NotFound();
            _Category = category1;
            return Page();
        }
    }
}
