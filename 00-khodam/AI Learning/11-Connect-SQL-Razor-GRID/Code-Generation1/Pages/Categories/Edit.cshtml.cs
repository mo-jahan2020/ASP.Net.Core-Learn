using Code_Generation.Data;
using Code_Generation1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Code_Generation1.Pages.Categories
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Category Category { get; set; } = new();

        public IActionResult OnGet(long id)
        {
            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();
            Category = category;
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            var existing = _context.Categories.Find(Category.CategoryId);
            if (existing == null) return NotFound();

            existing.Name = Category.Name;
            _context.SaveChanges();
            return RedirectToPage("/Categories/Index");
        }
    }
}
