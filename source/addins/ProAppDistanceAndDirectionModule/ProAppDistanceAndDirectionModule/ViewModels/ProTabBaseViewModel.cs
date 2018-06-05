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

using ArcGIS.Core.Data;
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
using ProAppDistanceAndDirectionModule.ViewModels;

namespace ProAppDistanceAndDirectionModule.ViewModels
{
    public class ProTabBaseViewModel : BaseViewModel
    {
        public string MAP_TOOL_NAME = SketchTool.ToolId;

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
            Mediator.Register(DistanceAndDirectionLibrary.Constants.KEYPRESS_ESCAPE, OnKeypressEscape);
            Mediator.Register(DistanceAndDirectionLibrary.Constants.POINT_TEXT_KEYDOWN, OnPointTextBoxKeyDown);
            Mediator.Register(DistanceAndDirectionLibrary.Constants.RADIUS_DIAMETER_KEYDOWN, OnRadiusDiameterTextBoxKeyDown);

            // Pro Events
            // Note: will fail if called from Unit Tests, so catch exception for this case
            try
            {
                ArcGIS.Desktop.Framework.Events.ActiveToolChangedEvent.Subscribe(OnActiveToolChanged);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine("Probably Running from Unit Tests");
            }

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
        private void OnEditPropertiesDialog()
        {
            var dlg = new ProEditPropertiesView();
            dlg.DataContext = new EditPropertiesViewModel();

            dlg.ShowDialog();
        }
        
        #endregion

        #region Properties
        /// <summary>
        /// Property to keep track of manual entry for radius or diameter value
        /// </summary>
        public bool isManualRadiusDiameterEntered { get; set; }

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

        /// <summary>
        /// save last active tool used, so we can set back to this 
        /// </summary>
        private string lastActiveToolName;

        private bool isToolActive = false;
        public virtual bool IsToolActive
        {
            get
            {
                return isToolActive;
            }

            set
            {
                isToolActive = value;
                if (isToolActive)
                {
                    FrameworkApplication.SetCurrentToolAsync(MAP_TOOL_NAME);
                }
                else
                {
                    DeactivateTool(MAP_TOOL_NAME);
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
        internal bool HasPoint3 = false;

        public bool HasMapGraphics
        {
            get
            {
                // Call helper method (must be run on MCT)
                bool hasFeatures = QueuedTask.Run<bool>(async() =>
                {
                    return await this.HasLayerFeatures();
                }).Result;

                return hasFeatures;
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
                    if (Point1 != null)
                    {
                        // only format if the Point1 data was generated from a mouse click
                        string outFormattedString = string.Empty;
                        CoordinateConversionLibrary.Models.CoordinateType ccType = CoordinateConversionLibrary.Helpers.ConversionUtils.GetCoordinateString(GetFormattedPoint(Point1), out outFormattedString);
                        return outFormattedString;
                    }
                    return string.Empty;
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
                    if (!IsToolActive)
                        point1 = null; // reset the point if the user erased (TRICKY: tool sets to "" on click)

                    point1Formatted = string.Empty;
                    RaisePropertyChanged(() => Point1Formatted);
                    return;
                }
                // try to convert string to an IPoint                
                string outFormattedString = string.Empty;
                CoordinateConversionLibrary.Models.CoordinateType ccType = CoordinateConversionLibrary.Helpers.ConversionUtils.GetCoordinateString(value, out outFormattedString);
                MapPoint point = (ccType != CoordinateConversionLibrary.Models.CoordinateType.Unknown) ? GetMapPointFromString(outFormattedString) : null;
                if (point != null)
                {
                    // clear temp graphics
                    ClearTempGraphics();
                    point1Formatted = value;
                    HasPoint1 = true;
                    Point1 = point;

                    AddGraphicToMap(Point1, ColorFactory.Instance.GreenRGB, null, true, 5.0);

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
                    if (Point2 != null)
                    {
                        // only format if the Point1 data was generated from a mouse click
                        string outFormattedString = string.Empty;
                        CoordinateConversionLibrary.Models.CoordinateType ccType = CoordinateConversionLibrary.Helpers.ConversionUtils.GetCoordinateString(GetFormattedPoint(Point2), out outFormattedString);
                        return outFormattedString;
                    }
                    return string.Empty;
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
                    if (!IsToolActive) 
                        point2 = null; // reset the point if the user erased (TRICKY: tool sets to "" on click)

                    point2Formatted = string.Empty;
                    RaisePropertyChanged(() => Point2Formatted);
                    return;
                }

                // Point1Formatted should never equal to Point2Formatted
                if (Point1Formatted.ToLower().Trim().Equals(value.ToLower().Trim()))
                {
                    Point2 = null;
                    HasPoint2 = false;
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.EndPointAndStartPointSameError);
                }

                // try to convert string to a MapPoint
                string outFormattedString = string.Empty;
                CoordinateConversionLibrary.Models.CoordinateType ccType = CoordinateConversionLibrary.Helpers.ConversionUtils.GetCoordinateString(value, out outFormattedString);
                MapPoint point = (ccType != CoordinateConversionLibrary.Models.CoordinateType.Unknown) ? GetMapPointFromString(outFormattedString) : null;
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

                // Prevents a stack overflow if the point is past -180 latitude, for example
                if (double.IsNaN(value))
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                        DistanceAndDirectionLibrary.Properties.Resources.MsgOutOfAOI,
                        DistanceAndDirectionLibrary.Properties.Resources.MsgOutOfAOI,
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);

                    // Reset the points/distance or MessageBox may pop-up indefinitely
                    Reset(false);

                    return;
                }

                distance = value;
                DistanceString = distance.ToString("G");
                RaisePropertyChanged(() => Distance);
                RaisePropertyChanged(() => DistanceString);
            }
        }

