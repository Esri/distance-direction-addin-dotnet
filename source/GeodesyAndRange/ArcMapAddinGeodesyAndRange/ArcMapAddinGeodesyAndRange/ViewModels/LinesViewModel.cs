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
    public class LinesViewModel : BaseViewModel
    {
        public LinesViewModel()
        {
            // props
            LineType = LineTypes.Geodesic;
            LineFromType = LineFromTypes.Points;
            LineDistanceType = DistanceTypes.Meters;
            LineAzimuthType = AzimuthTypes.Degrees;

            // commands

            ClearGraphicsCommand = new RelayCommand(OnClearGraphicsCommand);
            ActivateToolCommand = new RelayCommand(OnActivateToolCommand);

            // lets listen for new points from the map point tool
            Mediator.Register(Constants.NEW_MAP_POINT, OnNewMapPoint);
            Mediator.Register(Constants.MOUSE_MOVE_POINT, OnMouseMovePoint);
        }

        #region Properties

        public LineTypes LineType { get; set; }
        public LineFromTypes LineFromType { get; set; }
        public DistanceTypes LineDistanceType { get; set; }
        public AzimuthTypes LineAzimuthType { get; set; }

        private IPoint point1 = null;
        public IPoint Point1
        {
            get
            {
                return point1;
            }
            set
            {
                point1 = value;
                RaisePropertyChanged(() => Point1);
                RaisePropertyChanged(() => Point1Formatted);
            }
        }
        private IPoint point2 = null;
        public IPoint Point2
        {
            get
            {
                return point2;
            }
            set
            {
                point2 = value;
                RaisePropertyChanged(() => Point2);
                RaisePropertyChanged(() => Point2Formatted);
            }
        }

        public string Point1Formatted
        {
            get { return string.Format("{0:0.0#####} {1:0.0#####}", Point1.X, Point1.Y); }
        }

        public string Point2Formatted
        {
            get { return string.Format("{0:0.0#####} {1:0.0#####}", Point2.X, Point2.Y); }
        }

        public string DistanceString { get; set; }
        public string AzimuthString { get; set; }

        #endregion

        #region Commands

        public RelayCommand ClearGraphicsCommand { get; set; }
        public RelayCommand ActivateToolCommand { get; set; }

        private void OnClearGraphicsCommand(object obj)
        {
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

        private void OnActivateToolCommand(object obj)
        {
            SetToolActiveInToolBar(ArcMap.Application, "Esri_ArcMapAddinGeodesyAndRange_MapPointTool");
        }

        public void SetToolActiveInToolBar(ESRI.ArcGIS.Framework.IApplication application, System.String toolName)
        {
            ESRI.ArcGIS.Framework.ICommandBars commandBars = application.Document.CommandBars;
            ESRI.ArcGIS.esriSystem.UID commandID = new ESRI.ArcGIS.esriSystem.UIDClass();
            commandID.Value = toolName; 
            ESRI.ArcGIS.Framework.ICommandItem commandItem = commandBars.Find(commandID, false, false);

            if (commandItem != null)
                application.CurrentTool = commandItem;
        }

        private void CreatePolyline()
        {
            try
            {
                var construct = new Polyline() as IConstructGeodetic;

                if (construct == null)
                    return;

                var srf3 = new ESRI.ArcGIS.Geometry.SpatialReferenceEnvironment() as ISpatialReferenceFactory3;
                var linearUnit = srf3.CreateUnit((int)esriSRUnitType.esriSRUnit_Meter) as ILinearUnit;

                construct.ConstructGeodeticLineFromPoints(GetEsriGeodeticType(), Point1, Point2, linearUnit, esriCurveDensifyMethod.esriCurveDensifyByDeviation, 1000.0);

                var mxdoc = ArcMap.Application.Document as IMxDocument;
                var av = mxdoc.FocusMap as IActiveView;

                UpdateDistance(construct as IGeometry);

                AddGraphicToMap(construct as IGeometry);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void UpdateDistance(IGeometry geometry)
        {
            var polyline = geometry as IPolyline;

            if (polyline == null)
                return;

            var polycurvegeo = geometry as IPolycurveGeodetic;

            var geodeticLength = polycurvegeo.get_LengthGeodetic(GetEsriGeodeticType(), GetLinearUnit());

            DistanceString = string.Format("{0:0.00}", geodeticLength);
            RaisePropertyChanged(() => DistanceString);
        }

        private ILinearUnit GetLinearUnit()
        {
            int unitType = (int)esriSRUnitType.esriSRUnit_Meter;
            var srf3 = new ESRI.ArcGIS.Geometry.SpatialReferenceEnvironment() as ISpatialReferenceFactory3;

            switch(LineDistanceType)
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

        private esriGeodeticType GetEsriGeodeticType()
        {
            esriGeodeticType type = esriGeodeticType.esriGeodeticTypeGeodesic;

            switch(LineType)
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

        private void AddGraphicToMap(IGeometry geom)
        {
            var pc = geom as IPolycurve;

            if(pc !=  null)
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

        private void DrawPolyline(ESRI.ArcGIS.Carto.IActiveView activeView, IGeometry geom)
        {
            if (activeView == null || geom == null)
            {
                return;
            }

            ESRI.ArcGIS.Display.IScreenDisplay screenDisplay = activeView.ScreenDisplay;

            // Constant.
            screenDisplay.StartDrawing(screenDisplay.hDC, (System.Int16)
                ESRI.ArcGIS.Display.esriScreenCache.esriNoScreenCache); // Explicit cast.
            ESRI.ArcGIS.Display.IRgbColor rgbColor = new ESRI.ArcGIS.Display.RgbColorClass();
            rgbColor.Red = 255;

            ESRI.ArcGIS.Display.IColor color = rgbColor; // Implicit cast.
            ESRI.ArcGIS.Display.ISimpleLineSymbol simpleLineSymbol = new
                ESRI.ArcGIS.Display.SimpleLineSymbolClass();
            simpleLineSymbol.Color = color;

            ESRI.ArcGIS.Display.ISymbol symbol = (ESRI.ArcGIS.Display.ISymbol)
                simpleLineSymbol; // Explicit cast.

            screenDisplay.SetSymbol(symbol);
            screenDisplay.DrawPolyline(geom);
            screenDisplay.FinishDrawing();
        }
        
        #endregion

        #region Mediator methods

        bool HasPoint1 = false;
        bool HasPoint2 = false;
        INewLineFeedback feedback = null;
        IGeodeticLineFeedback geoFeedback = null;

        private void OnNewMapPoint(object obj)
        {
            var mxdoc = ArcMap.Application.Document as IMxDocument;
            var av = mxdoc.FocusMap as IActiveView;
            var point = obj as IPoint;

            if (point == null)
                return;

            if (!HasPoint1)
            {
                Point1 = point;
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
                Point2 = point;
                HasPoint2 = true;
            }

            if(HasPoint1 && HasPoint2)
            {
                CreatePolyline();
                HasPoint1 = HasPoint2 = false;
            }
        }

        private void OnMouseMovePoint(object obj)
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
