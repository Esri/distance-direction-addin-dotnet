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
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using DistanceAndDirectionLibrary;
using DistanceAndDirectionLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProAppDistanceAndDirectionModule.ViewModels
{
    public class ProLinesViewModel : ProTabBaseViewModel
    {
        public ProLinesViewModel()
        {
            IsActiveTab = true;

            LineFromType = LineFromTypes.Points;
            LineAzimuthType = AzimuthTypes.Degrees;

            ActivateToolCommand = new ArcGIS.Desktop.Framework.RelayCommand(async ()=> 
                {
                    await FrameworkApplication.SetCurrentToolAsync("ProAppDistanceAndDirectionModule_SketchTool");
                    Mediator.NotifyColleagues("SET_SKETCH_TOOL_TYPE", ArcGIS.Desktop.Mapping.SketchGeometryType.Line);
                });

            Mediator.Register("SKETCH_COMPLETE", OnSketchComplete);
        }
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
                Reset(false);
                RaisePropertyChanged(() => DistanceBearingReady);
            }
        }

        LineTypes lineType = LineTypes.Geodesic;
        public override LineTypes LineType
        {
            get { return lineType;  }
            set
            {
                lineType = value;
                RaisePropertyChanged(() => LineType);
                ResetFeedback();
            }
        }

        public GeodeticCurveType DeriveCurveType(LineTypes type)
        {
            GeodeticCurveType curveType = GeodeticCurveType.Geodesic;
            if (type == LineTypes.Geodesic)
                curveType = GeodeticCurveType.Geodesic;
            else if (type == LineTypes.GreatElliptic)
                curveType = GeodeticCurveType.GreatElliptic;
            else if (type == LineTypes.Loxodrome)
                curveType = GeodeticCurveType.Loxodrome;
            return curveType;
        }

        public System.Windows.Visibility LineTypeComboVisibility
        {
            get { return System.Windows.Visibility.Visible; }
        }


        AzimuthTypes lineAzimuthType = AzimuthTypes.Degrees;
        public AzimuthTypes LineAzimuthType
        {
            get { return lineAzimuthType; }
            set
            {
                if (LineFromType == LineFromTypes.Points)
                {
                    var before = lineAzimuthType;
                    lineAzimuthType = value;
                    UpdateAzimuthFromTo(before, value);
                }
                else
                {
                    lineAzimuthType = value;
                }
            }
        }

        public override MapPoint Point1
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
                RaisePropertyChanged(() => DistanceBearingReady);
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

                if (LineFromType == LineFromTypes.BearingAndDistance)
                    UpdateManualFeedback();
                else
                    UpdateFeedback();

                DistanceString = distance.ToString("G");
                RaisePropertyChanged(() => DistanceString);
            }
        }

        public override DistanceTypes LineDistanceType
        {
            get
            {
                return base.LineDistanceType;
            }
            set
            {
                if (LineFromType == LineFromTypes.Points)
                {
                    var before = base.LineDistanceType;
                    Distance = ConvertFromTo(before, value, Distance);
                }
                base.LineDistanceType = value;
            }
        }

        public LinearUnit DeriveUnit(DistanceTypes distType)
        {
            LinearUnit lu = LinearUnit.Meters;
            if (distType == DistanceTypes.Feet)
                lu = LinearUnit.Feet;
            else if (distType == DistanceTypes.Kilometers)
                lu = LinearUnit.Kilometers;
            else if (distType == DistanceTypes.Meters)
                lu = LinearUnit.Meters;
            else if (distType == DistanceTypes.Miles)
                lu = LinearUnit.Miles;
            else if (distType == DistanceTypes.NauticalMile)
                lu = LinearUnit.NauticalMiles;
            else if (distType == DistanceTypes.Yards)
                lu = LinearUnit.Yards;
            return lu;
        }

        internal override async void UpdateFeedback()
        {
            GeodeticCurveType curveType = DeriveCurveType(LineType);
            LinearUnit lu = DeriveUnit(LineDistanceType);
            if (LineFromType == LineFromTypes.Points)
            {
                if (Point1 == null || Point2 == null)
                    return;

                var segment = QueuedTask.Run(() =>
                {
                    try
                    {
                        var point2proj = GeometryEngine.Instance.Project(Point2, Point1.SpatialReference);
                        return LineBuilder.CreateLineSegment(Point1, (MapPoint)point2proj);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        return null;
                    }
                }).Result;

                if (segment == null)
                    return;
               
                UpdateAzimuth(segment.Angle);

                await UpdateFeedbackWithGeoLine(segment, curveType, lu);
            }
            else
            {
                UpdateManualFeedback();
            }
        }

        double? azimuth = 0.0;
        public double? Azimuth
        {
            get { return azimuth; }
            set
            {
                if ((value != null) && (value >= 0.0))
                    azimuth = value;
                else
                    azimuth = null;

                RaisePropertyChanged(() => Azimuth);

                if (LineFromType == LineFromTypes.BearingAndDistance)
                {
                    UpdateFeedback();
                }

                if ((value == null) || (value < 0.0))
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);

                if (LineAzimuthType == AzimuthTypes.Degrees)
                {
                    if (value > 360)
                    {
                        throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                    }
                }
                else
                {
                    if (value > 6400)
                    {
                        throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                    }
                }
                AzimuthString = azimuth.Value.ToString("G");
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

                if (LineFromType == LineFromTypes.BearingAndDistance)
                {
                    // update azimuth
                    double d = 0.0;
                    if (double.TryParse(azimuthString, out d))
                    {
                        if (Azimuth == d)
                            return;

                        Azimuth = d;

                        UpdateManualFeedback();
                    }
                    else
                    {
                        Azimuth = null;
                        throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                    }
                }
            }
        }

        private async void UpdateManualFeedback()
        {
            if (LineFromType == LineFromTypes.BearingAndDistance && Azimuth.HasValue && HasPoint1 && Point1 != null)
            {
                GeodeticCurveType curveType = DeriveCurveType(LineType);
                LinearUnit lu = DeriveUnit(LineDistanceType);
                // update feedback
                var segment = QueuedTask.Run(() =>
                {
                    var mpList = new List<MapPoint>() { Point1 };
                    // get point 2
                    // SDK Bug, GeometryEngine.GeodesicMove seems to not honor the LinearUnit passed in, always does Meters
                    var tempDistance = ConvertFromTo(LineDistanceType, DistanceTypes.Meters, Distance);
                    var results = GeometryEngine.Instance.GeodeticMove(mpList, MapView.Active.Map.SpatialReference, tempDistance, LinearUnit.Meters /*GetLinearUnit(LineDistanceType)*/, GetAzimuthAsRadians().Value, GetCurveType());
                    foreach (var mp in results)
                        Point2 = mp;
                    if (Point2 != null)
                    {
                        var point2Proj = GeometryEngine.Instance.Project(Point2, Point1.SpatialReference);
                        return LineBuilder.CreateLineSegment(Point1, (MapPoint)point2Proj);
                    }
                    else
                        return null;
                }).Result;

                if (segment != null)
                    await UpdateFeedbackWithGeoLine(segment, curveType, lu);
            }
            else
            {
                ClearTempGraphics(); // if not, or no longer, valid clear 
            }
        }

        /// <summary>
        /// On top of the base class we need to make sure we have an azimuth
        /// </summary>
        public override bool CanCreateElement
        {
            get
            {
                return (Azimuth.HasValue && base.CanCreateElement);
            }
        }

        // sketch doesn't support geodesic lines
        // not using this for now
        private void OnSketchComplete(object obj)
        {
            AddGraphicToMap(obj as ArcGIS.Core.Geometry.Geometry);
        }

        public ArcGIS.Desktop.Framework.RelayCommand ActivateToolCommand { get; set; }

        /// <summary>
        /// Overrides TabBaseViewModel CreateMapElement
        /// </summary>
        internal override Geometry CreateMapElement()
        {
            Geometry geom = null;
            if (!CanCreateElement)
                return geom;

            base.CreateMapElement();
            geom = CreatePolyline();
            Reset(false);

            return geom;
        }

        internal override void Reset(bool toolReset)
        {
            base.Reset(toolReset);

            Azimuth = 0.0;
        }

        internal async override void OnNewMapPointEvent(object obj)
        {
            if (!IsActiveTab)
                return;

            var point = obj as MapPoint;

            if (point == null)
                return;

            // Bearing and Distance Mode
            if (LineFromType == LineFromTypes.BearingAndDistance)
            {
                ClearTempGraphics();
                HasPoint1 = true;
                Point1 = point;
                await AddGraphicToMapAsync(Point1, ColorFactory.Instance.GreenRGB, null, true, 5.0);
                return;
            }

            base.OnNewMapPointEvent(obj);
        }

        internal override async void OnMouseMoveEvent(object obj)
        {
            GeodeticCurveType curveType = DeriveCurveType(LineType);
            LinearUnit lu = DeriveUnit(LineDistanceType);
            if (!IsActiveTab)
                return;

            var point = obj as MapPoint;

            if (point == null)
                return;

            if (LineFromType == LineFromTypes.BearingAndDistance)
                return;

            if (HasPoint1 && !HasPoint2)
            {
                // update azimuth from segment
                var segment = QueuedTask.Run(() =>
                {
                    try
                    {
                        var pointProj = GeometryEngine.Instance.Project(point, Point1.SpatialReference);
                        return LineBuilder.CreateLineSegment(Point1, (MapPoint)pointProj);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        return null;
                    }
                }).Result;

                if (segment == null)
                    return;

                UpdateAzimuth(segment.Angle);
                await UpdateFeedbackWithGeoLine(segment, curveType, lu);                        
            }

            base.OnMouseMoveEvent(obj);
        }

        private Geometry CreatePolyline()
        {
            if (Point1 == null || Point2 == null)
                return null;

            GeodeticCurveType curveType = DeriveCurveType(LineType);
            LinearUnit lu = DeriveUnit(LineDistanceType);
            try
            {
                // create line
                var polyline = QueuedTask.Run(() =>
                    {
                        var point2Proj = GeometryEngine.Instance.Project(Point2, Point1.SpatialReference);
                        var segment = LineBuilder.CreateLineSegment(Point1, (MapPoint)point2Proj);
                        return PolylineBuilder.CreatePolyline(segment);
                    }).Result;
                Geometry newline = GeometryEngine.Instance.GeodeticDensifyByLength(polyline, 0, lu, curveType);

                // Hold onto the attributes in case user saves graphics to file later
                LineAttributes lineAttributes = new LineAttributes(){mapPoint1 = Point1, mapPoint2 = Point2, _distance = distance, angle = (double)azimuth, angleunit = LineAzimuthType.ToString(), distanceunit = LineDistanceType.ToString(), originx=Point1.X, originy = Point1.Y, destinationx=Point2.X, destinationy=Point2.Y};

                bool success = false;
                QueuedTask.Run(async () =>
                    success = await AddFeatureToLayer(newline, (ProGraphicAttributes)lineAttributes));

                ResetPoints();

                return (Geometry)newline;
            }
            catch(Exception ex)
            {
                // do nothing
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        private void UpdateAzimuth(double radians)
        {
            var degrees = radians * (180.0 / Math.PI);
            if (degrees <= 90.0)
                degrees = 90.0 - degrees;
            else
                degrees = 360.0 - (degrees - 90.0);

            if (LineAzimuthType == AzimuthTypes.Degrees)
                Azimuth = degrees;
            else if (LineAzimuthType == AzimuthTypes.Mils)
                Azimuth = degrees * 17.777777778;
        }

        private double? GetAzimuthAsRadians()
        {
            double? result = GetAzimuthAsDegrees();

            return result * (Math.PI / 180.0);
        }

        private double? GetAzimuthAsDegrees()
        {
            if (LineAzimuthType == AzimuthTypes.Mils)
            {
                return Azimuth * 0.05625;
            }

            return Azimuth;
        }

        private double GetAngleDegrees(double angle)
        {
            double bearing = (180.0 * angle) / Math.PI;
            if (bearing < 90.0)
                bearing = 90 - bearing;
            else
                bearing = 360.0 - (bearing - 90);

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
                double angle = Azimuth.GetValueOrDefault();

                if (fromType == AzimuthTypes.Degrees && toType == AzimuthTypes.Mils)
                    angle *= 17.777777778;
                else if (fromType == AzimuthTypes.Mils && toType == AzimuthTypes.Degrees)
                    angle *= 0.05625;

                Azimuth = angle;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        public bool DistanceBearingReady
        {

            get
            {
                if (LineFromType == LineFromTypes.BearingAndDistance && Point1 != null)
                    return true;
                else
                    return false;
            }
        }

        // ******************************************************************************
        // Feature Support below - will be moved to base/utility class in future
        // ******************************************************************************

        public async Task<bool> HasLineFeatures()
        {
            FeatureClass fc = null;

            await QueuedTask.Run(async () =>
            {
                fc = await GetLineFeatureClass();
            });

            return fc == null ? false : fc.GetCount() > 0;
        }

        private async Task<FeatureClass> GetLineFeatureClass(bool addToMapIfNotPresent = false)
        {
            string featureLayerName = "Lines of Bearing";

            FeatureLayer featureLayer = GetFeatureLayerByNameInActiveView(featureLayerName);

            if ((featureLayer == null) && (addToMapIfNotPresent))
            {
                await System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(async () =>
                {
                    await AddLayerPackageToMapAsync();
                }));

                // Verify added correctly
                featureLayer = GetFeatureLayerByNameInActiveView(featureLayerName);
            }

            if (featureLayer == null)
                return null;

            FeatureClass lineFeatureClass = featureLayer.GetTable() as FeatureClass;

            //****************************************************
            // TODO: check lineFeatureClass has require fields
            //****************************************************

            return lineFeatureClass;
        }

        private async Task<bool> AddFeatureToLayer(Geometry geom, ProGraphicAttributes p = null)
        {
            LineAttributes attributes = p as LineAttributes;

            if (attributes == null)
            {
                // ERROR
                return false;
            }

            FeatureClass lineFeatureClass = await GetLineFeatureClass(addToMapIfNotPresent: true);
            if (lineFeatureClass == null)
            {
                // ERROR
                return false;
            }

            string message = String.Empty;
            bool creationResult = false;

            FeatureClassDefinition lineDefinition = lineFeatureClass.GetDefinition();

            EditOperation editOperation = new EditOperation();
            editOperation.Name = "Line Feature Insert";
            editOperation.Callback(context =>
            {
                try
                {
                    RowBuffer rowBuffer = lineFeatureClass.CreateRowBuffer();

                    if (lineDefinition.FindField("Distance") >= 0)
                        rowBuffer["Distance"] = attributes._distance;     // Double

                    if (lineDefinition.FindField("DistUnit") >= 0)
                        rowBuffer["DistUnit"] = attributes.distanceunit; // Text

                    if (lineDefinition.FindField("Angle") >= 0)
                        rowBuffer["Angle"] = attributes.angle;       // Double

                    if (lineDefinition.FindField("AngleUnit") >= 0)
                        rowBuffer["AngleUnit"] = attributes.angleunit;   // Text

                    if (lineDefinition.FindField("OriginX") >= 0)
                        rowBuffer["OriginX"] = attributes.originx;   // Double

                    if (lineDefinition.FindField("OriginY") >= 0)
                        rowBuffer["OriginY"] = attributes.originy;   // Double

                    if (lineDefinition.FindField("DestX") >= 0)
                        rowBuffer["DestX"] = attributes.destinationx;   // Double

                    if (lineDefinition.FindField("DestY") >= 0)
                        rowBuffer["DestY"] = attributes.destinationy;   // Double

                    rowBuffer["Shape"] = GeometryEngine.Instance.Project(geom, lineDefinition.GetSpatialReference());

                    Feature feature = lineFeatureClass.CreateRow(rowBuffer);
                    feature.Store();

                    //To Indicate that the attribute table has to be updated
                    context.Invalidate(feature);
                }
                catch (GeodatabaseException geodatabaseException)
                {
                    message = geodatabaseException.Message;
                }
            }, lineFeatureClass);

            await QueuedTask.Run(async () =>
            {
                creationResult = await editOperation.ExecuteAsync();
            });

            if (!creationResult)
            {
                message = editOperation.ErrorMessage;
            }

            if (!creationResult)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message);
            }

            return true;
        }

        public async Task<bool> DeleteAllFeatures()
        {
            bool success = false;

            FeatureClass lineFeatureClass = await GetLineFeatureClass(addToMapIfNotPresent: false);
            if (lineFeatureClass != null)
            {
                success = await DeleteAllFeatures(lineFeatureClass);
            }

            if (!success)
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Failed to Delete Lines Features"); // TODO: Add as resource

            return success;
        }
    }
}

