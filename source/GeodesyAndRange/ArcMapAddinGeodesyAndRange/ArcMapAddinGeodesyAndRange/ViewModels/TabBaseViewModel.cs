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
using System.Windows.Controls;
using ESRI.ArcGIS.esriSystem;
using ArcMapAddinGeodesyAndRange.Helpers;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;

namespace ArcMapAddinGeodesyAndRange.ViewModels
{
    public class TabBaseViewModel : BaseViewModel
    {
        public TabBaseViewModel()
        {
            //properties
            LineType = LineTypes.Geodesic;
            LineDistanceType = DistanceTypes.Meters;

            //commands
            ClearGraphicsCommand = new RelayCommand(OnClearGraphics);
            ActivateToolCommand = new RelayCommand(OnActivateTool);

            // Mediator
            Mediator.Register(Constants.NEW_MAP_POINT, OnNewMapPointEvent);
            Mediator.Register(Constants.MOUSE_MOVE_POINT, OnMouseMoveEvent);
            Mediator.Register(Constants.TAB_ITEM_SELECTED, OnTabItemSelected);
        }

        #region Properties

        internal bool HasPoint1 = false;
        internal bool HasPoint2 = false;
        internal INewLineFeedback feedback = null;

        private IPoint point1 = null;
        public virtual IPoint Point1
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
        public virtual IPoint Point2
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


        private bool isActiveTab = false;
        public bool IsActiveTab
        {
            get
            {
                return isActiveTab;
            }
            set
            {
                isActiveTab = value;
                RaisePropertyChanged(() => IsActiveTab);
            }
        }

        DistanceTypes lineDistanceType = DistanceTypes.Meters;
        public DistanceTypes LineDistanceType
        {
            get { return lineDistanceType; }
            set
            {
                var before = lineDistanceType;
                lineDistanceType = value;
                UpdateDistanceFromTo(before, value);
            }
        }

        double distance = 0.0;
        public virtual double Distance
        {
            get { return distance; }
            set
            {
                distance = value;
                DistanceString = string.Format("{0:0.00}", distance);
                RaisePropertyChanged(() => Distance);
                RaisePropertyChanged(() => DistanceString);
            }
        }
        string distanceString = String.Empty;
        public virtual string DistanceString
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

        public LineTypes LineType { get; set; }

        #endregion Properties

        #region Commands

        public RelayCommand ClearGraphicsCommand { get; set; }
        public RelayCommand ActivateToolCommand { get; set; }
        
        #endregion

        internal virtual void CreateMapElement()
        {
            throw new NotImplementedException();
        }

        #region Private Event Functions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void OnClearGraphics(object obj)
        {
            //HasPoint1 = HasPoint2 = false;

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

        internal virtual void OnNewMapPointEvent(object obj)
        {
            if (!IsActiveTab)
                return;

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
                CreateFeedback(point, av);
                feedback.Start(point);
            }
            else if (!HasPoint2)
            {
                ResetFeedback();
                Point2 = point;
                HasPoint2 = true;
            }

            if (HasPoint1 && HasPoint2)
            {
                CreateMapElement();
                ResetPoints();
            }
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
        internal virtual void ResetPoints()
        {
            HasPoint1 = HasPoint2 = false;
        }

        /// <summary>
        /// 
        /// </summary>
        internal void ResetFeedback()
        {
            if (feedback == null)
                return;

            feedback.Stop();
            feedback = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void OnTabItemSelected(object obj)
        {
            if (obj == null)
                return;

            IsActiveTab = (obj == this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        internal void AddGraphicToMap(IGeometry geom)
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
        /// <returns></returns>
        internal ILinearUnit GetLinearUnit()
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
        /// <returns></returns>
        internal esriGeodeticType GetEsriGeodeticType()
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
        /// <param name="geometry"></param>
        internal void UpdateDistance(IGeometry geometry)
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

        internal virtual void OnMouseMoveEvent(object obj)
        {
            if (!IsActiveTab)
                return;

            var point = obj as IPoint;

            if (point == null)
                return;

            if (HasPoint1 && !HasPoint2)
            {
                feedback.MoveTo(point);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="av"></param>
        internal void CreateFeedback(IPoint point, IActiveView av)
        {
            feedback = new NewLineFeedback();
            var geoFeedback = feedback as IGeodeticLineFeedback;
            geoFeedback.GeodeticConstructionMethod = GetEsriGeodeticType();
            geoFeedback.UseGeodeticConstruction = true;
            geoFeedback.SpatialReference = point.SpatialReference;
            var displayFB = feedback as IDisplayFeedback;
            displayFB.Display = av.ScreenDisplay;
        }

        #endregion Private Functions

    }
}
