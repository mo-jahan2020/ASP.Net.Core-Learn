using Code_Generation.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SportsStoreConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();

//default razar page folder is /Pages
builder.Services.AddRazorPages();
//set custom path for razaor page to /Views
//builder.Services.AddRazorPages(options =>
//{
//    options.RootDirectory = "/Views";
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();


app.MapRazorPages();

// حذف MapControllerRoute تکراری - فقط یک مسیر default کافی است
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();