        protected string distanceString = String.Empty;
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
                    if (d == 0.0)
                    {
                        // Don't set property(Distance) because this will overwrite the string if user entering zeros "00000"
                        distance = 0.0;
                        return;
                    }

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
        public virtual LineTypes LineType { get; set; }

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

        private void OnActiveToolChanged(ArcGIS.Desktop.Framework.Events.ToolEventArgs args)
        {
            string currentActiveToolName = args.CurrentID;

            if (currentActiveToolName != MAP_TOOL_NAME)
            {
                lastActiveToolName = currentActiveToolName;
                IsToolActive = false;
            }
        }

        internal async void AddGraphicToMap(Geometry geom, ProGraphicAttributes p = null, bool IsTempGraphic = false, double size = 1.0)
        {
            // default color Red
            await AddGraphicToMapAsync(geom, ColorFactory.Instance.RedRGB, p, IsTempGraphic, size);
        }

        internal void AddGraphicToMap(Geometry geom, CIMColor color, ProGraphicAttributes p = null, bool IsTempGraphic = false, double size = 1.0)
        {
            QueuedTask.Run(async () =>
            {
                await AddGraphicToMapAsync(geom, color, p, IsTempGraphic, size);
            }); 
        }

        internal async Task AddGraphicToMapAsync(Geometry geom, CIMColor color, ProGraphicAttributes p = null, bool IsTempGraphic = false, double size = 1.0)
        {
            if (geom == null || MapView.Active == null)
                return;

            CIMSymbolReference symbol = null;

            if(geom.GeometryType == GeometryType.Point)
            {
                await QueuedTask.Run(() =>
                    {
                        var s = SymbolFactory.Instance.ConstructPointSymbol(color, size, SimpleMarkerStyle.Circle);
                        symbol = new CIMSymbolReference() { Symbol = s };
                    });
            }
            else if(geom.GeometryType == GeometryType.Polyline)
            {
                await QueuedTask.Run(() =>
                    {
                        var s = SymbolFactory.Instance.ConstructLineSymbol(color, size);
                        symbol = new CIMSymbolReference() { Symbol = s };
                    });
            }
            else if(geom.GeometryType == GeometryType.Polygon)
            {
                await QueuedTask.Run(() =>
                {
                    var outline = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.RedRGB, 1.0, SimpleLineStyle.Solid);
                    var s = SymbolFactory.Instance.ConstructPolygonSymbol(color, SimpleFillStyle.Null, outline);
                    symbol = new CIMSymbolReference() { Symbol = s };
                });
            }

            await QueuedTask.Run(() =>
                {
                    var disposable = MapView.Active.AddOverlay(geom, symbol);
                    overlayObjects.Add(disposable);

                    var gt = GetGraphicType();

                    GraphicsList.Add(new Graphic(gt, disposable, geom, this, p, IsTempGraphic));
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
        private void OnClearGraphics()
        {
            QueuedTask.Run<bool>(() =>
            {
                return this.DeleteAllFeatures();
            });

            RaisePropertyChanged(() => HasMapGraphics);
        }

        /// <summary>
        /// Method to clear all temp graphics
        /// </summary>
        internal void ClearTempGraphics()
        {
            // Locking GraphicsList while removing objects
            lock (GraphicsList)
            {
                var list = GraphicsList.Where(g => g.IsTemp == true).ToList();

                foreach (var item in list)
                {
                    item.Disposable.Dispose();
                    GraphicsList.Remove(item);
                }
            }
        }

        /// <summary>
        /// Handler for the "Enter"key command
        /// Calls CreateMapElement
        /// </summary>
        /// <param name="obj"></param>
        internal virtual async void OnEnterKeyCommand(object obj)
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
                await ZoomToExtent(geom.Extent);
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

                AddGraphicToMap(Point1, ColorFactory.Instance.GreenRGB, null, true, 5.0);

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
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    FrameworkApplication.SetCurrentToolAsync(lastActiveToolName);
                });
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
            
