// Copyright 2016 Esri 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;

namespace ArcMapAddinGeodesyAndRange.ViewModels
{
    public class EllipseViewModel : TabBaseViewModel
    {
        /// <summary>
        /// CTOR
        /// </summary>
        public EllipseViewModel()
        {
            EllipseType = EllipseTypes.Semi;
            ElementTag = Guid.NewGuid().ToString();
        }

        #region Enum
        private enum ElementColor
        {
            Red, Green, Blue
        }
        #endregion

        #region Properties
        public EllipseTypes EllipseType { get; set; }
        public IPoint CenterPoint { get; set; }
        public string ElementTag { get; set; }

        AzimuthTypes azimuthType = AzimuthTypes.Degrees;
        public AzimuthTypes AzimuthType
        {
            get { return azimuthType; }
            set
            {
                var before = azimuthType;
                azimuthType = value;
                UpdateAzimuthFromTo(before, value);
            }
        }

        private IPoint point2 = null;
        public override IPoint Point2
        {
            get
            {
                return point2;
            }
            set
            {
                point2 = value;
                RaisePropertyChanged(() => Point2);
            }
        }

        private bool HasPoint3 = false;
        private IPoint point3 = null;
        public IPoint Point3
        {
            get
            {
                return point3;
            }
            set
            {
                point3 = value;
                RaisePropertyChanged(() => Point3);
            }
        }

        private double minorAxisDistance = 0.0;
        public double MinorAxisDistance
        {
            get
            {
                return minorAxisDistance;
            }
            set
            {
                minorAxisDistance = value;
                MinorAxisDistanceString = string.Format("{0:0.00}", minorAxisDistance);
                RaisePropertyChanged(() => MinorAxisDistance);
                RaisePropertyChanged(() => MinorAxisDistanceString);
            }
        }

        private string minorAxisDistanceString = string.Empty;
        public string MinorAxisDistanceString
        {
            get
            {
                return string.Format("{0:0.00}", MinorAxisDistance);
            }
            set
            {
                minorAxisDistanceString = value;
            }
        }

        private double majorAxisDistance = 0.0;
        public double MajorAxisDistance
        {
            get
            {
                return majorAxisDistance;
            }
            set
            {
                majorAxisDistance = value;
                MajorAxisDistanceString = string.Format("{0:0.00}", majorAxisDistance);
                RaisePropertyChanged(() => MajorAxisDistance);
                RaisePropertyChanged(() => MajorAxisDistanceString);
            }
        }

        private string majorAxisDistanceString = string.Empty;
        public string MajorAxisDistanceString
        {
            get
            {
                return string.Format("{0:0.00}", MajorAxisDistance);
            }
            set
            {
                majorAxisDistanceString = value;
            }
        }

        private double distance2 = 0.0;
        public double Distance2
        {
            get { return distance2; }
            set
            {
                distance2 = value;
                Distance2String = string.Format("{0:0.00}", distance2);
                RaisePropertyChanged(() => Distance2);
                RaisePropertyChanged(() => Distance2String);
            }
        }
        string distance2String = String.Empty;
        public string Distance2String
        {
            get
            {
                return string.Format("{0:0.00}", Distance2);
            }
            set
            {
                distance2String = value;
            }
        }

        double azimuth = 0.0;
        public double Azimuth
        {
            get { return azimuth; }
            set
            {
                azimuth = value;
                RaisePropertyChanged(() => Azimuth);

                AzimuthString = string.Format("{0:0.00}", azimuth);
                RaisePropertyChanged(() => AzimuthString);
            }
        }
        string azimuthString = string.Empty;
        public string AzimuthString
        {
            get { return azimuthString; }
            set
            {
                if (string.Equals(azimuthString, value))
                    return;

                azimuthString = value;
            }
        }
        #endregion

        internal override void CreateMapElement()
        {
            if (Point1 == null || Point2 == null || Point3 == null)
            {
                return;
            }
            DrawEllipse();
            Reset(false);
        }

