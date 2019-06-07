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

using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using DistanceAndDirectionLibrary;
using DistanceAndDirectionLibrary.Helpers;
using DistanceAndDirectionLibrary.Models;
using DistanceAndDirectionLibrary.ViewModels;
using DistanceAndDirectionLibrary.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;

namespace ProAppDistanceAndDirectionModule.ViewModels
{
    public class ProRangeViewModel : ProTabBaseViewModel
    {
        public ProRangeViewModel()
        {
            OutputDistanceView = new ProOutputDistanceView();
            Mediator.Register(DistanceAndDirectionLibrary.Constants.MOUSE_DOUBLE_CLICK, OnMouseDoubleClick);
            Mediator.Register(DistanceAndDirectionLibrary.Constants.TEXTCHANGE_DELETE, OnTextChangeEvent);
            Mediator.Register(DistanceAndDirectionLibrary.Constants.LAYER_PACKAGE_LOADED, OnLayerPackageLoaded);
        }
        public ProOutputDistanceView OutputDistanceView { get; set; }
        #region Properties

        RingTypes ringType = RingTypes.Fixed;
        public override RingTypes RingType
        {
            get { return ringType; }
            set
            {
                ringType = value;
                RaisePropertyChanged(() => RingType);

                ResetValue();
            }
        }

        public override bool IsToolActive
        {
            get
            {
                return base.IsToolActive;
            }
            set
            {
                base.IsToolActive = value;

                if (CanCreateElement)
                    CreateMapElement();

                maxDistance = 0.0;
                if (RingType == RingTypes.Interactive)
                    NumberOfRings = 0;
            }
        }


        // keep track of the max distance for drawing of radials in interactive mode
        double maxDistance = 0.0;

        int numberOfRings = 10;
        /// <summary>
        /// Property for the number or rings
        /// </summary>
        public int NumberOfRings
        {
            get { return numberOfRings; }
            set
            {
                if (RingType == RingTypes.Fixed)
                {
                    if (value < 1 || value > 180)
                        throw new ArgumentException(string.Format(DistanceAndDirectionLibrary.Properties.Resources.AENumOfRings, 1, 180));
                }
                numberOfRings = value;
                RaisePropertyChanged(() => NumberOfRings);
            }
        }

        int numberOfRadials = 0;
        /// <summary>
        /// Property for the number of radials
        /// </summary>
        public int NumberOfRadials
        {
            get { return numberOfRadials; }
            set
            {
                if (value < 0 || value > 180)
                    throw new ArgumentException(string.Format(DistanceAndDirectionLibrary.Properties.Resources.AENumOfRadials, 0, 180));

                numberOfRadials = value;
                RaisePropertyChanged(() => NumberOfRadials);
            }
        }

