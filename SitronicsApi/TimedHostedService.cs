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
                    var buses = route.RouteByBusStations;
                    var schedules = context.Schedules;
                    var todaySchedules = schedules.Where(s => s.IdBusNavigation.IdRoute == route.IdRoute && s.Time.Date == DateTime.Today.Date);
                    if (todaySchedules.Any())
                        continue;
                    DateTime today = DateTime.Today;
                    DateTime startTime = today.AddHours(route.StartTime.Hour).AddMinutes(route.StartTime.Minute);
                    DateTime endTime = today.AddHours(route.EndTime.Hour).AddMinutes(route.EndTime.Minute);
                    List<Schedule> schedule = await Task.Run(() => algorithm.GenerateRouteSchedule(
                        startTime,
                        endTime,
                        route.IdRoute,
                        route.RouteByBusStations.ToList(),
                        route.Buses.ToList(),
                        "",
                        ""
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
