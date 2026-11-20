using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Code_Generation1.Models;

namespace Code_Generation1.Pages.Suppliers
{
    public class UserFormModel : PageModel
    {
        [BindProperty]
        public UserInfoViewModel UserInfo { get; set; } = new UserInfoViewModel();

        public void OnGet() { }

        public IActionResult OnPostSave()
        {
            if (!ModelState.IsValid) return Page();

            // Add your logic to save to database here
            return Page();
        }

        public IActionResult OnPostClear()
        {
            ModelState.Clear();
            UserInfo = new UserInfoViewModel();
            return Page();
        }
    }
    
}
