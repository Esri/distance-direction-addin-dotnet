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
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using DistanceAndDirectionLibrary;

namespace ArcMapAddinDistanceAndDirection.ViewModels
{
    public class LinesViewModel : TabBaseViewModel
    {
        public LinesViewModel()
        {
            // props
            LineFromType = LineFromTypes.Points;
            LineAzimuthType = AzimuthTypes.Degrees;
        }

        #region Properties

        LineFromTypes lineFromType = LineFromTypes.Points;
        public LineFromTypes LineFromType 
        {
            get { return lineFromType; }
            set
            {
                lineFromType = value;
                RaisePropertyChanged(() => LineFromType);
                
                // reset points
                ResetPoints();

                // stop feedback when from type changes
                ResetFeedback();
            }
        }

        AzimuthTypes lineAzimuthType = AzimuthTypes.Degrees;
        public AzimuthTypes LineAzimuthType 
        {
            get { return lineAzimuthType; }
            set
            {
                var before = lineAzimuthType;
                lineAzimuthType = value;
                UpdateAzimuthFromTo(before, value);
            }
        }

        public override IPoint Point1
        {
            get
            {
                return base.Point1;
            }
            set
            {
                base.Point1 = value;

                if (LineFromType == LineFromTypes.BearingAndDistance)
                {
                    ResetFeedback();
                }

                UpdateFeedback();
            }
        }

        public override IPoint Point2
        {
            get
            {
                return base.Point2;
            }
            set
            {
                base.Point2 = value;

                if (LineFromType == LineFromTypes.Points)
                {
                    UpdateFeedback();
                }
            }
        }

        double distance = 0.0;
        public override double Distance
        {
            get { return distance; }
            set
            {
                if (value < 0.0)
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);

                distance = value;
                RaisePropertyChanged(() => Distance);

                if(LineFromType == LineFromTypes.BearingAndDistance)
                {
                    // update feedback
                    UpdateFeedback();
                }

                DistanceString = distance.ToString("G");
                RaisePropertyChanged(() => DistanceString);
            }
        }

        double? azimuth = 0.0;
        public double? Azimuth
        {
            get { return azimuth; }
            set
            {
                if (value < 0.0)
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);

                azimuth = value;
                RaisePropertyChanged(() => Azimuth);

                if (!azimuth.HasValue)
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);

                if (LineFromType == LineFromTypes.BearingAndDistance)
                {
                    // update feedback
                    UpdateFeedback();
                }

