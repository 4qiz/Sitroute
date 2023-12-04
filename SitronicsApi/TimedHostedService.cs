using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitronicsApi
{
    internal static class ConverterGeometry
    {
        public static Geometry GetPointByString(string value)
        {
            if (value == null)
            {
                return null;
            }
            var startCoordinate = value.IndexOf("(") + 1;
            var startSecondCoordinate = value.IndexOf(" ", startCoordinate) + 1;
            var x = double.Parse(value.Substring(startCoordinate, startSecondCoordinate - startCoordinate - 1).Replace('.', ','));
            var y = double.Parse(value.Substring(startSecondCoordinate, value.IndexOf(")") - startSecondCoordinate).Replace('.', ','));
            return new Point(x, y) { SRID = 4326 };
        }
    }
}
