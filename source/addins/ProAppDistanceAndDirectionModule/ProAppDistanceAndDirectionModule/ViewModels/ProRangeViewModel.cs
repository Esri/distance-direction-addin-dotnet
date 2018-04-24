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
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using DistanceAndDirectionLibrary;
using DistanceAndDirectionLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

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
                RaisePropertyChanged(() => DistanceString);
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
                    return (Point1 != null && NumberOfRings > 0 && NumberOfRadials >= 0 && RangeIntervals.Count > 0 /*Distance > 0.0*/);
            }
        }

        private string DelimiterString { get; set; }
        private string rangeDistanceString = string.Empty;
        private double DistanceLimit = 20000000;
        public override string DistanceString
        {
            get
            {
                return rangeDistanceString;
            }

            set
            {
                // lets avoid an infinite loop here
                if (string.Equals(distanceString, value))
                {
                    return;
                }
                rangeDistanceString = value;

                //Check for delimiters
                DelimiterString = "";
                foreach (var delimiter in new List<string>() { ",", ";", " " })
                {
                    if (value.IndexOf(delimiter) != -1)
                    {
                        DelimiterString = delimiter;
                        break;
                    }
                }

                //Split the distance string
                var intervalList = value.Split(DelimiterString.ToCharArray()).ToList();
                if (intervalList.Count > 2 && NumberOfRings != intervalList.Count)
                {
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.RingsIntervalsMismatchError);
                }

                foreach (var v in intervalList)
                {
                    double d = 0.0;
                    if (string.IsNullOrEmpty(v)) continue;
                    if (string.IsNullOrWhiteSpace(v)) continue;
                    if (double.TryParse(v, out d))
                    {
                        //Must be positive 
                        if (d < 0.0)
                        {
                            throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);
                        }
                        // Prevent graphical glitches from excessively high inputs
                        double distanceInMeters = ConvertFromTo(LineDistanceType, DistanceTypes.Meters, d);
                        if (distanceInMeters > DistanceLimit)
                        {
                            ClearTempGraphics();
                            throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                        }
                    }
                    else
                    {
                        throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                    }
                }
            }
        }

        private List<double> RangeIntervals { get; set; }
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
                        
                        var results = GeometryEngine.Instance.GeodeticMove(mpList, 
                            MapView.Active.Map.SpatialReference, radialLength, GetLinearUnit(LineDistanceType), GetAzimuthAsRadians(azimuth), GetCurveType());

                        // update feedback
                        //UpdateFeedback();
                        foreach (var mp in results)
                            movedMP = mp;
                        if (movedMP != null)
                        {
                            var movedMPproj = GeometryEngine.Instance.Project(movedMP, Point1.SpatialReference);
                            var segment = LineBuilder.CreateLineSegment(Point1, (MapPoint)movedMPproj);
                            return PolylineBuilder.CreatePolyline(segment);
                        }
                        else
                            return null;
                    }).Result;
                    Geometry newline = GeometryEngine.Instance.GeodeticDensifyByLength(polyline, 0, LinearUnit.Meters, GeodeticCurveType.Loxodrome);
                    if (newline != null)
                    {
                        // Hold onto the attributes in case user saves graphics to file later
                        RangeAttributes rangeAttributes = new RangeAttributes() { mapPoint = Point1, numRings = NumberOfRings, distance = radialLength, numRadials = NumberOfRadials, centerx=Point1.X, centery=Point1.Y, distanceunit=LineDistanceType.ToString() };
                        AddGraphicToMap(newline, rangeAttributes);
                    }

                    azimuth += interval;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
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
                    radius += RangeIntervals.Count == 1 ? RangeIntervals[0] : RangeIntervals[x]; //Distance;

                    var param = new GeodesicEllipseParameter();
                    
                    param.Center = new Coordinate2D(Point1);
                    param.AxisDirection = 0.0;
                    param.LinearUnit = GetLinearUnit(LineDistanceType);
                    param.OutGeometryType = GeometryType.Polyline;
                    param.SemiAxis1Length = radius;
                    param.SemiAxis2Length = radius;
                    param.VertexCount = VertexCount;

                    geom = GeometryEngine.Instance.GeodesicEllipse(param, MapView.Active.Map.SpatialReference);

                    // Hold onto the attributes in case user saves graphics to file later
                    RangeAttributes rangeAttributes = new RangeAttributes() { mapPoint = Point1, numRings = numberOfRings, distance = radius, numRadials = numberOfRadials, centerx=Point1.X, centery=Point1.Y, distanceunit=LineDistanceType.ToString() };

                    AddGraphicToMap(geom, rangeAttributes);
                }

                return geom;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
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
                AddGraphicToMap(Point1, ColorFactory.Instance.GreenRGB, null, true, 5.0);

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
                    AddGraphicToMap(Point1, ColorFactory.Instance.GreenRGB, null, true, 5.0);

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

        internal override void OnEnterKeyCommand(object obj)
        {
            //Iterate through the distance string
            foreach (var v in DistanceString.Split(DelimiterString.ToCharArray()).ToList())
            {
                double d = 0.0;
                if (double.TryParse(v, out d))
                {
                    //Add interval
                    if (RangeIntervals == null)
                    {
                        RangeIntervals = new List<double>();
                    }
                    RangeIntervals.Add(d);
                }
            }
            base.OnEnterKeyCommand(obj);
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

            RangeIntervals = new List<double>();
            NumberOfRadials = 0;
            NumberOfRings = 2;
        }

        private void ConstructGeoCircle()
        {
            if (Point1 == null || double.IsNaN(Distance))
                return;

            var param = new GeodesicEllipseParameter();

            param.Center = new Coordinate2D(Point1);
            param.AxisDirection = 0.0;
            param.LinearUnit = GetLinearUnit(LineDistanceType);
            param.OutGeometryType = GeometryType.Polyline;
            param.SemiAxis1Length = Distance;
            param.SemiAxis2Length = Distance;
            param.VertexCount = VertexCount;

            maxDistance = Math.Max(maxDistance, Distance);

            var geom = GeometryEngine.Instance.GeodesicEllipse(param, MapView.Active.Map.SpatialReference);

            // Hold onto the attributes in case user saves graphics to file later
            RangeAttributes rangeAttributes = new RangeAttributes() { mapPoint = Point1, numRings = NumberOfRings, distance = Distance, numRadials = NumberOfRadials, centerx=Point1.X, centery=Point1.Y, distanceunit=LineDistanceType.ToString() };

            AddGraphicToMap(geom, rangeAttributes);
        }

        private void UpdateFeedbackWithGeoCircle()
        {
            if (Point1 == null || double.IsNaN(Distance) || Distance <= 0.0)
                return;

            var param = new GeodesicEllipseParameter();

            param.Center = new Coordinate2D(Point1);
            param.AxisDirection = 0.0;
            param.LinearUnit = GetLinearUnit(LineDistanceType);
            param.OutGeometryType = GeometryType.Polyline;
            param.SemiAxis1Length = Distance;
            param.SemiAxis2Length = Distance;
            param.VertexCount = VertexCount;

            var geom = GeometryEngine.Instance.GeodesicEllipse(param, MapView.Active.Map.SpatialReference);
            ClearTempGraphics();

            // Hold onto the attributes in case user saves graphics to file later
            RangeAttributes rangeAttributes = new RangeAttributes() { mapPoint = Point1, numRings = NumberOfRings, distance = Distance, numRadials = NumberOfRadials, centerx=Point1.X, centery=Point1.Y, distanceunit=LineDistanceType.ToString()};

            AddGraphicToMap(Point1, ColorFactory.Instance.GreenRGB, null, true, 5.0);
            AddGraphicToMap(geom, ColorFactory.Instance.GreyRGB, rangeAttributes, true);
        }
    }
}
