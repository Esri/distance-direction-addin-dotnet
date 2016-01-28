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
            ClearGraphicsCommand = new RelayCommand(OnClearGraphicsCommand);
            ActivateToolCommand = new RelayCommand(OnActivateToolCommand);
        }

        #region Commands
        public RelayCommand ClearGraphicsCommand { get; set; }
        public RelayCommand ActivateToolCommand { get; set; }
        #endregion

        #region Properties
        public CircleFromTypes CircleType { get; set; }
        public DistanceTypes LineDistanceType { get; set; }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void OnActivateToolCommand(object obj)
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
        /// <param name="activeView"></param>
        /// <param name="geom"></param>
        private void DrawCircle(ESRI.ArcGIS.Carto.IActiveView activeView, IGeometry geom)
        {            
            if (activeView == null || geom == null)
            {
                return;
            }

            double Px, Py, Radius;

            IGraphicsContainer graphicsContainer = activeView.GraphicsContainer;

            // set up the color
            IRgbColor rgbColor = new RgbColor();
            rgbColor.Red = 0;
            rgbColor.Green = 255;
            rgbColor.Blue = 0;

            IColor color = rgbColor;

            // make the line and define its color, style and width
            ISimpleLineSymbol LineSymbol = new SimpleLineSymbol();
            LineSymbol.Color = color;
            LineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
            LineSymbol.Width = 2;

            ISymbol symbol = LineSymbol as ISymbol;

            //create circle element
            ILineElement pCircleLineElement = new LineElementClass();
            pCircleLineElement.Symbol = LineSymbol;
            IElement pCircleElement = pCircleLineElement as IElement;

            // create point and radius
            Px = 0.0;
            Py = 0.0;
            Radius = 10.0;

            // Create circle geometry
            IPoint centerPoint = new Point();
            centerPoint.PutCoords(Px,Py);

            IConstructCircularArc pCircularArc = new CircularArcClass();

            pCircularArc.ConstructCircle(centerPoint, Radius, true);


            ISegment Seg = (ISegment)pCircularArc;
            ISegmentCollection SegCollection = new PolylineClass();
            SegCollection.AddSegment(Seg,null,null);

            pCircleElement.Geometry = SegCollection as IGeometry;

            //add the element to the map and draw it.
            graphicsContainer.AddElement(pCircleElement,0);

            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics,null,null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activeView"></param>
        /// <param name="geom"></param>
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
            ESRI.ArcGIS.Display.ISimpleLineSymbol simpleLineSymbol = new ESRI.ArcGIS.Display.SimpleLineSymbolClass();
            simpleLineSymbol.Color = color;

            ESRI.ArcGIS.Display.ISymbol symbol = (ESRI.ArcGIS.Display.ISymbol) simpleLineSymbol; // Explicit cast.

            screenDisplay.SetSymbol(symbol);
            screenDisplay.DrawPolyline(geom);
            screenDisplay.FinishDrawing();
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

        private void OnNewMapPoint(object obj)
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
