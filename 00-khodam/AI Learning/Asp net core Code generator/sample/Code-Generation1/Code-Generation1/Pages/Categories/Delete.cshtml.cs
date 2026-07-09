using Code_Generation.Data;
using Code_Generation1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Code_Generation1.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
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
            var category = _context.Categories.Find(Category.CategoryId);
            if (category == null) return NotFound();

            _context.Categories.Remove(category);
            _context.SaveChanges();
            return RedirectToPage("/Categories/Index");
        }
    }
}
