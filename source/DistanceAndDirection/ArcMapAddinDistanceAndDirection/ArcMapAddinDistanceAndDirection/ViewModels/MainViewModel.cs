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

namespace ArcMapAddinDistanceAndDirection.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            // set some views
            _linesView = new GRLinesView();
            _linesView.DataContext = new LinesViewModel();

            _circleView = new GRCircleView();
            _circleView.DataContext = new CircleViewModel();
            
            _ellipseView = new GREllipseView();
            _ellipseView.DataContext = new EllipseViewModel();
            
            _rangeView = new GRRangeView();
            _rangeView.DataContext = new RangeViewModel();
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
                Mediator.NotifyColleagues(Constants.TAB_ITEM_SELECTED, ((tabItem.Content as UserControl).Content as UserControl).DataContext);
            }
        }

        #endregion

        #region Views

        private GRLinesView _linesView;
        public GRLinesView LinesView
        {
            get { return _linesView; }
            set
            {
                _linesView = value;
            }
        }
        private GRCircleView _circleView;
        public GRCircleView CircleView
        {
            get { return _circleView; }
            set
            {
                _circleView = value;
            }
        }
        private GREllipseView _ellipseView;
        public GREllipseView EllipseView
        {
            get { return _ellipseView; }
            set
            {
                _ellipseView = value;
            }
        }
        private GRRangeView _rangeView;
        public GRRangeView RangeView
        {
            get { return _rangeView; }
            set
            {
                _rangeView = value;
            }
        }

        #endregion
    }
}
