﻿using Microsoft.EntityFrameworkCore;
using SitronicsApi.Data;
using SitronicsApi.Models;

namespace SitronicsApi.Algorithm
{
    public class BusScheduleAlgorithm
    {
        public List<Schedule> GenerateRouteSchedule(DateTime startDate, DateTime endDate, int idRoute, List<RouteByBusStation> routeByBusStation, List<Bus> buses, string weatherInfo, string roadConditions)
        {
            List<Schedule> schedules = new List<Schedule>();
            DateTime busStartTime;
            int busCount = buses.Count;
            if (busCount <= 1)
            {
                return schedules;
            }
            int workMinutes = endDate.Hour * 60 + endDate.Minute - startDate.Hour * 60 + startDate.Minute;
            int routeTime = GetIntervalInMinutesBetweenBusStations(idRoute, routeByBusStation.First().IdBusStation, routeByBusStation.Last().IdBusStation);//frequencyInMinutes * (routeByBusStation.Count - 1) * 2;

            int delay = routeTime / (busCount / 2);

            int rushTimeDelay = 3;
            int chillTime = 5;

            int round = workMinutes / routeTime;
            int halfRouteTime = routeTime / 2;
            int halfBusCount = busCount / 2;
            for (int i = 0; i < busCount; i++)
            {
                busStartTime = startDate;
                for (int j = 0; j < round; j++)
                {
                    busStartTime = busStartTime.AddMinutes(halfRouteTime * j);
                    if (i >= halfBusCount)
                        busStartTime = busStartTime.AddMinutes(halfRouteTime);
                    busStartTime = busStartTime.AddMinutes(IsRushTime(busStartTime) ? rushTimeDelay * i : delay * i);
                    if (busStartTime > endDate)
                        break;
                    schedules.AddRange(MakeBusSchedule(buses[i], routeByBusStation, busStartTime, weatherInfo));
                    busStartTime = startDate.AddMinutes(routeTime * (j + 1) + chillTime * (j + 1));
                }
            }
            return schedules;
        }

        public double GetRouteProfitModifier(int idRoute)
        {
            using (var context = new SitrouteDataContext())
            {
                var peopleSum = 0.0;
                var route = context.Routes
                    .Include(r => r.RouteByBusStations)
                    .ThenInclude(r => r.IdBusStationNavigation)
                    .FirstOrDefault(r => r.IdRoute == idRoute);
                var routeByBusStations = route.RouteByBusStations;
                int routeTime = GetIntervalInMinutesBetweenBusStations(idRoute, routeByBusStations.First().IdBusStation,
                    routeByBusStations.Last().IdBusStation);
                foreach (RouteByBusStation item in routeByBusStations)
                {
                    var IdBusStation = item.IdBusStation;
                    var averagePeople = GetAveragePeopleOnBusStationByRoute(idRoute, IdBusStation);
                    if (averagePeople != null)
                    {
                        peopleSum += (double)averagePeople;
                    }
                }
                return peopleSum / routeTime;
            }
        }

        public List<Schedule> MakeBusSchedule(Bus bus, List<RouteByBusStation> busStops, DateTime busStartTime, string weatherCondition)
        {
            List<Schedule> schedules = new List<Schedule>();

            RouteByBusStation startBusStop, endBusStop;
            double busTimeBetweenStations;
            for (int i = 0; i < busStops.Count; i++)
            {
                if (i != 0)
                    startBusStop = busStops[i - 1];
                else
                    startBusStop = busStops[i];
                endBusStop = busStops[i];
                busTimeBetweenStations = Math.Abs(GetIntervalInMinutesBetweenBusStations(
                    (int)bus.IdRoute, startBusStop.IdBusStation, endBusStop.IdBusStation)) / GetWeatherFactor(weatherCondition);
                busStartTime = busStartTime.AddMinutes(busTimeBetweenStations);
                var schedule = new Schedule
                {
                    IdBus = bus.IdBus,
                    IdBusStation = endBusStop.IdBusStation,
                    Time = busStartTime
                };
                schedules.Add(schedule);

            }
            return schedules;
        }

        private bool IsRushTime(DateTime currentDateTime)
        {
            var start = new TimeSpan(8, 0, 0);
            var end = new TimeSpan(10, 0, 0);
            var start2 = new TimeSpan(17, 0, 0);
            var end2 = new TimeSpan(21, 0, 0);
            TimeSpan now = currentDateTime.TimeOfDay;
            return now >= start && now < end || now >= start2 && now < end2;
        }

        public int CalculateBusCount(int minutesToSolveRoute, int delay)
        {
            return minutesToSolveRoute * 2 / delay;
        }

        private double GetWeatherFactor(string weatherInfo)
        {
            var badWeatherFactors = new Dictionary<string, double>
            {
                { "Thunderstorm", 0.6 },
                { "Drizzle", 0.7 },
                { "Rain", 0.8 },
                { "Snow", 0.9 },
                { "Tornado", 0.9 }
            };
            if (badWeatherFactors.ContainsKey(weatherInfo))
                return badWeatherFactors[weatherInfo];
            else
                return 1;
        }

        public int GetAmountPeopleOnBusStations(int idRoute)
        {
            using (var context = new SitrouteDataContext())
            {
                var route = context.Routes
                    .Where(r => r.IdRoute == idRoute)
                    .Include(r => r.RouteByBusStations)
                    .ThenInclude(r => r.IdBusStationNavigation).FirstOrDefault();
                return (int)route.RouteByBusStations.Sum(r => r.IdBusStationNavigation.PeopleCount);
            }
        }

        public int GetPeopleOnRouteByDay(DateTime date, int idRoute)
        {
            using (var context = new SitrouteDataContext())
            {
                var route = context.Routes
                    .Where(r => r.IdRoute == idRoute)
                    .Include(r => r.Buses)
                    .ThenInclude(b => b.Schedules).FirstOrDefault(r => r.IdRoute == idRoute);

                return (int)route.Buses.Sum(b => b.Schedules.Where(s => s.Time.Date == date.Date).Sum(s => s.PeopleCountBoardingBus));
            }
        }

        public double? GetAveragePeopleOnBusStationByRoute(int idRoute, int idBusStation)
        {
            using (var context = new SitrouteDataContext())
            {
                var route = context.Routes
                    .Where(r => r.IdRoute == idRoute)
                    .Include(r => r.Buses)
                    .ThenInclude(b => b.Schedules).FirstOrDefault(r => r.IdRoute == idRoute);
                double? peopleOnBoard = route.Buses
                    .Average(b => b.Schedules
                    .Where(s => s.IdBusStation == idBusStation).Average(s => s.PeopleCountBoardingBus));
                return peopleOnBoard;
            }
        }
        private int GetIntervalInMinutesBetweenBusStations(int idRoute, int idStartBusStation, int idEndBusStation)
        {
            using (var context = new SitrouteDataContext())
            {
                var routeByBusStations = context.RouteByBusStations
                    .Where(s => s.IdRoute == idRoute)
                    .ToList();

                return Convert.ToInt32(Math.Round((
                    GetArrivalTime(routeByBusStations, idEndBusStation) -
                    GetArrivalTime(routeByBusStations, idStartBusStation)).TotalMinutes));
            }
        }

        private DateTime GetArrivalTime(List<RouteByBusStation> routeByBusStations, int idBusStation)
        {
            return routeByBusStations.FirstOrDefault(s => s.IdBusStation == idBusStation).StandardArrivalTime;
        }
    }
}