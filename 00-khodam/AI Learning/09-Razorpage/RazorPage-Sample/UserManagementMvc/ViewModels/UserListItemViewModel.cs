namespace UserManagementMvc.ViewModels;

// هر ردیف جدول کاربران در صفحه لیست با این ViewModel نمایش داده می‌شود.
public class UserListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Family { get; set; } = "";
    public DateTime? BirthDate { get; set; }
    public string City { get; set; } = "";
    public string Email { get; set; } = "";
    public string Tel { get; set; } = "";
}
