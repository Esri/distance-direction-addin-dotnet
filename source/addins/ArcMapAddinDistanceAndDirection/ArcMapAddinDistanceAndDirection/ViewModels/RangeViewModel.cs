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
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;

using DistanceAndDirectionLibrary.Helpers;
using DistanceAndDirectionLibrary;
using System.Collections.Generic;

namespace ArcMapAddinDistanceAndDirection.ViewModels
{
    public class RangeViewModel : TabBaseViewModel
    {
        public RangeViewModel()
        {
            Mediator.Register(Constants.MOUSE_DOUBLE_CLICK, OnMouseDoubleClick);
        }

        #region Properties

        private double DistanceLimit = 20000000;

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

        DistanceTypes lineDistanceType = DistanceTypes.Meters;
        /// <summary>
        /// Property for the distance type
        /// </summary>
        public override DistanceTypes LineDistanceType
        {
            get { return lineDistanceType; }
            set
            {
                lineDistanceType = value;

                double distanceInMeters = ConvertFromTo(value, DistanceTypes.Meters, Distance);
                if (distanceInMeters > DistanceLimit)
                {
                    ClearTempGraphics();
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }

                UpdateFeedback();
                RaisePropertyChanged(() => Distance);
                RaisePropertyChanged(() => DistanceString);
            }
        }

        double distance = 0.0;
        /// <summary>
        /// Property for the distance/length
        /// </summary>
        public override double Distance
        {
            get { return distance; }
            set
            {
                if (value < 0.0)
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);

                distance = value;

                // Prevent graphical glitches from excessively high inputs
                double distanceInMeters = ConvertFromTo(LineDistanceType, DistanceTypes.Meters, value);
                if (distanceInMeters > DistanceLimit)
                {
                    ClearTempGraphics();
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }

                DistanceString = distance.ToString("G");
                RaisePropertyChanged(() => Distance);
                RaisePropertyChanged(() => DistanceString);
                RaisePropertyChanged(() => LineDistanceType);
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
                    return (Point1 != null && NumberOfRadials >=0);
                else
                    return (Point1 != null && NumberOfRings > 0 && NumberOfRadials >= 0 && Distance > 0.0);
            }
        }

        #endregion Properties

