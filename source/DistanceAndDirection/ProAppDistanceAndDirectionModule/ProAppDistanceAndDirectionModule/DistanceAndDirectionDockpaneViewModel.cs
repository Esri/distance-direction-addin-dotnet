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

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using DistanceAndDirectionLibrary;
using DistanceAndDirectionLibrary.Helpers;
using DistanceAndDirectionLibrary.Models;
using DistanceAndDirectionLibrary.Views;
using ProAppDistanceAndDirectionModule.ViewModels;
using System.Windows.Controls;

namespace ProAppDistanceAndDirectionModule
{
    internal class DistanceAndDirectionDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "ProAppDistanceAndDirectionModule_DistanceAndDirectionDockpane";

        protected DistanceAndDirectionDockpaneViewModel() 
        {
            // set some views and datacontext
            LinesView = new GRLinesView();
            LinesView.DataContext = new ProLinesViewModel();
            // Hide the ComboBox until we have more than one line type to select from
            LinesView.CmbLineType.Visibility = System.Windows.Visibility.Collapsed;

            CircleView = new GRCircleView();
            CircleView.DataContext = new ProCircleViewModel();

            EllipseView = new GREllipseView();
            EllipseView.DataContext = new ProEllipseViewModel();

            RangeView = new GRRangeView();
            RangeView.DataContext = new ProRangeViewModel();

            // load the configuration file
            DistanceAndDirectionConfig.AddInConfig.LoadConfiguration();  
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        #region Properties

        object selectedTab = null;
        public object SelectedTab
        {
            get { return selectedTab; }
            set
            {
                if (selectedTab == value)
                    return;

                selectedTab = value;
                var tabItem = selectedTab as TabItem;
                if ((tabItem.Content as UserControl).Content != null)
                    Mediator.NotifyColleagues(Constants.TAB_ITEM_SELECTED, ((tabItem.Content as UserControl).Content as UserControl).DataContext);
            }
        }

        #region Views

        public GRLinesView LinesView { get; set; }
        public GRCircleView CircleView { get; set; }
        public GREllipseView EllipseView { get; set; }
        public GRRangeView RangeView { get; set; }
        public GRSaveAsFormatView SelectSaveAsFormatView { get; set; }

        #endregion

        #endregion

    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class DistanceAndDirectionDockpane_ShowButton : ArcGIS.Desktop.Framework.Contracts.Button
    {
        protected override void OnClick()
        {
            DistanceAndDirectionDockpaneViewModel.Show();
        }
    }
}
