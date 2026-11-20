using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Code_Generation1.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ProductID { get; set; }

        [Required(ErrorMessage = "وارد کردن نام محصول اجباری است")]
        [MaxLength(25, ErrorMessage = "نام محصول نباید بیشتر از 25 کاراکتر باشد")]
        public string? Name { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "وارد کردن قیمت اجباری است")]
        [Range(0, double.MaxValue, ErrorMessage = "قیمت باید عددی مثبت باشد")]
        public decimal Price { get; set; }

        // Foreign keys
        public long CategoryId { get; set; }
        public long SupplierId { get; set; }

        // Navigation properties
        public Category? Category { get; set; }
        public Supplier? Supplier { get; set; }
    }
}