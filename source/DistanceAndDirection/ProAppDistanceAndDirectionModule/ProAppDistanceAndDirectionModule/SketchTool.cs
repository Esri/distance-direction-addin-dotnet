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

        protected override async void OnToolMouseMove(MapViewMouseEventArgs e)
        {
            try
            {
                var mp = await QueuedTask.Run(() =>
                {
                    return MapView.Active.ClientToMap(e.ClientPoint);
                });
                Mediator.NotifyColleagues(DistanceAndDirectionLibrary.Constants.MOUSE_MOVE_POINT, mp);
            }
            catch(Exception ex)
            {

            }
            base.OnToolMouseMove(e);
        }

        protected override async void OnToolDoubleClick(MapViewMouseButtonEventArgs e)
        {
            try
            {
                var mp = await QueuedTask.Run(() =>
                {
                    return MapView.Active.ClientToMap(e.ClientPoint);
                });
                Mediator.NotifyColleagues(DistanceAndDirectionLibrary.Constants.MOUSE_DOUBLE_CLICK, mp);
            }
            catch(Exception ex)
            {

            }
            base.OnToolDoubleClick(e);
        }
    }
}
