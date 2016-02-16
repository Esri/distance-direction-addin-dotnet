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
        CircleFromTypes circleType = CircleFromTypes.Radius;
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
        /// <summary>
        /// Distance is always the radius
        /// Update DistanceString for user
        /// Do nothing for Radius mode, double the radius for Diameter mode
        /// </summary>
        public override string DistanceString
        {
            get
            {
                if(CircleType == CircleFromTypes.Diameter)
                {
                    return (Distance * 2.0).ToString("N");
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
                if(double.TryParse(value, out d))
                {
                    if(CircleType == CircleFromTypes.Diameter)
                        d /= 2.0;

                    Distance = d;

                    UpdateFeedback();
                }
                else
                {
                    throw new ArgumentException(Properties.Resources.AEInvalidInput);
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


        #region Private Functions

        internal override void CreateMapElement()
        {
            base.CreateMapElement();
            CreateCircle();
            Reset(false);
        }
        /// <summary>
        /// 
        /// </summary>
        private void CreateCircle()
        {
            if (Point1 == null && Point2 == null)
            {
                return;
            }

            var polyLine = new Polyline() as IPolyline;
            polyLine.SpatialReference = Point1.SpatialReference;
            var ptCol = polyLine as IPointCollection;
            ptCol.AddPoint(Point1); ptCol.AddPoint(Point2);

            UpdateDistance(polyLine as IGeometry);

            try
            {
                var construct = new Polyline() as IConstructGeodetic;
                if (construct != null)
                {
                    construct.ConstructGeodesicCircle(Point1, GetLinearUnit(), Distance, esriCurveDensifyMethod.esriCurveDensifyByDeviation, 0.0001);
                    this.AddGraphicToMap(construct as IGeometry);
                    Point2 = null; 
                    HasPoint2 = false;
                    ResetFeedback();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void UpdateFeedback()
        {
            if (Point1 != null)
            {
                if (feedback == null)
                {
                    var mxdoc = ArcMap.Application.Document as IMxDocument;
                    CreateFeedback(Point1, mxdoc.FocusMap as IActiveView);
                    feedback.Start(Point1);
                }

                // now get second point from distance and bearing
                var construct = new Polyline() as IConstructGeodetic;
                if (construct == null)
                    return;

                construct.ConstructGeodeticLineFromDistance(GetEsriGeodeticType(), Point1, GetLinearUnit(), Distance, 0.0, esriCurveDensifyMethod.esriCurveDensifyByDeviation, -1.0);

                var line = construct as IPolyline;

                if (line.ToPoint != null)
                {
                    FeedbackMoveTo(line.ToPoint);
                    Point2 = line.ToPoint;
                }
            }
        }
        #endregion
    }
}