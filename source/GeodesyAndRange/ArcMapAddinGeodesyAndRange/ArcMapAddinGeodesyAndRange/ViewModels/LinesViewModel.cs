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
    public class LinesViewModel : TabBaseViewModel
    {
        public LinesViewModel()
        {
            // props
            LineFromType = LineFromTypes.Points;
            LineAzimuthType = AzimuthTypes.Degrees;
        }

        #region Properties

        LineFromTypes lineFromType = LineFromTypes.Points;
        public LineFromTypes LineFromType 
        {
            get { return lineFromType; }
            set
            {
                lineFromType = value;
                RaisePropertyChanged(() => LineFromType);
                
                // reset points
                ResetPoints();

                // stop feedback when from type changes
                ResetFeedback();
            }
        }

        AzimuthTypes lineAzimuthType = AzimuthTypes.Degrees;
        public AzimuthTypes LineAzimuthType 
        {
            get { return lineAzimuthType; }
            set
            {
                var before = lineAzimuthType;
                lineAzimuthType = value;
                UpdateAzimuthFromTo(before, value);
            }
        }

        public override IPoint Point1
        {
            get
            {
                return base.Point1;
            }
            set
            {
                base.Point1 = value;

                if (LineFromType == LineFromTypes.BearingAndDistance)
                {
                    ResetFeedback();
                }

                UpdateFeedback();
            }
        }

        double distance = 0.0;
        public override double Distance
        {
            get { return distance; }
            set
            {
                distance = value;
                RaisePropertyChanged(() => Distance);

                if(LineFromType == LineFromTypes.BearingAndDistance)
                {
                    // update feedback
                    UpdateFeedback();
                }

                DistanceString = distance.ToString("N");
                RaisePropertyChanged(() => DistanceString);
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
                if(LineFromType == LineFromTypes.BearingAndDistance)
                {
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
        }
        double azimuth = 0.0;
        public double Azimuth
        {
            get { return azimuth; }
            set
            {
                azimuth = value;
                RaisePropertyChanged(() => Azimuth);

                if (LineFromType == LineFromTypes.BearingAndDistance)
                {
                    // update feedback
                    UpdateFeedback();
                }

                AzimuthString = azimuth.ToString("N");
                RaisePropertyChanged(() => AzimuthString);
            }
        }
        string azimuthString = string.Empty;
        public string AzimuthString 
        {
            get { return azimuthString; } 
            set
            {
                // lets avoid an infinite loop here
                if (string.Equals(azimuthString, value))
                    return;
                
                azimuthString = value;
                if(LineFromType == LineFromTypes.BearingAndDistance)
                {
                    try
                    {
                        // update azimuth
                        double d = 0.0;
                        if(double.TryParse(azimuthString, out d))
                        {
                            Azimuth = d;
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }

        #endregion

        #region Commands

        // when someone hits the enter key, create geodetic graphic
        internal override void OnEnterKeyCommand(object obj)
        {
            if(LineFromType == LineFromTypes.Points)
            {
                base.OnEnterKeyCommand(obj);
            }
            else
            {
                ClearTempGraphics();
                // Bearing and Distance
                UpdateFeedback();
                feedback.AddPoint(Point2);
                var polyline = feedback.Stop();
                ResetFeedback();
                AddGraphicToMap(polyline);
            }
        }
        
        private IPolyline CreatePolyline()
        {
            try
            {
                if (Point1 == null || Point2 == null)
                    return null;

                var construct = new Polyline() as IConstructGeodetic;

                if (construct == null)
                    return null;

                if (srf3 == null)
                    srf3 = new ESRI.ArcGIS.Geometry.SpatialReferenceEnvironment() as ISpatialReferenceFactory3;

                var linearUnit = srf3.CreateUnit((int)esriSRUnitType.esriSRUnit_Meter) as ILinearUnit;

                construct.ConstructGeodeticLineFromPoints(GetEsriGeodeticType(), Point1, Point2, linearUnit, esriCurveDensifyMethod.esriCurveDensifyByDeviation, -1.0);

                var mxdoc = ArcMap.Application.Document as IMxDocument;
                var av = mxdoc.FocusMap as IActiveView;

                UpdateDistance(construct as IGeometry);
                UpdateAzimuth(construct as IGeometry);

                AddGraphicToMap(construct as IGeometry);
                ResetPoints();

                return construct as IPolyline;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
        }

        private void UpdateAzimuth(IGeometry geometry)
        {
            var curve = geometry as ICurve;

            if (curve == null)
                return;

            var line = new Line() as ILine;
            
            curve.QueryTangent(esriSegmentExtension.esriNoExtension, 0.5, true, 10, line);
            
            if(line == null)
                return;

            Azimuth = GetAngleDegrees(line.Angle);
        }

        private double GetAngleDegrees(double angle)
        {
            double bearing = (180.0 * angle) / Math.PI;
            if (bearing < 90.0)
                bearing = 90 - bearing;
            else
                bearing= 360.0 - (bearing - 90);

            if (LineAzimuthType == AzimuthTypes.Degrees)
            {
                return bearing;
            }

            if (LineAzimuthType == AzimuthTypes.Mils)
            {
                return bearing * 17.777777778;
            }

            return 0.0;
        }

        private void UpdateAzimuthFromTo(AzimuthTypes fromType, AzimuthTypes toType)
        {
            try
            {
                double angle = Azimuth;

                if (fromType == AzimuthTypes.Degrees && toType == AzimuthTypes.Mils)
                    angle *= 17.777777778;
                else if (fromType == AzimuthTypes.Mils && toType == AzimuthTypes.Degrees)
                    angle *= 0.05625;

                Azimuth = angle;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #endregion

        #region Mediator methods

        internal override void OnNewMapPointEvent(object obj)
        {
            if (!IsActiveTab)
                return;

            var point = obj as IPoint;

            if (point == null)
                return;

            // Bearing and Distance Mode
            if(LineFromType == LineFromTypes.BearingAndDistance)
            {
                Point1 = point;
                HasPoint1 = true;
                return;
            }

            base.OnNewMapPointEvent(obj);
        }

        internal override void CreateMapElement()
        {
            base.CreateMapElement();
            CreatePolyline();
            Reset(false);
        }

        internal override void OnMouseMoveEvent(object obj)
        {
            if (!IsActiveTab)
                return;

            var point = obj as IPoint;

            if (point == null)
                return;

            if (LineFromType == LineFromTypes.BearingAndDistance)
                return;

            if (HasPoint1 && !HasPoint2)
            {
                // update azimuth from feedback
                var polyline = GetPolylineFromFeedback(Point1, point);
                UpdateAzimuth(polyline);
            }

            base.OnMouseMoveEvent(obj);
        }

        #endregion

        private void UpdateFeedback()
        {
            // for now lets stick with only updating feedback here with Bearing and Distance case
            if (LineFromType != LineFromTypes.BearingAndDistance)
                return;

            if(Point1 != null && HasPoint1)
            {
                if(feedback == null)
                {
                    var mxdoc = ArcMap.Application.Document as IMxDocument;
                    CreateFeedback(Point1, mxdoc.FocusMap as IActiveView);
                    feedback.Start(Point1);
                }

                // now get second point from distance and bearing
                var construct = new Polyline() as IConstructGeodetic;
                if (construct == null)
                    return;

                construct.ConstructGeodeticLineFromDistance(GetEsriGeodeticType(), Point1, GetLinearUnit(), Distance, GetAzimuthAsDegrees(), esriCurveDensifyMethod.esriCurveDensifyByDeviation, -1.0);

                var line = construct as IPolyline;

                if (line.ToPoint != null)
                {
                    feedback.MoveTo(line.ToPoint);
                    Point2 = line.ToPoint;
                }
            }
        }

        private double GetAzimuthAsDegrees()
        {
            if(LineAzimuthType == AzimuthTypes.Mils)
            {
                return Azimuth * 0.05625;
            }

            return Azimuth;
        }

        internal override void Reset(bool toolReset)
        {
            base.Reset(toolReset);

            Azimuth = 0.0;
        }

    }
}
