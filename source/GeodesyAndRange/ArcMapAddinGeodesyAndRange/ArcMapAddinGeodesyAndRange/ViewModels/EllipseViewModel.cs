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
using ESRI.ArcGIS.esriSystem;

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
        public ISymbol FeedbackSymbol { get; set; }

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
                if (string.Equals(minorAxisDistanceString, value))
                    return;

                minorAxisDistanceString = value;
                double d = 0.0;
                if (double.TryParse(minorAxisDistanceString, out d))
                {
                    MinorAxisDistance = d;
                    RaisePropertyChanged(() => MinorAxisDistance);

                    // update feedback
                    //Point3 = UpdateFeedback(Point1, minorAxisDistance);
                }
                else
                {
                    throw new ArgumentException(Properties.Resources.AEInvalidInput);
                }
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

                Point2 = UpdateFeedback(Point1, MajorAxisDistance);

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
                if (EllipseType == EllipseTypes.Full)
                {
                    return (MajorAxisDistance * 2.0).ToString("N");
                }
                return string.Format("{0:0.00}", MajorAxisDistance);
            }
            set
            {
                if (string.Equals(majorAxisDistanceString, value))
                    return;

                majorAxisDistanceString = value;
                double d = 0.0;
                if (double.TryParse(majorAxisDistanceString, out d))
                {                            
                    MajorAxisDistance = d;
                }
                else
                {
                    throw new ArgumentException(Properties.Resources.AEInvalidInput);
                }
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

                // update feedback
                Point2 = UpdateFeedback(Point1, MajorAxisDistance);

                AzimuthString = azimuth.ToString("N");
                RaisePropertyChanged(() => AzimuthString);
            }
        }
        string azimuthString = string.Empty;
        public string AzimuthString
        {
            get { return azimuthString; }
            set
            {
                // lets avoid an infinite loop here
                if (string.Equals(azimuthString, value))
                    return;

                azimuthString = value;
                // update azimuth
                double d = 0.0;
                if (double.TryParse(azimuthString, out d))
                {
                    Azimuth = d;
                }
                else
                {
                    throw new ArgumentException(Properties.Resources.AEInvalidInput);
                }
            }
        }
        #endregion

        #region Commands
        // when someone hits the enter key, create geodetic graphic
        internal override void OnEnterKeyCommand(object obj)
        {
            if (MajorAxisDistance == 0.0 || Point1 == null || 
                MinorAxisDistance == 0.0 || Azimuth == 0.0)
            {
                return;
            }
            if (Point3 == null)
            {
                Point3 = UpdateFeedback(Point1, MinorAxisDistance);
            }
            base.OnEnterKeyCommand(obj);
        }
        #endregion

        #region Overriden Functions
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
                var polyline = CreateGeodeticLine(Point1, point);
                // get major distance from polyline
                MajorAxisDistance = GetGeodeticLengthFromPolyline(polyline);
                // update bearing
                Azimuth = GetAzimuth(polyline);
                // update feedback
                FeedbackMoveTo(point);

                //test
                ClearTempGraphics();
                var ellipticArc = new Polyline() as IConstructGeodetic;
                ellipticArc.ConstructGeodesicEllipse(Point1, GetLinearUnit(), MajorAxisDistance, MajorAxisDistance, Azimuth, esriCurveDensifyMethod.esriCurveDensifyByAngle, 0.45);
                var line = ellipticArc as IPolyline;
                if (line != null)
                {
                    AddGraphicToMap(line as IGeometry, true);
                }

            }
            else if (HasPoint1 && HasPoint2 && !HasPoint3)
            {
                var polyline = CreateGeodeticLine(Point1, point); //GetPolylineFromFeedback(Point1, point);
                // get minor distance from polyline
                if (polyline != null)
                {
                    MinorAxisDistance = GetGeodeticLengthFromPolyline(polyline);
                }
                if (FeedbackSymbol == null)
                {
                    FeedbackSymbol = feedback.Symbol;
                }                
                feedback.Symbol = new SimpleLineSymbol() as ISymbol;
                // update feedback              
                if (MajorAxisDistance > MinorAxisDistance)
                {
                    if (FeedbackSymbol != null)
                    {
                        feedback.Symbol = FeedbackSymbol;
                    }
                    FeedbackMoveTo(point);
                }
                
                //test
                ClearTempGraphics();
                var ellipticArc = new Polyline() as IConstructGeodetic;
                ellipticArc.ConstructGeodesicEllipse(Point1, GetLinearUnit(), MajorAxisDistance, MinorAxisDistance, Azimuth, esriCurveDensifyMethod.esriCurveDensifyByAngle, 0.45);
                var line = ellipticArc as IPolyline;
                if (line != null)
                {
                    AddGraphicToMap(line as IGeometry, true);
                }

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
                if (MajorAxisDistance >= MinorAxisDistance)
                {
                    ResetFeedback();
                    Point3 = point;
                    HasPoint3 = true;

                    var line = CreateGeodeticLine(Point1, Point3);
                    Distance2 = GetDistance(line);
                    if (Distance2 > Distance)
                    {
                        line = CreateGeodeticLine(Point1, Point3, Distance);
                        Distance2 = GetDistance(line);
                    }
                    AddGraphicToMap(mxdoc.FocusMap, point as IGeometry, CreateRGBColor(ElementColor.Red, 255), CreateRGBColor(ElementColor.Red, 255));
                    AddGraphicToMap(mxdoc.FocusMap, line as IGeometry, CreateRGBColor(ElementColor.Red, 255), CreateRGBColor(ElementColor.Red, 255));
                }
            }

            if (HasPoint1 && HasPoint2 && HasPoint3)
            {
                CreateMapElement();
                ResetPoints();
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
        #endregion

        #region Private Functions
        private IPoint UpdateFeedback(IPoint centerPoint, double axisTypeDistance)
        {
            if (centerPoint != null && axisTypeDistance > 0.0)
            {
                if (feedback == null)
                {
                    var mxdoc = ArcMap.Application.Document as IMxDocument;
                    CreateFeedback(centerPoint, mxdoc.FocusMap as IActiveView);
                    feedback.Start(centerPoint);
                }

                // now get second point from distance and bearing
                var construct = new Polyline() as IConstructGeodetic;
                if (construct == null)
                {
                    return null;
                }                    

                construct.ConstructGeodeticLineFromDistance(GetEsriGeodeticType(), centerPoint, GetLinearUnit(), axisTypeDistance, 
                    GetAzimuthAsDegrees(), esriCurveDensifyMethod.esriCurveDensifyByDeviation, -1.0);

                var line = construct as IPolyline;

                if (line.ToPoint != null)
                {
                    FeedbackMoveTo(line.ToPoint);
                    return line.ToPoint;
                }
            }
            return null;
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

        private void AddMinorAxisGraphic(IPoint minorPoint)
        {
            var simpleMarkerSymbol = new SimpleMarkerSymbol() as ISimpleMarkerSymbol;
            simpleMarkerSymbol.Color = CreateRGBColor(ElementColor.Red, 255);
            simpleMarkerSymbol.Outline = true;
            simpleMarkerSymbol.OutlineColor = CreateRGBColor(ElementColor.Red, 255);
            simpleMarkerSymbol.Size = 10;
            simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;

            var markerElement = new MarkerElement() as IMarkerElement;
            markerElement.Symbol = simpleMarkerSymbol;
            var element = markerElement as IElement;
            element.Geometry = minorPoint as IGeometry;
            var elementProps = element as IElementProperties;
            elementProps.Name = "ellipse_minor_axis_point";

            ((IGraphicsContainer)((IMxDocument)ArcMap.Application.Document).FocusMap).AddElement(element, 0);
            ((IActiveView)((IMxDocument)ArcMap.Application.Document).FocusMap).PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        private void RemoveMinorAxisGraphic()
        {
            var elementList = new List<IElement>();
            var gc = ((IMxDocument)ArcMap.Application.Document).FocusMap as IGraphicsContainer;
            gc.Reset();
            var element = gc.Next();
            while (element != null)
            {
                if (element.Geometry.GeometryType == esriGeometryType.esriGeometryPoint)
                {
                    var eleProps = element as IElementProperties;
                    if (eleProps.Name == "ellipse_minor_axis_point")
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
            ((IMxDocument)ArcMap.Application.Document).ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        private void AddGraphicToMap(IMap map, IGeometry geometry, IRgbColor rgbColor, IRgbColor outlineRgbColor)
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

        private IPolyline CreateGeodeticLine(IPoint fromPoint, IPoint toPoint, double distance = 0.0)
        {
            var construct = new Polyline() as IConstructGeodetic;
            if (construct == null)
            {
                return null;
            }
            if (distance == 0)
            {
                construct.ConstructGeodeticLineFromPoints(GetEsriGeodeticType(), fromPoint, toPoint, GetLinearUnit(), esriCurveDensifyMethod.esriCurveDensifyByDeviation, -1.0);
            }
            else
            {
                var minorPolyline = new Polyline() as IPolyline;
                minorPolyline.SpatialReference = Point1.SpatialReference;
                minorPolyline.FromPoint = Point1;
                minorPolyline.ToPoint = Point3;
                construct.ConstructGeodeticLineFromDistance(GetEsriGeodeticType(), fromPoint, GetLinearUnit(), distance, GetAzimuth(minorPolyline as IGeometry), 
                    esriCurveDensifyMethod.esriCurveDensifyByDeviation, -1.0);
            }

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
                
                var ellipticArc = new Polyline() as IConstructGeodetic;
                ellipticArc.ConstructGeodesicEllipse(Point1, GetLinearUnit(), MajorAxisDistance, MinorAxisDistance, Azimuth, esriCurveDensifyMethod.esriCurveDensifyByDeviation, 0.0001);
                var line = ellipticArc as IPolyline;
                if (line != null)
                {
                    AddGraphicToMap(line as IGeometry);
                }

                ElementTag = Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion
    }
}