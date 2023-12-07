using Microsoft.EntityFrameworkCore;
using SitronicsApi;
using SitronicsApi.Algorithm;
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

app.MapDelete("/route/{idRoute}", (int idRoute, SitrouteDataContext context) =>
{
    
    try
    {
        context.Routes.Where(r => r.IdRoute == idRoute).ExecuteDelete();
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest();
    }
});

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

app.MapGet("/routesByBusStation/{idDriver}", (int idDriver, SitrouteDataContext context) => context.Routes
                                            .Include(r => r.RouteByBusStations)
                                            .ThenInclude(rp => rp.IdBusStationNavigation)
                                            .FirstOrDefault(bs=>bs.Buses
                                            .Any(d=>d.IdDrivers.Any(d=>d.IdDriver == idDriver))));

app.MapGet("/routesStats", (SitrouteDataContext context) => context.Routes
                    .Include(r => r.Buses)
                    .ThenInclude(b => b.Schedules)
                    .ThenInclude(s => s.IdBusStationNavigation)
                    .ToList());

app.MapGet("/routes", (SitrouteDataContext context) => context.Routes.Include(r => r.RouteByBusStations).ToList());
app.MapGet("/factors", (SitrouteDataContext context) => context.Factors.ToList());

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
               || idDriver == m.IdRecipient)
               .OrderBy(m => m.Time)
               .ToList();
});

app.MapGet("/bus/{idDriver}", (int idDriver, SitrouteDataContext context) => context.Buses
        .Include(b => b.IdDrivers)
        .FirstOrDefault(b => b.IdDrivers.Any(d => d.IdDriver == idDriver)));

app.MapGet("/bus/{idBus}", (int idBus, SitrouteDataContext context) => context.Buses
        .Include(b => b.IdDrivers)
        .FirstOrDefault(b => b.IdBus == idBus));

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
    var routes = context.Routes.ToList();
    if (!routes.Any(r => r.Name == route.Name && r.IsBacked == route.IsBacked))
    {
        context.Add(route);
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
    return Results.Ok();
});

app.MapPatch("/message/reply", (Message message, SitrouteDataContext context) =>
{
    context.Messages.Where(m => m.IdRecipient == null && m.IdSender == message.IdSender)
                    .ExecuteUpdate(setters => setters.SetProperty(m => m.IdRecipient, message.IdRecipient));
    context.SaveChanges();
});

app.MapPut("/bus", (Bus bus, SitrouteDataContext context) =>
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

app.MapGet("/schedules/{IdRoute}", async (SitrouteDataContext context, int IdRoute) =>
{
    try
    {
        BusScheduleAlgorithm algorithm = new BusScheduleAlgorithm();
        var route = context.Routes
        .Include(r => r.RouteByBusStations)
        .ThenInclude(r => r.IdBusStationNavigation)
        .Include(r => r.Buses)
        .Where(r => r.IdRoute == IdRoute)
        .FirstOrDefault();
        if (route == null)
        {
            Results.NotFound(route);
            return new List<Schedule>();
        }
        var buses = route.RouteByBusStations;
        var schedules = context.Schedules;
        var todaySchedules = await schedules.Where(s => s.IdBusNavigation.IdRoute == route.IdRoute && s.Time.Date == DateTime.Today.Date).ToListAsync();
        var routeByBusStations = route.RouteByBusStations.ToList();
        if (todaySchedules.Any())
        {
            Results.Ok(todaySchedules);
            return todaySchedules;
        }
        DateTime today = DateTime.Today;
        DateTime startTime = today.AddHours(route.StartTime.Hour).AddMinutes(route.StartTime.Minute);
        DateTime endTime = today.AddHours(route.EndTime.Hour).AddMinutes(route.EndTime.Minute);
        double latitude = routeByBusStations.FirstOrDefault().IdBusStationNavigation.Location.Coordinate.Y;
        double longitude = routeByBusStations.FirstOrDefault().IdBusStationNavigation.Location.Coordinate.X;
        var weatherInfo = await WeatherManager.GetWeatherCondition(latitude, longitude);
        List<Schedule> schedule = await Task.Run(() => algorithm.GenerateRouteSchedule(
            startTime,
            endTime,
            route.IdRoute,
            routeByBusStations,
            route.Buses.ToList(),
            weatherInfo
            ));
        if (schedule.Any())
        {
            schedules.RemoveRange(schedules.Where(s => s.Time.Date == DateTime.Today.Date && s.IdBusNavigation.IdRoute == route.IdRoute));
            await schedules.AddRangeAsync(schedule);
            await context.SaveChangesAsync();
            Results.Ok(schedule);
            return schedule;
        }
        return new List<Schedule>();
    }
    catch (Exception ex)
    {
        Results.BadRequest(ex.Message);
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