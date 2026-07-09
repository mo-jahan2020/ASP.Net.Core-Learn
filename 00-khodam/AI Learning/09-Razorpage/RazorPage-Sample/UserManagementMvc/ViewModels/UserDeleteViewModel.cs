namespace UserManagementMvc.ViewModels;

// ViewModel مخصوص صفحه تأیید حذف
public class UserDeleteViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Family { get; set; } = "";
    public string Email { get; set; } = "";
    public string Tel { get; set; } = "";
}
