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

using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using DistanceAndDirectionLibrary;
using DistanceAndDirectionLibrary.Helpers;
using System;
using System.Threading.Tasks;

namespace ProAppDistanceAndDirectionModule.ViewModels
{
    public class ProEllipseViewModel : ProTabBaseViewModel
    {
        public ProEllipseViewModel()
        {
            ActivateToolCommand = new ArcGIS.Desktop.Framework.RelayCommand(async () =>
            {
                await FrameworkApplication.SetCurrentToolAsync("ProAppDistanceAndDirectionModule_SketchTool");
                Mediator.NotifyColleagues("SET_SKETCH_TOOL_TYPE", ArcGIS.Desktop.Mapping.SketchGeometryType.AngledEllipse);
            });

            // we may need this in the future, leave commented out for now
            //Mediator.Register("SKETCH_COMPLETE", OnSketchComplete);
            Mediator.Register(DistanceAndDirectionLibrary.Constants.LAYER_PACKAGE_LOADED, OnLayerPackageLoaded);

            EllipseType = EllipseTypes.Semi;
        }

        private void OnSketchComplete(object obj)
        {
// TODO: DETERMINE IF THIS IS USED ANYWHERE? (appear to be 0 references)
            AddGraphicToMap(obj as ArcGIS.Core.Geometry.Geometry);
        }

        public ArcGIS.Desktop.Framework.RelayCommand ActivateToolCommand { get; set; }

        #region Properties

        public EllipseTypes EllipseType { get; set; }
        public MapPoint CenterPoint { get; set; }

        AzimuthTypes azimuthType = AzimuthTypes.Degrees;
        public AzimuthTypes AzimuthType
        {
            get { return azimuthType; }
            set
            {
                var before = azimuthType;
                azimuthType = value;
                UpdateAzimuthFromTo(before, value);
            }
        }

        private MapPoint point2 = null;
        public override MapPoint Point2
        {
            get
            {
                return point2;
            }
            set
            {
                point2 = value;
                RaisePropertyChanged(() => Point2);
            }
        }

        private MapPoint point3 = null;
        public MapPoint Point3
        {
            get
            {
                return point3;
            }
            set
            {
                point3 = value;
                RaisePropertyChanged(() => Point3);
            }
        }

        private double minorAxisDistance = 0.0;
        public double MinorAxisDistance
        {
            get
            {
                return minorAxisDistance;
            }
            set
            {
                if (value < 0.0)
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);

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

                minorAxisDistance = value;

                UpdateFeedbackWithEllipse();

                RaisePropertyChanged(() => MinorAxisDistance);
                RaisePropertyChanged(() => MinorAxisDistanceString);                
            }
        }

