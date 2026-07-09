namespace LearnMVC.Interfaces;

/// <summary>
/// قرارداد (Contract) سرویس سلام‌دهی
/// DI فقط این Interface را می‌شناسد، نه پیاده‌سازی را
/// </summary>
public interface IGreetingService
{
    string GetGreeting(string name);
    string GetTimeBasedGreeting();
    List<string> GetGreetingHistory();
}
