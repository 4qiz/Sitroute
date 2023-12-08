using Microsoft.EntityFrameworkCore;
using SitronicsApi.Algorithm;
using SitronicsApi.Data;
using SitronicsApi.Models;
using System.Diagnostics;
using System.Security.Cryptography;

namespace SitronicsApi
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private Timer? _timer = null;

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(GenerateSchedule, null, TimeSpan.Zero,
                TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        private async void GenerateSchedule(object? state)
        {
            BusScheduleAlgorithm algorithm = new BusScheduleAlgorithm();
            using (var context = new SitrouteDataContext())
            {
                var routes = context.Routes
                    .Include(r => r.RouteByBusStations)
                    .ThenInclude(r => r.IdBusStationNavigation)
                    .Include(r => r.Buses).ToList();
                foreach (var route in routes)
                {
                    Debug.WriteLine($"{route.IdRoute} {route.Name}");
                    var buses = route.Buses;
                    var schedules = context.Schedules;
                    var todaySchedules = schedules.Where(s => s.IdBusNavigation.IdRoute == route.IdRoute && s.Time.Date == DateTime.Today.Date);
                    var routeByBusStations = route.RouteByBusStations.ToList();
                    if (todaySchedules.Any() || buses.Count <= 1)
                        continue;
                    DateTime today = DateTime.Today;
                    DateTime startTime = today.AddHours(route.StartTime.Hour).AddMinutes(route.StartTime.Minute);
                    DateTime endTime = today.AddHours(route.EndTime.Hour).AddMinutes(route.EndTime.Minute);
                    double latitude = routeByBusStations.FirstOrDefault().IdBusStationNavigation.Location.Coordinate.Y;
                    double longitude  = routeByBusStations.FirstOrDefault().IdBusStationNavigation.Location.Coordinate.X;
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
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }
}
