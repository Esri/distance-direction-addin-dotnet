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
using System.Windows.Input;
using System.Windows.Threading;

namespace ProAppDistanceAndDirectionModule
{
    class SketchTool : MapTool
    {
        public SketchTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            UseSnapping = true;
            // will need to use this in the future, commented out for now
            //Mediator.Register("SET_SKETCH_TOOL_TYPE", (sgType) => SketchType = (SketchGeometryType)sgType);
        }

        private readonly DebounceDispatcher _throttleMouse = new DebounceDispatcher();

        public static string ToolId
        {
            // Important: this must match the Tool ID used in the DAML
            get { return "ProAppDistanceAndDirectionModule_SketchTool"; }
        }

        // If the user presses Escape cancel the sketch
        protected override void OnToolKeyDown(MapViewKeyEventArgs k)
        {
            if (k.Key == Key.Escape)
            {
                k.Handled = true;
                Mediator.NotifyColleagues(DistanceAndDirectionLibrary.Constants.KEYPRESS_ESCAPE, null);
            }
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            try
            {
                var mp = geometry as MapPoint;
                Mediator.NotifyColleagues(DistanceAndDirectionLibrary.Constants.NEW_MAP_POINT, mp);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return base.OnSketchCompleteAsync(geometry);
        }

        protected override void OnToolMouseMove(MapViewMouseEventArgs e)
        {
            try
            {
                //lets limit how many times we call this
                // take the latest event args every so often
                // this will keep us from drawing too many feedback geometries
                _throttleMouse.ThrottleAndFireAtInterval(150, async (args) =>
                {
                    var mp = await QueuedTask.Run(() => MapView.Active.ClientToMap(e.ClientPoint));
                    Mediator.NotifyColleagues(DistanceAndDirectionLibrary.Constants.MOUSE_MOVE_POINT, mp);
                }, priority: DispatcherPriority.Normal);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        protected override async void OnToolDoubleClick(MapViewMouseButtonEventArgs e)
        {
            try
            {
                var mp = await QueuedTask.Run(() => MapView.Active.ClientToMap(e.ClientPoint));
                Mediator.NotifyColleagues(DistanceAndDirectionLibrary.Constants.MOUSE_DOUBLE_CLICK, mp); //TODO add event for this
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message); //TODO replace these with pro diagnostic entries
            }
            base.OnToolDoubleClick(e);
        }
    }
}
