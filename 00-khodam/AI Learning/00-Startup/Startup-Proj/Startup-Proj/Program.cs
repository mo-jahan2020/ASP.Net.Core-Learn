using Startup_Proj.Midelwares;
using Startup_Proj.Controllers;
using System.Globalization;

var faCulture = new CultureInfo("fa-IR")
{
    DateTimeFormat = { Calendar = new GregorianCalendar() }
};
CultureInfo.DefaultThreadCurrentCulture = faCulture;
CultureInfo.DefaultThreadCurrentUICulture = faCulture;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddControllers().AddXmlSerializerFormatters();
var app = builder.Build();

//app.UseMiddleware<MyCustomMiddleware>();
//app.UseNewMiddleware();

app.MapControllers();
app.Run();
