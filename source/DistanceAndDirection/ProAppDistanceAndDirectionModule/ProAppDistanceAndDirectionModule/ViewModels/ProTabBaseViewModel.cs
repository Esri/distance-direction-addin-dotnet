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
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using DistanceAndDirectionLibrary.Views;
using DistanceAndDirectionLibrary.ViewModels;
using DistanceAndDirectionLibrary.Helpers;
using DistanceAndDirectionLibrary.Models;
using DistanceAndDirectionLibrary;
using ProAppDistanceAndDirectionModule.Models;
using ProAppDistanceAndDirectionModule.Views;

namespace ProAppDistanceAndDirectionModule.ViewModels
{
    public class ProTabBaseViewModel : BaseViewModel
    {
        public const System.String MAP_TOOL_NAME = "ProAppDistanceAndDirectionModule_SketchTool";

        public ProTabBaseViewModel()
        {
            //properties
            LineType = LineTypes.Geodesic;
            LineDistanceType = DistanceTypes.Meters;

            //commands
            SaveAsCommand = new ArcGIS.Desktop.Framework.RelayCommand(() => OnSaveAs());
            ClearGraphicsCommand = new ArcGIS.Desktop.Framework.RelayCommand(() => OnClearGraphics());
            //ActivateToolCommand = new RelayCommand(OnActivateTool);
            EnterKeyCommand = new DistanceAndDirectionLibrary.Helpers.RelayCommand(OnEnterKeyCommand);
            EditPropertiesDialogCommand = new ArcGIS.Desktop.Framework.RelayCommand(() => OnEditPropertiesDialog());

            // Mediator
            Mediator.Register(DistanceAndDirectionLibrary.Constants.NEW_MAP_POINT, OnNewMapPointEvent);
            Mediator.Register(DistanceAndDirectionLibrary.Constants.MOUSE_MOVE_POINT, OnMouseMoveEvent);
            Mediator.Register(DistanceAndDirectionLibrary.Constants.TAB_ITEM_SELECTED, OnTabItemSelected);

            // Get Current tool
            CurrentTool = FrameworkApplication.CurrentTool;

            configObserver = new PropertyObserver<DistanceAndDirectionConfig>(DistanceAndDirectionConfig.AddInConfig)
            .RegisterHandler(n => n.DisplayCoordinateType, n =>
            {
                RaisePropertyChanged(() => Point1Formatted);
                RaisePropertyChanged(() => Point2Formatted);
            });

        }

        internal const int VertexCount = 99;

        PropertyObserver<DistanceAndDirectionConfig> configObserver;

        #region Commands

        public ArcGIS.Desktop.Framework.RelayCommand SaveAsCommand { get; set; }
        public ArcGIS.Desktop.Framework.RelayCommand ClearGraphicsCommand { get; set; }
        //public ArcGIS.Desktop.Framework.RelayCommand ActivateToolCommand { get; set; }
        public DistanceAndDirectionLibrary.Helpers.RelayCommand EnterKeyCommand { get; set; }
        public ArcGIS.Desktop.Framework.RelayCommand EditPropertiesDialogCommand { get; set; }

        /// <summary>
        /// Handler for opening the edit properties dialog
        /// </summary>
        /// <param name="obj"></param>
        private void OnEditPropertiesDialog()
        {
            var dlg = new EditPropertiesView();
            dlg.DataContext = new EditPropertiesViewModel();

            dlg.ShowDialog();
        }
        
        #endregion

        #region Properties

        private bool isActiveTab = false;
        /// <summary>
        /// Property to keep track of which tab/viewmodel is the active item
        /// </summary>
        public bool IsActiveTab
        {
            get
            {
                return isActiveTab;
            }
            set
            {
                Reset(true);
                isActiveTab = value;
                RaisePropertyChanged(() => IsActiveTab);
            }
        }

        public string CurrentTool
        {
            get; set;
        }

        public virtual bool IsToolActive
        {
            get
            {
                if (FrameworkApplication.CurrentTool != null)
                    return FrameworkApplication.CurrentTool == MAP_TOOL_NAME;

                return false;
            }

            set
            {
                if (value)
                {
                    CurrentTool = FrameworkApplication.CurrentTool;
                    FrameworkApplication.SetCurrentToolAsync(MAP_TOOL_NAME);
                }
                else
                {
                    if (FrameworkApplication.CurrentTool != null)
                    {
                        DeactivateTool(MAP_TOOL_NAME);
                    }
                }

                RaisePropertyChanged(() => IsToolActive);
            }
        }

