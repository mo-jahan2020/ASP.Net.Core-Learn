using LearnMVC.Interfaces;

namespace LearnMVC.Services;

/// <summary>
/// پیاده‌سازی واقعی سرویس
/// این کلاس توسط DI Container ساخته و مدیریت می‌شود
/// </summary>
public class GreetingService : IGreetingService
{
    // این لیست نشان می‌دهد که سرویس در طول request زنده است
    private readonly List<string> _history = new();
    private readonly ILogger<GreetingService> _logger;

    // DI Container به‌طور خودکار ILogger را تزریق می‌کند
    public GreetingService(ILogger<GreetingService> logger)
    {
        _logger = logger;
        _logger.LogInformation("✅ GreetingService ساخته شد - Scoped: یک بار در هر Request");
    }

    public string GetGreeting(string name)
    {
        var greeting = $"سلام {name}! خوش آمدی.";
        _history.Add(greeting);
        _logger.LogInformation("پیام سلام برای {Name} ساخته شد", name);
        return greeting;
    }

    public string GetTimeBasedGreeting()
    {
        var hour = DateTime.Now.Hour;
        var greeting = hour switch
        {
            >= 6 and < 12  => "🌅 صبح بخیر!",
            >= 12 and < 18 => "🌞 ظهر بخیر!",
            >= 18 and < 22 => "🌆 عصر بخیر!",
            _               => "🌙 شب بخیر!"
        };

        _history.Add(greeting);
        return greeting;
    }

    public List<string> GetGreetingHistory() => _history;
}
