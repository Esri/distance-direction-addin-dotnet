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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcMapAddinGeodesyAndRange.Helpers;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;

namespace ArcMapAddinGeodesyAndRange.ViewModels
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
        
        public CircleFromTypes CircleType { get; set; }

        #endregion


        #region Private Functions

        internal override void CreateMapElement()
        {
            CreateCircle();
        }
        /// <summary>
        /// 
        /// </summary>
        private void CreateCircle()
        {
            if (Point1 == null || Point2 == null)
            {
                return;
            }

            var polyLine = new Polyline() as IPolyline;
            polyLine.SpatialReference = Point1.SpatialReference;
            var ptCol = polyLine as IPointCollection;
            ptCol.AddPoint(Point1);
            ptCol.AddPoint(Point2);
            if (CircleType == CircleFromTypes.Diameter)
            {
                var centerPoint = new Point() as IPoint;
                polyLine.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, centerPoint);
                polyLine.FromPoint = Point1 = centerPoint;
            }
            UpdateDistance(polyLine as IGeometry);

            var construct = new Polyline() as IConstructGeodetic;
            construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), Distance, esriCurveDensifyMethod.esriCurveDensifyByDeviation, 1000.0);

            var mxdoc = ArcMap.Application.Document as IMxDocument;
            var av = mxdoc.FocusMap as IActiveView;

            AddGraphicToMap(construct as IGeometry);
        }

        #endregion
    }
}
