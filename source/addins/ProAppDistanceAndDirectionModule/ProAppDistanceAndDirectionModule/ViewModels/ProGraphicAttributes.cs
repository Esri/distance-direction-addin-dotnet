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
        public MapPoint mapPoint1 { get; set; }
        public MapPoint mapPoint2 { get; set; }
        public double _distance { get; set; }
        public string distanceunit { get; set; }
        public double angle { get; set; }
        public string angleunit { get; set; }
        public double originx { get; set; }
        public double originy { get; set; }
        public double destinationx { get; set; }
        public double destinationy { get; set; }
    }
    
    public class CircleAttributes : ProGraphicAttributes
    {
        public MapPoint mapPoint { get; set; }
        public Double distance { get; set; }
        public String distanceunit { get; set; }
        public CircleFromTypes circleFromTypes { get; set; }
        public String circletype { get; set; }
        public Double centerx { get; set; }
        public Double centery { get; set; }
    }

    public class EllipseAttributes : ProGraphicAttributes
    {
        public MapPoint mapPoint { get; set; }
        public double majorAxis{ get; set; }
        public double minorAxis { get; set; }
        public String distanceunit { get; set; }
        public double angle { get; set; }
        public string angleunit { get; set; }
        public Double centerx { get; set; }
        public Double centery { get; set; }
    }

    public class RangeAttributes : ProGraphicAttributes
    {
        public MapPoint mapPoint { get; set; }
        public int numRings { get; set; }
        public double distance { get; set; }
        public String distanceunit { get; set; }
        public int numRadials { get; set; }
        public Double centerx { get; set; }
        public Double centery { get; set; }
        public String ringorradial { get; set; }
    }

}
