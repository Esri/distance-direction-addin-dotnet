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

// System
using System;

// Esri
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using DistanceAndDirectionLibrary;

namespace ArcMapAddinDistanceAndDirection.ViewModels
{
    public class EllipseViewModel : TabBaseViewModel
    {
        /// <summary>
        /// CTOR
        /// </summary>
        public EllipseViewModel()
        {
        }

        #region Properties

        public IPoint CenterPoint { get; set; }
        public ISymbol FeedbackSymbol { get; set; }

        private EllipseTypes ellipseType = EllipseTypes.Semi;
        public EllipseTypes EllipseType 
        {
            get { return ellipseType; }
            set
            {
                ellipseType = value;

                RaisePropertyChanged(() => MajorAxisDistance);
                RaisePropertyChanged(() => MajorAxisDistanceString);
            }
        }

        AzimuthTypes azimuthType = AzimuthTypes.Degrees;
        public AzimuthTypes AzimuthType
        {
            get { return azimuthType; }
            set
            {
                var before = azimuthType;
                azimuthType = value;
                Azimuth = Azimuth;
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
                if (value < 0.0)
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);

                minorAxisDistance = value;

                UpdateFeedbackWithEllipse();

                RaisePropertyChanged(() => MinorAxisDistance);
                RaisePropertyChanged(() => MinorAxisDistanceString);                
            }
        }

