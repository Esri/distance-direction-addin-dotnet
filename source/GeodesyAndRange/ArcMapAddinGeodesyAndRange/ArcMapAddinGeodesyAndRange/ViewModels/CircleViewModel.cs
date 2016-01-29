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
    public class CircleViewModel : BaseViewModel
    {
        /// <summary>
        /// CTOR
        /// </summary>
        public CircleViewModel()
        {
            //properties
            CircleType = CircleFromTypes.Radius;
            LineDistanceType = DistanceTypes.Meters;

            //commands
            ClearGraphicsCommand = new RelayCommand(OnClearGraphics);
            ActivateToolCommand = new RelayCommand(OnActivateTool);

            // lets listen for new points from the map point tool
            Mediator.Register(Constants.NEW_MAP_POINT, OnNewCenterPoint);
            Mediator.Register(Constants.MOUSE_MOVE_POINT, OnMouseMoveEvent);
        }

        #region Commands
        public RelayCommand ClearGraphicsCommand { get; set; }
        public RelayCommand ActivateToolCommand { get; set; }
        #endregion

        #region Properties
        public CircleFromTypes CircleType { get; set; }
        public LineTypes LineType { get; set; }

        DistanceTypes lineDistanceType = DistanceTypes.Meters;
        public DistanceTypes LineDistanceType
        {
            get { return lineDistanceType; }
            set
            {
                UpdateDistanceFromTo(lineDistanceType, value);
                lineDistanceType = value;
            }
        }

        private IPoint startPoint = null;
        /// <summary>
        /// 
        /// </summary>
        public IPoint StartPoint
        {
            get
            {
                return startPoint;
            }
            set
            {
                startPoint = value;
                RaisePropertyChanged(() => StartPoint);
                RaisePropertyChanged(() => StartPointFormatted);
            }
        }

        private IPoint endPoint = null;
        /// <summary>
        /// 
        /// </summary>
        public IPoint EndPoint
        {
            get
            {
                return endPoint;
            }
            set
            {
                endPoint = value;
                RaisePropertyChanged(() => EndPoint);
                RaisePropertyChanged(() => EndPointFormatted);
            }
        }

        double distance = 0.0;
        public double Distance
        {
            get { return distance; }
            set
            {
                distance = value;
                DistanceString = string.Format("#,##0.00", distance);
                RaisePropertyChanged(() => Distance);
                RaisePropertyChanged(() => DistanceString);
            }
        }
        string distanceString = String.Empty;
        public string DistanceString { 
            get 
            {
                return string.Format("#,##0.00", Distance);
            }
            set
            {
                distanceString = value;
            } 
        }

        /// <summary>
        /// 
        /// </summary>
        public string StartPointFormatted
        {
            get { return string.Format("{0:0.0#####} {1:0.0#####}", StartPoint.X, StartPoint.Y); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string EndPointFormatted
        {
            get { return string.Format("{0:0.0#####} {1:0.0#####}", EndPoint.X, EndPoint.Y); }
        }
        #endregion

        #region Private Event Functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void OnClearGraphics(object obj)
        {
            HasPoint1 = HasPoint2 = false;

            var mxdoc = ArcMap.Application.Document as IMxDocument;
            if (mxdoc == null)
                return;
            var av = mxdoc.FocusMap as IActiveView;
            if (av == null)
                return;
            var gc = av as IGraphicsContainer;
            if (gc == null)
                return;

            gc.DeleteAllElements();
            av.Refresh();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void OnActivateTool(object obj)
        {
            SetToolActiveInToolBar(ArcMap.Application, "Esri_ArcMapAddinGeodesyAndRange_MapPointTool");
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="application"></param>
        /// <param name="toolName"></param>
        public void SetToolActiveInToolBar(ESRI.ArcGIS.Framework.IApplication application, System.String toolName)
        {
            ESRI.ArcGIS.Framework.ICommandBars commandBars = application.Document.CommandBars;
            ESRI.ArcGIS.esriSystem.UID commandID = new ESRI.ArcGIS.esriSystem.UIDClass();
            commandID.Value = toolName;
            ESRI.ArcGIS.Framework.ICommandItem commandItem = commandBars.Find(commandID, false, false);

            if (commandItem != null)
                application.CurrentTool = commandItem;
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        private void AddGraphicToMap(IGeometry geom)
        {
            var pc = geom as IPolycurve;

            if (pc != null)
            {
                // create graphic then add to map
                var le = new LineElementClass() as ILineElement;
                var element = le as IElement;
                element.Geometry = geom;
                ESRI.ArcGIS.Display.IRgbColor rgbColor = new ESRI.ArcGIS.Display.RgbColorClass();
                rgbColor.Red = 255;

                ESRI.ArcGIS.Display.IColor color = rgbColor; // Implicit cast.
                var lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Color = color;
                lineSymbol.Width = 2;
                var mxdoc = ArcMap.Application.Document as IMxDocument;
                var av = mxdoc.FocusMap as IActiveView;
                var gc = av as IGraphicsContainer;
                gc.AddElement(element, 0);

                av.Refresh();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateCircle()
        {
            if (StartPoint == null || EndPoint == null)
            {
                return;
            }

            //var srf3 = new ESRI.ArcGIS.Geometry.SpatialReferenceEnvironment() as ISpatialReferenceFactory3;
            //var linearUnit = srf3.CreateUnit((int)esriSRUnitType.esriSRUnit_Meter) as ILinearUnit;
            //var constructLine = new Polyline() as IConstructGeodetic;
            //constructLine.ConstructGeodeticLineFromPoints(GetEsriGeodeticType(), StartPoint, EndPoint, linearUnit, esriCurveDensifyMethod.esriCurveDensifyByDeviation, 1000.0);
            //var newPolyLine = constructLine as IPolycurve;
            //if (CircleType == CircleFromTypes.Diameter)
            //{
            //    var centerPoint = new Point() as IPoint;
            //    newPolyLine.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, centerPoint);
            //    newPolyLine.FromPoint = StartPoint = centerPoint;
            //}
            //UpdateDistance(newPolyLine as IGeometry);

            var polyLine = new Polyline() as IPolyline;
            polyLine.SpatialReference = StartPoint.SpatialReference;
            var ptCol = polyLine as IPointCollection;
            ptCol.AddPoint(StartPoint);
            ptCol.AddPoint(EndPoint);
            if (CircleType == CircleFromTypes.Diameter)
            {
                var centerPoint = new Point() as IPoint;
                polyLine.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, centerPoint);
                polyLine.FromPoint = StartPoint = centerPoint;
            }
            UpdateDistance(polyLine as IGeometry);

            var construct = new Polyline() as IConstructGeodetic;
            construct.ConstructGeodesicCircle(StartPoint, GetLinearUnit(), Distance, esriCurveDensifyMethod.esriCurveDensifyByDeviation, 1000.0);

            var mxdoc = ArcMap.Application.Document as IMxDocument;
            var av = mxdoc.FocusMap as IActiveView;

            AddGraphicToMap(construct as IGeometry);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ILinearUnit GetLinearUnit()
        {
            int unitType = (int)esriSRUnitType.esriSRUnit_Meter;
            var srf3 = new ESRI.ArcGIS.Geometry.SpatialReferenceEnvironment() as ISpatialReferenceFactory3;

            switch (LineDistanceType)
            {
                case DistanceTypes.Feet:
                    unitType = (int)esriSRUnitType.esriSRUnit_Foot;
                    break;
                case DistanceTypes.Kilometers:
                    unitType = (int)esriSRUnitType.esriSRUnit_Kilometer;
                    break;
                case DistanceTypes.Meters:
                    unitType = (int)esriSRUnitType.esriSRUnit_Meter;
                    break;
                default:
                    unitType = (int)esriSRUnitType.esriSRUnit_Meter;
                    break;
            }

            return srf3.CreateUnit(unitType) as ILinearUnit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        private void UpdateDistance(IGeometry geometry)
        {
            var polyline = geometry as IPolyline;

            if (polyline == null)
                return;

            var polycurvegeo = geometry as IPolycurveGeodetic;

            var geodeticType = GetEsriGeodeticType();
            var linearUnit = GetLinearUnit();
            var geodeticLength = polycurvegeo.get_LengthGeodetic(geodeticType, linearUnit);

            Distance = geodeticLength;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private esriGeodeticType GetEsriGeodeticType()
        {
            esriGeodeticType type = esriGeodeticType.esriGeodeticTypeGeodesic;

            switch (LineType)
            {
                case LineTypes.Geodesic:
                    type = esriGeodeticType.esriGeodeticTypeGeodesic;
                    break;
                case LineTypes.GreatElliptic:
                    type = esriGeodeticType.esriGeodeticTypeGreatElliptic;
                    break;
                case LineTypes.Loxodrome:
                    type = esriGeodeticType.esriGeodeticTypeLoxodrome;
                    break;
                default:
                    type = esriGeodeticType.esriGeodeticTypeGeodesic;
                    break;
            }

            return type;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        private void UpdateDistanceFromTo(DistanceTypes fromType, DistanceTypes toType)
        {
            try
            {
                double length = Distance;

                if (fromType == DistanceTypes.Meters && toType == DistanceTypes.Kilometers)
                    length /= 1000.0;
                else if (fromType == DistanceTypes.Meters && toType == DistanceTypes.Feet)
                    length *= 3.28084;
                else if (fromType == DistanceTypes.Kilometers && toType == DistanceTypes.Meters)
                    length *= 1000.0;
                else if (fromType == DistanceTypes.Kilometers && toType == DistanceTypes.Feet)
                    length *= 3280.84;
                else if (fromType == DistanceTypes.Feet && toType == DistanceTypes.Kilometers)
                    length *= 0.0003048;
                else if (fromType == DistanceTypes.Feet && toType == DistanceTypes.Meters)
                    length *= 0.3048;

                Distance = length;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreatePolyline()
        {
            try
            {
                var construct = new Polyline() as IConstructGeodetic;

                if (construct == null)
                    return;

                var srf3 = new ESRI.ArcGIS.Geometry.SpatialReferenceEnvironment() as ISpatialReferenceFactory3;
                var linearUnit = srf3.CreateUnit((int)esriSRUnitType.esriSRUnit_Meter) as ILinearUnit;

                construct.ConstructGeodeticLineFromPoints(GetEsriGeodeticType(), StartPoint, EndPoint, linearUnit, esriCurveDensifyMethod.esriCurveDensifyByDeviation, 1000.0);

                var mxdoc = ArcMap.Application.Document as IMxDocument;
                var av = mxdoc.FocusMap as IActiveView;

                AddGraphicToMap(construct as IGeometry);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        #endregion

        #region Mediator methods

        bool HasPoint1 = false;
        bool HasPoint2 = false;
        INewLineFeedback feedback = null;
        IGeodeticLineFeedback geoFeedback = null;

        private void OnNewCenterPoint(object obj)
        {
            var mxdoc = ArcMap.Application.Document as IMxDocument;
            var av = mxdoc.FocusMap as IActiveView;
            var point = obj as IPoint;

            if (point == null)
                return;

            if (!HasPoint1)
            {
                StartPoint = point;
                HasPoint1 = true;

                // lets try feedback
                feedback = new NewLineFeedback();
                geoFeedback = feedback as IGeodeticLineFeedback;
                geoFeedback.GeodeticConstructionMethod = GetEsriGeodeticType();
                geoFeedback.UseGeodeticConstruction = true;
                geoFeedback.SpatialReference = point.SpatialReference;
                var displayFB = feedback as IDisplayFeedback;
                displayFB.Display = av.ScreenDisplay;
                feedback.Start(point);
            }
            else if(!HasPoint2)
            {
                feedback.Stop();
                EndPoint = point;
                HasPoint2 = true;
            }

            if(HasPoint1 && HasPoint2)
            {
                CreateCircle();
                HasPoint1 = HasPoint2 = false;
            }
        }

        private void OnMouseMoveEvent(object obj)
        {
            var point = obj as IPoint;

            if (point == null)
                return;

            if(HasPoint1 && !HasPoint2)
            {
                feedback.MoveTo(point);
            }
        }

        #endregion
    }
}
