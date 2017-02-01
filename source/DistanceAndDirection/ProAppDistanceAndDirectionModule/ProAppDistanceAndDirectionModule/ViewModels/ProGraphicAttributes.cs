using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using DistanceAndDirectionLibrary;

namespace ProAppDistanceAndDirectionModule
{
    public class ProGraphicAttributes
    {
    }

    public class LineAttributes : ProGraphicAttributes
    {
        Tuple<MapPoint, MapPoint, double, double> attributes;

        public LineAttributes(MapPoint startPt, MapPoint endPt, double distance, double angle)
        {
            attributes = new Tuple<MapPoint, MapPoint, double, double>(startPt, endPt, distance, angle);
        }

        public Tuple<MapPoint,  MapPoint, double, double> GetAttributes()
        {
            return attributes;
        }
    }
    
    public class CircleAttributes : ProGraphicAttributes
    {
        Tuple<MapPoint, Double, CircleFromTypes> attributes;

        public CircleAttributes(MapPoint centerPt, double distance, CircleFromTypes circleFromTypes)
        {
            attributes = new Tuple<MapPoint, double, CircleFromTypes>(centerPt, distance, circleFromTypes);
        }

        public Tuple<MapPoint, Double, CircleFromTypes> GetAttributes()
        {
            return attributes;
        }
    }

    public class EllipseAttributes : ProGraphicAttributes
    {
        Tuple<MapPoint, double, double, double> attributes;

        public EllipseAttributes(MapPoint centerPt, double minorX, double majorX, double orientX)
        {
            attributes = new Tuple<MapPoint, double, double, double>(centerPt, minorX, majorX, orientX);
        }

        public Tuple<MapPoint, double, double, double> GetAttributes()
        {
            return attributes;
        }
    }

    public class RangeAttributes : ProGraphicAttributes
    {
        Tuple<MapPoint, int, double, int> attributes;

        public RangeAttributes(MapPoint centerPt, int rings, double distance, int radials)
        {
            attributes = new Tuple<MapPoint, int, double, int>(centerPt, rings, distance, radials);
        }

        public Tuple<MapPoint, int, double, int> GetAttributes()
        {
            return attributes;
        }
    }

}
