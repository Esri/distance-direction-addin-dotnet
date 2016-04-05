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

namespace ProAppDistanceAndDirectionModule.ViewModels
{
    public class ProCircleViewModel : ProTabBaseViewModel
    {
        public ProCircleViewModel() 
        {
            ActivateToolCommand = new ArcGIS.Desktop.Framework.RelayCommand(async () =>
            {
                FrameworkApplication.SetCurrentToolAsync("ProAppDistanceAndDirectionModule_SketchTool");
                Mediator.NotifyColleagues("SET_SKETCH_TOOL_TYPE", ArcGIS.Desktop.Mapping.SketchGeometryType.Circle);
            });

            //Mediator.Register("SKETCH_COMPLETE", OnSketchComplete);

            //properties
            CircleType = CircleFromTypes.Radius;
        }

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

                // reset distance
                RaisePropertyChanged(() => Distance);
                RaisePropertyChanged(() => DistanceString);
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
                //var before = timeUnit;
                timeUnit = value;
                //timeValue = ConvertTime(before, value);

                UpdateDistance(travelTime * travelRate, RateUnit);

                RaisePropertyChanged(() => TimeUnit);
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
                UpdateDistance(travelRate * travelTime, RateUnit);

                RaisePropertyChanged(() => TravelTime);
            }
        }

        private void UpdateDistance(double distance, DistanceTypes fromDistanceType)
        {
            Distance = distance;
            UpdateDistanceFromTo(fromDistanceType, LineDistanceType);
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

                UpdateDistance(travelRate * travelTime, RateUnit);

                RaisePropertyChanged(() => TravelRate);
            }
        }

        DistanceTypes rateUnit = DistanceTypes.Meters;
        public DistanceTypes RateUnit
        {
            get
            {
                return rateUnit;
            }
            set
            {
                if (rateUnit == value)
                {
                    return;
                }
                //var before = rateUnit;
                rateUnit = value;
                //UpdateDistanceFromTo(before, value);
                //rateValue = Distance;

                UpdateDistance(travelTime * travelRate, RateUnit);

                RaisePropertyChanged(() => RateUnit);
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
                    AddGraphicToMap(Point1, ColorFactory.Green, true);

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
                if (string.Equals(base.DistanceString, value))
                    return;

                // divide the manual input by 2
                double d = 0.0;
                if (double.TryParse(value, out d))
                {
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
                UpdateDistance(travelRate * travelTime, RateUnit);
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
                // get distance from feedback
                //var polyline = GetGeoPolylineFromPoints(Point1, point);
                //UpdateDistance(polyline);
                Distance = GeometryEngine.GeodesicDistance(Point1, point);
            }

            // update feedback
            if (HasPoint1 && !HasPoint2 && !IsDistanceCalcExpanded)
            {
                UpdateFeedbackWithGeoCircle();
            }
        }

        private void UpdateFeedbackWithGeoCircle()
        {
            if (Point1 == null || Distance <= 0.0)
                return;

            //TODO update geocircle feedback
            //var construct = new Polyline() as IConstructGeodetic;
            //if (construct != null)
            //{
            //    ClearTempGraphics();
            //    AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true);
            //    construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), Distance, esriCurveDensifyMethod.esriCurveDensifyByAngle, 0.45);
            //    Point2 = (construct as IPolyline).ToPoint;
            //    var color = new RgbColorClass() as IColor;
            //    this.AddGraphicToMap(construct as IGeometry, color, true, rasterOpCode: esriRasterOpCode.esriROPNotXOrPen);
            //}
        }

        #endregion

        #region Private Functions

        internal override void CreateMapElement()
        {
            base.CreateMapElement();
            CreateCircle();
            Reset(false);
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
        /// Create geodetic circle
        /// </summary>
        private void CreateCircle()
        {
            if (Point1 == null && Point2 == null)
            {
                return;
            }

            var param = new GeometryEngine.GeodesicEllipseParameter();

            param.Center = new Coordinate(Point1);
            param.AxisDirection = 0.0;
            param.LinearUnit = LinearUnit.Meters;
            param.OutGeometryType = GeometryType.Polygon;
            param.SemiAxis1Length = Distance;
            param.SemiAxis2Length = Distance;
            param.VertexCount = 99;

            var sr = QueuedTask.Run(() =>
            {
                return SpatialReferenceBuilder.CreateSpatialReference(4326);
            }).Result;

            var geom = GeometryEngine.GeodesicEllipse(param, sr);

            AddGraphicToMap(geom, new CIMRGBColor() { R=255,B=0,G=0,Alpha=25});
        }

        #endregion

    }
}
