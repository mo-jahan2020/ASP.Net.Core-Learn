using InputSubmit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InputSubmit.Pages

{
    public class ContactModel : PageModel
    {
        [BindProperty]
        public ContactInput Input { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public void OnGet()
        {
        }
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            SuccessMessage = $"ثبت شد: {Input.Name} — {Input.Phone} — {Input.Email}";
            Input = new ContactInput();
            return Page();
        }
    }
}