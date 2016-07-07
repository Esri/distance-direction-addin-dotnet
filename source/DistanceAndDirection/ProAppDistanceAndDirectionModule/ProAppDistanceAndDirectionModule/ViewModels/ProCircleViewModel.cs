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

using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using DistanceAndDirectionLibrary;
using DistanceAndDirectionLibrary.Helpers;
using System;
using System.Threading.Tasks;

namespace ProAppDistanceAndDirectionModule.ViewModels
{
    public class ProCircleViewModel : ProTabBaseViewModel
    {
        public ProCircleViewModel() 
        {
            ActivateToolCommand = new ArcGIS.Desktop.Framework.RelayCommand(async () =>
            {
                await FrameworkApplication.SetCurrentToolAsync("ProAppDistanceAndDirectionModule_SketchTool");
                Mediator.NotifyColleagues("SET_SKETCH_TOOL_TYPE", ArcGIS.Desktop.Mapping.SketchGeometryType.Circle);
            });

            // we may need this in the future
            //Mediator.Register("SKETCH_COMPLETE", OnSketchComplete);

            //properties
            CircleType = CircleFromTypes.Radius;
        }

        // future use
        //private void OnSketchComplete(object obj)
        //{
        //    AddGraphicToMap(obj as ArcGIS.Core.Geometry.Geometry);
        //}

        public ArcGIS.Desktop.Framework.RelayCommand ActivateToolCommand { get; set; }

        #region Properties

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

                if (IsDistanceCalcExpanded)
                {
                    UpdateDistance(TravelRateInSeconds * TravelTimeInSeconds, RateUnit);
                }
                else
                {
                    if (value == CircleFromTypes.Diameter)
                        Distance /= 2.0;
                    else
                        Distance *= 2.0;
                }

                // reset distance
                RaisePropertyChanged(() => DistanceString);
                //RaisePropertyChanged(() => Distance);

                UpdateFeedback();
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

                UpdateDistance(TravelTimeInSeconds * TravelRateInSeconds, RateUnit);

                RaisePropertyChanged(() => TimeUnit);
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
                            return travelTime * 60.0;
                        }
                    case TimeUnits.Hours:
                        {
                            return travelTime * 3600.0;
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
                        return TravelRate / 3600.0;
                    default:
                        return TravelRate;
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
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);

                travelTime = value;

                // we need to make sure we are in the same units as the Distance property before setting
                UpdateDistance(TravelRateInSeconds * TravelTimeInSeconds, RateUnit);

