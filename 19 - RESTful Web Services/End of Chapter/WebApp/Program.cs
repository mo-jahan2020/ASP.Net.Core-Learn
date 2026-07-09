using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
//using System.Security.Claims;
//using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
 builder.Services.AddSwaggerGen(); //مهم

builder.Services.AddDbContext<DataContext>(opts => {
    opts.UseSqlServer(builder.Configuration[
        "ConnectionStrings:ProductConnection"]);
    opts.EnableSensitiveDataLogging(true);
});

builder.Services.AddControllers();

builder.Services.Configure<JsonOptions>(opts => {
    opts.JsonSerializerOptions.DefaultIgnoreCondition
        = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

app.UseMiddleware<WebApp.TestMiddleware>();//call middleware

if (app.Environment.IsDevelopment())
{
    //مهم
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();


//app.MapGet("/hello", () => "Hello World!");

var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
SeedData.SeedDatabase(context);
//Data Source=.;Initial Catalog=SportsStore;Integrated Security=True;Trust Server Certificate=True
//Data Source=.;Initial Catalog=SportsStore;Integrated Security=True;Trust Server Certificate=True
app.Run();