            try
            {
                ToGeoCoordinateParameter tgparam = null;

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
                System.Diagnostics.Debug.WriteLine(ex.Message);
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

        /// <summary>
        /// Handler for the escape key press event
        /// Helps cancel operation when escape key is pressed
        /// </summary>
        /// <param name="obj">always null</param>
        private void OnKeypressEscape(object obj)
        {
            if (isActiveTab)
            {
                if (FrameworkApplication.CurrentTool != null)
                {
                    // User has activated the Map Point tool but not created a point
                    // Or User has previously finished creating a graphic
                    // Either way, assume they want to disable the Map Point tool
                    if ((IsToolActive && !HasPoint1) || (IsToolActive && HasPoint3))
                    {
                        Reset(true);
                        IsToolActive = false;
                        return;
                    }

                    // User has activated Map Point tool and created a point but not completed the graphic
                    // Assume they want to cancel any graphic creation in progress 
                    // but still keep the Map Point tool active
                    if (IsToolActive && HasPoint1)
                    {
                        Reset(false);
                        return;
                    }
                }

                //Clear manual flag for radius/diameter property
                isManualRadiusDiameterEntered = false;
            }
        }

        /// <summary>
        /// Handler for when key is manually pressed in a Point Text Box
        /// </summary>
        /// <param name="obj">always null</param>
        private void OnPointTextBoxKeyDown(object obj)
        {
            if (isActiveTab)
            {
                // deactivate the map point tool when a point is manually entered
                if (IsToolActive)
                    IsToolActive = false;
            }
        }

        /// <summary>
        /// Handler for when key is manually pressed in the Radius/Diameter text box
        /// </summary>
        /// <param name="obj"></param>
        private void OnRadiusDiameterTextBoxKeyDown(object obj)
        {
            isManualRadiusDiameterEntered = true;
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
            if ((p1 == null) || (p2 == null))
                return 0.0;
            try
            {
                var p2proj = GeometryEngine.Instance.Project(p2, p1.SpatialReference);
                var meters = GeometryEngine.Instance.GeodesicDistance(p1, p2proj);
                // convert to current linear unit
                return ConvertFromTo(DistanceTypes.Meters, LineDistanceType, meters);
            }catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return Double.NaN;
            }
        }

        private void SetGeodesicDistance(MapPoint p1, MapPoint p2)
        {
            // convert to current linear unit
            Distance = GetGeodesicDistance(p1, p2);
        }

        internal async Task UpdateFeedbackWithGeoLine(LineSegment segment, GeodeticCurveType type, LinearUnit lu)
        {
          
            if (Point1 == null || segment == null)
                return;

            var polyline = await QueuedTask.Run(() =>
            {
                return PolylineBuilder.CreatePolyline(segment);
            });

            ClearTempGraphics();
            Geometry newline = GeometryEngine.Instance.GeodeticDensifyByLength(polyline, 0, lu, type);
            await AddGraphicToMapAsync(Point1, ColorFactory.Instance.GreenRGB, null, true, 5.0);
            await AddGraphicToMapAsync(newline, ColorFactory.Instance.GreyRGB, null, true);
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

        internal GeodeticCurveType GetCurveType()
        {
            if (LineType == LineTypes.Geodesic)
                return GeodeticCurveType.Geodesic;
            else if (LineType == LineTypes.GreatElliptic)
                return GeodeticCurveType.GreatElliptic;
            else if (LineType == LineTypes.Loxodrome)
                return GeodeticCurveType.Loxodrome;

            return GeodeticCurveType.Geodesic;
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

            if (string.IsNullOrWhiteSpace(coordinate) || coordinate.Length < 3) // basic check
                return null;

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
                    // Make sure there is an active map
                    if ((MapView.Active == null) || (MapView.Active.Map == null) ||
                        (MapView.Active.Map.SpatialReference == null))
                        point = null;
                    else
                        point = QueuedTask.Run(() =>
                        {
                            return MapPointBuilder.FromGeoCoordinateString(coordinate, MapView.Active.Map.SpatialReference, type, FromGeoCoordinateMode.Default);
                        }).Result;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                if (point != null)
                    return point;
            }

            try
            {
                // Make sure there is an active map
                if ((MapView.Active == null) || (MapView.Active.Map == null) ||
                        (MapView.Active.Map.SpatialReference == null))
                    point = null;
                else
                    point = QueuedTask.Run(() =>
                    {
                        return MapPointBuilder.FromGeoCoordinateString(coordinate, MapView.Active.Map.SpatialReference, GeoCoordinateType.UTM, FromGeoCoordinateMode.UtmNorthSouth);
                    }).Result;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            if (point == null)
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
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }
            }

