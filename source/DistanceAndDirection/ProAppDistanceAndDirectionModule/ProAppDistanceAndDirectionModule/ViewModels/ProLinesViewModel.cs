using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using DistanceAndDirectionLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProAppDistanceAndDirectionModule.ViewModels
{
    public class ProLinesViewModel : ProTabBaseViewModel
    {
        public ProLinesViewModel()
        {
            ActivateToolCommand = new ArcGIS.Desktop.Framework.RelayCommand(async ()=> 
                {
                    FrameworkApplication.SetCurrentToolAsync("ProAppDistanceAndDirectionModule_SketchTool");
                    Mediator.NotifyColleagues("SET_SKETCH_TOOL_TYPE", ArcGIS.Desktop.Mapping.SketchGeometryType.Line);
                });

            Mediator.Register("SKETCH_COMPLETE", OnSketchComplete);
        }

        private void OnSketchComplete(object obj)
        {
            AddGraphicToMap(obj as ArcGIS.Core.Geometry.Geometry);
        }

        public ArcGIS.Desktop.Framework.RelayCommand ActivateToolCommand { get; set; }

        internal override void CreateMapElement()
        {
            if (!CanCreateElement)
                return;

            base.CreateMapElement();
            CreatePolyline();
            Reset(false);
        }

        private void CreatePolyline()
        {
            if (Point1 == null || Point2 == null)
                return;

            try
            {
                // create line
                var polyline = QueuedTask.Run(() =>
                    {
                        var segment = LineBuilder.CreateLineSegment(new Coordinate(Point1), new Coordinate(Point2));
                        return PolylineBuilder.CreatePolyline(segment);
                    }).Result;
                // update distance
                Distance = GeometryEngine.GeodesicDistance(Point1, Point2);
                // update azimuth
                UpdateAzimuth(polyline);

                AddGraphicToMap(polyline);
                ResetPoints();
            }
            catch(Exception ex)
            {
                // do nothing
            }
        }

        private void UpdateAzimuth(Polyline polyline)
        {
            
        }

    }
}
