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

        //double distance = 0.0;
        //public override double Distance
        //{
        //    get { return distance; }
        //    set
        //    {
        //        distance = value;
        //        DistanceString = string.Format("{0:0.00}", distance);
        //        RaisePropertyChanged(() => Distance);
        //        RaisePropertyChanged(() => DistanceString);
        //    }
        //}
        string distanceString = String.Empty;
        public override string DistanceString
        {
            get
            {
                return string.Format("{0:0.00}", Distance);
            }
            set
            {
                distanceString = value;
            }
        }

        #endregion


        #region Private Functions

        internal override void CreateMapElement()
        {
            base.CreateMapElement();
            CreateCircle();
        }
        /// <summary>
        /// 
        /// </summary>
        private void CreateCircle()
        {
            if (this.Point1 == null || this.Point2 == null)
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
                var area = polyLine.Envelope as IArea;
                var queryPoint = area.Centroid as IPoint;
                var hitTest = polyLine as IHitTest;
                var centroidPoint = new Point() as IPoint;
                var distance = 0.0;
                var hitPartIndex = 0;
                var hitSegmentIndex = 0;
                var isOnRightSide = false;
                var isHit = hitTest.HitTest(queryPoint, 2.0,
                    esriGeometryHitPartType.esriGeometryPartMidpoint,
                    centroidPoint, ref distance, ref hitPartIndex,
                    ref hitSegmentIndex, ref isOnRightSide);
                polyLine.FromPoint = this.Point1 = centroidPoint;
            }
            this.UpdateDistance(polyLine as IGeometry);

            var construct = new Polyline() as IConstructGeodetic;
            if (construct != null)
            {
                construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), Distance, esriCurveDensifyMethod.esriCurveDensifyByDeviation, 0.0001);
                this.AddGraphicToMap(construct as IGeometry);

                if (CircleType == CircleFromTypes.Diameter)
                {
                    DistanceString = string.Format("{0:0.00}", (Distance / 1000.0));
                }
            }
        }

        #endregion
    }
}