                AzimuthString = azimuth.Value.ToString("G");
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
                if(LineFromType == LineFromTypes.BearingAndDistance)
                {
                    // update azimuth
                    double d = 0.0;
                    if (double.TryParse(azimuthString, out d))
                    {
                        Azimuth = d;
                    }
                    else
                    {
                        Azimuth = null;
                        throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                    }
                }
            }
        }

        /// <summary>
        /// On top of the base class we need to make sure we have an azimuth
        /// </summary>
        public override bool CanCreateElement
        {
            get
            {
                return (Azimuth.HasValue && base.CanCreateElement);
            }
        }

        #endregion

        #region Commands

        // when someone hits the enter key, create geodetic graphic
        internal override void OnEnterKeyCommand(object obj)
        {

            if (LineFromType == LineFromTypes.Points)
            {
                Point1 = GetPointFromString(Point1Formatted);
                Point2 = GetPointFromString(Point2Formatted);
                if (!Azimuth.HasValue || Point1 == null || Point2 == null)
                    return;
            }
            else
            {
                Point1 = GetPointFromString(Point1Formatted);
                if (!Azimuth.HasValue || Point1 == null)
                    return;
            }
            HasPoint1 = true;
            HasPoint2 = true;
            IGeometry geo = CreatePolyline();
            IPolyline line = geo as IPolyline;
            AddGraphicToMap(line);
            ResetPoints();
            ClearTempGraphics();
            base.OnEnterKeyCommand(obj);
        }

        /// <summary>
        /// Create a geodetic line
        /// </summary>
        private IGeometry CreatePolyline()
        {
            try
            {
                if (Point1 == null || Point2 == null)
                    return null;

                var construct = new Polyline() as IConstructGeodetic;

                if (construct == null)
                    return null;

                if (srf3 == null)
                {
                    // if you don't use the activator, you will get exceptions
                    Type srType = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
                    srf3 = Activator.CreateInstance(srType) as ISpatialReferenceFactory3;
                }

                var linearUnit = srf3.CreateUnit((int)esriSRUnitType.esriSRUnit_Meter) as ILinearUnit;
                esriGeodeticType type = GetEsriGeodeticType();
                IGeometry geo = Point1;
                if(LineFromType == LineFromTypes.Points)
                    construct.ConstructGeodeticLineFromPoints(GetEsriGeodeticType(), Point1, Point2, GetLinearUnit(), esriCurveDensifyMethod.esriCurveDensifyByDeviation, -1.0);
                else
                    construct.ConstructGeodeticLineFromDistance(type, Point1, GetLinearUnit(), Distance, (double)Azimuth, esriCurveDensifyMethod.esriCurveDensifyByDeviation,-1.0);
                var mxdoc = ArcMap.Application.Document as IMxDocument;
                var av = mxdoc.FocusMap as IActiveView;
                if (LineFromType == LineFromTypes.Points)
                {
                    UpdateDistance(construct as IGeometry);
                    UpdateAzimuth(construct as IGeometry);
                }
                

                //var color = new RgbColorClass() { Red = 255 } as IColor;
                AddGraphicToMap(construct as IGeometry);

                if (HasPoint1 && HasPoint2)
                {
                    //Get line distance type
                    DistanceTypes dtVal = (DistanceTypes)LineDistanceType;
                    //Get azimuth type
                    AzimuthTypes atVal = (AzimuthTypes)LineAzimuthType;
                    //Get mid point of geodetic line
                    var midPoint = new Point() as IPoint;
                    ((IPolyline)((IGeometry)construct)).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, false, midPoint);
                    //Create text symbol using text and midPoint
                    AddTextToMap(midPoint != null ? midPoint : Point2, 
                        string.Format("{0}:{1} {2}{3}{4}:{5} {6}", 
                        "Distance", 
                        Math.Round(Distance,2).ToString("N2"),
                        dtVal.ToString(), 
                        Environment.NewLine,
                        "Angle",
                        Math.Round(azimuth.Value,2),
                        atVal.ToString()));
                }

                ResetPoints();

                return construct as IGeometry;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private void UpdateAzimuth(IGeometry geometry)
        {
            var curve = geometry as ICurve;

            if (curve == null)
                return;

            var line = new Line() as ILine;
            
            curve.QueryTangent(esriSegmentExtension.esriNoExtension, 0.5, true, 10, line);
            
            if(line == null)
                return;

            Azimuth = GetAngleDegrees(line.Angle);
        }

        private double GetAngleDegrees(double angle)
        {
            double bearing = (180.0 * angle) / Math.PI;
            if (bearing < 90.0)
                bearing = 90 - bearing;
            else
                bearing= 360.0 - (bearing - 90);

            if (LineAzimuthType == AzimuthTypes.Degrees)
            {
                return bearing;
            }

            if (LineAzimuthType == AzimuthTypes.Mils)
            {
                return bearing * 17.777777778;
            }

            return 0.0;
        }

        private void UpdateAzimuthFromTo(AzimuthTypes fromType, AzimuthTypes toType)
        {
            try
            {
                double angle = Azimuth.GetValueOrDefault();

                if (fromType == AzimuthTypes.Degrees && toType == AzimuthTypes.Mils)
                    angle *= 17.777777778;
                else if (fromType == AzimuthTypes.Mils && toType == AzimuthTypes.Degrees)
                    angle *= 0.05625;

                Azimuth = angle;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #endregion

        #region Mediator methods

        internal override void OnNewMapPointEvent(object obj)
        {
            if (!IsActiveTab)
                return;

            var point = obj as IPoint;

            if (point == null)
                return;

            // Bearing and Distance Mode
            if(LineFromType == LineFromTypes.BearingAndDistance)
            {
                ClearTempGraphics();
                Point1 = point;
                HasPoint1 = true;
                var color = new RgbColorClass() { Green = 255 } as IColor;
                AddGraphicToMap(Point1, color, true);
                return;
            }

            base.OnNewMapPointEvent(obj);
        }

        /// <summary>
        /// Overrides TabBaseViewModel CreateMapElement
        /// </summary>
        internal override IGeometry CreateMapElement()
        {
            IGeometry geom = null;
            if (!CanCreateElement)
                return geom;

            base.CreateMapElement();
            geom = CreatePolyline();

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

            if (LineFromType == LineFromTypes.BearingAndDistance)
                return;

            if (HasPoint1 && !HasPoint2)
            {
                // update azimuth from feedback
                var polyline = GetGeoPolylineFromPoints(Point1, point);
                UpdateAzimuth(polyline);
            }

            base.OnMouseMoveEvent(obj);
        }

        #endregion

        internal override void UpdateFeedback()
        {
            // for now lets stick with only updating feedback here with Bearing and Distance case
            if (LineFromType != LineFromTypes.BearingAndDistance)
            {
                if(Point1 != null && Point2 != null && HasPoint1)
                {
                    var polyline = GetGeoPolylineFromPoints(Point1, Point2);
                    UpdateAzimuth(polyline);
                }
            }
            else
            {
                if (Point1 != null && HasPoint1)
                {
                    if (feedback == null)
                    {
                        var mxdoc = ArcMap.Application.Document as IMxDocument;
                        CreateFeedback(Point1, mxdoc.FocusMap as IActiveView);
                        feedback.Start(Point1);
                    }

                    // now get second point from distance and bearing
                    var construct = new Polyline() as IConstructGeodetic;
                    if (construct == null)
                        return;

                    construct.ConstructGeodeticLineFromDistance(GetEsriGeodeticType(), Point1, GetLinearUnit(), Distance, GetAzimuthAsDegrees(), esriCurveDensifyMethod.esriCurveDensifyByDeviation, -1.0);

                    var line = construct as IPolyline;

                    if (line.ToPoint != null)
                    {
                        FeedbackMoveTo(line.ToPoint);
                        Point2 = line.ToPoint;
                    }
                }
            }
        }

        private double GetAzimuthAsDegrees()
        {
            if(LineAzimuthType == AzimuthTypes.Mils)
            {
                return Azimuth.GetValueOrDefault() * 0.05625;
            }

            return Azimuth.GetValueOrDefault();
        }

        internal override void Reset(bool toolReset)
        {
            base.Reset(toolReset);

            Azimuth = 0.0;
        }

    }
}
