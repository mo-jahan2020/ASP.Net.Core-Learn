using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Test_App1.Models
{
    public class Product
    {
        public long? ProductID { get; set; }

        [Required(ErrorMessage = "نام محصول الزامی است.")]
        [StringLength(100, ErrorMessage = "نام محصول نمی‌تواند بیش از ۱۰۰ کاراکتر باشد.")]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; }= string.Empty;

        [Required(ErrorMessage = "قیمت محصول الزامی است.")]
        [Range(0, 1000000, ErrorMessage = "قیمت باید بین ۰ و ۱,۰۰۰,۰۰۰ باشد.")]
        [Column(TypeName = "decimal(8, 2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "مدل محصول الزامی است.")]
        public string Category { get; set; } = string.Empty;
    }
}
