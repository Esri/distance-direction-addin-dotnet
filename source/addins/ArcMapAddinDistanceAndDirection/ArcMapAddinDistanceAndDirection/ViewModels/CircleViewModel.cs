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
using ESRI.ArcGIS.Display;
using DistanceAndDirectionLibrary;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using System.Collections.Generic;

namespace ArcMapAddinDistanceAndDirection.ViewModels
{
    public class CircleViewModel : TabBaseViewModel
    {
        /// <summary>
        /// CTOR
        /// </summary>
        public CircleViewModel()
        {
            //properties
            CircleType = CircleFromTypes.Radius;
        }

        #region Properties

        private double DistanceLimit = 20000000;

        CircleFromTypes circleType = CircleFromTypes.Radius;
        /// <summary>
        /// Type of circle property
        /// </summary>
        public CircleFromTypes CircleType
        {
            get { return circleType; }
            set
            {
                if (circleType == value)
                    return;

                circleType = value;

                if (!isDistanceCalcExpanded)
                {
                    // We have to explicitly redo the graphics here because otherwise DistanceString has not changed
                    // and thus no graphics update will be triggered
                    double distanceInMeters = ConvertFromTo(LineDistanceType, DistanceTypes.Meters, Distance);
                    
                    if (distanceInMeters > DistanceLimit)
                    {
                        ClearTempGraphics();
                        if (HasPoint1)
                        {
                            IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                            ptAttributes.Add("X", Point1.X);
                            ptAttributes.Add("Y", Point1.Y);
                            // Re-add the point as it was cleared by ClearTempGraphics() but we still want to see it
                            AddGraphicToMap( Point1, new RgbColor() { Green = 255 } as IColor, true, attributes: ptAttributes);
                        }
                        throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                    }
                    // Avoid null reference exception during automated testing
                    if (ArcMap.Application != null)
                    {
                        UpdateFeedbackWithGeoCircle();
                    }
                }
                else
                {
                    // We just want to update the value in the Radius / Diameter field
                    if (circleType == CircleFromTypes.Radius)
                    {
                        Distance = Distance / 2;
                    }
                    else
                    {
                        Distance = Distance * 2;
                    }
                }

                // reset distance
                RaisePropertyChanged(() => Distance);
                RaisePropertyChanged(() => DistanceString);
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

                

                UpdateFeedbackWithGeoCircle();
            }
        }
        TimeUnits timeUnit = TimeUnits.Minutes;
        /// <summary>
        /// Type of time units
        /// </summary>
        public TimeUnits TimeUnit
        {
            get
            {
                return timeUnit;
            }
            set
            {
                if (timeUnit == value)
                {
                    return;
                }
                timeUnit = value;

                // Prevent graphical glitches from excessively high inputs
                double distanceInMeters = ConvertFromTo(RateUnit, DistanceTypes.Meters, TravelRateInSeconds * TravelTimeInSeconds);
                
                if (distanceInMeters > DistanceLimit)
                {
                    RaisePropertyChanged(() => TravelTimeString);
                    UpdateDistance(TravelRateInSeconds * TravelTimeInSeconds, RateUnit, false);
                    ClearTempGraphics();
                    if (HasPoint1)
                    {
                        IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                        ptAttributes.Add("X", Point1.X);
                        ptAttributes.Add("Y", Point1.Y);
                        // Re-add the point as it was cleared by ClearTempGraphics() but we still want to see it
                        AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, esriSimpleMarkerStyle.esriSMSCircle, esriRasterOpCode.esriROPNOP, ptAttributes);
                    }
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }

                UpdateDistance(TravelTimeInSeconds * TravelRateInSeconds, RateUnit, true);

                // Trigger validation to clear error messages as necessary
                RaisePropertyChanged(() => RateTimeUnit);
                RaisePropertyChanged(() => TimeUnit);
                RaisePropertyChanged(() => TravelRateString);
                RaisePropertyChanged(() => TravelTimeString);
                RaisePropertyChanged(() => LineDistanceType);
            }
        }

        /// <summary>
        /// Property for travel time in seconds
        /// </summary>
        private double TravelTimeInSeconds
        {
            get
            {
                switch (TimeUnit)
                {
                    case TimeUnits.Seconds:
                        {
                            return travelTime;
                        }
                    case TimeUnits.Minutes:
                        {
                            return travelTime * 60;
                        }
                    case TimeUnits.Hours:
                        {
                            return travelTime * 3600;
                        }
                    default:
                        return travelTime;
                }
            }
        }