        private string minorAxisDistanceString = string.Empty;
        public string MinorAxisDistanceString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(minorAxisDistanceString))
                {
                    if(EllipseType == EllipseTypes.Full)
                    {
                        return (MinorAxisDistance * 2).ToString("0.##");
                    }
                    return MinorAxisDistance.ToString("0.##");
                }
                else
                    return minorAxisDistanceString;
            }
            set
            {
                if (string.Equals(minorAxisDistanceString, value))
                    return;

                minorAxisDistanceString = value;
                double d = 0.0;
                if (double.TryParse(minorAxisDistanceString, out d))
                {
                    if (EllipseType == EllipseTypes.Full)
                    {
                        MinorAxisDistance = d / 2;
                    }
                    else if (d > MajorAxisDistance)
                    {
                        minorAxisDistance = d;
                        throw new ArgumentException("Minor Axis can not be greater that Major Axis");
                    }
                    else
                    {
                        MinorAxisDistance = d;
                        MajorAxisDistance = MajorAxisDistance;
                    }

                    if (MinorAxisDistance == d)
                        return;

                    RaisePropertyChanged(() => MinorAxisDistance);
                }
                else
                {
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }
            }
        }

        private double majorAxisDistance = 0.0;
        public double MajorAxisDistance
        {
            get
            {
                return majorAxisDistance;
            }
            set
            {
                if (value < 0.0)
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);

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

                majorAxisDistance = value;

                UpdateFeedbackWithEllipse();

                RaisePropertyChanged(() => MajorAxisDistance);
                RaisePropertyChanged(() => MajorAxisDistanceString);
            }
        }

        private string majorAxisDistanceString = string.Empty;
        public string MajorAxisDistanceString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(majorAxisDistanceString))
                {
                    if (EllipseType == EllipseTypes.Full)
                    {
                        return (MajorAxisDistance * 2.0).ToString("0.##");
                    }
                    return MajorAxisDistance.ToString("0.##");
                }
                else
                    return majorAxisDistanceString;
            }
            set
            {
                if (string.Equals(majorAxisDistanceString, value))
                    return;

                majorAxisDistanceString = value;
                double d = 0.0;
                if (double.TryParse(majorAxisDistanceString, out d))
                {
                    if (EllipseType == EllipseTypes.Full)
                        MajorAxisDistance = d / 2.0;
                    else if (d < MinorAxisDistance)
                    {
                        majorAxisDistance = d;
                        throw new ArgumentException("Major Axis can not be smaller that Minor Axis");
                    }
                    else
                    {
                        MajorAxisDistance = d;
                        MinorAxisDistance = MinorAxisDistance;
                    }
                    if (MajorAxisDistance == d)
                        return;

                    UpdateFeedbackWithEllipse();
                }
                else
                {
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }
            }
        }

        double azimuth = 0.0;
        public double Azimuth
        {
            get 
            {
                return azimuth;
            }
            set
            {
                if (value < 0.0)
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEMustBePositive);

                azimuth = value;
                RaisePropertyChanged(() => Azimuth);

                UpdateFeedbackWithEllipse();

                AzimuthString = azimuth.ToString("0.##");
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
                // update azimuth
                double d = 0.0;
                if (double.TryParse(azimuthString, out d))
                {
                    if (Azimuth == d)
                        return;

                    Azimuth = d;
                }
                else
                {
                    throw new ArgumentException(DistanceAndDirectionLibrary.Properties.Resources.AEInvalidInput);
                }
            }
        }

        #endregion

        #region Overridden Functions


        /// <summary>
        /// Overrides TabBaseViewModel CreateMapElement
        /// </summary>
        internal override Geometry CreateMapElement()
        {
            Geometry geom = null;
            if (!CanCreateElement)
            {
                return geom;
            }
            geom = DrawEllipse();
            Reset(false);

            return geom;
        }

        public override bool CanCreateElement
        {
            get
            {
                return (HasPoint1 && MajorAxisDistance > 0.0 && MinorAxisDistance > 0.0);
            }
        }

        internal override void OnMouseMoveEvent(object obj)
        {
            if (!IsActiveTab)
                return;

            var point = obj as MapPoint;

            if (point == null)
                return;

            if (!HasPoint1)
            {
                Point1 = point;
            }
            else if (HasPoint1 && !HasPoint2)
            {
                // get major distance from polyline
                MajorAxisDistance = GetGeodesicDistance(Point1, point);
                // update bearing
                var segment = QueuedTask.Run(() =>
                {
                    try
                    {
                        var pointproj = GeometryEngine.Instance.Project(point, Point1.SpatialReference);
                        return LineBuilder.CreateLineSegment(Point1, (MapPoint)pointproj);
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
            }
            else if (HasPoint1 && HasPoint2 && !HasPoint3)
            {
                MinorAxisDistance = GetGeodesicDistance(Point1, point);
            }
        }

        private void UpdateFeedbackWithEllipse(bool HasMinorAxis = true)
        {
            if (!HasPoint1 || double.IsNaN(MajorAxisDistance) || double.IsNaN(MinorAxisDistance))
                return;
            
            var minorAxis = MinorAxisDistance;
            if (!HasMinorAxis || minorAxis == 0.0)
                minorAxis = MajorAxisDistance;

            if (minorAxis > MajorAxisDistance)
                minorAxis = MajorAxisDistance;
            try
            {
                var param = new GeodesicEllipseParameter();

                param.Center = new Coordinate2D(Point1);
                param.AxisDirection = GetRadiansFrom360Degrees(GetAzimuthAsDegrees());
                param.LinearUnit = GetLinearUnit(LineDistanceType);
                param.OutGeometryType = GeometryType.Polyline;
                param.SemiAxis1Length = MajorAxisDistance;
                param.SemiAxis2Length = minorAxis;
                param.VertexCount = VertexCount;

                var geom = GeometryEngine.Instance.GeodesicEllipse(param, MapView.Active.Map.SpatialReference);

                ClearTempGraphics();

                // Hold onto the attributes in case user saves graphics to file later
                //EllipseAttributes ellipseAttributes = new EllipseAttributes(Point1, minorAxis, majorAxisDistance, para.AxisDirection);

                // Point
                AddGraphicToMap(Point1, ColorFactory.Instance.GreenRGB, null, true, 5.0);
                // Ellipse
                AddGraphicToMap(geom, ColorFactory.Instance.GreyRGB, null, true);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        internal override void ResetPoints()
        {
            HasPoint1 = HasPoint2 = HasPoint3 = false;
        }

        internal override void OnNewMapPointEvent(object obj)
        {
            if (!IsActiveTab)
                return;

            var point = obj as MapPoint;

            if (point == null)
                return;

            if (!HasPoint1)
            {
                Point1 = point;
                HasPoint1 = true;
                Point1Formatted = string.Empty;
                AddGraphicToMap(Point1, ColorFactory.Instance.GreenRGB, null, true, 5.0);

            }
            else if (!HasPoint2)
            {
                Point2 = point;
                HasPoint2 = true;
            }
            else if (!HasPoint3)
            {
                if (MajorAxisDistance >= MinorAxisDistance)
                {
                    ResetFeedback();
                    Point3 = point;
                    HasPoint3 = true;
                }
            }

            if (HasPoint1 && HasPoint2 && HasPoint3)
            {
                CreateMapElement();
                ResetPoints();
            }
        }

        internal override void Reset(bool toolReset)
        {
            base.Reset(toolReset);
            HasPoint3 = false;
            Point3 = null;

            majorAxisDistanceString = string.Empty;
            minorAxisDistanceString = string.Empty;

            MajorAxisDistance = 0.0;
            MinorAxisDistance = 0.0;
            Azimuth = 0.0;
        }

        internal override void UpdateFeedback()
        {
            UpdateFeedbackWithEllipse();
        }

        #endregion

        #region Private Functions

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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private double GetAzimuthAsDegrees()
        {
            if (AzimuthType == AzimuthTypes.Mils)
            {
                return Azimuth * 0.05625;
            }

            return Azimuth;
        }

        private void UpdateAzimuth(double radians)
        {
            //System.Diagnostics.Debug.Print(string.Format("radians {0}", radians));
            var degrees = radians * (180.0 / Math.PI);
            if (degrees <= 90.0)
                degrees = 90.0 - degrees;
            else
                degrees = 360.0 - (degrees - 90.0);

            if (AzimuthType == AzimuthTypes.Degrees)
                Azimuth = degrees;
            else if (AzimuthType == AzimuthTypes.Mils)
                Azimuth = degrees * 17.777777778;
        }

        private void OnLayerPackageLoaded(object obj)
        {
            RemoveSpatialIndexOfLayer(GetLayerName());
        }

        /// <summary>
        /// gets radians in a format the PRO sdk likes
        /// from 360 degrees, straight up is ZERO
        /// Radians from far right to far left counter clockwise is 0 to positive PI
        /// Radians from far right to far left clockwise is 0 to negative PI
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        private static double GetRadiansFrom360Degrees(double degrees)
        {
            double temp;
            if (degrees >= 0.0 && degrees <= 270.0)
                temp = (degrees - 90.0) * -1.0;
            else
                temp = 450.0 - degrees;

            var radians = temp * (Math.PI / 180.0);

            return radians;
        }

        private Geometry DrawEllipse()
        {
            if (Point1 == null || double.IsNaN(MajorAxisDistance) || double.IsNaN(MinorAxisDistance))
                return null;

            try
            {
                var param = new GeodesicEllipseParameter();

                param.Center = new Coordinate2D(Point1);
                param.AxisDirection = GetRadiansFrom360Degrees(GetAzimuthAsDegrees());
                param.LinearUnit = GetLinearUnit(LineDistanceType);
                param.OutGeometryType = GeometryType.Polygon;
                param.SemiAxis1Length = MajorAxisDistance;
                param.SemiAxis2Length = MinorAxisDistance;
                param.VertexCount = VertexCount;

                var geom = GeometryEngine.Instance.GeodesicEllipse(param, MapView.Active.Map.SpatialReference);

                // Hold onto the attributes in case user saves graphics to file later
                EllipseAttributes ellipseAttributes = new EllipseAttributes() {
                    mapPoint = Point1, minorAxis = MinorAxisDistance,
                    majorAxis = MajorAxisDistance, angle = Azimuth,
                    angleunit = AzimuthType.ToString(), centerx=Point1.X,
                    centery = Point1.Y, distanceunit = LineDistanceType.ToString() };

                CreateEllipseFeature(geom, ellipseAttributes);

                return (Geometry)geom;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }
        #endregion

        public override string GetLayerName()
        {
            return "Ellipses";
        }

        private async void CreateEllipseFeature(Geometry geom, EllipseAttributes ellipseAttributes)
        {
            string message = string.Empty;
            await QueuedTask.Run(async () =>
                message = await AddFeatureToLayer(geom, ellipseAttributes));

            RaisePropertyChanged(() => HasMapGraphics);

            if (!string.IsNullOrEmpty(message))
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message,
                    DistanceAndDirectionLibrary.Properties.Resources.ErrorFeatureCreateTitle);
        }

        private async Task<string> AddFeatureToLayer(Geometry geom, EllipseAttributes attributes)
        {
            string message = String.Empty;

            if (attributes == null)
            {
                message = "Attributes are Empty"; // For debug does not need to be resource
                return message;
            }

            FeatureClass ellipseFeatureClass = await GetFeatureClass(addToMapIfNotPresent: true);
            if (ellipseFeatureClass == null)
            {
                message = DistanceAndDirectionLibrary.Properties.Resources.ErrorFeatureClassNotFound + this.GetLayerName();
                return message;
            }

            bool creationResult = false;

            FeatureClassDefinition ellipseDefinition = ellipseFeatureClass.GetDefinition();

            EditOperation editOperation = new EditOperation();
            editOperation.Name = "Ellipse Feature Insert";
            editOperation.Callback(context =>
            {
                try
                {
                    RowBuffer rowBuffer = ellipseFeatureClass.CreateRowBuffer();

                    if (ellipseDefinition.FindField("Major") >= 0)
                        rowBuffer["Major"] = attributes.majorAxis;       // Text

                    if (ellipseDefinition.FindField("Minor") >= 0)
                        rowBuffer["Minor"] = attributes.minorAxis;       // Double

                    if (ellipseDefinition.FindField("DistUnit") >= 0)
                        rowBuffer["DistUnit"] = attributes.distanceunit; // Text

                    if (ellipseDefinition.FindField("Angle") >= 0)
                        rowBuffer["Angle"] = attributes.angle;           // Double

                    if (ellipseDefinition.FindField("AngleUnit") >= 0)
                        rowBuffer["AngleUnit"] = attributes.angleunit;   // Text

                    if (ellipseDefinition.FindField("CenterX") >= 0)
                        rowBuffer["CenterX"] = attributes.centerx;       // Double

                    if (ellipseDefinition.FindField("CenterY") >= 0)
                        rowBuffer["CenterY"] = attributes.centery;       // Double

                    rowBuffer["Shape"] = GeometryEngine.Instance.Project(geom, ellipseDefinition.GetSpatialReference());

                    Feature feature = ellipseFeatureClass.CreateRow(rowBuffer);
                    feature.Store();

                    //To indicate that the attribute table has to be updated
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
            }, ellipseFeatureClass);

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
