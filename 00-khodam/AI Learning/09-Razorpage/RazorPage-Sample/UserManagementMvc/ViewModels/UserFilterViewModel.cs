using System.ComponentModel.DataAnnotations;

namespace UserManagementMvc.ViewModels;

// این ViewModel فقط برای دریافت پارامترهای جستجو، فیلتر و مرتب‌سازی است.
public class UserFilterViewModel
{
    [Display(Name = "جستجو")]
    public string? SearchTerm { get; set; }

    [Display(Name = "شهر")]
    public string? City { get; set; }

    public string SortField { get; set; } = "Id";

    public string SortDirection { get; set; } = "asc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 5;
}
