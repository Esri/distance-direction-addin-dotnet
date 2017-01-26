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

namespace ArcMapAddinDistanceAndDirection.ViewModels
{
    public class RangeViewModel : TabBaseViewModel
    {
        public RangeViewModel()
        {
            Mediator.Register(Constants.MOUSE_DOUBLE_CLICK, OnMouseDoubleClick);
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

                    construct.ConstructGeodeticLineFromDistance(GetEsriGeodeticType(), Point1, GetLinearUnit(), radialLength, azimuth, esriCurveDensifyMethod.esriCurveDensifyByDeviation, -1.0);
                    AddGraphicToMap(construct as IGeometry);

                    azimuth += interval;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
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
                    var polyLine = new Polyline() as IPolyline;
                    polyLine.SpatialReference = Point1.SpatialReference;
                    construct = polyLine as IConstructGeodetic;
                    construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), radius, esriCurveDensifyMethod.esriCurveDensifyByAngle, 0.001);
                    AddGraphicToMap(construct as IGeometry);

                    // Use negative radius to get the location for the distance label
                    DistanceTypes dtVal = (DistanceTypes)LineDistanceType;
                    construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), -radius, esriCurveDensifyMethod.esriCurveDensifyByAngle, 0.001);
                    this.AddTextToMap(construct as IGeometry, String.Format("{0} {1}", radius.ToString(), dtVal.ToString()));
                }

                return construct as IGeometry;
            }
            catch(Exception ex)
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

            var point = obj as IPoint;

            if (point == null)
                return;

            if (!IsInteractive)
            {
                Point1 = point;
                HasPoint1 = true;

                ClearTempGraphics();
                var color = new RgbColorClass() { Green = 255 } as IColor;
                AddGraphicToMap(Point1, color, true);

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
                    AddGraphicToMap(Point1, color, true);

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
            var construct = new Polyline() as IConstructGeodetic;
            if (construct != null)
            {
                construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), Distance, esriCurveDensifyMethod.esriCurveDensifyByAngle, 0.45);
                Point2 = (construct as IPolyline).ToPoint;
                this.AddGraphicToMap(construct as IGeometry);
                maxDistance = Math.Max(Distance, maxDistance);

                // Use negative Distance to get the location for the distance label
                construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), -Distance, esriCurveDensifyMethod.esriCurveDensifyByAngle, 0.45);
                this.AddTextToMap(construct as IGeometry, String.Format("{0} {1}{2}", Math.Round(Distance, 2).ToString(), GetLinearUnit().Name, "s"));
            }
        }

        private void UpdateFeedbackWithGeoCircle()
        {
            if (Point1 == null || Distance <= 0.0)
                return;

            var construct = new Polyline() as IConstructGeodetic;
            if (construct != null)
            {
                ClearTempGraphics();
                AddGraphicToMap(Point1, new RgbColor() { Green = 255 } as IColor, true);
                construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), Distance, esriCurveDensifyMethod.esriCurveDensifyByAngle, 0.45);
                Point2 = (construct as IPolyline).ToPoint;
                var color = new RgbColorClass() as IColor;
                this.AddGraphicToMap(construct as IGeometry, color, true, rasterOpCode: esriRasterOpCode.esriROPNotXOrPen);
            }
        }

    }
}
