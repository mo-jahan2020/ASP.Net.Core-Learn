using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages
{
    public class CounterModel : PageModel
    {
        [BindProperty]
        public int Count { get; set; }
        public void OnGet()
        {
            Count = 0;
        }
        public void OnPost()
        {
            Count++;
            //ModelState.Remove(nameof(Count));
            //OR 
            ModelState.SetModelValue(nameof(Count), Count.ToString(), Count.ToString());
        }
    }
}
