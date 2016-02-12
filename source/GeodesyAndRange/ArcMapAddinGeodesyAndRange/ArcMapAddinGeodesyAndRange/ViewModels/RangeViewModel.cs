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

using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcMapAddinGeodesyAndRange.ViewModels
{
    public class RangeViewModel : TabBaseViewModel
    {
        public RangeViewModel()
        {

        }

        #region Properties

        int numberOfRings = 10;
        /// <summary>
        /// Property for the number or rings
        /// </summary>
        public int NumberOfRings
        {
            get { return numberOfRings; }
            set
            {
                if (value < 1 || value > 180)
                    throw new ArgumentException(string.Format(Properties.Resources.AENumOfRings, 1, 180));

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
                    throw new ArgumentException(string.Format(Properties.Resources.AENumOfRadials, 0, 180));

                numberOfRadials = value;
                RaisePropertyChanged(() => NumberOfRadials);
            }
        }

        #endregion Properties

        /// <summary>
        /// Method used to create the needed map elements to add to the graphics container
        /// Is called by the base class when the "Enter" key is pressed
        /// </summary>
        internal override void CreateMapElement()
        {
            // do we have enough data?
            if (Point1 == null && NumberOfRings <= 0 && NumberOfRadials < 0 && Distance <= 0.0)
                return;

            base.CreateMapElement();

            DrawRings();

            DrawRadials();

            Reset(false);
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
            double radialLength = Distance * NumberOfRings;

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
        private void DrawRings()
        {
            double radius = 0.0;

            try
            {
                for (int x = 0; x < numberOfRings; x++)
                {
                    // set the current radius
                    radius += Distance;
                    var polyLine = new Polyline() as IPolyline;
                    polyLine.SpatialReference = Point1.SpatialReference;
                    var construct = polyLine as IConstructGeodetic;
                    construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), radius, esriCurveDensifyMethod.esriCurveDensifyByDeviation, 0.0001);
                    AddGraphicToMap(construct as IGeometry);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
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

            Point1 = point;
            HasPoint1 = true;

            AddGraphicToMap(Point1, true);

            // Reset formatted string
            Point1Formatted = string.Empty;
        }

        /// <summary>
        /// Override the mouse move event to dynamically update the center point
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
        }

        internal override void Reset(bool toolReset)
        {
            base.Reset(toolReset);

            NumberOfRadials = 0;
        }
    }
}
