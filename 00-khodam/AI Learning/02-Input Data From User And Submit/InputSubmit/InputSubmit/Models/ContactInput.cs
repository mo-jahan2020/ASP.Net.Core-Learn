using System.ComponentModel.DataAnnotations;

namespace InputSubmit.Models
{

    public class ContactInput
    {
        [Required(ErrorMessage = "نام الزامی است")]
        [StringLength(25)]
        [Display(Name = "نام")]
        public string Name { get; set; } = string.Empty;
       
        [Required(ErrorMessage = "شماره تلفن الزامی است")]
        [Phone(ErrorMessage = "شماره تلفن معتبر نیست")]
        [Display(Name = "شماره تلفن")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; } = string.Empty;
    }
}