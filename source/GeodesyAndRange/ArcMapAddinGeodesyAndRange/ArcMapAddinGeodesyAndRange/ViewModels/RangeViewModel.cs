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

        int numberOfRings = 3;
        public int NumberOfRings
        {
            get{return numberOfRings;}
            set
            {
                if (value < 1 || value > 180)
                    throw new ArgumentException("The number of rings must be between 0 and 180");

                numberOfRings = value;
                RaisePropertyChanged(() => NumberOfRings);
            }
        }

        int numberOfRadials = 0;
        public int NumberOfRadials
        {
            get{return numberOfRadials;}
            set
            {
                if (value < 0 || value > 180)
                    throw new ArgumentException("The number of radials must be between 0 and 180");

                numberOfRadials = value;
                RaisePropertyChanged(() => NumberOfRadials);
            }
        }

        string distanceString = string.Empty;
        public override string DistanceString
        {
            get { return distanceString; }
            set
            {
                // lets avoid an infinite loop here
                if (string.Equals(distanceString, value))
                    return;

                distanceString = value;
                try
                {
                    // update distance
                    double d = 0.0;
                    if (double.TryParse(distanceString, out d))
                    {
                        Distance = d;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        #endregion Properties

        internal override void CreateMapElement()
        {
            // do we have enough data
            if (Point1 == null && NumberOfRings <= 0 && NumberOfRadials < 0 && Distance <= 0.0)
                return;

            DrawCenterPoint();

            DrawRings();

            DrawRadials();
        }

        private void DrawRadials()
        {
            if (NumberOfRadials < 1)
                return;

            double azimuth = 0.0;
            int count = NumberOfRadials * 2;
            double interval = 360.0 / count;
            double radialLength = Distance * NumberOfRings;

            try
            {
                // for each radial, draw from center point
                for (int x = 0; x < count; x++)
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

        private void DrawRings()
        {
            double radius = 0.0;
            try
            {
                for (int x = 0; x < numberOfRings; x++)
                {
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

        private void DrawCenterPoint()
        {
        }

        internal override void OnNewMapPointEvent(object obj)
        {
            if (!IsActiveTab)
                return;

            var mxdoc = ArcMap.Application.Document as IMxDocument;
            var av = mxdoc.FocusMap as IActiveView;
            var point = obj as IPoint;

            if (point == null)
                return;

            Point1 = point;
            //point1Formatted = string.Empty;
            //RaisePropertyChanged(() => Point1Formatted);
        }

        internal override void OnMouseMoveEvent(object obj)
        {
        }
    }
}