        internal override void OnMouseMoveEvent(object obj)
        {
            if (!IsActiveTab)
                return;

            var point = obj as IPoint;

            if (point == null)
                return;

            if (feedback == null)
            {
                var mxdoc = ArcMap.Application.Document as IMxDocument;
                var av = mxdoc.FocusMap as IActiveView;
                CreateFeedback(point, av);
            }

            //dynamic updates
            if (!HasPoint1)
            {
                Point1 = point;
            }
            else if (HasPoint1 && !HasPoint2)
            {
                // update major
                var polyline = GetPolylineFromFeedback(Point1, point);
                // get major distance from polyline
                MajorAxisDistance = GetGeodeticLengthFromPolyline(polyline);
                // update bearing
                Azimuth = GetAzimuth(polyline);
            }
            else if (HasPoint1 && HasPoint2 && !HasPoint3)
            {
                var polyline = GetPolylineFromFeedback(Point1, point);
                // get minor distance from polyline
                MinorAxisDistance = GetGeodeticLengthFromPolyline(polyline);
            }

            //update feedback
            if (HasPoint1 && !HasPoint2)
            {
                feedback.MoveTo(point);
            }
            else if (HasPoint1 && HasPoint2 && !HasPoint3)
            {
                feedback.MoveTo(point);
            }
        }

        internal override void ResetPoints()
        {
            HasPoint1 = HasPoint2 = HasPoint3 = false;
        }

        internal override void OnNewMapPointEvent(object obj)
        {
            if (!IsActiveTab)
                return;

            var mxdoc = ArcMap.Application.Document as IMxDocument;
            var av = mxdoc.FocusMap as IActiveView;
            var point = obj as IPoint;

            if (point == null)
                return;

            if (!HasPoint1)
            {
                Point1 = point;
                HasPoint1 = true;
                Point1Formatted = string.Empty;
                if (feedback == null)
                {
                    CreateFeedback(point, av);
                }
                feedback.Start(point);
                AddGraphicToMap(mxdoc.FocusMap, point as IGeometry, CreateRGBColor(ElementColor.Green, 255), CreateRGBColor(ElementColor.Green, 255));
            }
            else if (!HasPoint2)
            {
                Point2 = point;
                HasPoint2 = true;
                if (feedback != null)
                {
                    feedback.Stop();
                    feedback.Start(Point1);
                }
                var line = CreateGeodeticLine(Point1, Point2);
                Distance = GetDistance(line);
                AddGraphicToMap(mxdoc.FocusMap, point as IGeometry, CreateRGBColor(ElementColor.Red, 255), CreateRGBColor(ElementColor.Red, 255));
                AddGraphicToMap(mxdoc.FocusMap, line as IGeometry, CreateRGBColor(ElementColor.Red, 255), CreateRGBColor(ElementColor.Red, 255));
            }
            else if (!HasPoint3)
            {
                ResetFeedback();
                Point3 = point;
                HasPoint3 = true;

                var line = CreateGeodeticLine(Point1, Point3);
                Distance2 = GetDistance(line);
                AddGraphicToMap(mxdoc.FocusMap, point as IGeometry, CreateRGBColor(ElementColor.Red, 255), CreateRGBColor(ElementColor.Red, 255));
                AddGraphicToMap(mxdoc.FocusMap, line as IGeometry, CreateRGBColor(ElementColor.Red, 255), CreateRGBColor(ElementColor.Red, 255));
            }

            if (HasPoint1 && HasPoint2 && HasPoint3)
            {
                CreateMapElement();
                ResetPoints();
            }
        }