        private List<IDisposable> overlayObjects = new List<IDisposable>();
        // lists to store GUIDs of graphics, temp feedback and map graphics
        private static List<Graphic> GraphicsList = new List<Graphic>();
        private FeatureClassUtils fcUtils = new FeatureClassUtils();
        private KMLUtils kmlUtils = new KMLUtils();

        internal bool HasPoint1 = false;
        internal bool HasPoint2 = false;

        public bool HasMapGraphics
        {
            get
            {
                if (this is ProLinesViewModel)
                {
                    return GraphicsList.Any(g => g.GraphicType == GraphicTypes.Line && g.IsTemp == false);
                }
                else if (this is ProCircleViewModel)
                {
                    return GraphicsList.Any(g => g.GraphicType == GraphicTypes.Circle && g.IsTemp == false);
                }
                else if (this is ProEllipseViewModel)
                {
                    return GraphicsList.Any(g => g.GraphicType == GraphicTypes.Ellipse && g.IsTemp == false);
                }
                else if (this is ProRangeViewModel)
                {
                    return GraphicsList.Any(g => g.GraphicType == GraphicTypes.RangeRing && g.IsTemp == false);
                }

                return false;
            }
        }

        private MapPoint point1 = null;
        /// <summary>
        /// Property for the first IPoint
        /// </summary>
        public virtual MapPoint Point1
        {
            get
            {
                return point1;
            }
            set
            {
                // do not add anything to the map from here
                point1 = value;
                RaisePropertyChanged(() => Point1);
                RaisePropertyChanged(() => Point1Formatted);
            }
        }

