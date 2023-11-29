using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Sitronics.MapManager
{
    public static class MapManager
    {
        public static GMapMarker CreateMarker(PointLatLng point, ref GMapControl mapView, GMapMarker lastMarker = null)
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

                    Stroke = Brushes.Black
                    ,
                    Width = 20,
                    Height = 20,
                    Data = (System.Windows.Media.Geometry)converter.ConvertFrom("M 808.6 403.2 c 0 -178.8 -129.8 -308.5 -308.5 -308.5 c -170.1 0 -308.5 138.4 -308.5 308.5 c 0 125.6 170.6 338.3 262.3 452.6 l 6.8 8.4 c 9.6 12 24 18.9 39.5 18.9 c 15.4 0 29.8 -6.9 39.5 -18.9 l 6.8 -8.4 c 91.5 -114.3 262.1 -327 262.1 -452.6 Z m -310.1 89.4 c -62.9 0 -114 -51.1 -114 -114 s 51.1 -114 114 -114 s 114 51.1 114 114 s -51.1 114 -114 114 Z M 500.1 67.8 c -184.9 0 -335.4 150.4 -335.4 335.4 c 0 135 174.5 352.5 268.2 469.4 l 6.7 8.4 c 14.8 18.4 36.8 29 60.4 29 s 45.6 -10.6 60.4 -29 l 6.8 -8.4 C 661 755.7 835.4 538.2 835.4 403.2 c 0 -194.3 -141 -335.4 -335.3 -335.4 Z m 0 815.3 c -15.4 0 -29.8 -6.9 -39.5 -18.9 l -6.8 -8.4 c -91.7 -114.3 -262.3 -327 -262.3 -452.6 c 0 -170.1 138.4 -308.5 308.5 -308.5 c 178.8 0 308.5 129.8 308.5 308.5 c 0 125.6 -170.6 338.3 -262.3 452.6 l -6.8 8.4 c -9.5 12 -23.9 18.9 -39.3 18.9 Z")
                };
                //(currentMarker.Shape as CustomMarker).SetContent(point, "1"); This method can trigger SetContent
                currentMarker.ZIndex = -1;
                currentMarker.Position = point;
                mapView.Markers.Add(currentMarker);
                return currentMarker;
            }
        }
    }
}

