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
using System.Windows.Controls;

using DistanceAndDirectionLibrary.Helpers;
using DistanceAndDirectionLibrary.Views;
using DistanceAndDirectionLibrary;
using DistanceAndDirectionLibrary.ViewModels;
using DistanceAndDirectionLibrary.Models;

namespace ArcMapAddinDistanceAndDirection.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            // set some views and datacontext
            LinesView = new GRLinesView();
            LinesView.DataContext = new LinesViewModel();

            CircleView = new GRCircleView();
            CircleView.DataContext = new CircleViewModel();
            
            EllipseView = new GREllipseView();
            EllipseView.DataContext = new EllipseViewModel();
            
            RangeView = new GRRangeView();
            RangeView.DataContext = new RangeViewModel();

            // load the configuration file
            DistanceAndDirectionConfig.AddInConfig.LoadConfiguration();  
        }

        #region Properties

        object selectedTab = null;
        public object SelectedTab
        {
            get { return selectedTab; }
            set
            {
                // Don't raise event if same tab selected
                if (selectedTab == value)
                    return;

                selectedTab = value;
                var tabItem = selectedTab as TabItem;

                if (tabItem == null)
                    return;

                Mediator.NotifyColleagues(Constants.TAB_ITEM_SELECTED, ((tabItem.Content as UserControl).Content as UserControl).DataContext);
            }
        }

        #region Views

        public GRLinesView LinesView { get; set; }
        public GRCircleView CircleView { get; set; }
        public GREllipseView EllipseView { get; set; }
        public GRRangeView RangeView { get; set; }

        #endregion

        #endregion
    }
}