                RaisePropertyChanged(() => TravelTime);
            }
        }

        private void UpdateDistance(double distance, DistanceTypes fromDistanceType)
        {
            if(CircleType == CircleFromTypes.Diameter)
                Distance = ConvertFromTo(fromDistanceType, LineDistanceType, distance) * 2.0;
            else
                Distance = ConvertFromTo(fromDistanceType, LineDistanceType, distance);
            UpdateFeedbackWithGeoCircle();
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
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);

                travelRate = value;

                UpdateDistance(TravelRateInSeconds * TravelTimeInSeconds, RateUnit);

                RaisePropertyChanged(() => TravelRate);
            }
        }

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
                    if (CircleType == CircleFromTypes.Diameter)
                        Distance = temp * 2.0;
                    else
                        Distance = temp;
                }

                base.LineDistanceType = value;
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

                UpdateDistance(TravelTimeInSeconds * TravelRateInSeconds, RateUnit);

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

                UpdateDistance(TravelTimeInSeconds * TravelRateInSeconds, RateUnit);

                RaisePropertyChanged(() => RateTimeUnit);
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
                    TravelRate = 0.0;
                    TravelTime = 0.0;
                    Distance = 0.0;
                    ResetFeedback();
                }
                else
                {
                    Reset(false);
                }

                ClearTempGraphics();
                if (HasPoint1)
                    AddGraphicToMap(Point1, ColorFactory.Green, true, 5.0);

                RaisePropertyChanged(() => IsDistanceCalcExpanded);
            }
        }
        /// <summary>
        /// Distance is always the radius
        /// Update DistanceString for user
        /// Do nothing for Radius mode, double the radius for Diameter mode
        /// </summary>
        public override string DistanceString
        {
            get
            {
                if (CircleType == CircleFromTypes.Diameter)
                {
                    return (Distance * 2.0).ToString("G");
                }

                return base.DistanceString;
            }
            set
            {
                // lets avoid an infinite loop here
                if (CircleType == CircleFromTypes.Diameter)
                {
                    if (string.Equals(base.DistanceString, (Convert.ToDouble(value)*2.0).ToString()))
                        return;
                }
                else
                {
                    if (string.Equals(base.DistanceString, value))
                        return;
                }

                base.DistanceString = value;

                // divide the manual input by 2
                double d = 0.0;
                if (double.TryParse(value, out d))
                {
                    if (CircleType == CircleFromTypes.Diameter)
                    {
                        if (Distance * 2.0 == d)
                            return;
                    }
                    else
                    {
                        if (Distance == d)
                            return;
                    }
                    if (CircleType == CircleFromTypes.Diameter)
                        d /= 2.0;

                    Distance = d;

                    UpdateFeedbackWithGeoCircle();
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
            if (Distance == 0 || Point1 == null)
            {
                return;
            }
            base.OnEnterKeyCommand(obj);
        }

        #endregion

        #region override events

        internal override void OnNewMapPointEvent(object obj)
        {
            var point = obj as MapPoint;
            if (point == null)
                return;

            if (IsDistanceCalcExpanded)
            {
                HasPoint1 = false;
            }

            base.OnNewMapPointEvent(obj);

            if (IsDistanceCalcExpanded)
            {
                UpdateDistance(TravelTimeInSeconds * TravelRateInSeconds, RateUnit);
            }
        }

        internal override void OnMouseMoveEvent(object obj)
        {
            if (!IsActiveTab)
                return;

            var point = obj as MapPoint;

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
                Distance = GetGeodesicDistance(Point1, point);
            }

            // update feedback
            if (HasPoint1 && !HasPoint2 && !IsDistanceCalcExpanded)
            {
                UpdateFeedbackWithGeoCircle();
            }
        }

        internal override void UpdateFeedback()
        {
            UpdateFeedbackWithGeoCircle();
        }

        private void UpdateFeedbackWithGeoCircle()
        {
            if (Point1 == null || Distance <= 0.0)
                return;

            CreateCircle(true, true);
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Overrides TabBaseViewModel CreateMapElement
        /// </summary>
        /// <param name="interactiveMode">indicates whether the Enter key was pressed (interactiveMode = false) or mouse click (interactiveMode = true)</param>
        internal override void CreateMapElement(bool interactiveMode = true)
        {
            base.CreateMapElement();
            CreateCircle(false, interactiveMode);
            Reset(false);
        }

        public override bool CanCreateElement
        {
            get
            {
                return (HasPoint1 && Distance > 0.0);
            }
        }

        internal override void Reset(bool toolReset)
        {
            base.Reset(toolReset);
            TravelTime = 0.0;
            TravelRate = 0.0;
        }
        /// <summary>
        /// Create geodetic circle
        /// </summary>
        private async Task CreateCircle(bool isFeedback, bool interactiveMode)
        {
            if (Point1 == null || double.IsNaN(Distance) || Distance <= 0.0)
            {
                return;
            }

            var param = new GeometryEngine.GeodesicEllipseParameter();

            param.Center = new Coordinate(Point1);
            param.AxisDirection = 0.0;
            param.LinearUnit = GetLinearUnit(LineDistanceType);
            param.OutGeometryType = GeometryType.Polygon;
            if (isFeedback)
                param.OutGeometryType = GeometryType.Polyline;
            param.SemiAxis1Length = Distance;
            param.SemiAxis2Length = Distance;
            param.VertexCount = VertexCount;

            var geom = GeometryEngine.GeodesicEllipse(param, MapView.Active.Map.SpatialReference);

            CIMColor color =  new CIMRGBColor() { R=255,B=0,G=0,Alpha=25};
            if(isFeedback)
            {
                color = ColorFactory.Grey;
                ClearTempGraphics();
                AddGraphicToMap(Point1, ColorFactory.Green, true, 5.0);
            }
            AddGraphicToMap(geom, color, IsTempGraphic: isFeedback);

            if (!interactiveMode)
            {
                // Zoom to extent of circle
                if (geom != null)
                {
                    var env = geom.Extent;

                    double extentPercent = (env.XMax - env.XMin) > (env.YMax - env.YMin) ? (env.XMax - env.XMin) * .3 : (env.YMax - env.YMin) * .3;
                    double xmax = env.XMax + extentPercent;
                    double xmin = env.XMin - extentPercent;
                    double ymax = env.YMax + extentPercent;
                    double ymin = env.YMin - extentPercent;

                    //Create the envelope
                    var envelope = await QueuedTask.Run(() => ArcGIS.Core.Geometry.EnvelopeBuilder.CreateEnvelope(xmin, ymin, xmax, ymax, MapView.Active.Map.SpatialReference));

                    //Zoom the view to a given extent.
                    await MapView.Active.ZoomToAsync(envelope, TimeSpan.FromSeconds(2));
                }
            }
        }

        #endregion

    }
}