            return point;
        }

        /// <summary>
        /// Saves graphics to file gdb or shp file
        /// </summary>
        private async void OnSaveAs()
        {
            // TODO: All of these prompts below may need to be added as resources

            // Save edits so everything in feature class will be exported
            // Without save, only new edits since last save will be exported (which seems counterintuitive) 
            if (ArcGIS.Desktop.Core.Project.Current.HasEdits)
            {
                // Prompt for confirmation, and if answer is no, return.
                var result = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                    "Edits must be saved before proceeding. Save edits?", "Save All Edits", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Asterisk);

                // Return if cancel value is chosen
                if (Convert.ToString(result) == "OK")
                {
                    await ArcGIS.Desktop.Core.Project.Current.SaveEditsAsync();
                }
                else // operation cancelled
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Save As cancelled.");
                    return;                
                }
            }

            //Clear any selected features before executing export process
            if (!ExecuteBuiltinCommand("esri_mapping_clearSelectionButton"))
            {
                System.Diagnostics.Trace.WriteLine("Unable to clear selected features");
            }

            var dlg = new GRSaveAsFormatView();
            dlg.DataContext = new ProSaveAsFormatViewModel();
            var vm = (ProSaveAsFormatViewModel)dlg.DataContext;

            if (dlg.ShowDialog() == true)
            {                
                string path = fcUtils.PromptUserWithSaveDialog(vm.FeatureShapeIsChecked);
                if (path != null)
                {
                    bool success = false;
                    try
                    {                        
                        string folderName = System.IO.Path.GetDirectoryName(path);

                        SaveAsType saveAsType = SaveAsType.FileGDB;
                        if (path.IndexOf(".gdb") == -1)
                            saveAsType = SaveAsType.Shapefile;
                        if (vm.KmlIsChecked)
                            saveAsType = SaveAsType.KML;

                        _progressDialog = new ProgressDialog("Exporting Layer: " + this.GetLayerName());
                        _progressDialog.Show();
                        success = await fcUtils.ExportLayer(this.GetLayerName(), path, saveAsType);
                        _progressDialog.Hide();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }

                    if (!success)
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Save As process failed.");
                }
            }
        }

        #endregion Private Functions

        #region Feature Class Support

        /// <summary>
        /// This is the name of the feature layer in the Table of Contents that contains the 
        /// features for the graphic type (ex. "Lines" "Ellipses" etc.). It should be overridden
        /// in derived classes with the correct layer name 
        /// </summary>
        /// <returns>Layer Name in Table of Contents to save features</returns>
        public virtual string GetLayerName()
        {
            return "UNKNOWN";
        }

        private ProgressDialog _progressDialog = null;

        private async Task<bool> AddLayerPackageToMapAsync()
        {         
            _progressDialog = new ProgressDialog("Loading Required Layer Package...");

            bool success = false;

            try
            {
                _progressDialog.Show();

                await QueuedTask.Run(() =>
                {
                    string layerFileName = "DistanceAndDirection.lpkx";
                    string layerPath = System.IO.Path.Combine(Models.FeatureClassUtils.AddinAssemblyLocation(), "Data", layerFileName);

                    // Do a final check that another queued thread has not already loaded this layer
                    if (GetFeatureLayerByNameInActiveView(this.GetLayerName()) == null)
                    {
                        // Now add the layer package
                        Layer layerAdded = LayerFactory.Instance.CreateLayer(
                            new Uri(layerPath), MapView.Active.Map);
                            success = (layerAdded != null);
                    }
                });

                // Save the project, so layer stays in project
                // Note: Must be called on Main/UI Thread
                if (success)
                    await ArcGIS.Desktop.Framework.FrameworkApplication.Current.Dispatcher.Invoke(async () =>
                    {
                        bool success2 = await ArcGIS.Desktop.Core.Project.Current.SaveAsync();
                    });

                _progressDialog.Hide();
            }
            catch (Exception exception)
            {
                // Catch any exception found and display a message box.
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Exception caught: " + exception.Message);
            }

            return success;
        }

        protected FeatureLayer GetFeatureLayerByNameInActiveView(string featureLayerName)
        {
            if ((MapView.Active == null) || (MapView.Active.Map == null))
                return null;

            var viewLayer =
                MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().
                    FirstOrDefault(f => f.Name == featureLayerName);

            // TODO: May need to also verify that the layer is child of "Distance and Direction" 
            // group since the layer names used are pretty common and may not be unique

            return viewLayer;
        }

        protected async Task<FeatureClass> GetFeatureClass(bool addToMapIfNotPresent = false)
        {
            string featureLayerName = this.GetLayerName();

            FeatureLayer featureLayer = GetFeatureLayerByNameInActiveView(featureLayerName);

            if ((featureLayer == null) && (addToMapIfNotPresent))
            {
                await System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(async () =>
                {
                    await AddLayerPackageToMapAsync();
                }));

                // Verify added correctly
                featureLayer = GetFeatureLayerByNameInActiveView(featureLayerName);

                // If feature layer is still not found, report the problem: 
                // ex: "Could not find required layer in the active map"
                if (featureLayer == null)
                {
                    // Note: Must be called on Main/UI Thread
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Could not find required layer in the active map: " + this.GetLayerName());
                    });
                }
            }

            if (featureLayer == null)
                return null;

            FeatureClass featureClass = featureLayer.GetTable() as FeatureClass;

            return featureClass;
        }

        protected async Task<bool> HasLayerFeatures()
        {
            FeatureClass fc = null;

            await QueuedTask.Run(async () =>
            {
                fc = await GetFeatureClass(addToMapIfNotPresent: false);
            });

            return fc == null ? false : fc.GetCount() > 0;
        }

        protected async Task<bool> DeleteAllFeatures()
        {
            bool success = false;

            FeatureClass featureClass = await GetFeatureClass(addToMapIfNotPresent: false);
            if (featureClass != null)
            {
                success = await DeleteAllFeatures(featureClass);
            }

            if (!success)
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                    DistanceAndDirectionLibrary.Properties.Resources.ErrorDeleteFailed 
                    + this.GetLayerName(), DistanceAndDirectionLibrary.Properties.Resources.ErrorDeleteFailed); 

            return success;
        }

        protected async Task<bool> DeleteAllFeatures(ArcGIS.Core.Data.FeatureClass featureClass)
        {
            if (featureClass == null)
                return false;

            string error = String.Empty;
            bool result = false;
            await QueuedTask.Run(async () =>
            {
                using (ArcGIS.Core.Data.Table table = featureClass as ArcGIS.Core.Data.Table)
                {
                    try
                    {
                        using (var rowCursor = table.Search(null, false))
                        {
                            var editOperation = new ArcGIS.Desktop.Editing.EditOperation()
                            {
                                Name = string.Format(@"Deleted All Features")
                            };

                            editOperation.Callback(context =>
                            {
                                while (rowCursor.MoveNext())
                                {
                                    System.Threading.Thread.Yield();
                                    using (var row = rowCursor.Current)
                                    {
                                        context.Invalidate(row);
                                        row.Delete();
                                    }
                                }
                            }, table);

                            result = await editOperation.ExecuteAsync();

                            if (!result)
                                error = editOperation.ErrorMessage;
                        }
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                    }
                }
            });

            if (!result)
            {
                System.Diagnostics.Trace.WriteLine("Could not delete features: " + error);
                //Important/Note: MessageBox will deadlock thread if called on MCT - 
                //Therefore need to ensure called on UI thread
                // ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(String.Format("Could not delete features : {0}",
                //    error));
            }

            return result;
        }

        protected static bool ExecuteBuiltinCommand(string commandId)
        {
            bool success = false;

            // Important/Note: Must be called on UI Thread (i.e. from a button or tool)
            ArcGIS.Desktop.Framework.FrameworkApplication.Current.Dispatcher.Invoke(() =>
            {
                // Use the built-in Pro button/command
                var wrapper = ArcGIS.Desktop.Framework.FrameworkApplication.GetPlugInWrapper(commandId);
                var command = wrapper as System.Windows.Input.ICommand;
                if ((command != null) && command.CanExecute(null))
                {
                    command.Execute(null);
                    success = true;
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("Warning - unable to execute command: " + commandId);
                }
            });

            return success;
        }
        #endregion

    }
}
