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

// System
using System;

// Esri
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

using DistanceAndDirectionLibrary.Helpers;
using DistanceAndDirectionLibrary;

namespace ArcMapAddinDistanceAndDirection
{
    public class MapPointTool : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        public MapPointTool()
        {
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }

        protected override void OnMouseDown(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
            if (arg.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            try
            {
                //Get the active view from the ArcMap static class.
                IActiveView activeView = ArcMap.Document.FocusMap as IActiveView;

                var point = activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y) as IPoint;

                Mediator.NotifyColleagues(Constants.NEW_MAP_POINT, point);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
        protected override void OnMouseMove(MouseEventArgs arg)
        {
            IActiveView activeView = ArcMap.Document.FocusMap as IActiveView;

            var point = activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y) as IPoint;

            Mediator.NotifyColleagues(Constants.MOUSE_MOVE_POINT, point);
        }

        protected override void OnDoubleClick()
        {
            Mediator.NotifyColleagues(Constants.MOUSE_DOUBLE_CLICK, null);
        }

    }

}
