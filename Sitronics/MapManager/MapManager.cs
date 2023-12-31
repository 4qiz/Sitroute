﻿using GMap.NET;
using GMap.NET.WindowsPresentation;
using Sitronics.Models;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Sitronics.MapManager
{
    /// <summary>
    /// Класс для работы с картой
    /// </summary>
    public static class MapManager
    {
        public static string BasicIcon
        {
            get =>
                "M 808.6 403.2 c 0 -178.8 -129.8 -308.5 -308.5 -308.5 c -170.1 0 -308.5 138.4 -308.5 308.5 c 0 125.6 170.6 338.3 262.3 452.6 l 6.8 8.4 c 9.6 12 24 18.9 39.5 18.9 c 15.4 0 29.8 -6.9 39.5 -18.9 l 6.8 -8.4 c 91.5 -114.3 262.1 -327 262.1 -452.6 Z m -310.1 89.4 c -62.9 0 -114 -51.1 -114 -114 s 51.1 -114 114 -114 s 114 51.1 114 114 s -51.1 114 -114 114 Z M 500.1 67.8 c -184.9 0 -335.4 150.4 -335.4 335.4 c 0 135 174.5 352.5 268.2 469.4 l 6.7 8.4 c 14.8 18.4 36.8 29 60.4 29 s 45.6 -10.6 60.4 -29 l 6.8 -8.4 C 661 755.7 835.4 538.2 835.4 403.2 c 0 -194.3 -141 -335.4 -335.3 -335.4 Z m 0 815.3 c -15.4 0 -29.8 -6.9 -39.5 -18.9 l -6.8 -8.4 c -91.7 -114.3 -262.3 -327 -262.3 -452.6 c 0 -170.1 138.4 -308.5 308.5 -308.5 c 178.8 0 308.5 129.8 308.5 308.5 c 0 125.6 -170.6 338.3 -262.3 452.6 l -6.8 8.4 c -9.5 12 -23.9 18.9 -39.3 18.9 Z";
        }
        /// <summary>
        /// Функция для создания маркера на карте
        /// </summary>
        /// <param name="point">Точка создания маркера</param>
        /// <param name="mapView">Объект карты</param>
        /// <param name="Icon">Иконка для маркера</param>
        /// <param name="lastMarker">Предыдущий маркер</param>
        /// <param name="fillColor">Цвет заливки маркера</param>
        /// <returns></returns>
        public static GMapMarker CreateMarker(PointLatLng point, ref GMapControl mapView, string? Icon = null, GMapMarker? lastMarker = null, SolidColorBrush? fillColor = null)
        {
            var converter = TypeDescriptor.GetConverter(typeof(System.Windows.Media.Geometry));
            if (lastMarker != null)
                mapView.Markers.Remove(lastMarker);
            var currentMarker = new GMapMarker(point);
            {
                currentMarker.Shape = new Path()
                {
                    StrokeThickness = 1,
                    Stretch = Stretch.Fill,
                    Stroke = Brushes.Black,
                    Width = 15,
                    Height = 15,
                    Data = (System.Windows.Media.Geometry)converter.ConvertFrom(Icon == null ? BasicIcon : Icon),
                    Fill = fillColor

                };
                //(currentMarker.Shape as CustomMarker).SetContent(point, "1"); This method can trigger SetContent
                currentMarker.ZIndex = -1;
                currentMarker.Position = point;
                mapView.Markers.Add(currentMarker);
                return currentMarker;
            }
        }
        /// <summary>
        /// Функция для создания маркера автобуса
        /// </summary>
        /// <param name="point">Точка создания маркера</param>
        /// <param name="mapView">Объект карты</param>
        /// <param name="bus">Объект автобуса</param>
        /// <param name="lastMarker"> предыдущий маркер</param>
        /// <returns></returns>
        public static GMapMarker CreateBusMarker(PointLatLng point, ref GMapControl mapView, Bus bus, GMapMarker? lastMarker = null)
        {
            string busIcon = "M288 0C422.4 0 512 35.2 512 80V96l0 32c17.7 0 32 14.3 32 32v64c0 17.7-14.3 32-32 32l0 160c0 17.7-14.3 32-32 32v32c0 17.7-14.3 32-32 32H416c-17.7 0-32-14.3-32-32V448H192v32c0 17.7-14.3 32-32 32H128c-17.7 0-32-14.3-32-32l0-32c-17.7 0-32-14.3-32-32l0-160c-17.7 0-32-14.3-32-32V160c0-17.7 14.3-32 32-32h0V96h0V80C64 35.2 153.6 0 288 0zM128 160v96c0 17.7 14.3 32 32 32H272V128H160c-17.7 0-32 14.3-32 32zM304 288H416c17.7 0 32-14.3 32-32V160c0-17.7-14.3-32-32-32H304V288zM144 400a32 32 0 1 0 0-64 32 32 0 1 0 0 64zm288 0a32 32 0 1 0 0-64 32 32 0 1 0 0 64zM384 80c0-8.8-7.2-16-16-16H208c-8.8 0-16 7.2-16 16s7.2 16 16 16H368c8.8 0 16-7.2 16-16z";
            var peopleInBus = bus.Schedules.Sum(s => s.PeopleCountBoardingBus) -
                        bus.Schedules.Sum(s => s.PeopleCountGettingOffBus);
            SolidColorBrush fillColor;
            if (((double)peopleInBus / (double)bus.Сapacity) >= 0.8)
                fillColor = Brushes.Red;
            else if (((double)peopleInBus / (double)bus.Сapacity) >= 0.50)
                fillColor = Brushes.Yellow;
            else
                fillColor = Brushes.Green;
            var currentMarker = CreateMarker(point, ref mapView, busIcon, lastMarker, fillColor: fillColor);
            return currentMarker;
        }

        /// <summary>
        /// Функция для создания маркера автобусной остановки
        /// </summary>
        /// <param name="point">Точка создания маркера</param>
        /// <param name="mapView">Объект карты</param>
        /// <param name="busStation"></param>
        /// <param name="lastMarker"></param>
        /// <returns></returns>
        public static GMapMarker CreateBusStationMarker(PointLatLng point, ref GMapControl mapView, BusStation busStation, GMapMarker? lastMarker = null)
        {
            string busIcon = "M480 0c-17.7 0-32 14.3-32 32V192 512h64V192H624c8.8 0 16-7.2 16-16V48c0-8.8-7.2-16-16-16H512c0-17.7-14.3-32-32-32zM416 159L276.8 39.7c-12-10.3-29.7-10.3-41.7 0l-224 192C1 240.4-2.7 254.5 2 267.1S18.6 288 32 288H64V480c0 17.7 14.3 32 32 32h64c17.7 0 32-14.3 32-32V384c0-17.7 14.3-32 32-32h64c17.7 0 32 14.3 32 32v96c0 17.7 14.3 32 32 32h64.7l.2 0h-1V159z";
            var peopleOnBusStation = busStation.PeopleCount;
            SolidColorBrush fillColor;
            fillColor = Brushes.AliceBlue;
            var currentMarker = CreateMarker(point, ref mapView, busIcon, lastMarker, fillColor: fillColor);
            return currentMarker;
        }
        /// <summary>
        /// Функция для создания маркера происшествия
        /// </summary>
        /// <param name="point">Точка создания маркера</param>
        /// <param name="mapView">Объект карты</param>
        /// <param name="factor"></param>
        /// <param name="lastMarker"></param>
        /// <returns></returns>
        public static GMapMarker CreateIncidentMarker(PointLatLng point, ref GMapControl mapView, Factor factor, GMapMarker? lastMarker = null)
        {
            string incidentIcon = "M256 32c14.2 0 27.3 7.5 34.5 19.8l216 368c7.3 12.4 7.3 27.7 .2 40.1S486.3 480 472 480H40c-14.3 0-27.6-7.7-34.7-20.1s-7-27.8 .2-40.1l216-368C228.7 39.5 241.8 32 256 32zm0 128c-13.3 0-24 10.7-24 24V296c0 13.3 10.7 24 24 24s24-10.7 24-24V184c0-13.3-10.7-24-24-24zm32 224a32 32 0 1 0 -64 0 32 32 0 1 0 64 0z";
            SolidColorBrush fillColor;
            fillColor = Brushes.Red;
            var currentMarker = CreateMarker(point, ref mapView, incidentIcon, lastMarker, fillColor: fillColor);
            return currentMarker;
        }
    }
}

