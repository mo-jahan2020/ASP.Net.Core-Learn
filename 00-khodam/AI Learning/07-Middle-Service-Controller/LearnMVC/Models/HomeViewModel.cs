namespace LearnMVC.Models;

public class HomeViewModel
{
    public string PersonalGreeting { get; set; } = "";
    public string TimeGreeting { get; set; } = "";
    public List<string> GreetingHistory { get; set; } = new();
    public string RequestPath { get; set; } = "";
    public DateTime RequestTime { get; set; }
}
