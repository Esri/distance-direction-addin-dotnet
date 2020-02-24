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
using ProAppDistanceAndDirectionModule.Common;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using ArcGIS.Desktop.Framework;
using ProAppDistanceAndDirectionModule.ViewModels;
using ProAppDistanceAndDirectionModule.Views;

namespace ProAppDistanceAndDirectionModule
{
    class SketchTool : MapTool
    {

        private const string NEW_MAP_POINT = "NEW_MAP_POINT";
        private const string MOUSE_MOVE_POINT = "MOUSE_MOVE_POINT";
        private const string TAB_ITEM_SELECTED = "TAB_ITEM_SELECTED";
        private const string MOUSE_DOUBLE_CLICK = "MOUSE_DOUBLE_CLICK";
        private const string KEYPRESS_ESCAPE = "KEYPRESS_ESCAPE";
        //private const string POINT_TEXT_KEYDOWN = "POINT_TEXT_KEYDOWN";
        //private const string RADIUS_DIAMETER_KEYDOWN = "RADIUS_DIAMETER_KEYDOWN";
        private const string TOC_ITEMS_CHANGED = "TOC_ITEMS_CHANGED";
        private const string TEXTCHANGE_DELETE = "TEXTCHANGE_DELETE";
        private const string LAYER_PACKAGE_LOADED = "LAYER_PACKAGE_LOADED";

        private readonly DebounceDispatcher _throttleMouse = new DebounceDispatcher();
        public SketchTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            UseSnapping = true;
        }

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
                SketchMouseEvents(null, KEYPRESS_ESCAPE);
            }
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            try
            {
                MapPoint mp = geometry as MapPoint;
                SketchMouseEvents(mp, NEW_MAP_POINT);
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
                    // avoid chaining issues
                    var mapView = MapView.Active;

                    MapPoint mp = await QueuedTask.Run(() => mapView.ClientToMap(e.ClientPoint));
                    SketchMouseEvents(mp, MOUSE_MOVE_POINT); //TODO this should be a custom Pro event so it can be called from within the QTR and avoid another await
                }, priority: DispatcherPriority.Normal);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            base.OnToolMouseMove(e);
        }

        protected override async void OnToolDoubleClick(MapViewMouseButtonEventArgs e)
        {
            try
            {
                MapPoint mp = await QueuedTask.Run(() =>
                {
                    return MapView.Active.ClientToMap(e.ClientPoint);
                });
                SketchMouseEvents(mp, MOUSE_DOUBLE_CLICK);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            base.OnToolDoubleClick(e);
        }

        private void SketchMouseEvents(MapPoint mp,string mouseevent)
        {
            //Get the instance of the Main ViewModel from the dock pane

            DistanceAndDirectionDockpaneViewModel ddVM = DistanceAndDirectionModule.DistanceAndDirectionVM;

            if (ddVM != null)
            {
                System.Windows.Controls.TabItem tabItem = ddVM.SelectedTab as System.Windows.Controls.TabItem;
                if (tabItem != null)
                {
                    if (tabItem.Header.Equals(Properties.Resources.LabelTabLines))
                    {
                        ProLinesView plView = (tabItem.Content as System.Windows.Controls.UserControl).Content as ProLinesView;
                        ProLinesViewModel plViewmodel = plView.DataContext as ProLinesViewModel;
                        if(mouseevent.Equals(NEW_MAP_POINT))
                        {
                            plViewmodel.NewMapPointEvent.Execute(mp);
                        }
                        else if(mouseevent.Equals(MOUSE_MOVE_POINT))
                        {
                            plViewmodel.MouseMoveEvent.Execute(mp);
                        }
                        else if (mouseevent.Equals(KEYPRESS_ESCAPE))
                        {
                            plViewmodel.KeypressEscape.Execute(mp);
                        }
                            
                    }
                    else if (tabItem.Header.Equals(Properties.Resources.LabelTabCircle))
                    {
                        ProCircleView pcView = (tabItem.Content as System.Windows.Controls.UserControl).Content as ProCircleView;
                        ProCircleViewModel pcViewmodel = pcView.DataContext as ProCircleViewModel;
                        if (mouseevent.Equals(NEW_MAP_POINT))
                        {
                            pcViewmodel.NewMapPointEvent.Execute(mp);
                        }
                        else if (mouseevent.Equals(MOUSE_MOVE_POINT))
                        {
                            pcViewmodel.MouseMoveEvent.Execute(mp);
                        }
                        else if (mouseevent.Equals(KEYPRESS_ESCAPE))
                        {
                            pcViewmodel.KeypressEscape.Execute(mp);
                        }
                    }
                    else if (tabItem.Header.Equals(Properties.Resources.LabelTabEllipse))
                    {
                        ProEllipseView pelView = (tabItem.Content as System.Windows.Controls.UserControl).Content as ProEllipseView;
                        ProEllipseViewModel pelViewmodel = pelView.DataContext as ProEllipseViewModel;
                        if (mouseevent.Equals(NEW_MAP_POINT))
                        {
                            pelViewmodel.NewMapPointEvent.Execute(mp);
                        }
                        else if (mouseevent.Equals(MOUSE_MOVE_POINT))
                        {
                            pelViewmodel.MouseMoveEvent.Execute(mp);
                        }
                        else if (mouseevent.Equals(KEYPRESS_ESCAPE))
                        {
                            pelViewmodel.KeypressEscape.Execute(mp);
                        }
                    }
                    else if (tabItem.Header.Equals(Properties.Resources.LabelTabRange))
                    {
                        ProRangeView prView = (tabItem.Content as System.Windows.Controls.UserControl).Content as ProRangeView;
                        ProRangeViewModel prViewmodel = prView.DataContext as ProRangeViewModel;
                        if (mouseevent.Equals(MOUSE_DOUBLE_CLICK))
                        {
                            prViewmodel.MouseDoubleClick.Execute(mp);
                        }
                        else if (mouseevent.Equals(NEW_MAP_POINT))
                        {
                            prViewmodel.NewMapPointEvent.Execute(mp);
                        }
                        else if (mouseevent.Equals(MOUSE_MOVE_POINT))
                        {
                            prViewmodel.MouseMoveEvent.Execute(mp);
                        }
                        else if (mouseevent.Equals(KEYPRESS_ESCAPE))
                        {
                            prViewmodel.KeypressEscape.Execute(mp);
                        }
                    }

                }

            }
        }
    }
}
