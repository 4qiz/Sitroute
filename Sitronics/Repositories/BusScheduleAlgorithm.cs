using Sitronics.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
            DateTime busStartTime = startDate;
            var x = 1;
            var rounds = workMinutes / (routeTime * 2);
            for (int i = 0; i < busCount; i++)
            {
                busStartTime = startDate;
                for (int j = 1; j <= (workMinutes / routeTime); j++)
                {
                    busStartTime = busStartTime.AddMinutes(IsRushTime(busStartTime) ? rushTimeDelay * i : delay * i);
                    schedules.AddRange(MakeBusSchedule(buses[i], routeByBusStation, busStartTime, frequencyInMinutes));
                    busStartTime = startDate.AddMinutes(routeTime*j+chillTime);
                }
            }
            return schedules;
        }


        public List<Schedule> MakeBusSchedule(Bus bus, List<RouteByBusStation> busStops, DateTime busStartTime, int frequencyInMinutes)
        {
            List<Schedule> schedules = new List<Schedule>();
            foreach (var busStop in busStops)
            {
                var schedule = new Schedule
                {
                    IdBus = bus.IdBus,
                    IdBusStation = busStop.IdBusStation,
                    Time = busStartTime
                };
                schedules.Add(schedule);
                busStartTime = busStartTime.AddMinutes(frequencyInMinutes); // busStartTime.AddMinute(время до остановки)
            }
            return schedules;
        }

        private static bool IsRushTime(DateTime currentDateTime)
        {
            var start = new TimeSpan(8,0,0);
            var end = new TimeSpan(10, 0, 0);
            TimeSpan now = currentDateTime.TimeOfDay;
            return (now >= start) && (now <= end);
        }

        private static bool IsRushOur(DateTime currentDateTime)
        {
            return Enumerable.Range(9, 12).Contains(currentDateTime.Hour) || Enumerable.Range(18, 21).Contains(currentDateTime.Hour);
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

    }
}