        private string minorAxisDistanceString = string.Empty;
        public string MinorAxisDistanceString
        {
            get
            {
                if (EllipseType == EllipseTypes.Full)
                {
                    return (MinorAxisDistance * 2).ToString("G");
                }
                return MinorAxisDistance.ToString("G");
            }
            set
            {
                if (string.Equals(minorAxisDistanceString, value))
                    return;

                minorAxisDistanceString = value;
                double d = 0.0;
                if (double.TryParse(minorAxisDistanceString, out d))
                {
                    if (EllipseType == EllipseTypes.Full)
                    {
                        MinorAxisDistance = d / 2;
                    }
                    else
                        MinorAxisDistance = d;

                    if (MinorAxisDistance == d)
                        return;
                }
                else
                {
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
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
                if (value < 0.0)
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);

                majorAxisDistance = value;

                Point2 = UpdateFeedback(Point1, MajorAxisDistance);

                UpdateFeedbackWithEllipse();

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
                    return (MajorAxisDistance * 2.0).ToString("G");
                }
                return MajorAxisDistance.ToString("G");
            }
            set
            {
                if (string.Equals(majorAxisDistanceString, value))
                    return;

                majorAxisDistanceString = value;

                double d = 0.0;
                if (double.TryParse(majorAxisDistanceString, out d))
                {
                    if (EllipseType == EllipseTypes.Full)
                        MajorAxisDistance = d / 2.0;
                    else
                        MajorAxisDistance = d;
                }
                else
                {
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }
            }
        }

        double azimuth = 0.0;
        public double Azimuth
        {
            get { return azimuth; }
            set
            {
                if (value < 0.0)
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);

                azimuth = value;
                RaisePropertyChanged(() => Azimuth);

                // update feedback
                Point2 = UpdateFeedback(Point1, MajorAxisDistance);

                UpdateFeedbackWithEllipse();

                AzimuthString = azimuth.ToString("G");
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
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }
            }
        }

        #endregion

        #region Commands

        // when someone hits the enter key, create geodetic graphic
        internal override void OnEnterKeyCommand(object obj)
        {
            if (MajorAxisDistance == 0.0 || Point1 == null || 
                MinorAxisDistance == 0.0)
            {
                return;
            }
            if (Point3 == null)
            {
                Point3 = UpdateFeedback(Point1, MinorAxisDistance);
            }
            base.OnEnterKeyCommand(obj);
        }

        #endregion Commands

        #region Overriden Functions

        /// <summary>
        /// Overrides TabBaseViewModel CreateMapElement
        /// </summary>
        internal override IGeometry CreateMapElement()
        {
            IGeometry geom = null;
            if (Point1 == null || Point2 == null || Point3 == null)
            {
                return geom;
            }
            geom = DrawEllipse();
            Reset(false);

            return geom;
        }

        internal override void OnMouseMoveEvent(object obj)
        {
            if (!IsActiveTab)
                return;

            var point = obj as IPoint;

            if (point == null)
                return;

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
                UpdateFeedbackWithEllipse(false);
            }
            else if (HasPoint1 && HasPoint2 && !HasPoint3)
            {
                var polyline = CreateGeodeticLine(Point1, point); 
                
                // get minor distance from polyline
                if (polyline != null)
                {
                    MinorAxisDistance = GetGeodeticLengthFromPolyline(polyline);
                }

                // update feedback              
                if (MajorAxisDistance > MinorAxisDistance)
                {
                    UpdateFeedbackWithEllipse();
                }
            }
        }

        private void UpdateFeedbackWithEllipse(bool HasMinorAxis = true)
        {
            if (!HasPoint1)
                return;
            
            ClearTempGraphics();

            AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true);

            var ellipticArc = new Polyline() as IConstructGeodetic;

            var minorAxis = MinorAxisDistance;
            if (!HasMinorAxis || minorAxis == 0.0)
                minorAxis = MajorAxisDistance;

            if (minorAxis > MajorAxisDistance)
                minorAxis = MajorAxisDistance;

            ellipticArc.ConstructGeodesicEllipse(Point1, GetLinearUnit(), MajorAxisDistance, minorAxis, GetAzimuthAsDegrees(), esriCurveDensifyMethod.esriCurveDensifyByAngle, 0.45);
            var line = ellipticArc as IPolyline;
            if (line != null)
            {
                var color = new RgbColor() as IColor;
                AddGraphicToMap(line as IGeometry, color, true, rasterOpCode: esriRasterOpCode.esriROPNotXOrPen);
            }
        }

        internal override void UpdateFeedback()
        {
            UpdateFeedbackWithEllipse();
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
                AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true);

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
            }
            else if (!HasPoint3)
            {
                if (MajorAxisDistance >= MinorAxisDistance)
                {
                    ResetFeedback();
                    Point3 = point;
                    HasPoint3 = true;
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
                    //FeedbackMoveTo(line.ToPoint);
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
            try
            {
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
            }
            catch { }

            return construct as IPolyline;
        }

        /// <summary>
        /// Create a geodetic ellipse
        /// </summary>
        private IGeometry DrawEllipse()
        {
            try
            {
                var ellipticArc = new Polyline() as IConstructGeodetic;
                double bearing;
                if (AzimuthType==AzimuthTypes.Mils)
                {
                    bearing = GetAzimuthAsDegrees();
                }
                else
                {
                    bearing = Azimuth;
                }
                ellipticArc.ConstructGeodesicEllipse(Point1, GetLinearUnit(), MajorAxisDistance, MinorAxisDistance, bearing, esriCurveDensifyMethod.esriCurveDensifyByAngle, 0.01);
                var line = ellipticArc as IPolyline;
                if (line != null)
                {
                    AddGraphicToMap(line as IGeometry);
                    //Convert ellipse polyline to polygon
                    var newPoly = PolylineToPolygon((IPolyline)ellipticArc);
                    if (newPoly != null)
                    {
                        //Get centroid of polygon
                        var area = newPoly as IArea;
                        //Add text using centroid point                        
                        DistanceTypes dtVal = (DistanceTypes)LineDistanceType; //Get line distance type                                                    
                        AzimuthTypes atVal = (AzimuthTypes)AzimuthType; //Get azimuth type
                        if (area != null)
                        {
                            AddTextToMap(area.Centroid, string.Format("{0}:{1} {2}{3}{4}:{5} {6}{7}{8}:{9} {10}",
                                "Major Axis",
                                Math.Round(majorAxisDistance,2),
                                dtVal.ToString(),
                                Environment.NewLine,
                                "Minor Axis",
                                Math.Round(minorAxisDistance,2),
                                dtVal.ToString(),
                                Environment.NewLine,
                                "Orientation Angle",
                                Math.Round(azimuth,2),
                                atVal.ToString()));
                        }
                    }
                }
                return line as IGeometry;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        #endregion
    }
}