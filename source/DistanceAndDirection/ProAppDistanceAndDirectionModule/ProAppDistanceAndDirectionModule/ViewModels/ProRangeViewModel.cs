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

using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using DistanceAndDirectionLibrary;
using DistanceAndDirectionLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProAppDistanceAndDirectionModule.ViewModels
{
    public class ProRangeViewModel : ProTabBaseViewModel
    {
        public ProRangeViewModel()
        {
            Mediator.Register(DistanceAndDirectionLibrary.Constants.MOUSE_DOUBLE_CLICK, OnMouseDoubleClick);
        }

        #region Properties

        private bool isInteractive = false;
        public bool IsInteractive
        {
            get
            {
                return isInteractive;
            }
            set
            {
                isInteractive = value;
                if (value)
                {
                    maxDistance = 0.0;
                    NumberOfRings = 0;
                }
                else
                    NumberOfRings = 10;
            }
        }

        public override bool IsToolActive
        {
            get
            {
                return base.IsToolActive;
            }
            set
            {
                base.IsToolActive = value;

                if (CanCreateElement)
                    CreateMapElement();

                maxDistance = 0.0;
                if (IsInteractive)
                    NumberOfRings = 0;
            }
        }


        // keep track of the max distance for drawing of radials in interactive mode
        double maxDistance = 0.0;

        int numberOfRings = 10;
        /// <summary>
        /// Property for the number or rings
        /// </summary>
        public int NumberOfRings
        {
            get { return numberOfRings; }
            set
            {
                if (!IsInteractive)
                {
                    if (value < 1 || value > 180)
                        throw new ArgumentException(string.Format(DistanceAndDirectionLibrary.Properties.Resources.AENumOfRings, 1, 180));
                }
                numberOfRings = value;
                RaisePropertyChanged(() => NumberOfRings);
            }
        }

        int numberOfRadials = 0;
        /// <summary>
        /// Property for the number of radials
        /// </summary>
        public int NumberOfRadials
        {
            get { return numberOfRadials; }
            set
            {
                if (value < 0 || value > 180)
                    throw new ArgumentException(string.Format(DistanceAndDirectionLibrary.Properties.Resources.AENumOfRadials, 0, 180));

                numberOfRadials = value;
                RaisePropertyChanged(() => NumberOfRadials);
            }
        }

        public override bool CanCreateElement
        {
            get
            {
                if (IsInteractive)
                    return (Point1 != null && NumberOfRadials >= 0);
                else
                    return (Point1 != null && NumberOfRings > 0 && NumberOfRadials >= 0 && Distance > 0.0);
            }
        }

        #endregion Properties

        /// <summary>
        /// Method used to create the needed map elements to add to the graphics container
        /// Is called by the base class when the "Enter" key is pressed
        /// </summary>
        internal override Geometry CreateMapElement()
        {
            Geometry geom = null;
            // do we have enough data?
            if (!CanCreateElement)
                return geom;

            if (!IsInteractive)
            {
                base.CreateMapElement();

                geom = DrawRings();
            }

            DrawRadials();

            Reset(false);

            return geom;
        }

        /// <summary>
        /// Method to draw the radials inside the range rings
        /// Must have at least 1 radial
        /// All radials are drawn from the center point to the farthest ring
        /// </summary>
        private void DrawRadials()
        {
            // must have at least 1
            if (NumberOfRadials < 1)
                return;

            double azimuth = 0.0;
            double interval = 360.0 / NumberOfRadials;
            double radialLength = 0.0;

            if (IsInteractive)
                radialLength = maxDistance;
            else
                radialLength = Distance * NumberOfRings;

            try
            {
                // for each radial, draw from center point
                for (int x = 0; x < NumberOfRadials; x++)
                {
                    var polyline = QueuedTask.Run(() =>
                    {
                        MapPoint movedMP = null;
                        var mpList = new List<MapPoint>() { Point1 };
                        // get point 2
                        
                        var results = GeometryEngine.GeodeticMove(mpList, MapView.Active.Map.SpatialReference, radialLength, GetLinearUnit(LineDistanceType), GetAzimuthAsRadians(azimuth), GetCurveType());
                        // update feedback
                        //UpdateFeedback();
                        foreach (var mp in results)
                            movedMP = mp;
                        if (movedMP != null)
                        {
                            var segment = LineBuilder.CreateLineSegment(Point1, movedMP);
                            return PolylineBuilder.CreatePolyline(segment);
                        }
                        else
                            return null;
                    }).Result;
                    Geometry newline = GeometryEngine.GeodeticDensifyByLength(polyline, 0, LinearUnit.Meters, CurveType.Loxodrome);
                    if (newline != null)
                        
                        AddGraphicToMap(newline);


                    azimuth += interval;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private double GetAzimuthAsRadians(double azimuth)
        {
            return azimuth * (Math.PI / 180.0);
        }

        /// <summary>
        /// Method used to draw the rings at the desired interval
        /// Rings are constructed as geodetic circles
        /// </summary>
        private Geometry DrawRings()
        {
            if (Point1 == null || double.IsNaN(Distance))
                return null;

            double radius = 0.0;

            try
            {
                Geometry geom = null;
                for (int x = 0; x < numberOfRings; x++)
                {
                    // set the current radius
                    radius += Distance;

                    var param = new GeometryEngine.GeodesicEllipseParameter();

                    param.Center = new Coordinate(Point1);
                    param.AxisDirection = 0.0;
                    param.LinearUnit = GetLinearUnit(LineDistanceType);
                    param.OutGeometryType = GeometryType.Polyline;
                    param.SemiAxis1Length = radius;
                    param.SemiAxis2Length = radius;
                    param.VertexCount = VertexCount;

                    geom = GeometryEngine.GeodesicEllipse(param, MapView.Active.Map.SpatialReference);

                    AddGraphicToMap(geom);   
                }

                return geom;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// Override the on new map point event to only handle one point for the center point
        /// </summary>
        /// <param name="obj"></param>
        internal override void OnNewMapPointEvent(object obj)
        {
            // only if we are the active tab
            if (!IsActiveTab)
                return;

            var point = obj as MapPoint;

            if (point == null)
                return;

            if (!IsInteractive)
            {
                Point1 = point;
                HasPoint1 = true;

                ClearTempGraphics();
                AddGraphicToMap(Point1, ColorFactory.GreenRGB, true, 5.0);

                // Reset formatted string
                Point1Formatted = string.Empty;
            }
            else
            {
                // we are in interactive mode
                if (!HasPoint1)
                {
                    Point1 = point;
                    HasPoint1 = true;

                    ClearTempGraphics();
                    AddGraphicToMap(Point1, ColorFactory.GreenRGB, true, 5.0);

                    // Reset formatted string
                    Point1Formatted = string.Empty;
                }
                else
                {
                    // update Distance
                    Distance = GetGeodesicDistance(Point1, point);

                    // draw a geo ring
                    ConstructGeoCircle();

                    NumberOfRings++;
                }
            }
        }

        /// <summary>
        /// Override the mouse move event to dynamically update the center point
        /// Also dynamically update the ring feedback
        /// </summary>
        /// <param name="obj"></param>
        internal override void OnMouseMoveEvent(object obj)
        {
            // only if we are the active tab
            if (!IsActiveTab)
                return;

            var point = obj as MapPoint;

            if (point == null)
                return;

            if (!HasPoint1)
            {
                Point1 = point;
            }
            else if (HasPoint1 && IsInteractive)
            {
                Distance = GetGeodesicDistance(Point1, point);

                // update ring feedback, distance
                UpdateFeedbackWithGeoCircle();
            }
        }

        /// <summary>
        /// Method to handle map point tool double click
        /// End interactive drawing of range rings
        /// </summary>
        /// <param name="obj"></param>
        private void OnMouseDoubleClick(object obj)
        {
            if (IsInteractive && IsToolActive)
                IsToolActive = false;
        }

        internal override void Reset(bool toolReset)
        {
            base.Reset(toolReset);

            NumberOfRadials = 0;
        }

        private void ConstructGeoCircle()
        {
            if (Point1 == null || double.IsNaN(Distance))
                return;

            var param = new GeometryEngine.GeodesicEllipseParameter();

            param.Center = new Coordinate(Point1);
            param.AxisDirection = 0.0;
            param.LinearUnit = GetLinearUnit(LineDistanceType);
            param.OutGeometryType = GeometryType.Polyline;
            param.SemiAxis1Length = Distance;
            param.SemiAxis2Length = Distance;
            param.VertexCount = VertexCount;

            maxDistance = Math.Max(maxDistance, Distance);

            var geom = GeometryEngine.GeodesicEllipse(param, MapView.Active.Map.SpatialReference);

            AddGraphicToMap(geom);
        }

        private void UpdateFeedbackWithGeoCircle()
        {
            if (Point1 == null || double.IsNaN(Distance) || Distance <= 0.0)
                return;

            var param = new GeometryEngine.GeodesicEllipseParameter();

            param.Center = new Coordinate(Point1);
            param.AxisDirection = 0.0;
            param.LinearUnit = GetLinearUnit(LineDistanceType);
            param.OutGeometryType = GeometryType.Polyline;
            param.SemiAxis1Length = Distance;
            param.SemiAxis2Length = Distance;
            param.VertexCount = VertexCount;

            var geom = GeometryEngine.GeodesicEllipse(param, MapView.Active.Map.SpatialReference);
            ClearTempGraphics();
            AddGraphicToMap(Point1, ColorFactory.GreenRGB, true, 5.0);
            AddGraphicToMap(geom, ColorFactory.GreyRGB, true);
        }
    }
}