        /// <summary>
        /// Property for travel rate in seconds
        /// </summary>
        private double TravelRateInSeconds
        {
            get
            {
                switch (RateTimeUnit)
                {
                    case RateTimeTypes.FeetHour:
                    case RateTimeTypes.KilometersHour:
                    case RateTimeTypes.MetersHour:
                    case RateTimeTypes.MilesHour:
                    case RateTimeTypes.NauticalMilesHour:
                        return TravelRate / 3600;
                    default:
                        return TravelRate;
                }
            }
        }

        /// <summary>
        /// String of time display
        /// </summary>
        public string TravelTimeString
        {
            get
            {
                return TravelTime.ToString("G");
            }
            set
            {
                // divide the manual input by 2
                double t = 0.0;
                if (double.TryParse(value, out t))
                {
                    TravelTime = t;
                }
                else
                {
                    TravelTime = 0.0;
                    ClearTempGraphics();
                    if (HasPoint1)
                    {
                        IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                        ptAttributes.Add("X", Point1.X);
                        ptAttributes.Add("Y", Point1.Y);
                        // Re-add the point as it was cleared by ClearTempGraphics() but we still want to see it
                        AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, esriSimpleMarkerStyle.esriSMSCircle, esriRasterOpCode.esriROPNOP, ptAttributes);
                    }
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEEnterValue);
                }
            }
        }

        double travelTime = 0.0;
        /// <summary>
        /// Property for time display
        /// </summary>
        public double TravelTime
        {
            get
            {
                return travelTime;
            }
            set
            {
                
                if (value < 0.0)
                {
                    UpdateFeedbackWithGeoCircle();
                    ClearTempGraphics();
                    if (HasPoint1)
                    {
                        IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                        ptAttributes.Add("X", Point1.X);
                        ptAttributes.Add("Y", Point1.Y);
                        // Re-add the point as it was cleared by ClearTempGraphics() but we still want to see it
                        AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, attributes: ptAttributes);
                    }
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);
                }

                travelTime = value;

                // Prevent graphical glitches from excessively high inputs
                double distanceInMeters = ConvertFromTo(RateUnit, DistanceTypes.Meters, TravelRateInSeconds * TravelTimeInSeconds);
                
                if (distanceInMeters > DistanceLimit)
                {
                    RaisePropertyChanged(() => TravelTimeString);
                    UpdateDistance(TravelRateInSeconds * TravelTimeInSeconds, RateUnit, false);
                    ClearTempGraphics();
                    if (HasPoint1)
                    {
                        IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                        ptAttributes.Add("X", Point1.X);
                        ptAttributes.Add("Y", Point1.Y);
                        // Re-add the point as it was cleared by ClearTempGraphics() but we still want to see it
                        AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, attributes: ptAttributes);
                    }
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }

                // we need to make sure we are in the same units as the Distance property before setting
                UpdateDistance(TravelRateInSeconds * TravelTimeInSeconds, RateUnit, true);

                // Trigger validation to clear error messages as necessary
                RaisePropertyChanged(() => RateTimeUnit);
                RaisePropertyChanged(() => TimeUnit);
                RaisePropertyChanged(() => TravelRateString);
                RaisePropertyChanged(() => TravelTimeString);
                RaisePropertyChanged(() => LineDistanceType);
            }
        }

        private void UpdateDistance(double distance, DistanceTypes fromDistanceType, bool belowLimit)
        {
            Distance = ConvertFromTo(fromDistanceType, LineDistanceType, distance);

            if (belowLimit)
            {
                UpdateFeedbackWithGeoCircle();
            }
        }

        /// <summary>
        /// String of rate display
        /// </summary>
        public string TravelRateString
        {
            get
            {
                return TravelRate.ToString("G");
            }
            set
            {
                // divide the manual input by 2
                double t = 0.0;
                
                if (double.TryParse(value, out t))
                {
                    TravelRate = t;
                }
                else
                {
                    ClearTempGraphics();
                    if (HasPoint1)
                    {
                        IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                        ptAttributes.Add("X", Point1.X);
                        ptAttributes.Add("Y", Point1.Y);
                        // Re-add the point as it was cleared by ClearTempGraphics() but we still want to see it
                        AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, attributes: ptAttributes);
                    }
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEEnterValue);
                }
            }
        }

        double travelRate = 0.0;
        /// <summary>
        /// Property of rate display
        /// </summary>
        public double TravelRate
        {
            get
            {
                return travelRate;
            }
            set
            {
                
                if (value < 0.0)
                {
                    ClearTempGraphics();
                    if (HasPoint1)
                    {
                        IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                        ptAttributes.Add("X", Point1.X);
                        ptAttributes.Add("Y", Point1.Y);
                        // Re-add the point as it was cleared by ClearTempGraphics() but we still want to see it
                        AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, attributes: ptAttributes);
                    }
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);
                }

                travelRate = value;

                // Prevent graphical glitches from excessively high inputs
                double distanceInMeters = ConvertFromTo(RateUnit, DistanceTypes.Meters, TravelRateInSeconds * TravelTimeInSeconds);
                if (distanceInMeters > DistanceLimit)
                {
                    UpdateDistance(TravelRateInSeconds * TravelTimeInSeconds, RateUnit, false);
                    RaisePropertyChanged(() => TravelRateString);
                    ClearTempGraphics();
                    if (HasPoint1)
                    {
                        IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                        ptAttributes.Add("X", Point1.X);
                        ptAttributes.Add("Y", Point1.Y);
                        // Re-add the point as it was cleared by ClearTempGraphics() but we still want to see it
                        AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, attributes: ptAttributes);
                    }
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }

                UpdateDistance(TravelRateInSeconds * TravelTimeInSeconds, RateUnit, true);
                RaisePropertyChanged(() => TravelRateString);

                // Trigger validation to clear error messages as necessary
                RaisePropertyChanged(() => TravelTimeString);
                RaisePropertyChanged(() => RateTimeUnit);
                RaisePropertyChanged(() => TimeUnit);
                RaisePropertyChanged(() => LineDistanceType);
            }
        }

        DistanceTypes rateUnit = DistanceTypes.Meters;
        public DistanceTypes RateUnit
        {
            get
            {
                switch (RateTimeUnit)
                {
                    case RateTimeTypes.FeetHour:
                    case RateTimeTypes.FeetSec:
                        return DistanceTypes.Feet;
                    case RateTimeTypes.KilometersHour:
                    case RateTimeTypes.KilometersSec:
                        return DistanceTypes.Kilometers;
                    case RateTimeTypes.MetersHour:
                    case RateTimeTypes.MetersSec:
                        return DistanceTypes.Meters;
                    case RateTimeTypes.MilesHour:
                    case RateTimeTypes.MilesSec:
                        return DistanceTypes.Miles;
                    case RateTimeTypes.NauticalMilesHour:
                    case RateTimeTypes.NauticalMilesSec:
                        return DistanceTypes.NauticalMile;
                    default:
                        return DistanceTypes.Meters;
                }
            }
            set
            {
                if (rateUnit == value)
                {
                    return;
                }

                rateUnit = value;
                
                // Prevent graphical glitches from excessively high inputs
                double distanceInMeters = ConvertFromTo(rateUnit, DistanceTypes.Meters, TravelRateInSeconds * TravelTimeInSeconds);
                if (distanceInMeters > DistanceLimit)
                {
                    RaisePropertyChanged(() => TravelTimeString);
                    UpdateDistance(TravelRateInSeconds * TravelTimeInSeconds, RateUnit, false);
                    ClearTempGraphics();
                    if (HasPoint1)
                    {
                        IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                        ptAttributes.Add("X", Point1.X);
                        ptAttributes.Add("Y", Point1.Y);
                        // Re-add the point as it was cleared by ClearTempGraphics() but we still want to see it
                        AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, attributes: ptAttributes);
                    }
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }

                UpdateDistance(TravelTimeInSeconds * TravelRateInSeconds, RateUnit, true);

                RaisePropertyChanged(() => RateUnit);
            }
        }

        RateTimeTypes rateTimeUnit = RateTimeTypes.MilesHour;
        public RateTimeTypes RateTimeUnit
        {
            get
            {
                return rateTimeUnit;
            }
            set
            {
                if (rateTimeUnit == value)
                {
                    return;
                }
                rateTimeUnit = value;

                // Prevent graphical glitches from excessively high inputs
                double distanceInMeters = ConvertFromTo(RateUnit, DistanceTypes.Meters, TravelRateInSeconds * TravelTimeInSeconds);
                
                if (distanceInMeters > DistanceLimit)
                {
                    RaisePropertyChanged(() => TravelTimeString);
                    UpdateDistance(TravelRateInSeconds * TravelTimeInSeconds, RateUnit, false);
                    ClearTempGraphics();
                    if (HasPoint1)
                    {
                        IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                        ptAttributes.Add("X", Point1.X);
                        ptAttributes.Add("Y", Point1.Y);
                        // Re-add the point as it was cleared by ClearTempGraphics() but we still want to see it
                        AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, attributes: ptAttributes);
                    }
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }

                UpdateDistance(TravelTimeInSeconds * TravelRateInSeconds, RateUnit, true);

                // Trigger validation to clear error messages as necessary
                RaisePropertyChanged(() => RateTimeUnit);
                RaisePropertyChanged(() => TimeUnit);
                RaisePropertyChanged(() => TravelTimeString);
                RaisePropertyChanged(() => TravelRateString);
                RaisePropertyChanged(() => LineDistanceType);
            }
        }

        bool isDistanceCalcExpanded = false;
        public bool IsDistanceCalcExpanded
        {
            get { return isDistanceCalcExpanded; }
            set
            {
                isDistanceCalcExpanded = value;
                if (value == true)
                {
                    TravelRate = 0;
                    TravelTime = 0;
                    Distance = 0.0;
                    ResetFeedback();
                }
                else
                {
                    Reset(false);
                }
                
                ClearTempGraphics();
                if (HasPoint1)
                {
                    IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                    ptAttributes.Add("X", Point1.X);
                    ptAttributes.Add("Y", Point1.Y);
                    AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, attributes: ptAttributes);
                }
                    

                RaisePropertyChanged(() => IsDistanceCalcExpanded);
            }
        }
        /// <summary>
        /// Update DistanceString for user
        /// </summary>
        public override string DistanceString
        {
            get
            {
                return base.DistanceString;
            }
            set
            {
                // lets avoid an infinite loop here
                if (string.Equals(base.DistanceString, value))
                    return;
                
                // divide the manual input by 2
                double d = 0.0;
                
                if (double.TryParse(value, out d))
                { 
                    
                    Distance = d;

                    double distanceInMeters = ConvertFromTo(LineDistanceType, DistanceTypes.Meters, Distance);

                    if (distanceInMeters > DistanceLimit)
                    {
                        ClearTempGraphics();
                        if (HasPoint1)
                        {
                            IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                            ptAttributes.Add("X", Point1.X);
                            ptAttributes.Add("Y", Point1.Y);
                            // Re-add the point as it was cleared by ClearTempGraphics() but we still want to see it
                            AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, attributes: ptAttributes);
                        }
                        throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                    }

                    UpdateFeedbackWithGeoCircle();
                }
                else
                {
                    ClearTempGraphics();
                    if (HasPoint1)
                    {
                        IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                        ptAttributes.Add("X", Point1.X);
                        ptAttributes.Add("Y", Point1.Y);
                        // Re-add the point as it was cleared by ClearTempGraphics() but we still want to see it
                        AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, attributes: ptAttributes);
                    }
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }

                // Trigger update to clear exception highlighting if necessary
                RaisePropertyChanged(() => LineDistanceType);
            }
        }

        #endregion

        #region Commands

        // when someone hits the enter key, create geodetic graphic
        internal override void OnEnterKeyCommand(object obj)
        {
            if (Distance == 0 || Point1 == null)
            {
                return;
            }
            base.OnEnterKeyCommand(obj);
        }

        #endregion

        #region override events

        public override DistanceTypes LineDistanceType
        {
            get
            {
                return base.LineDistanceType;
            }
            set
            {
                if (IsDistanceCalcExpanded)
                {
                    var before = base.LineDistanceType;
                    var temp = ConvertFromTo(before, value, Distance);
                    Distance = temp;
                }

                base.LineDistanceType = value;
                
                double distanceInMeters = ConvertFromTo(value, DistanceTypes.Meters, Distance);
                if (distanceInMeters > DistanceLimit)
                {
                    ClearTempGraphics();
                    if (HasPoint1)
                    {
                        IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                        ptAttributes.Add("X", Point1.X);
                        ptAttributes.Add("Y", Point1.Y);
                        // Re-add the point as it was cleared by ClearTempGraphics() but we still want to see it
                        AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, attributes: ptAttributes);
                    }
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }

                // Avoid null reference exception during automated testing
                if (ArcMap.Application != null)
                {
                    UpdateFeedbackWithGeoCircle();
                }
            }
        }

        internal override void OnNewMapPointEvent(object obj)
        {
            var point = obj as IPoint;
            if (point == null)
                return;

            if (IsDistanceCalcExpanded)
            {
                HasPoint1 = false;
            }

            base.OnNewMapPointEvent(obj);

            if (IsDistanceCalcExpanded)
            {
                // Prevent graphical glitches from excessively high inputs
                double distanceInMeters = ConvertFromTo(RateUnit, DistanceTypes.Meters, TravelRateInSeconds * TravelTimeInSeconds);
                
                if (distanceInMeters > DistanceLimit)
                {
                    RaisePropertyChanged(() => TravelTimeString);
                    UpdateDistance(TravelRateInSeconds * TravelTimeInSeconds, RateUnit, false);
                    ClearTempGraphics();
                    if (HasPoint1)
                    {
                        IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                        ptAttributes.Add("X", Point1.X);
                        ptAttributes.Add("Y", Point1.Y);
                        // Re-add the point as it was cleared by ClearTempGraphics() but we still want to see it
                        AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, attributes: ptAttributes);
                    }
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }

                UpdateDistance(TravelRateInSeconds * TravelTimeInSeconds, RateUnit, true);
            }
        }

        internal override void OnMouseMoveEvent(object obj)
        {
            if (!IsActiveTab)
                return;

            var point = obj as IPoint;

            if (point == null)
                return;

            // dynamically update start point if not set yet
            if (!HasPoint1)
            {
                Point1 = point;
            }
            else if (HasPoint1 && !HasPoint2 && !IsDistanceCalcExpanded)
            {
                Point2Formatted = string.Empty;
                // get distance from feedback
                var polyline = GetGeoPolylineFromPoints(Point1, point);
                UpdateDistance(polyline);
            }

            // update feedback
            if (HasPoint1 && !HasPoint2 && !IsDistanceCalcExpanded)
            {
                UpdateFeedbackWithGeoCircle();
            }
        }

        private void UpdateFeedbackWithGeoCircle()
        {
            if (Point1 == null || Distance <= 0)
                return;
         
            var construct = new Polyline() as IConstructGeodetic;

            if (construct != null)
            {
                ClearTempGraphics();
                IDictionary<String, System.Object> circleAttributes = new Dictionary<String, System.Object>();
                if (HasPoint1)
                {
                    IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                    ptAttributes.Add("X", Point1.X);
                    ptAttributes.Add("Y", Point1.Y);
                    circleAttributes.Add("centerx", Point1.X);
                    circleAttributes.Add("centery", Point1.Y);
                    // Re-add the point as it was cleared by ClearTempGraphics() but we still want to see it
                    AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, attributes: ptAttributes);


                    circleAttributes.Add("radius", Distance);
                    circleAttributes.Add("disttype", CircleType.ToString());

                    construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), Distance, esriCurveDensifyMethod.esriCurveDensifyByAngle, 0.45);
                       
                    Point2 = (construct as IPolyline).ToPoint;
                    var color = new RgbColorClass() as IColor;
                    this.AddGraphicToMap(construct as IGeometry, color, true, rasterOpCode: esriRasterOpCode.esriROPNotXOrPen, attributes: circleAttributes);
                }
            }


        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Overrides TabBaseViewModel CreateMapElement
        /// </summary>
        internal override IGeometry CreateMapElement()
        {
            base.CreateMapElement();
            var geom = CreateCircle();
            Reset(false);

            return geom;
        }

        public override bool CanCreateElement
        {
            get
            {
                return (HasPoint1 && Distance != 0.0);
            }
        }

        internal override void Reset(bool toolReset)
        {
            base.Reset(toolReset);
            TravelTime = 0;
            TravelRate = 0;
        }

        /// <summary>
        /// Create a geodetic circle
        /// </summary>
        private IGeometry CreateCircle()
        {
            if (Point1 == null && Point2 == null)
            {
                return null;
            }

            // This section including UpdateDistance serves to handle Diameter appropriately
            var polyLine = new Polyline() as IPolyline;
            polyLine.SpatialReference = Point1.SpatialReference;
            var ptCol = polyLine as IPointCollection;
            ptCol.AddPoint(Point1);
            ptCol.AddPoint(Point2);

            UpdateDistance(polyLine as IGeometry);

            try
            {
                var construct = new Polyline() as IConstructGeodetic;
                if (construct != null)
                {
                    construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), Distance, esriCurveDensifyMethod.esriCurveDensifyByAngle, 0.01);
                    IDictionary<String, System.Object> circleAttributes = new Dictionary<String, System.Object>();
                    double radiusOrDiameterDistance = 0.0;
                    if (CircleType == CircleFromTypes.Diameter)
                        radiusOrDiameterDistance = Distance * 2;
                    else
                        radiusOrDiameterDistance = Distance;

                    //Construct a polygon from geodesic polyline
                    var newPoly = this.PolylineToPolygon((IPolyline)construct);
                    if (newPoly != null)
                    {
                        //Get centroid of polygon
                        var area = newPoly as IArea;
                     
                        string unitLabel = "";
                        int roundingFactor = 0;
                        // If Distance Calculator is in use, use the unit from the Rate combobox
                        // to label the circle
                        if (IsDistanceCalcExpanded)
                        {
                            // Select appropriate label and number of decimal places
                            switch (RateUnit)
                            {
                                case DistanceTypes.Feet:
                                case DistanceTypes.Meters:
                                case DistanceTypes.Yards:
                                    unitLabel = RateUnit.ToString();
                                    roundingFactor = 0;
                                    break;
                                case DistanceTypes.Miles:
                                case DistanceTypes.Kilometers:
                                    unitLabel = RateUnit.ToString();
                                    roundingFactor = 2;
                                    break;
                                case DistanceTypes.NauticalMile:
                                    unitLabel = "Nautical Miles";
                                    roundingFactor = 2;
                                    break;
                                default:
                                    break;
                            }
                        }
                        // Else Distance Calculator not in use, use the unit from the Radius / Diameter combobox
                        // to label the circle
                        else
                        {
                            // Select appropriate number of decimal places
                            switch (LineDistanceType)
                            {
                                case DistanceTypes.Feet:
                                case DistanceTypes.Meters:
                                case DistanceTypes.Yards:
                                    unitLabel = RateUnit.ToString();
                                    roundingFactor = 0;
                                    break;
                                case DistanceTypes.Miles:
                                case DistanceTypes.Kilometers:
                                    unitLabel = RateUnit.ToString();
                                    roundingFactor = 2;
                                    break;
                                case DistanceTypes.NauticalMile:
                                    unitLabel = "Nautical Miles";
                                    roundingFactor = 2;
                                    break;
                                default:
                                    break;
                            }

                            DistanceTypes dtVal = (DistanceTypes)LineDistanceType;
                            unitLabel = dtVal.ToString();
                        }

                        string circleTypeLabel = circleType.ToString();
                        string distanceLabel ="";
                        // Use the unit from Rate combobox if Distance Calculator is expanded
                        if (IsDistanceCalcExpanded)
                        {
                            radiusOrDiameterDistance = ConvertFromTo(LineDistanceType, RateUnit, radiusOrDiameterDistance);
                            distanceLabel = (TrimPrecision(radiusOrDiameterDistance, RateUnit, true)).ToString("N"+roundingFactor.ToString());
                        }
                        else
                        {
                            distanceLabel = (TrimPrecision(radiusOrDiameterDistance, LineDistanceType, false)).ToString("N" + roundingFactor.ToString());
                        }

                        //Add text using centroid point
                        this.AddTextToMap(area.Centroid, string.Format("{0}:{1} {2}",
                            circleTypeLabel,
                            distanceLabel,
                            unitLabel));
                    }

                    double radiusDistance = radiusOrDiameterDistance;
                    if (CircleType == CircleFromTypes.Diameter)
                        radiusDistance = radiusOrDiameterDistance / 2;

                    circleAttributes.Add("radius", radiusDistance);
                    circleAttributes.Add("disttype", CircleType.ToString());
                    circleAttributes.Add("centerx", Point1.X);
                    circleAttributes.Add("centery", Point1.Y);
                    if(IsDistanceCalcExpanded)
                        circleAttributes.Add("distanceunit", RateUnit.ToString());
                    else
                        circleAttributes.Add("distanceunit", LineDistanceType.ToString());
                    var color = new RgbColorClass() { Red = 255 } as IColor;
                    this.AddGraphicToMap(construct as IGeometry, color, attributes: circleAttributes);
                    Point2 = null;
                    HasPoint2 = false;
                    ResetFeedback();
                }
                return construct as IGeometry;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }
        #endregion
    }
}