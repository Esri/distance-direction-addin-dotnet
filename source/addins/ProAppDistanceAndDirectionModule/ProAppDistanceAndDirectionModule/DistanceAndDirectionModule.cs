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

namespace ProAppDistanceAndDirectionModule
{
    internal class DistanceAndDirectionModule : Module
    {
        private static DistanceAndDirectionModule _this = null;

        private const string _dockPaneID = "ProAppDistanceAndDirectionModule_DistanceAndDirectionDockpane";

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static DistanceAndDirectionModule Current
        {
            get
            {
                return _this ?? (_this = (DistanceAndDirectionModule)FrameworkApplication.FindModule("ProAppDistanceAndDirectionModule_Module"));
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

        /// <summary>
        /// Stores the instance of the Feature Selection dock pane viewmodel
        /// </summary>
        private static DistanceAndDirectionDockpaneViewModel _dockPane;
        internal static DistanceAndDirectionDockpaneViewModel DistanceAndDirectionVM
        {
            get
            {
                if (_dockPane == null)
                {
                    _dockPane = FrameworkApplication.DockPaneManager.Find(_dockPaneID) as DistanceAndDirectionDockpaneViewModel;
                }
                return _dockPane;
            }
        }



        ///// <summary>
        ///// Stores the instance of the proLineViewModel
        ///// </summary>
        //public static ProAppDistanceAndDirectionModule.ViewModels.ProLinesViewModel proLineVM;

        ///// <summary>
        ///// Stores the instance of the proCircleViewModel
        ///// </summary>
        //public static ProAppDistanceAndDirectionModule.ViewModels.ProCircleViewModel proCircleVM;

        ///// <summary>
        ///// Stores the instance of the proEllipseViewModel
        ///// </summary>
        //public static ProAppDistanceAndDirectionModule.ViewModels.ProEllipseViewModel proEllipseVM;

        ///// <summary>
        ///// Stores the instance of the proRangeViewModel
        ///// </summary>
        //public static ProAppDistanceAndDirectionModule.ViewModels.ProRangeViewModel proRangeVM;

    }
}
