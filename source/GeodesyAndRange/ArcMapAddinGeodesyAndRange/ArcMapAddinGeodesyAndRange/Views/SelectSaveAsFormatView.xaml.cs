/******************************************************************************* 
  * Copyright 2016 Esri 
  *  
  *  Licensed under the Apache License, Version 2.0 (the "License"); 
  *  you may not use this file except in compliance with the License. 
  *  You may obtain a copy of the License at 
  *  
  *  http://www.apache.org/licenses/LICENSE-2.0 
  *   
  *   Unless required by applicable law or agreed to in writing, software 
  *   distributed under the License is distributed on an "AS IS" BASIS, 
  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
  *   See the License for the specific language governing permissions and 
  *   limitations under the License. 
  ******************************************************************************/

using System.Windows;
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
