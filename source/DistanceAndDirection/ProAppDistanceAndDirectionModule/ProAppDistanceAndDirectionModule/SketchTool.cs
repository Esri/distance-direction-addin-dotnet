using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using DistanceAndDirectionLibrary.Helpers;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace ProAppDistanceAndDirectionModule
{
    class SketchTool : MapTool
    {
        public SketchTool()
        {
            // we may use the sketch tool if it supports geodesic feedback
            // but it doesn't look like it does
            // need to see what geodesic support is available
            IsSketchTool = false;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
            Mediator.Register("SET_SKETCH_TOOL_TYPE", (sgType) => SketchType = (SketchGeometryType)sgType);
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            QueuedTask.Run(() =>
                {
                    Mediator.NotifyColleagues("SKETCH_COMPLETE", geometry);
                });
            return base.OnSketchCompleteAsync(geometry);
        }

        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            try
            {
                QueuedTask.Run(() =>
                {
                    var mp = MapView.Active.ClientToMap(e.ClientPoint);
                    Mediator.NotifyColleagues(DistanceAndDirectionLibrary.Constants.NEW_MAP_POINT, mp);
                });
            }
            catch(Exception ex)
            {

            }
            base.OnToolMouseDown(e);
        }

        protected override void OnToolMouseMove(MapViewMouseEventArgs e)
        {
            try
            {
                QueuedTask.Run(() =>
                {
                    var mp = MapView.Active.ClientToMap(e.ClientPoint);
                    Mediator.NotifyColleagues(DistanceAndDirectionLibrary.Constants.MOUSE_MOVE_POINT, mp);
                });
            }
            catch(Exception ex)
            {

            }
            base.OnToolMouseMove(e);
        }

        protected override void OnToolDoubleClick(MapViewMouseButtonEventArgs e)
        {
            try
            {
                QueuedTask.Run(() =>
                {
                    var mp = MapView.Active.ClientToMap(e.ClientPoint);
                    Mediator.NotifyColleagues(DistanceAndDirectionLibrary.Constants.MOUSE_DOUBLE_CLICK, mp);
                });
            }
            catch(Exception ex)
            {

            }
            base.OnToolDoubleClick(e);
        }
    }
}
