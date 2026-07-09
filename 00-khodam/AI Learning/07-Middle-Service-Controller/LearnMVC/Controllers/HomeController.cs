using LearnMVC.Interfaces;
using LearnMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace LearnMVC.Controllers;

/// <summary>
/// HomeController - نقطه ورود اصلی برنامه
/// 
/// DI Container به‌طور خودکار IGreetingService را اینجا تزریق می‌کند
/// Controller نمی‌داند پشت Interface چه کلاسی است!
/// </summary>
public class HomeController : Controller
{
    private readonly IGreetingService _greetingService;
    private readonly ILogger<HomeController> _logger;

    // Constructor Injection: DI Container این constructor را صدا می‌زند
    // و سرویس‌های لازم را خودش پیدا و تزریق می‌کند
    public HomeController(IGreetingService greetingService, ILogger<HomeController> logger)
    {
        _greetingService = greetingService;
        _logger = logger;

        _logger.LogInformation("🎮 HomeController ساخته شد");
    }

    // GET: /Home/Index  یا  /
    public IActionResult Index()
    {
        _logger.LogInformation("📄 Action: Index");

        var model = new HomeViewModel
        {
            PersonalGreeting  = _greetingService.GetGreeting("کاربر عزیز"),
            TimeGreeting      = _greetingService.GetTimeBasedGreeting(),
            GreetingHistory   = _greetingService.GetGreetingHistory(),
            RequestPath       = HttpContext.Request.Path,
            RequestTime       = (DateTime)(HttpContext.Items["RequestStartTime"] ?? DateTime.Now)
        };

        return View(model);
    }

    // GET: /Home/Greet?name=علی
    public IActionResult Greet(string name = "مهمان")
    {
        _logger.LogInformation("👋 Action: Greet برای {Name}", name);

        var model = new HomeViewModel
        {
            PersonalGreeting  = _greetingService.GetGreeting(name),
            TimeGreeting      = _greetingService.GetTimeBasedGreeting(),
            GreetingHistory   = _greetingService.GetGreetingHistory(),
            RequestPath       = HttpContext.Request.Path,
            RequestTime       = (DateTime)(HttpContext.Items["RequestStartTime"] ?? DateTime.Now)
        };

        return View("Index", model);
    }
}
