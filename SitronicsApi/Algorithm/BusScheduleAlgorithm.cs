using Microsoft.EntityFrameworkCore;
using SitronicsApi.Data;
using SitronicsApi.Models;

namespace SitronicsApi.Algorithm
{
    public class BusScheduleAlgorithm
    {
        /// <summary>
        /// Функция для формирования расписания маршрута по параметрам
        /// </summary>
        /// <param name="startDate">Дата начала генерации</param>
        /// <param name="endDate">Дата конца генерации</param>
        /// <param name="idRoute">Идентификатор маршрута</param>
        /// <param name="routeByBusStation">Список остановок на маршруте</param>
        /// <param name="buses">Список автобусов на маршруте</param>
        /// <param name="weatherInfo">Погодные условия на момент генерации</param>
        /// <returns>Расписание</returns>
        public List<Schedule> GenerateRouteSchedule(DateTime startDate, DateTime endDate, int idRoute, List<RouteByBusStation> routeByBusStation, List<Bus> buses, string weatherInfo)
        {
            List<Schedule> schedules = new List<Schedule>();
            DateTime busStartTime;
            int busCount = buses.Count;
            if (busCount <= 1)
            {
                return schedules;
            }
            int workMinutes = endDate.Hour * 60 + endDate.Minute - startDate.Hour * 60 + startDate.Minute;
            int routeTime = GetIntervalInMinutesBetweenBusStations(idRoute, routeByBusStation.OrderBy(r => r.SerialNumberBusStation).First().IdBusStation, routeByBusStation.OrderBy(r => r.SerialNumberBusStation).Last().IdBusStation);//frequencyInMinutes * (routeByBusStation.Count - 1) * 2;

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
        /// <summary>
        /// Функция для получения релевантности маршрута по его айди
        /// </summary>
        /// <param name="idRoute"></param>
        /// <returns></returns>
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
                int routeTime = GetIntervalInMinutesBetweenBusStations(idRoute, routeByBusStations.OrderBy(r => r.SerialNumberBusStation).First().IdBusStation,
                    routeByBusStations.OrderBy(r => r.SerialNumberBusStation).Last().IdBusStation);
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
        /// <summary>
        /// Сформировать расписание для автобуса
        /// </summary>
        /// <param name="bus">Автобус</param>
        /// <param name="busStops">Остановки</param>
        /// <param name="busStartTime">Начальное время</param>
        /// <param name="weatherCondition">Погодные условия</param>
        /// <returns></returns>
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
                    (int)bus.IdRoute, startBusStop.IdBusStation, endBusStop.IdBusStation)) / GetWeatherFactor(weatherCondition); // Вычисляет время прибытия на остановку
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
        /// <summary>
        /// Функция для определения часа-пик
        /// </summary>
        /// <param name="currentDateTime">Текущее время</param>
        /// <returns></returns>
        private bool IsRushTime(DateTime currentDateTime)
        {
            var start = new TimeSpan(8, 0, 0);
            var end = new TimeSpan(10, 0, 0);
            var start2 = new TimeSpan(17, 0, 0);
            var end2 = new TimeSpan(21, 0, 0);
            TimeSpan now = currentDateTime.TimeOfDay;
            return now >= start && now < end || now >= start2 && now < end2;
        }
        /// <summary>
        /// Функция для расчета необходимого количества автобусов при определенной задержке между ними
        /// </summary>
        /// <param name="minutesToSolveRoute">Время на маршрут в одну сторону</param>
        /// <param name="delay">Необходимая задержка</param>
        /// <returns></returns>
        public int CalculateBusCount(int minutesToSolveRoute, int delay)
        {
            return minutesToSolveRoute * 2 / delay;
        }
        /// <summary>
        /// Функция для определения коэффицента влияния погоды на скорость автобуса
        /// </summary>
        /// <param name="weatherInfo"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Функция для получения количества людей на остановках определенного маршрута
        /// </summary>
        /// <param name="idRoute"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Функция для получения количества пассажиров на маршруте за определенный день
        /// </summary>
        /// <param name="date"></param>
        /// <param name="idRoute"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Функция для получения среднего количества людей, заходящих на остановках по определенному маршруту
        /// </summary>
        /// <param name="idRoute"></param>
        /// <param name="idBusStation"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Функция для получения времени пути между 2 остановками
        /// </summary>
        /// <param name="idRoute"></param>
        /// <param name="idStartBusStation"></param>
        /// <param name="idEndBusStation"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Функция для получения времени прибытия на остановку
        /// </summary>
        /// <param name="routeByBusStations"></param>
        /// <param name="idBusStation"></param>
        /// <returns></returns>
        private DateTime GetArrivalTime(List<RouteByBusStation> routeByBusStations, int idBusStation)
        {
            return routeByBusStations.FirstOrDefault(s => s.IdBusStation == idBusStation).StandardArrivalTime;
        }
    }
}