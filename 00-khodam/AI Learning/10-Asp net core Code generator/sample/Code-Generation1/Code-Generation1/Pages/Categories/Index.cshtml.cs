using Code_Generation.Data;
using Code_Generation1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Code_Generation1.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Category> Categories { get; set; } = new();
        public async Task<IActionResult> OnGetAsync()
        {
            Categories = await _context.Categories.ToListAsync();
            return Page();
        }
    }
}