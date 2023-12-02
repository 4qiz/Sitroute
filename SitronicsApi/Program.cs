using Microsoft.EntityFrameworkCore;
using Sitronics.Data;
using Sitronics.Models;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SitrouteDataContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/users/{login}/{password}", (string login, string password, SitrouteDataContext context) =>
{
    var user = context.Users.Include(u => u.Admin)
                    .Where(u => u.Login == login.Trim())
                .FirstOrDefault();

    var hashInput = ComputeSha256Hash(password);

    User currentUser = new User();
    if (user != null && Convert.ToHexString(user.Password) == Convert.ToHexString(hashInput))
    {
        currentUser = user;
    }
    return Results.Ok(currentUser);
});

app.MapGet("/busStations", (SitrouteDataContext context) => context.BusStations.ToList());
app.MapGet("/buses", (SitrouteDataContext context) => context.Buses.Where(b => b.Location != null).ToList());
app.MapGet("/routes", (SitrouteDataContext context) => context.Routes
                                            .Include(r => r.RouteByBusStations).ThenInclude(rp => rp.IdBusStationNavigation));
app.MapGet("/routesStats", (SitrouteDataContext context) => context.Routes
                    .Include(r => r.Buses)
                    .ThenInclude(b => b.Schedules)
                    .ThenInclude(s => s.IdBusStationNavigation)
                    .ToList());

static byte[] ComputeSha256Hash(string rawData)
{
    using (SHA256 sha256Hash = SHA256.Create())
        return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
}



//var summaries = new[]
//{
//    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//};

//app.MapGet("/weatherforecast", () =>
//{
//    var forecast = Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();
//    return forecast;
//})
//.WithName("GetWeatherForecast")
//.WithOpenApi();

app.Run();

//internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}
