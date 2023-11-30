using Microsoft.EntityFrameworkCore;
using Sitronics.Data;
using Sitronics.Models;

namespace Sitronics
{
    public class BusScheduleAlgorithm
    {
        public List<Schedule> GenerateBusSchedule(DateTime startDate, DateTime endDate, int frequencyInMinutes, List<RouteByBusStation> routeByBusStation, List<Bus> buses, string weatherInfo, string roadConditions)
        {
            List<Schedule> schedules = new List<Schedule>();
            int workMinutes = endDate.Hour * 60 + endDate.Minute - startDate.Hour * 60 + startDate.Minute;
            int routeTime = frequencyInMinutes * (routeByBusStation.Count - 1) * 2; // Минут от начальной до конечной туда и обратно
            int busCount = buses.Count;
            int delay = (int)(routeTime / busCount);
            int rushTimeDelay = 3;
            int chillTime = 15;
            DateTime busStartTime;
            var x = 1;
            var rounds = workMinutes / (routeTime * 2);
            for (int i = 0; i < busCount; i++)
            {
                busStartTime = startDate;
                for (int j = 1; j <= (workMinutes / routeTime); j++)
                {
                    busStartTime = busStartTime.AddMinutes(IsRushTime(busStartTime) ? rushTimeDelay * i : delay * i);
                    schedules.AddRange(MakeBusSchedule(buses[i], routeByBusStation, busStartTime));
                    busStartTime = startDate.AddMinutes(routeTime * j + chillTime);
                }
            }
            return schedules;
        }


        public List<Schedule> MakeBusSchedule(Bus bus, List<RouteByBusStation> busStops, DateTime busStartTime)
        {
            List<Schedule> schedules = new List<Schedule>();

            for (int i = 1; i < busStops.Count; i++)
            {
                RouteByBusStation startBusStop = busStops[i - 1];
                RouteByBusStation endBusStop = busStops[i];
                var schedule = new Schedule
                {
                    IdBus = bus.IdBus,
                    IdBusStation = startBusStop.IdBusStation,
                    Time = busStartTime
                };
                schedules.Add(schedule);
                busStartTime = busStartTime.AddMinutes(GetIntervalInMinutesBetweenBusStations((int)bus.IdRoute, bus.IdBus, startBusStop.IdBusStation, endBusStop.IdBusStation)); // busStartTime.AddMinute(время до остановки)
            }
            return schedules;
        }

        private static bool IsRushTime(DateTime currentDateTime)
        {
            var start = new TimeSpan(8, 0, 0);
            var end = new TimeSpan(10, 0, 0);
            TimeSpan now = currentDateTime.TimeOfDay;
            return (now >= start) && (now <= end);
        }

        public int CalculateBusCount(int minutesToSolveRoute, int delay)
        {
            return minutesToSolveRoute * 2 / delay;
        }

        private double GetWeatherFactor(string weatherInfo)
        {
            var badWeatherFactors = new Dictionary<string, double>();
            badWeatherFactors.Add("icy condition", 0.6);
            badWeatherFactors.Add("fog", 0.7);
            badWeatherFactors.Add("snowfall", 0.8);
            if (badWeatherFactors.ContainsKey(weatherInfo))
                return badWeatherFactors[weatherInfo];
            else
                return 1;
        }

        private bool IsGoodRoadConditions(string roadConditions)
        {
            // Внедрить логику, чтобы проверить, подходят ли дорожные условия для поездок на автобусе
            return true;
        }
        private int GetIntervalInMinutesBetweenBusStations(int idRoute, int idBus, int idStartBusStation, int idEndBusStation)
        {
            using (var context = new SitrouteDataContext())
            {
                var routeByBusStations = context.RouteByBusStations
                    .Include(s => s.IdBusStationNavigation)
                    .ThenInclude(bs => bs.Schedules)
                    .Where(s => s.IdRoute == idRoute)
                    .ToList();

                return Convert.ToInt32(Math.Round((
                    GetArrivalTime(routeByBusStations, idEndBusStation, idBus) -
                    GetArrivalTime(routeByBusStations, idStartBusStation, idBus)).TotalMinutes));
            }
        }

        private DateTime GetArrivalTime(List<RouteByBusStation> routeByBusStations, int idBusStation, int idBus)
        {
            return routeByBusStations.Where(s => s.IdBusStation == idBusStation).First().IdBusStationNavigation.Schedules
                                    .Where(s => s.IdBus == idBus)
                                    .OrderBy(s => s.Time)
                                    .Select(s => new { s.Time })
                                    .First().Time;
        }
    }
}