        private void UpdateAzimuthFromTo(AzimuthTypes fromType, AzimuthTypes toType)
        {
            try
            {
                double angle = Azimuth;

                if (fromType == AzimuthTypes.Degrees && toType == AzimuthTypes.Mils)
                    angle *= 17.777777778;
                else if (fromType == AzimuthTypes.Mils && toType == AzimuthTypes.Degrees)
                    angle *= 0.05625;

                Azimuth = angle;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private double GetAzimuthAsDegrees()
        {
            if (AzimuthType == AzimuthTypes.Mils)
            {
                return Azimuth * 0.05625;
            }

            return Azimuth;
        }

        private IPolyline CreatePolyline(IPoint fromPoint, IPoint toPoint)
        {
            var newPolyline = new Polyline() as IPolyline;
            newPolyline.SpatialReference = fromPoint.SpatialReference;
            newPolyline.FromPoint = fromPoint;
            newPolyline.ToPoint = toPoint;
            return newPolyline;
        }

        private double GetDistance(IGeometry geometry)
        {
            var polyline = geometry as IPolyline;

            if (polyline == null)
                return 0.0;

            var polycurvegeo = geometry as IPolycurveGeodetic;

            var geodeticType = GetEsriGeodeticType();
            var linearUnit = GetLinearUnit();
            return polycurvegeo.get_LengthGeodetic(geodeticType, linearUnit);
        }

        public void AddGraphicToMap(IMap map, IGeometry geometry, IRgbColor rgbColor, IRgbColor outlineRgbColor)
        {
            var graphicsContainer = map as IGraphicsContainer;
            IElement element = null;
            if (geometry.GeometryType == esriGeometryType.esriGeometryPoint)
            {
                // Marker symbols
                var simpleMarkerSymbol = new SimpleMarkerSymbol() as ISimpleMarkerSymbol;
                simpleMarkerSymbol.Color = rgbColor;
                simpleMarkerSymbol.Outline = true;
                simpleMarkerSymbol.OutlineColor = outlineRgbColor;
                simpleMarkerSymbol.Size = 5;
                simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;

                var markerElement = new MarkerElement() as IMarkerElement;
                markerElement.Symbol = simpleMarkerSymbol;
                element = markerElement as IElement;
            }
            else if (geometry.GeometryType == esriGeometryType.esriGeometryPolyline)
            {
                //  Line elements
                var simpleLineSymbol = new SimpleLineSymbol() as ISimpleLineSymbol;
                simpleLineSymbol.Color = rgbColor;
                simpleLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                simpleLineSymbol.Width = 1;

                var lineElement = new LineElement() as ILineElement;
                lineElement.Symbol = simpleLineSymbol;
                element = lineElement as IElement;
            }
            else if (geometry.GeometryType == esriGeometryType.esriGeometryPolygon)
            {
                // Polygon elements
                var simpleFillSymbol = new SimpleFillSymbol() as ISimpleFillSymbol;
                simpleFillSymbol.Color = rgbColor;
                simpleFillSymbol.Style = esriSimpleFillStyle.esriSFSForwardDiagonal;
                var fillShapeElement = new PolygonElement() as IFillShapeElement;
                fillShapeElement.Symbol = simpleFillSymbol;
                element = fillShapeElement as IElement;
            }
            if (element != null)
            {
                element.Geometry = geometry;
                var eleProps = element as IElementProperties;
                eleProps.Name = ElementTag;
                graphicsContainer.AddElement(element, 0);
                ((IActiveView)map).Refresh();
            }
        }

        private void RemoveGraphics(IGraphicsContainer gc, string tagName, esriGeometryType geomType)
        {
            var elementList = new List<IElement>();
            gc.Reset();
            var element = gc.Next();
            while (element != null)
            {
                if (element.Geometry.GeometryType == geomType)
                {
                    var eleProps = element as IElementProperties;
                    if (eleProps.Name == tagName)
                    {
                        elementList.Add(element);
                    }
                }
                element = gc.Next();
            }
            foreach (var ele in elementList)
            {
                gc.DeleteElement(ele);
            }
            ((IMxDocument)ArcMap.Application.Document).ActiveView.Refresh();
        }

        private double GetAzimuth(IGeometry geometry)
        {
            var curve = geometry as ICurve;

            if (curve == null)
                return 0.0;

            var line = new Line() as ILine;

            curve.QueryTangent(esriSegmentExtension.esriNoExtension, 0.5, true, 10, line);

            if (line == null)
                return 0.0;

            return GetAngleDegrees(line.Angle);
        }

        private double GetAngleDegrees(double angle)
        {
            double bearing = (180.0 * angle) / Math.PI;
            if (bearing < 90.0)
                bearing = 90 - bearing;
            else
                bearing = 360.0 - (bearing - 90);

            if (AzimuthType == AzimuthTypes.Degrees)
            {
                return bearing;
            }

            if (AzimuthType == AzimuthTypes.Mils)
            {
                return bearing * 17.777777778;
            }

            return 0.0;
        }

        private IPolyline CreateGeodeticLine(IPoint fromPoint, IPoint toPoint)
        {
            var construct = new Polyline() as IConstructGeodetic;
            if (construct == null)
            {
                return null;
            }
            construct.ConstructGeodeticLineFromPoints(GetEsriGeodeticType(), fromPoint, toPoint, GetLinearUnit(), esriCurveDensifyMethod.esriCurveDensifyByDeviation, -1.0);

            return construct as IPolyline;
        }

        private IRgbColor CreateRGBColor(ElementColor color, int colorCode)
        {
            IRgbColor rgbColor = new RgbColor() as IRgbColor;
            switch (color)
            {
                case ElementColor.Blue:
                    rgbColor.Blue = colorCode;
                    break;
                case ElementColor.Red:
                    rgbColor.Red = colorCode;
                    break;
                case ElementColor.Green:
                    rgbColor.Green = colorCode;
                    break;
                default:
                    rgbColor.Blue = rgbColor.Green = rgbColor.Red = colorCode;
                    break;
            }
            return rgbColor;
        }


        private void DrawEllipse()
        {
            try
            {
                RemoveGraphics(((IMxDocument)ArcMap.Application.Document).ActivatedView.GraphicsContainer, 
                    ElementTag, esriGeometryType.esriGeometryPolyline);
                RemoveGraphics(((IMxDocument)ArcMap.Application.Document).ActivatedView.GraphicsContainer, 
                    ElementTag, esriGeometryType.esriGeometryPoint);

                var majorPolyline = new Polyline() as IPolyline;
                majorPolyline.SpatialReference = Point1.SpatialReference;

                var minorPolyline = new Polyline() as IPolyline;
                minorPolyline.SpatialReference = Point1.SpatialReference;

                double majorAxis, minorAxis;
                if (Distance > Distance2)
                {
                    majorAxis = Distance; minorAxis = Distance2;

                    majorPolyline.FromPoint = Point1;
                    majorPolyline.ToPoint = Point2;

                    minorPolyline.FromPoint = Point1;
                    minorPolyline.ToPoint = Point3;
                }
                else
                {
                    majorAxis = Distance2; minorAxis = Distance;

                    majorPolyline.FromPoint = Point1;
                    majorPolyline.ToPoint = Point3;

                    minorPolyline.FromPoint = Point1;
                    minorPolyline.ToPoint = Point2;
                }

                if (EllipseType == EllipseTypes.Semi)
                {
                    CenterPoint = Point1;
                }
                else
                {
                    CenterPoint = new Point() as IPoint;
                    minorPolyline.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, CenterPoint);

                    Distance = GetDistance(CreateGeodeticLine(CenterPoint, Point2) as IGeometry);
                    Distance2 = GetDistance(CreateGeodeticLine(CenterPoint, Point3) as IGeometry);

                    if (Distance > Distance2)
                    {
                        majorAxis = Distance; minorAxis = Distance2;

                        majorPolyline.FromPoint = CenterPoint;
                        majorPolyline.ToPoint = Point2;

                        minorPolyline.FromPoint = CenterPoint;
                        minorPolyline.ToPoint = Point3;
                    }
                    else
                    {
                        majorAxis = Distance2; minorAxis = Distance;

                        majorPolyline.FromPoint = CenterPoint;
                        majorPolyline.ToPoint = Point3;

                        minorPolyline.FromPoint = CenterPoint;
                        minorPolyline.ToPoint = Point2;
                    }

                }

                MajorAxisDistance = majorAxis;
                MinorAxisDistance = minorAxis;

                Azimuth = GetAzimuth(majorPolyline as IGeometry);

                var ellipticArc = new Polyline() as IConstructGeodetic;
                ellipticArc.ConstructGeodesicEllipse(CenterPoint, GetLinearUnit(), MajorAxisDistance, MinorAxisDistance, Azimuth, esriCurveDensifyMethod.esriCurveDensifyByDeviation, 0.0001);
                var line = ellipticArc as IPolyline;
                if (line != null)
                {
                    AddGraphicToMap(((IMxDocument)ArcMap.Application.Document).FocusMap, line as IGeometry, CreateRGBColor(ElementColor.Red, 255), CreateRGBColor(ElementColor.Red, 255));
                }

                ElementTag = Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        internal override void Reset(bool toolReset)
        {
            base.Reset(toolReset);
            HasPoint3 = false;
            Point3 = null;

            MajorAxisDistance = 0.0;
            MinorAxisDistance = 0.0;
            Azimuth = 0.0;
        }
    }
}