        private MapPoint point2 = null;
        /// <summary>
        /// Property for the second IPoint
        /// Not all tools need a second point
        /// </summary>
        public virtual MapPoint Point2
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
        string point1Formatted = string.Empty;
        /// <summary>
        /// String property for the first IPoint
        /// This is used to format the point for the UI and allow string input of different types of coordinates
        /// </summary>
        public string Point1Formatted
        {
            get
            {
                // return a formatted first point depending on how it was entered, manually or via map point tool
                if (string.IsNullOrWhiteSpace(point1Formatted))
                {
                    if (Point1 == null)
                        return string.Empty;

                    // only format if the Point1 data was generated from a mouse click

                    return GetFormattedPoint(Point1);
                }
                else
                {
                    // this was user inputed so just return the inputed string
                    return point1Formatted;
                }
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    point1Formatted = string.Empty;
                    RaisePropertyChanged(() => Point1Formatted);
                    return;
                }
                // try to convert string to an IPoint
                var point = GetMapPointFromString(value);
                if (point != null)
                {
                    // clear temp graphics
                    ClearTempGraphics();
                    point1Formatted = value;
                    HasPoint1 = true;
                    Point1 = point;

                    AddGraphicToMap(Point1, ColorFactory.GreenRGB, true, 5.0);

                    if (Point2 != null)
                    {
                        SetGeodesicDistance(Point1, Point2);
                    }
                }
                else
                {
                    // invalid coordinate, reset and throw exception
                    Point1 = null;
                    HasPoint1 = false;
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidCoordinate);
                }
            }
        }

        string point2Formatted = string.Empty;
        /// <summary>
        /// String property for the second IPoint
        /// This is used to format the point for the UI and allow string input of different types of coordinates
        /// Input types like GARS, MGRS, USNG, UTM
        /// </summary>
        public string Point2Formatted
        {
            get
            {
                // return a formatted second point depending on how it was entered, manually or via map point tool
                if (string.IsNullOrWhiteSpace(point2Formatted))
                {
                    if (Point2 == null)
                        return string.Empty;

                    // only format if the Point2 data was generated from a mouse click

                    return GetFormattedPoint(Point2);
                }
                else
                {
                    // this was user inputed so just return the inputed string
                    return point2Formatted;
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    point2Formatted = string.Empty;
                    RaisePropertyChanged(() => Point2Formatted);
                    return;
                }
                // try to convert string to a MapPoint
                var point = GetMapPointFromString(value);
                if (point != null)
                {
                    point2Formatted = value;
                    Point2 = point;
                    if (HasPoint1)
                    {
                        // lets try feedback
                        SetGeodesicDistance(Point1, Point2);
                    }       
                }
                else
                {
                    // invalid coordinate, reset and throw exception
                    Point2 = null;
                    HasPoint2 = false;
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidCoordinate);
                }
            }
        }

        DistanceTypes lineDistanceType = DistanceTypes.Meters;
        /// <summary>
        /// Property for the distance type
        /// </summary>
        public virtual DistanceTypes LineDistanceType
        {
            get { return lineDistanceType; }
            set
            {
                lineDistanceType = value;
                UpdateFeedback();
            }
        }

        internal virtual void UpdateFeedback()
        {

        }

        double distance = 0.0;
        /// <summary>
        /// Property for the distance/length
        /// </summary>
        public virtual double Distance
        {
            get { return distance; }
            set
            {
                if (value < 0.0)
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);

                distance = value;
                DistanceString = distance.ToString("G");
                RaisePropertyChanged(() => Distance);
                RaisePropertyChanged(() => DistanceString);
            }
        }
        string distanceString = String.Empty;
        /// <summary>
        /// Distance property as a string
        /// </summary>
        public virtual string DistanceString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(distanceString))
                    return Distance.ToString("G");
                else
                    return distanceString;
                
            }
            set
            {
                // lets avoid an infinite loop here
                if (string.Equals(distanceString, value))
                    return;

                distanceString = value;

                // update distance
                double d = 0.0;
                if (double.TryParse(distanceString, out d))
                {
                    if (Distance != d)
                    {
                        Distance = d;
                        UpdateFeedback();
                    }
                }
                else
                {
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }
            }
        }

        /// <summary>
        /// Property for the type of geodesy line
        /// </summary>
        public LineTypes LineType { get; set; }

        /// <summary>
        /// Property used to test if there is enough info to create a line map element
        /// </summary>
        public virtual bool CanCreateElement
        {
            get
            {
                return (Point1 != null && Point2 != null);
            }
        }

        #endregion

        internal async void AddGraphicToMap(Geometry geom, bool IsTempGraphic = false, double size = 1.0)
        {
            // default color Red
            await AddGraphicToMap(geom, ColorFactory.RedRGB, IsTempGraphic, size);
        }
        internal async Task AddGraphicToMap(Geometry geom, CIMColor color, bool IsTempGraphic = false, double size = 1.0)
        {
            if (geom == null || MapView.Active == null)
                return;

            CIMSymbolReference symbol = null;

            if(geom.GeometryType == GeometryType.Point)
            {
                await QueuedTask.Run(() =>
                    {
                        var s = SymbolFactory.ConstructPointSymbol(color, size, SimpleMarkerStyle.Circle);
                        symbol = new CIMSymbolReference() { Symbol = s };
                    });
            }
            else if(geom.GeometryType == GeometryType.Polyline)
            {
                await QueuedTask.Run(() =>
                    {
                        var s = SymbolFactory.ConstructLineSymbol(color, size);
                        symbol = new CIMSymbolReference() { Symbol = s };
                    });
            }
            else if(geom.GeometryType == GeometryType.Polygon)
            {
                await QueuedTask.Run(() =>
                {
                    var outline = SymbolFactory.ConstructStroke(ColorFactory.RedRGB, 1.0, SimpleLineStyle.Solid);
                    var s = SymbolFactory.ConstructPolygonSymbol(color, SimpleFillStyle.Null, outline);
                    symbol = new CIMSymbolReference() { Symbol = s };
                });
            }

            await QueuedTask.Run(() =>
                {
                    var disposable = MapView.Active.AddOverlay(geom, symbol);
                    overlayObjects.Add(disposable);

                    var gt = GetGraphicType();

                    GraphicsList.Add(new Graphic(gt, disposable, geom, this, IsTempGraphic));

                    RaisePropertyChanged(() => HasMapGraphics);
                });
        }

        private GraphicTypes GetGraphicType()
        {
            if (this is ProLinesViewModel)
                return GraphicTypes.Line;
            else if (this is ProCircleViewModel)
                return GraphicTypes.Circle;
            else if (this is ProEllipseViewModel)
                return GraphicTypes.Ellipse;
            else
                return GraphicTypes.RangeRing;
        }

        /// <summary>
        /// Method is called when a user pressed the "Enter" key or when a second point is created for a line from mouse clicks
        /// Derived class must override this method in order to create map elements
        /// Clears temp graphics by default
        /// </summary>
        internal virtual Geometry CreateMapElement()
        {
            ClearTempGraphics();
            return null;
        }

        #region Private Event Functions

        /// <summary>
        /// Clears all the graphics from the maps graphic container
        /// Inlucdes temp and map graphics
        /// Only removes temp and map graphics that were created by this add-in
        /// </summary>
        /// <param name="obj"></param>
        private void OnClearGraphics()
        {
            List<Graphic> removedGraphics = new List<Graphic>();

            if (MapView.Active == null)
                return;

            foreach (var item in GraphicsList)
            {
                Graphic graphic = item as Graphic;
                if (graphic != null && graphic.ViewModel == this)
                {
                    item.Disposable.Dispose();
                    removedGraphics.Add(graphic);
                }
                    
            }

            // clean up the GraphicsList and remove the necessary graphics from it
            foreach (Graphic graphic in removedGraphics)
            {
                GraphicsList.Remove(graphic);
            }
            //GraphicsList.Clear();

            RaisePropertyChanged(() => HasMapGraphics);
        }

        /// <summary>
        /// Method to clear all temp graphics
        /// </summary>
        internal void ClearTempGraphics()
        {
            var list = GraphicsList.Where(g => g.IsTemp == true).ToList();

            foreach (var item in list)
            {
                item.Disposable.Dispose();
                GraphicsList.Remove(item);
            }

            RaisePropertyChanged(() => HasMapGraphics);
        }

        /// <summary>
        /// Handler for the "Enter"key command
        /// Calls CreateMapElement
        /// </summary>
        /// <param name="obj"></param>
        internal virtual void OnEnterKeyCommand(object obj)
        {
            var depends = obj as System.Windows.DependencyObject;

            // check all children of dependency object for validation errors
            if (depends != null && !IsValid(depends))
                return;

            if (!CanCreateElement)
                return;

            var geom = CreateMapElement();

            if (geom != null)
            {
                ZoomToExtent(geom.Extent);
            }
        }

        private bool IsValid(System.Windows.DependencyObject obj)
        {
            // The dependency object is valid if it has no errors and all
            // of its children (that are dependency objects) are error-free.
            return !Validation.GetHasError(obj) &&
            System.Windows.LogicalTreeHelper.GetChildren(obj)
            .OfType<System.Windows.DependencyObject>()
            .All(IsValid);
        }

        /// <summary>
        /// Handler for the new map point click event
        /// </summary>
        /// <param name="obj">IPoint</param>
        internal virtual void OnNewMapPointEvent(object obj)
        {
            if (!IsActiveTab)
                return;

            var point = obj as MapPoint;

            if (point == null)
                return;

            if (!HasPoint1)
            {
                // clear temp graphics
                ClearTempGraphics();
                Point1 = point;
                HasPoint1 = true;
                Point1Formatted = string.Empty;

                AddGraphicToMap(Point1, ColorFactory.GreenRGB, true, 5.0);

                // lets try feedback
                //CreateFeedback(point, av);
                //feedback.Start(point);
            }
            else if (!HasPoint2)
            {
                ResetFeedback();
                Point2 = point;
                HasPoint2 = true;
                point2Formatted = string.Empty;
                RaisePropertyChanged(() => Point2Formatted);
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
        /// Method used to deactivate tool
        /// </summary>
        public void DeactivateTool(string toolname)
        {
            if (FrameworkApplication.CurrentTool != null &&
                FrameworkApplication.CurrentTool.Equals(toolname))
            {
                FrameworkApplication.SetCurrentToolAsync(CurrentTool);
            }
        }

        #endregion

        #region Private Functions

        private async Task ZoomToExtent(Envelope env)
        {
            if (env == null || MapView.Active == null || MapView.Active.Map == null)
                return;

            double extentPercent = (env.XMax - env.XMin) > (env.YMax - env.YMin) ? (env.XMax - env.XMin) * .3 : (env.YMax - env.YMin) * .3;
            double xmax = env.XMax + extentPercent;
            double xmin = env.XMin - extentPercent;
            double ymax = env.YMax + extentPercent;
            double ymin = env.YMin - extentPercent;

            //Create the envelope
            var envelope = await QueuedTask.Run(() => ArcGIS.Core.Geometry.EnvelopeBuilder.CreateEnvelope(xmin, ymin, xmax, ymax, MapView.Active.Map.SpatialReference));

            //Zoom the view to a given extent.
            await MapView.Active.ZoomToAsync(envelope, TimeSpan.FromSeconds(0.5));
        }

        /// <summary>
        /// Method will return a formatted point as a string based on the configuration settings for display coordinate type
        /// </summary>
        /// <param name="point">IPoint that is to be formatted</param>
        /// <returns>String that is formatted based on addin config display coordinate type</returns>
        private string GetFormattedPoint(MapPoint point)
        {
            if (point == null)
                return "NA";

            var result = string.Format("{0:0.0} {1:0.0}", point.Y, point.X);

            // .ToGeoCoordinate function calls will fail if there is no Spatial Reference
            if (point.SpatialReference == null)
                return result;

            ToGeoCoordinateParameter tgparam = null;
            
            try
            {
                switch (DistanceAndDirectionConfig.AddInConfig.DisplayCoordinateType)
                {
                    case CoordinateTypes.DD:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.DD);
                        tgparam.NumDigits = 6;
                        result = point.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateTypes.DDM:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.DDM);
                        tgparam.NumDigits = 4;
                        result = point.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateTypes.DMS:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.DMS);
                        tgparam.NumDigits = 2;
                        result = point.ToGeoCoordinateString(tgparam);
                        break;
                    //case CoordinateTypes.GARS:
                    //    tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.GARS);
                    //    result = point.ToGeoCoordinateString(tgparam);
                    //    break;
                    case CoordinateTypes.MGRS:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.MGRS);
                        result = point.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateTypes.USNG:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.USNG);
                        tgparam.NumDigits = 5;
                        result = point.ToGeoCoordinateString(tgparam);
                        break;
                    case CoordinateTypes.UTM:
                        tgparam = new ToGeoCoordinateParameter(GeoCoordinateType.UTM);
                        tgparam.GeoCoordMode = ToGeoCoordinateMode.UtmNorthSouth;
                        result = point.ToGeoCoordinateString(tgparam);
                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                // do nothing
            }
            return result;
        }
        /// <summary>
        /// Method used to totally reset the tool
        /// reset points, feedback
        /// clear out textboxes
        /// </summary>
        internal virtual void Reset(bool toolReset)
        {
            if (toolReset)
            {
                DeactivateTool(MAP_TOOL_NAME);
            }

            ResetPoints();
            Point1 = null;
            Point2 = null;
            Point1Formatted = string.Empty;
            Point2Formatted = string.Empty;

            ResetFeedback();

            Distance = 0.0;


            ClearTempGraphics();
        }
        /// <summary>
        /// Resets Points 1 and 2
        /// </summary>
        internal virtual void ResetPoints()
        {
            HasPoint1 = HasPoint2 = false;
        }

        /// <summary>
        /// Resets feedback aka cancels feedback
        /// </summary>
        internal void ResetFeedback()
        {
            //TODO Reset Feedback, future dev
        }

        /// <summary>
        /// Handler for the tab item selected event
        /// Helps keep track of which tab item/viewmodel is active
        /// </summary>
        /// <param name="obj">bool if selected or not</param>
        private void OnTabItemSelected(object obj)
        {
            if (obj == null)
                return;

            IsActiveTab = (obj == this);
        }

        internal double ConvertFromTo(DistanceTypes fromType, DistanceTypes toType, double input)
        {
            double result = 0.0;

            var linearUnitFrom = GetLinearUnit(fromType);
            var linearUnitTo = GetLinearUnit(toType);

            var unit = LinearUnit.CreateLinearUnit(linearUnitFrom.FactoryCode);

            result = unit.ConvertTo(input, linearUnitTo);

            return result;
        }

        /// <summary>
        /// Handler for the mouse move event
        /// When the mouse moves accross the map, IPoints are returned to aid in updating feedback to user
        /// </summary>
        /// <param name="obj">IPoint</param>
        internal virtual void OnMouseMoveEvent(object obj)
        {
            if (!IsActiveTab)
                return;

            var point = obj as MapPoint;

            if (point == null)
                return;

            // dynamically update start point if not set yet
            if (!HasPoint1)
            {
                Point1 = point;
            }
            else if (HasPoint1 && !HasPoint2)
            {
                Point2Formatted = string.Empty;
                Point2 = point;
                // get distance
                SetGeodesicDistance(Point1, point);
            }

            // update feedback
            if (HasPoint1 && !HasPoint2)
            {
                // future use
            }
        }

        internal double GetGeodesicDistance(MapPoint p1, MapPoint p2)
        {
            var meters = GeometryEngine.GeodesicDistance(p1, p2);
            // convert to current linear unit
            return ConvertFromTo(DistanceTypes.Meters, LineDistanceType, meters);
        }

        private void SetGeodesicDistance(MapPoint p1, MapPoint p2)
        {
            // convert to current linear unit
            Distance = GetGeodesicDistance(p1, p2);
        }

        internal async Task UpdateFeedbackWithGeoLine(LineSegment segment, CurveType type, LinearUnit lu)
        {
          
            if (Point1 == null || segment == null)
                return;

            var polyline = await QueuedTask.Run(() =>
            {
                return PolylineBuilder.CreatePolyline(segment);
            });

            ClearTempGraphics();
            Geometry newline = GeometryEngine.GeodeticDensifyByLength(polyline, 0, lu, type);
            await AddGraphicToMap(Point1, ColorFactory.GreenRGB, true, 5.0);
            await AddGraphicToMap(newline, ColorFactory.GreyRGB, true);
        }


        internal LinearUnit GetLinearUnit(DistanceTypes dtype)
        {
            LinearUnit result = LinearUnit.Meters;
            switch(dtype)
            {
                case DistanceTypes.Feet:
                    result = LinearUnit.Feet;
                    break;
                case DistanceTypes.Kilometers:
                    result = LinearUnit.Kilometers;
                    break;
                case DistanceTypes.Miles:
                    result = LinearUnit.Miles;
                    break;
                case DistanceTypes.NauticalMile:
                    result = LinearUnit.NauticalMiles;
                    break;
                case DistanceTypes.Yards:
                    result = LinearUnit.Yards;
                    break;
                case DistanceTypes.Meters:
                default:
                    result = LinearUnit.Meters;
                    break;
            }
            return result;
        }

        internal CurveType GetCurveType()
        {
            if (LineType == LineTypes.Geodesic)
                return CurveType.Geodesic;
            else if (LineType == LineTypes.GreatElliptic)
                return CurveType.GreatElliptic;
            else if (LineType == LineTypes.Loxodrome)
                return CurveType.Loxodrome;

            return CurveType.Geodesic;
        }

        /// <summary>
        /// Method used to convert a string to a known coordinate
        /// Assumes WGS84 for now
        /// </summary>
        /// <param name="coordinate">the coordinate as a string</param>
        /// <returns>MapPoint if successful, null if not</returns>
        internal MapPoint GetMapPointFromString(string coordinate)
        {
            MapPoint point = null;

            // future use if order of GetValues is not acceptable
            //var listOfTypes = new List<GeoCoordinateType>(new GeoCoordinateType[] {
            //    GeoCoordinateType.DD,
            //    GeoCoordinateType.DDM,
            //    GeoCoordinateType.DMS,
            //    GeoCoordinateType.GARS,
            //    GeoCoordinateType.GeoRef,
            //    GeoCoordinateType.MGRS,
            //    GeoCoordinateType.USNG,
            //    GeoCoordinateType.UTM
            //});

            var listOfTypes = Enum.GetValues(typeof(GeoCoordinateType)).Cast<GeoCoordinateType>();

            foreach(var type in listOfTypes)
            {
                try
                {
                    point = QueuedTask.Run(() =>
                    {
                        return MapPointBuilder.FromGeoCoordinateString(coordinate, MapView.Active.Map.SpatialReference, type, FromGeoCoordinateMode.Default);
                    }).Result;
                }
                catch (Exception ex)
                {
                    // do nothing
                }

                if (point != null)
                    return point;
            }

            try
            {
                point = QueuedTask.Run(() =>
                {
                    return MapPointBuilder.FromGeoCoordinateString(coordinate, MapView.Active.Map.SpatialReference, GeoCoordinateType.UTM, FromGeoCoordinateMode.UtmNorthSouth);
                }).Result;
            }
            catch(Exception ex)
            {
                // do nothing
            }

            if(point == null)
            {
                // lets support web mercator
                Regex regexMercator = new Regex(@"^(?<latitude>\-?\d+\.?\d*)[+,;:\s]*(?<longitude>\-?\d+\.?\d*)");

                var matchMercator = regexMercator.Match(coordinate);

                if (matchMercator.Success && matchMercator.Length == coordinate.Length)
                {
                    try
                    {
                        var Lat = Double.Parse(matchMercator.Groups["latitude"].Value);
                        var Lon = Double.Parse(matchMercator.Groups["longitude"].Value);
                        point = QueuedTask.Run(() =>
                        {
                            return MapPointBuilder.CreateMapPoint(Lon, Lat, SpatialReferences.WebMercator);
                        }).Result;
                    }
                    catch (Exception ex)
                    {
                        // do nothing
                    }
                }
            }

            return point;
        }
        /// <summary>
        /// Method to use when you need to move a feedback line to a point
        /// This forces a new point to be used, sometimes this method projects the point to a different spatial reference
        /// </summary>
        /// <param name="point"></param>
        //internal void FeedbackMoveTo(IPoint point)
        //{
        //    if (feedback == null || point == null)
        //        return;

        //    feedback.MoveTo(new Point() { X = point.X, Y = point.Y, SpatialReference = point.SpatialReference });
        //}

        /// <summary>
        /// Saves graphics to file gdb or shp file
        /// </summary>
        /// <param name="obj"></param>
        private async void OnSaveAs()
        {
            var dlg = new ProSaveAsFormatView();
            dlg.DataContext = new ProSaveAsFormatViewModel();
            var vm = dlg.DataContext as ProSaveAsFormatViewModel;
            GeomType geomType = GeomType.Polygon;

            if (dlg.ShowDialog() == true)
            {
                // Get the graphics list for the selected tab
                List<Graphic> typeGraphicsList = new List<Graphic>();
                if (this is ProLinesViewModel)
                {
                    typeGraphicsList = GraphicsList.Where(g => g.GraphicType == GraphicTypes.Line).ToList();
                    geomType = GeomType.PolyLine;
                }
                else if (this is ProCircleViewModel)
                {
                    typeGraphicsList = GraphicsList.Where(g => g.GraphicType == GraphicTypes.Circle).ToList();
                }
                else if (this is ProEllipseViewModel)
                {
                    typeGraphicsList = GraphicsList.Where(g => g.GraphicType == GraphicTypes.Ellipse).ToList();
                }
                else if (this is ProRangeViewModel)
                {
                    typeGraphicsList = GraphicsList.Where(g => g.GraphicType == GraphicTypes.RangeRing).ToList();
                    geomType = GeomType.PolyLine;
                }

                string path = fcUtils.PromptUserWithSaveDialog(vm.FeatureIsChecked, vm.ShapeIsChecked, vm.KmlIsChecked);
                if (path != null)
                {
                    try
                    {
                        string folderName = System.IO.Path.GetDirectoryName(path);

                        if (vm.FeatureIsChecked)
                        {
                            await fcUtils.CreateFCOutput(path, SaveAsType.FileGDB, typeGraphicsList, MapView.Active.Map.SpatialReference, MapView.Active, geomType);
                        }
                        else if (vm.ShapeIsChecked || vm.KmlIsChecked)
                        {
                            await fcUtils.CreateFCOutput(path, SaveAsType.Shapefile, typeGraphicsList, MapView.Active.Map.SpatialReference, MapView.Active, geomType, vm.KmlIsChecked);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        #endregion Private Functions

    }
}
