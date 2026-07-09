namespace UserManagementMvc.ViewModels;

// ViewModel مخصوص صفحه جزئیات
public class UserDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Family { get; set; } = "";
    public DateTime? BirthDate { get; set; }
    public string City { get; set; } = "";
    public string Address { get; set; } = "";
    public string Email { get; set; } = "";
    public string Tel { get; set; } = "";
}