        /// <summary>
        /// Method used to create the needed map elements to add to the graphics container
        /// Is called by the base class when the "Enter" key is pressed
        /// </summary>
        internal override IGeometry CreateMapElement()
        {
            IGeometry geom = null;
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
                    var construct = new Polyline() as IConstructGeodetic;
                    if (construct == null)
                        continue;

                    var color = new RgbColorClass() { Red = 255 } as IColor;
                    IDictionary<String, System.Object> rrAttributes = new Dictionary<String, System.Object>();
                    rrAttributes.Add("rings", NumberOfRings);
                    rrAttributes.Add("distance", radialLength);
                    rrAttributes.Add("distanceunit", lineDistanceType.ToString());
                    rrAttributes.Add("radials", NumberOfRadials);
                    rrAttributes.Add("centerx", Point1.X);
                    rrAttributes.Add("centery", Point1.Y);
                    construct.ConstructGeodeticLineFromDistance(esriGeodeticType.esriGeodeticTypeLoxodrome, Point1, GetLinearUnit(), radialLength, azimuth, esriCurveDensifyMethod.esriCurveDensifyByDeviation, -1.0);
                    AddGraphicToMap(construct as IGeometry, color, attributes:rrAttributes);

                    azimuth += interval;
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Method used to draw the rings at the desired interval
        /// Rings are constructed as geodetic circles
        /// </summary>
        private IGeometry DrawRings()
        {
            double radius = 0.0;

            try
            {
                IConstructGeodetic construct = null;
                for (int x = 0; x < numberOfRings; x++)
                {
                    // set the current radius
                    radius += Distance;
                    var polyLine = (IPolyline)new Polyline();
                    polyLine.SpatialReference = Point1.SpatialReference;
                    const double DENSIFY_ANGLE_IN_DEGREES = 5.0;
                    construct = (IConstructGeodetic)polyLine;
                    construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), radius, 
                        esriCurveDensifyMethod.esriCurveDensifyByAngle, DENSIFY_ANGLE_IN_DEGREES);
                    var color = (IColor)new RgbColorClass() { Red = 255 };
                    IDictionary<String, System.Object> rrAttributes = new Dictionary<String, System.Object>();
                    rrAttributes.Add("rings", NumberOfRings);
                    rrAttributes.Add("distance", radius);
                    rrAttributes.Add("distanceunit", lineDistanceType.ToString());
                    rrAttributes.Add("radials", NumberOfRadials);
                    rrAttributes.Add("centerx", Point1.X);
                    rrAttributes.Add("centery", Point1.Y);
                    AddGraphicToMap((IGeometry)construct, color, attributes:rrAttributes);

                    // Use negative radius to get the location for the distance label
                    // TODO: someone explain why we need to construct this circle twice, and what -radius means (top of circle or something)?
                    DistanceTypes dtVal = (DistanceTypes)LineDistanceType;
                    construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), -radius, 
                        esriCurveDensifyMethod.esriCurveDensifyByAngle, DENSIFY_ANGLE_IN_DEGREES);
                    this.AddTextToMap((IGeometry)construct, String.Format("{0} {1}", radius.ToString(), dtVal.ToString()));
                }

                return (IGeometry)construct;
            }
            catch(Exception ex)
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

            var point = obj as IPoint;

            if (point == null)
                return;

            if (!IsInteractive)
            {
                Point1 = point;
                HasPoint1 = true;

                ClearTempGraphics();
                var color = new RgbColorClass() { Green = 255 } as IColor;
                IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                ptAttributes.Add("X", Point1.X);
                ptAttributes.Add("Y", Point1.Y);
                AddGraphicToMap( Point1, color, true, esriSimpleMarkerStyle.esriSMSCircle, esriRasterOpCode.esriROPNOP, ptAttributes);

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
                    var color = new RgbColorClass() { Green = 255 } as IColor;
                    IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                    ptAttributes.Add("X", Point1.X);
                    ptAttributes.Add("Y", Point1.Y);
                    AddGraphicToMap(Point1, color, true, attributes: ptAttributes);

                    // Reset formatted string
                    Point1Formatted = string.Empty;
                }
                else
                {
                    // update Distance
                    var polyline = GetGeoPolylineFromPoints(Point1, point);
                    UpdateDistance(polyline);

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

            var point = obj as IPoint;

            if (point == null)
                return;

            if (!HasPoint1)
            {
                Point1 = point;
            }
            else if(HasPoint1 && IsInteractive)
            {
                var polyline = GetGeoPolylineFromPoints(Point1, point);
                UpdateDistance(polyline);

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
            if(IsInteractive && IsToolActive)
                IsToolActive = false;
        }

        internal override void Reset(bool toolReset)
        {
            base.Reset(toolReset);

            NumberOfRadials = 0;
        }

        private void ConstructGeoCircle()
        {
            var construct = (IConstructGeodetic)new Polyline();
            if (construct != null)
            {
                var color = new RgbColorClass() { Red = 255 } as IColor;
                IDictionary<String, System.Object> rrAttributes = new Dictionary<String, System.Object>();
                rrAttributes.Add("rings", NumberOfRings);
                rrAttributes.Add("distance", Distance);
                rrAttributes.Add("radials", NumberOfRadials);
                rrAttributes.Add("centerx", Point1.X);
                rrAttributes.Add("centery", Point1.Y);
                rrAttributes.Add("distanceunit", lineDistanceType.ToString());

                construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), Distance, esriCurveDensifyMethod.esriCurveDensifyByAngle, 0.45);
                Point2 = ((IPolyline)construct).ToPoint;
                AddGraphicToMap(construct as IGeometry, color, attributes: rrAttributes);
                maxDistance = Math.Max(Distance, maxDistance);

                // Use negative Distance to get the location for the distance label
                construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), -Distance, esriCurveDensifyMethod.esriCurveDensifyByAngle, 0.45);
                // Create a non geodesic circle
                var circleGeom = CreateCircleGeometry(Point1, maxDistance);
                // Get the basemap extent
                var basemapExt = GetBasemapExtent(ArcMap.Document.FocusMap);
                // Check if circle is within the basemap extent. If circle is within basemap boundary,
                // then use the geodesic circle for labeling. If it's not, then use the non-geodesic 
                // circle to label.
                bool isWithin = true;
                if (basemapExt != null)
                    isWithin = IsGeometryWithinExtent(circleGeom, basemapExt);
                AddTextToMap((isWithin) ? (IGeometry)construct : circleGeom, String.Format("{0} {1}", Math.Round(Distance, 2).ToString(), lineDistanceType.ToString()));
            }
        }

        private void UpdateFeedbackWithGeoCircle()
        {
            if (Point1 == null || Distance <= 0.0)
                return;

            var construct = (IConstructGeodetic)new Polyline();
                 
            if (construct != null)
            {
                IDictionary<String, System.Object> ptAttributes = new Dictionary<String, System.Object>();
                ptAttributes.Add("X", Point1.X);
                ptAttributes.Add("Y", Point1.Y);
                ClearTempGraphics();
                AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true, attributes: ptAttributes );

                IDictionary<String, System.Object> rrAttributes = new Dictionary<String, System.Object>();
                rrAttributes.Add("rings", NumberOfRings);
                rrAttributes.Add("distance", Distance);
                rrAttributes.Add("radials", NumberOfRadials);
                construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), Distance, esriCurveDensifyMethod.esriCurveDensifyByAngle, 0.45);
                Point2 = (construct as IPolyline).ToPoint;
                var color = new RgbColorClass() as IColor;
                this.AddGraphicToMap( construct as IGeometry, color, true, rasterOpCode: esriRasterOpCode.esriROPNotXOrPen, attributes:rrAttributes);
            }
        }

    }
}
