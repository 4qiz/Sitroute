using Microsoft.EntityFrameworkCore;
using SitronicsApi.Data;
using SitronicsApi.Models;
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

app.MapGet("/buses", (SitrouteDataContext context) => context.Buses.Where(b => b.Location != null).Include(b => b.Schedules).ToList());

app.MapGet("/routesByBusStations", (SitrouteDataContext context) => context.Routes
                                            .Include(r => r.RouteByBusStations)
                                            .ThenInclude(rp => rp.IdBusStationNavigation)
                                            .ToList());

app.MapGet("/routesStats", (SitrouteDataContext context) => context.Routes
                    .Include(r => r.Buses)
                    .ThenInclude(b => b.Schedules)
                    .ThenInclude(s => s.IdBusStationNavigation)
                    .ToList());

app.MapGet("/routes", (SitrouteDataContext context) => context.Routes.ToList());

app.MapGet("/chat/{idDriver}/{idDispatcher}", (int idDriver, int idDispatcher, SitrouteDataContext context) => context.Messages
                    .Include(m => m.IdRecipientNavigation)
                    .Include(m => m.IdSenderNavigation)
                    .ThenInclude(u => u.Driver)
                    .Where(m => m.IdSender == idDriver && m.IdRecipient == idDispatcher
                        || m.IdRecipient == idDriver && m.IdSender == idDispatcher
                        || m.IdRecipient == null && m.IdSender == idDriver)
                    .ToList());

app.MapPost("/busStation", (BusStation busStation, SitrouteDataContext context) =>
{
    context.BusStations.Add(busStation);
    context.SaveChanges();
});

app.MapPost("/route", (SitronicsApi.Models.Route route, SitrouteDataContext context) =>
{
    if (!context.Routes.Any(r => r.Name == route.Name && r.IsBacked == route.IsBacked))
    {
        context.Routes.Add(route);
        context.SaveChanges();
        return Results.Ok();
    }
    else
    {
        return Results.BadRequest();
    }
});

app.MapPost("/bus", (Bus bus, SitrouteDataContext context) =>
{
    if (!context.Buses.Any(b => b.Number == bus.Number))
    {
        context.Buses.Add(bus);
        context.SaveChanges();
        return Results.Ok();
    }
    else
    {
        return Results.BadRequest();
    }
});

app.MapPost("/message", (Message message, SitrouteDataContext context) =>
{
    context.Messages.Add(message);
    context.SaveChanges();
});

app.MapPatch("/message/reply", (Message message, SitrouteDataContext context) =>
{

    context.Messages.Where(m => m.IdRecipient == null && m.IdSender == message.IdSender)
                    .ExecuteUpdate(setters => setters.SetProperty(m => m.IdRecipient, message.IdRecipient));
    context.SaveChanges();
});

static byte[] ComputeSha256Hash(string rawData)
{
    using (SHA256 sha256Hash = SHA256.Create())
        return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
}

app.Run();