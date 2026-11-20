using Code_Generation.Data;
using Code_Generation1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Code_Generation1.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
        _context = context;
        }

        public List<Product> Products { get; set; } = new();
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int? pageIndex)
        {
            int currentPage = pageIndex ?? 1;

            if (currentPage < 1) currentPage = 1;

            TotalCount = await _context.Products.CountAsync();

            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            if (TotalPages == 0) TotalPages = 1;
            if (currentPage > TotalPages) currentPage = TotalPages;

            PageIndex = currentPage;

            Products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .OrderBy(p => p.ProductID)
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }
    }
}
