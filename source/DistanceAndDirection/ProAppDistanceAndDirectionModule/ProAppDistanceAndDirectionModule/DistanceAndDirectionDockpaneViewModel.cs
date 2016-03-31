using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using DistanceAndDirectionLibrary.Views;
using ProAppDistanceAndDirectionModule.ViewModels;

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

            CircleView = new GRCircleView();
            CircleView.DataContext = new ProCircleViewModel();

            EllipseView = new GREllipseView();
            //EllipseView.DataContext = new EllipseViewModel();

            RangeView = new GRRangeView();
            //RangeView.DataContext = new RangeViewModel();
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
                //var tabItem = selectedTab as TabItem;
                //Mediator.NotifyColleagues(Constants.TAB_ITEM_SELECTED, ((tabItem.Content as UserControl).Content as UserControl).DataContext);
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

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class DistanceAndDirectionDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            DistanceAndDirectionDockpaneViewModel.Show();
        }
    }
}
