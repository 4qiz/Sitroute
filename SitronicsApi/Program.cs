using Microsoft.EntityFrameworkCore;
using SitronicsApi;
using SitronicsApi.Data;
using SitronicsApi.Models;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<TimedHostedService>();

builder.Services.AddDbContext<SitrouteDataContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/admins/{login}/{password}", (string login, string password, SitrouteDataContext context) =>
{
    var user = context.Users.Include(u => u.Admin)
                            .FirstOrDefault(u => u.Login == login.Trim());

    return Authenticate(user, password);
});

app.MapGet("/drivers/{login}/{password}", (string login, string password, SitrouteDataContext context) =>
{
    var user = context.Users.Include(u => u.Driver).FirstOrDefault(u => u.Login == login.Trim());
    user.Driver.IdDriverNavigation = null;

    return Authenticate(user, password);
});

app.MapGet("/busStations", (SitrouteDataContext context) => context.BusStations.ToList());

app.MapGet("/buses", (SitrouteDataContext context) => context.Buses.Where(b => b.Location != null).Include(b => b.Schedules).ToList());

app.MapGet("/drivers", (SitrouteDataContext context) =>
{
    var drivers = context.Drivers.Include(d => d.IdDriverNavigation).ToList();
    drivers.ForEach(d => d.IdDriverNavigation.Driver = null);
    return drivers;
});

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

app.MapGet("/chat/{idDriver}/{idDispatcher}", (int idDriver, int idDispatcher, SitrouteDataContext context) =>
{
    return context.Messages
           .Include(m => m.IdRecipientNavigation)
           .Include(m => m.IdSenderNavigation)
           .Where(m => m.IdSender == idDriver && m.IdRecipient == idDispatcher
                || m.IdRecipient == idDriver && m.IdSender == idDispatcher
                || m.IdRecipient == null && m.IdSender == idDriver)
           .ToList();
});
app.MapGet("/chat/{idDriver}", (int idDriver, SitrouteDataContext context) =>
{
    var isDriverNull = context.Drivers.Any(d => d.IdDriver == idDriver);
    return context.Messages
               .Include(m => m.IdRecipientNavigation)
               .Include(m => m.IdSenderNavigation)
               .Where(m => idDriver == m.IdSender
               || idDriver == m.IdRecipient
               || null == m.IdRecipient)
               .OrderBy(m => m.Time)
               .ToList();
});

app.MapGet("/bus/{idDriver}", (int idDriver, SitrouteDataContext context) => context.Buses
        .Include(b=>b.IdDrivers)
        .FirstOrDefault(b=>b.IdDrivers.Any(d=>d.IdDriver == idDriver)));

app.MapPost("/busStation", (BusStation busStation, SitrouteDataContext context) =>
{
    try
    {
        context.BusStations.Add(busStation);
        context.SaveChanges();
        return Results.Ok(busStation);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
    }
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

app.MapPut("/bus", (Bus bus, SitrouteDataContext context)=>
{
    context.Update(bus);
    context.SaveChanges();
});

app.MapGet("/schedules/{IdRoute}/{IdBusStation}", (SitrouteDataContext context, int IdRoute, int IdBusStation) =>
{
    try
    {
        var todaySchedules = context.Schedules
        .Where(s => s.IdBusNavigation.IdRoute == IdRoute && s.Time.Date == DateTime.Today.Date && s.IdBusStation == IdBusStation)
        .OrderBy(s => s.Time)
        .ToList();
        Results.Ok(todaySchedules);
        return todaySchedules;
    }
    catch
    {
        Results.BadRequest();
        return new List<Schedule>();
    }
});
static byte[] ComputeSha256Hash(string rawData)
{
    using (SHA256 sha256Hash = SHA256.Create())
        return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
}

app.Run();

static IResult Authenticate(User? user, string password)
{
    var hashInput = ComputeSha256Hash(password);

    User currentUser = new User();
    if (user != null && Convert.ToHexString(user.Password) == Convert.ToHexString(hashInput))
    {
        currentUser = user;
    }
    return Results.Ok(currentUser);
}