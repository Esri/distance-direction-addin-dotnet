using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ESRI.ArcGIS.esriSystem;
using ArcMapAddinGeodesyAndRange.ViewModels;

namespace ArcMapAddinGeodesyAndRange.Views
{
    /// <summary>
    /// Interaction logic for SelectSaveAsFormatView.xaml change
    /// </summary>
    public partial class SelectSaveAsFormatView : Window
    {
        public SelectSaveAsFormatView()
        {
            InitializeComponent();

            var vm = this.DataContext as SelectSaveAsFormatViewModel;

            if (vm == null)
                return;

            var win = Window.GetWindow(this);

            if (win != null)
            {
                var temp = new System.Windows.Interop.WindowInteropHelper(win);

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as SelectSaveAsFormatViewModel;

            if (vm == null)
                return;
            
            DialogResult = true;
        }
    }
}
