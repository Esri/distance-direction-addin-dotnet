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
using System.Collections.Generic;
using System.Threading;

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
                Reset(false);
                RaisePropertyChanged(() => DistanceBearingReady);
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
                RaisePropertyChanged(() => DistanceBearingReady);
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

                distance = TrimPrecision(value, false);
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
                if ((value != null) && (value >= 0.0))
                    azimuth = value;
                else
                    azimuth = null;

                RaisePropertyChanged(() => Azimuth);

                if (LineFromType == LineFromTypes.BearingAndDistance)
                {
                    UpdateFeedback();
                }

                if ((value == null) || (value < 0.0))
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);
                if (LineAzimuthType == AzimuthTypes.Degrees)
                {
                    if (value > 360 && LineAzimuthType == AzimuthTypes.Degrees)
                        throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }
                else
                {
                    if (value > 6400 && LineAzimuthType == AzimuthTypes.Mils)
                        throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
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
            string outFormattedString = string.Empty;
            CoordinateConversionLibrary.Models.CoordinateType ccType = CoordinateConversionLibrary.Models.CoordinateType.Unknown;
            if (LineFromType == LineFromTypes.Points)
            {
                ccType = CoordinateConversionLibrary.Helpers.ConversionUtils.GetCoordinateString(Point1Formatted, out outFormattedString);
                Point1 = (ccType != CoordinateConversionLibrary.Models.CoordinateType.Unknown) ? GetPointFromString(outFormattedString) : null;

                ccType = CoordinateConversionLibrary.Helpers.ConversionUtils.GetCoordinateString(Point2Formatted, out outFormattedString);
                Point2 = (ccType != CoordinateConversionLibrary.Models.CoordinateType.Unknown) ? GetPointFromString(outFormattedString) : null;
                if (!Azimuth.HasValue || Point1 == null || Point2 == null)
                    return;
            }
            else
            {
                ccType = CoordinateConversionLibrary.Helpers.ConversionUtils.GetCoordinateString(Point1Formatted, out outFormattedString);
                Point1 = (ccType != CoordinateConversionLibrary.Models.CoordinateType.Unknown) ? GetPointFromString(outFormattedString) : null;
                if (!Azimuth.HasValue || Point1 == null)
                    return;
            }
            HasPoint1 = true;
            HasPoint2 = true;
            IGeometry geo = CreatePolyline();
            IPolyline line = geo as IPolyline;
            if (line == null)
                return;

            IDictionary<String, Double> lineAttributes = new Dictionary<String, Double>();
            lineAttributes.Add("distance", Distance);
            lineAttributes.Add("angle", (double)Azimuth);
            AddGraphicToMap(line, attributes:lineAttributes);
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
                if ((Point1 == null) || (Point2 == null))
                    return null;

                var construct = (IConstructGeodetic)new Polyline();

                if (srf3 == null)
                {
                    // if you don't use the activator, you will get exceptions
                    Type srType = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
                    srf3 = Activator.CreateInstance(srType) as ISpatialReferenceFactory3;
                }

                if (srf3 == null)
                    return null;

                esriGeodeticType type = GetEsriGeodeticType();
                if (LineFromType == LineFromTypes.Points)
                    construct.ConstructGeodeticLineFromPoints(GetEsriGeodeticType(), Point1, Point2, GetLinearUnit(), esriCurveDensifyMethod.esriCurveDensifyByDeviation, -1.0);
                else
                {
                    Double bearing = 0.0;
                    if(LineAzimuthType == AzimuthTypes.Mils)
                    {
                        bearing = GetAzimuthAsDegrees();
                    }
                    else
                    {
                        bearing = (double)Azimuth;
                    }
                    construct.ConstructGeodeticLineFromDistance(type, Point1, GetLinearUnit(), Distance, bearing, esriCurveDensifyMethod.esriCurveDensifyByDeviation, -1.0);
                }

                if (LineFromType == LineFromTypes.Points)
                {
                    UpdateDistance(construct as IGeometry);
                    UpdateAzimuth(construct as IGeometry);
                }

                IDictionary<String, System.Object> lineAttributes = new Dictionary<String, System.Object>();
                lineAttributes.Add("distance", Distance);
                lineAttributes.Add("distanceunit", LineDistanceType.ToString());
                lineAttributes.Add("angle", (double)Azimuth);
                lineAttributes.Add("angleunit", LineAzimuthType.ToString());
                lineAttributes.Add("startx", Point1.X);
                lineAttributes.Add("starty", Point1.Y);
                lineAttributes.Add("endx", Point2.X);
                lineAttributes.Add("endy", Point2.Y);
                var color = new RgbColorClass() { Red = 255 } as IColor;
                AddGraphicToMap(construct as IGeometry, color, attributes: lineAttributes);

                if (HasPoint1 && HasPoint2)
                {
                    //Get line distance type
                    DistanceTypes dtVal = (DistanceTypes)LineDistanceType;
                    //Get azimuth type
                    AzimuthTypes atVal = (AzimuthTypes)LineAzimuthType;
                    //Check if line crosses the international dateline
                    IPoint labelPoint = null;
                    if ((DoesLineCrossIntDateline((IPolyline)((IGeometry)construct))))
                    {
                        //Use the starting point for labeling
                        labelPoint = Point1;
                    }
                    else
                    {
                        //Get mid point of geodetic line for labeling 
                        var midPoint = new Point() as IPoint;
                        ((IPolyline)((IGeometry)construct)).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, false, midPoint);
                        labelPoint = midPoint;
                    }
                    //Create text symbol using text and midPoint
                    AddTextToMap(Point1 /*labelPoint*/, 
                        string.Format("{0}:{1} {2}{3}{4}:{5} {6}", 
                        "Distance", 
                        Math.Round(Distance,2).ToString("N2"),
                        dtVal.ToString(), 
                        Environment.NewLine,
                        "Angle",
                        Math.Round(azimuth.Value,2),
                        atVal.ToString()), (double)Azimuth, LineAzimuthType);
                }

                ResetPoints();

                return construct as IGeometry;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
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
                return Math.Round(bearing, 2);
            }

            if (LineAzimuthType == AzimuthTypes.Mils)
            {
                return Math.Round(bearing * 17.777777778, 2);
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
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Creates the international dateline as a polyline geometry
        /// 
        /// Source: https://github.com/Esri/developer-support/blob/master/arcobjects-net/international-dateline-draw-line-across/CreateLine.cs
        /// </summary>
        /// <returns>Polyline geometry</returns>
        private IPolyline GetIntDateline()
        {
            try
            {
                //Create WGS84
                Type factoryType = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
                var obj = Activator.CreateInstance(factoryType);
                var srf = obj as ISpatialReferenceFactory3;
                var WGS84 = srf.CreateSpatialReference(esriSRGeoCSType.esriSRGeoCS_WGS1984.GetHashCode());

                var pointCollection = new PolylineClass();

                // ------------- Ensure that both points have negative longitude values -------------------
                var point = new PointClass();
                point.PutCoords(180, 90); // Equivalent to 170 degrees WEST
                point.SpatialReference = WGS84;
                pointCollection.AddPoint(point);


                point = new PointClass();
                point.PutCoords(180, -90); // Equivalent to 160 degrees EAST
                point.SpatialReference = WGS84;
                pointCollection.AddPoint(point);
                // -----------------------------------------------------------------------

                var polyline = (IPolyline)pointCollection;
                polyline.SpatialReference = WGS84;

                polyline.Project(ArcMap.Document.FocusMap.SpatialReference);

                return polyline;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }            
            return null;
        }

        /// <summary>
        /// Checks if a polyline geometry crosses the international dateline
        /// </summary>
        /// <param name="inputLine"></param>
        /// <returns>bool</returns>
        private bool DoesLineCrossIntDateline(IPolyline inputLine)
        {                        
            return (inputLine != null) ? ((IRelationalOperator)inputLine).Crosses(GetIntDateline()) : false;
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
                var color = new RgbColorClass() { Green = 255 } as IColor;
                System.Collections.Generic.IDictionary<String, System.Object> ptAttributes = new System.Collections.Generic.Dictionary<String, System.Object>();
                ptAttributes.Add("X", point.X);
                ptAttributes.Add("Y", point.Y);
                this.AddGraphicToMap(point, color, true, esriSimpleMarkerStyle.esriSMSCircle, esriRasterOpCode.esriROPNOP, ptAttributes );
                HasPoint1 = true;
                
                Point1 = point;
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
            if ((ArcMap.Application == null) || (ArcMap.Document == null))
                return;

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
                if ((Point1 != null) && HasPoint1 && (Distance > 0.0))
                {
                    if (feedback == null)
                    {
                        if (ArcMap.Document == null)
                            return;

                        CreateFeedback(Point1, ArcMap.Document.FocusMap as IActiveView);
                        feedback.Start(Point1);
                    }

                    // now get second point from distance and bearing
                    var construct = (IConstructGeodetic)new Polyline();
                    if (construct == null)
                        return;

                    construct.ConstructGeodeticLineFromDistance(GetEsriGeodeticType(), Point1, GetLinearUnit(), Distance, GetAzimuthAsDegrees(), esriCurveDensifyMethod.esriCurveDensifyByDeviation, -1.0);

                    var line = construct as IPolyline;

                    if ((line != null) && (line.ToPoint != null))
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

        
        public bool DistanceBearingReady
        {
            
            get
            {
                if (LineFromType == LineFromTypes.BearingAndDistance && Point1 != null)
                    return true;
                else 
                    return false;
            }
        }

    }
}