        public override bool CanCreateElement
        {
            get
            {
                if (RingType == RingTypes.Interactive)
                    return (Point1 != null && NumberOfRadials >= 0);
                else if (RingType == RingTypes.Origin || RingType == RingTypes.Cumulative)
                {
                    if (Point1 != null && NumberOfRadials >= 0 && OutputDistanceViewModel.OutputDistanceListItem.Count > 0)
                    {
                        foreach (var item in OutputDistanceViewModel.OutputDistanceListItem)
                        {
                            if (item.OutputDistance == "" || item.OutputDistance == "0")
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    return false;
                }
                else
                    return (Point1 != null && NumberOfRings > 0 && NumberOfRadials >= 0 && Distance > 0.0);
            }
        }

        #endregion Properties

        private void ResetValue()
        {
            if (RingType == RingTypes.Fixed)
            {
                NumberOfRings = 10;
            }
            else
            {
                maxDistance = 0.0;
                NumberOfRings = 0;
            }
            if (OutputDistanceViewModel.OutputDistanceListItem == null)
                return;

            if (RingType == RingTypes.Fixed || RingType == RingTypes.Interactive)
            {
                OutputDistanceViewModel.OutputDistanceListItem.Clear();
            }
            else
            {
                if (OutputDistanceViewModel.OutputDistanceListItem.Count == 0)
                {
                    var outputItem = new OutputDistanceModel();
                    outputItem.UniqueRowId = OutputDistanceViewModel.UniqueRowId;
                    outputItem.OutputDistance = "0";
                    OutputDistanceViewModel.OutputDistanceListItem.Add(outputItem);
                    OutputDistanceViewModel.UniqueRowId++;
                }
            }
        }

        /// <summary>
        /// Method used to create the needed map elements to add to the graphics container
        /// Is called by the base class when the "Enter" key is pressed
        /// </summary>
        internal override Geometry CreateMapElement()
        {
            Geometry geom = null;
            // do we have enough data?
            if (!CanCreateElement)
                return geom;

            if (!(RingType == RingTypes.Interactive))
            {
                base.CreateMapElement();

                geom = DrawRings();
            }

            DrawRadials();

            Reset(false);

            return geom;
        }

        /// <summary>
        /// Method to draw the radials inside the range rings
        /// Must have at least 1 radial
        /// All radials are drawn from the center point to the farthest ring
        /// </summary>
        private void DrawRadials()
        {
            // must have at least 1
            if (NumberOfRadials < 1)
                return;


            var nameConverter = new EnumToFriendlyNameConverter();
            double azimuth = 0.0;
            double interval = 360.0 / NumberOfRadials;
            double radialLength = 0.0;

            if (!(RingType == RingTypes.Fixed))
                radialLength = maxDistance;
            else
                radialLength = Distance * NumberOfRings;

            try
            {
                // for each radial, draw from center point
                for (int x = 0; x < NumberOfRadials; x++)
                {
                    var polyline = QueuedTask.Run(() =>
                    {
                        MapPoint movedMP = null;
                        var mpList = new List<MapPoint>() { Point1 };
                        // get point 2

                        var results = GeometryEngine.Instance.GeodeticMove(mpList,
                            MapView.Active.Map.SpatialReference, radialLength, GetLinearUnit(LineDistanceType), GetAzimuthAsRadians(azimuth), GetCurveType());

                        // update feedback
                        //UpdateFeedback();
                        foreach (var mp in results)
                            movedMP = mp;

                        if (movedMP != null)
                        {
                            var movedMPproj = GeometryEngine.Instance.Project(movedMP, Point1.SpatialReference);
                            var segment = LineBuilder.CreateLineSegment(Point1, (MapPoint)movedMPproj);
                            return PolylineBuilder.CreatePolyline(segment);
                        }
                        else
                            return null;
                    }).Result;

                    Geometry newline = GeometryEngine.Instance.GeodeticDensifyByLength(polyline, 0, LinearUnit.Meters, GeodeticCurveType.Loxodrome);
                    if (newline != null)
                    {
                        // Hold onto the attributes in case user saves graphics to file later
                        var displayValue = nameConverter.Convert(LineDistanceType, typeof(string), new object(), CultureInfo.CurrentCulture);
                        RangeAttributes rangeAttributes = new RangeAttributes()
                        {
                            mapPoint = Point1,
                            numRings = NumberOfRings,
                            distance = radialLength,
                            centerx = Point1.X,
                            centery = Point1.Y,
                            distanceunit = displayValue.ToString(),
                            ringorradial = "Radial"
                        };

                        // AddGraphicToMap(newline, rangeAttributes);
                        CreateRangeRingOrRadialFeature(newline, rangeAttributes);
                    }

                    azimuth += interval;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private double GetAzimuthAsRadians(double azimuth)
        {
            return azimuth * (Math.PI / 180.0);
        }

        /// <summary>
        /// Method used to draw the rings at the desired interval
        /// Rings are constructed as geodetic circles
        /// </summary>
        private Geometry DrawRings()
        {
            if (Point1 == null || double.IsNaN(Distance))
                return null;

            double radius = 0.0;

            try
            {
                Geometry geom = null;
                if (RingType == RingTypes.Fixed)
                {
                    for (int x = 0; x < numberOfRings; x++)
                    {
                        // set the current radius
                        radius += Distance;
                        geom = CreateRangeRings(radius);
                    }
                }
                else
                {
                    foreach (var item in OutputDistanceViewModel.OutputDistanceListItem)
                    {
                        var outputDistance = Convert.ToDouble(item.OutputDistance);
                        if (outputDistance > 0.0)
                        {
                            if (RingType == RingTypes.Origin)
                            {
                                maxDistance = outputDistance > maxDistance ? outputDistance : maxDistance;
                                radius = outputDistance;
                            }
                            else if (RingType == RingTypes.Cumulative)
                            {
                                radius += outputDistance;
                                maxDistance = radius > maxDistance ? radius : maxDistance;

                            }
                            geom = CreateRangeRings(radius);
                        }
                    }
                }

                return geom;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return null;
            }
        }

        private Geometry CreateRangeRings(double radius)
        {
            Geometry geom = null;
            var param = new GeodesicEllipseParameter();

            param.Center = new Coordinate2D(Point1);
            param.AxisDirection = 0.0;
            param.LinearUnit = GetLinearUnit(LineDistanceType);
            param.OutGeometryType = GeometryType.Polyline;
            param.SemiAxis1Length = radius;
            param.SemiAxis2Length = radius;
            param.VertexCount = VertexCount;

            geom = GeometryEngine.Instance.GeodesicEllipse(param, MapView.Active.Map.SpatialReference);
            var nameConverter = new EnumToFriendlyNameConverter();
            var displayValue = nameConverter.Convert(LineDistanceType, typeof(string), new object(), CultureInfo.CurrentCulture);
            // Hold onto the attributes in case user saves graphics to file later
            RangeAttributes rangeAttributes = new RangeAttributes()
            {
                mapPoint = Point1,
                numRings = numberOfRings,
                distance = radius,
                centerx = Point1.X,
                centery = Point1.Y,
                distanceunit = displayValue.ToString(),
                ringorradial = "Ring"
            };

            CreateRangeRingOrRadialFeature(geom, rangeAttributes);

            return geom;
        }

        /// <summary>
        /// Override the on new map point event to only handle one point for the center point
        /// </summary>
        /// <param name="obj"></param>
        internal override void OnNewMapPointEvent(object obj)
        {
            // only if we are the active tab
            if (!IsActiveTab)
                return;

            if (ValidateDistances())
                return;

            var point = obj as MapPoint;

            if (point == null)
                return;

            if (!(RingType == RingTypes.Interactive))
            {
                Point1 = point;
                HasPoint1 = true;

                ClearTempGraphics();
                AddGraphicToMap(Point1, ColorFactory.Instance.GreenRGB, null, true, 5.0);

                // Reset formatted string
                Point1Formatted = string.Empty;
            }
            else
            {
                // we are in interactive mode
                if (!HasPoint1)
                {
                    Point1 = point;
                    HasPoint1 = true;

                    ClearTempGraphics();
                    AddGraphicToMap(Point1, ColorFactory.Instance.GreenRGB, null, true, 5.0);

                    // Reset formatted string
                    Point1Formatted = string.Empty;
                }
                else
                {
                    // update Distance
                    Distance = GetGeodesicDistance(Point1, point);

                    // draw a geo ring
                    ConstructGeoCircle();

                    NumberOfRings++;
                }
            }
        }

        private bool ValidateDistances()
        {
            if (RingType == RingTypes.Origin || RingType == RingTypes.Cumulative)
            {
                bool flag = true;
                foreach (var item in OutputDistanceViewModel.OutputDistanceListItem)
                {
                    if (item.OutputDistance != "" && item.OutputDistance != "0")
                    {
                        return flag = false;
                    }
                }
                if (flag)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(DistanceAndDirectionLibrary.Properties.Resources.MsgRingValidDistances);
                    return flag;
                }
            }
            return false;
        }

        private void OnLayerPackageLoaded(object obj)
        {
            RemoveSpatialIndexOfLayer(GetLayerName());
        }

        /// <summary>
        /// Override the mouse move event to dynamically update the center point
        /// Also dynamically update the ring feedback
        /// </summary>
        /// <param name="obj"></param>
        internal override void OnMouseMoveEvent(object obj)
        {
            // only if we are the active tab
            if (!IsActiveTab)
                return;

            var point = obj as MapPoint;

            if (point == null)
                return;

            if (!HasPoint1)
            {
                Point1 = point;
            }
            else if (HasPoint1 && RingType == RingTypes.Interactive)
            {
                Distance = GetGeodesicDistance(Point1, point);

                // update ring feedback, distance
                UpdateFeedbackWithGeoCircle();
            }
        }

        /// <summary>
        /// Method to handle map point tool double click
        /// End interactive drawing of range rings
        /// </summary>
        /// <param name="obj"></param>
        private void OnMouseDoubleClick(object obj)
        {
            if (RingType == RingTypes.Interactive && IsToolActive)
                IsToolActive = false;
        }

        private void OnTextChangeEvent(object obj)
        {
            if (OutputDistanceViewModel.OutputDistanceListItem != null && OutputDistanceViewModel.OutputDistanceListItem.Count > 1)
            {
                ObservableCollection<OutputDistanceModel> listOfDistances = new ObservableCollection<OutputDistanceModel>();
                foreach (var item in OutputDistanceViewModel.OutputDistanceListItem)
                {
                    if (item.OutputDistance == "")
                    {
                        listOfDistances.Add(item);
                    }
                }

                foreach (var item in listOfDistances)
                {
                    if (item.OutputDistance == "")
                    {
                        OutputDistanceViewModel.OutputDistanceListItem.Remove(item);
                    }
                }
            }
        }

        internal override void Reset(bool toolReset)
        {
            base.Reset(toolReset);

            if (IsToolActive && (RingType == RingTypes.Origin || RingType == RingTypes.Cumulative))
                IsToolActive = false;

            NumberOfRadials = 0;
            if (RingType != RingTypes.Fixed)
                NumberOfRings = 0;
        }

        private void ConstructGeoCircle()
        {
            if (Point1 == null || double.IsNaN(Distance))
                return;

            var nameConverter = new EnumToFriendlyNameConverter();
            var param = new GeodesicEllipseParameter();

            param.Center = new Coordinate2D(Point1);
            param.AxisDirection = 0.0;
            param.LinearUnit = GetLinearUnit(LineDistanceType);
            param.OutGeometryType = GeometryType.Polyline;
            param.SemiAxis1Length = Distance;
            param.SemiAxis2Length = Distance;
            param.VertexCount = VertexCount;

            maxDistance = Math.Max(maxDistance, Distance);

            var geom = GeometryEngine.Instance.GeodesicEllipse(param, MapView.Active.Map.SpatialReference);

            var displayValue = nameConverter.Convert(LineDistanceType, typeof(string), new object(), CultureInfo.CurrentCulture);
            // Hold onto the attributes in case user saves graphics to file later
            RangeAttributes rangeAttributes = new RangeAttributes()
            {
                mapPoint = Point1,
                numRings = NumberOfRings,
                distance = Distance,
                centerx = Point1.X,
                centery = Point1.Y,
                distanceunit = displayValue.ToString(),
                ringorradial = "Ring"
            };

            CreateRangeRingOrRadialFeature(geom, rangeAttributes);
        }

        private void UpdateFeedbackWithGeoCircle()
        {
            if (Point1 == null || double.IsNaN(Distance) || Distance <= 0.0)
                return;

            var param = new GeodesicEllipseParameter();

            param.Center = new Coordinate2D(Point1);
            param.AxisDirection = 0.0;
            param.LinearUnit = GetLinearUnit(LineDistanceType);
            param.OutGeometryType = GeometryType.Polyline;
            param.SemiAxis1Length = Distance;
            param.SemiAxis2Length = Distance;
            param.VertexCount = VertexCount;

            var geom = GeometryEngine.Instance.GeodesicEllipse(param, MapView.Active.Map.SpatialReference);
            ClearTempGraphics();

            var nameConverter = new EnumToFriendlyNameConverter();
            var displayValue = nameConverter.Convert(LineDistanceType, typeof(string), new object(), CultureInfo.CurrentCulture);

            // Hold onto the attributes in case user saves graphics to file later
            RangeAttributes rangeAttributes = new RangeAttributes()
            {
                mapPoint = Point1,
                numRings = NumberOfRings,
                distance = Distance,
                centerx = Point1.X,
                centery = Point1.Y,
                distanceunit = displayValue.ToString()
            };

            AddGraphicToMap(Point1, ColorFactory.Instance.GreenRGB, null, true, 5.0);
            AddGraphicToMap(geom, ColorFactory.Instance.GreyRGB, rangeAttributes, true);
        }

        public override string GetLayerName()
        {
            return "Range Rings";
        }

        private async void CreateRangeRingOrRadialFeature(Geometry geom, RangeAttributes rangeAttributes)
        {
            string message = string.Empty;
            await QueuedTask.Run(async () =>
                message = await AddFeatureToLayer(geom, rangeAttributes));

            RaisePropertyChanged(() => HasMapGraphics);

            if (!string.IsNullOrEmpty(message))
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message,
                    DistanceAndDirectionLibrary.Properties.Resources.ErrorFeatureCreateTitle);
        }

        private async Task<string> AddFeatureToLayer(Geometry geom, RangeAttributes attributes)
        {
            string message = String.Empty;

            if (attributes == null)
            {
                message = "Attributes are Empty"; // For debug does not need to be resource
                return message;
            }

            FeatureClass ringFeatureClass = await GetFeatureClass(addToMapIfNotPresent: true);
            if (ringFeatureClass == null)
            {
                message = DistanceAndDirectionLibrary.Properties.Resources.ErrorFeatureClassNotFound + this.GetLayerName();
                return message;
            }

            bool creationResult = false;

            FeatureClassDefinition ringDefinition = ringFeatureClass.GetDefinition();

            EditOperation editOperation = new EditOperation();
            editOperation.Name = "Ring Feature Insert";
            editOperation.Callback(context =>
            {
                try
                {
                    RowBuffer rowBuffer = ringFeatureClass.CreateRowBuffer();

                    if (ringDefinition.FindField("Distance") >= 0)
                        rowBuffer["Distance"] = attributes.distance;     // Double

                    if (ringDefinition.FindField("DistUnit") >= 0)
                        rowBuffer["DistUnit"] = attributes.distanceunit; // Text

                    if (ringDefinition.FindField("Rings") >= 0)
                        rowBuffer["Rings"] = attributes.numRings;        // Double

                    if (ringDefinition.FindField("CenterX") >= 0)
                        rowBuffer["CenterX"] = attributes.centerx;       // Double

                    if (ringDefinition.FindField("CenterY") >= 0)
                        rowBuffer["CenterY"] = attributes.centery;       // Double

                    if (ringDefinition.FindField("RRType") >= 0)
                        rowBuffer["RRType"] = attributes.ringorradial;   // Double

                    // Ensure Z removed (this feature class does not have Z)
                    var geoNoZ = geom;
                    if (geom.HasZ)
                    {
                        PolylineBuilder pb = new PolylineBuilder((Polyline)geom);
                        pb.HasZ = false;
                        geoNoZ = pb.ToGeometry();
                    }

                    rowBuffer["Shape"] = GeometryEngine.Instance.Project(geoNoZ, ringDefinition.GetSpatialReference());

                    Feature feature = ringFeatureClass.CreateRow(rowBuffer);
                    feature.Store();

                    //To Indicate that the attribute table has to be updated
                    context.Invalidate(feature);
                }
                catch (GeodatabaseException geodatabaseException)
                {
                    message = geodatabaseException.Message;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
            }, ringFeatureClass);

            await QueuedTask.Run(async () =>
            {
                creationResult = await editOperation.ExecuteAsync();
            });

            if (!creationResult)
            {
                message = editOperation.ErrorMessage;
                await Project.Current.DiscardEditsAsync();
            }
            else
            {
                await Project.Current.SaveEditsAsync();
            }

            return message;
        }

    }
}
