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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using DistanceAndDirectionLibrary.Views;
using DistanceAndDirectionLibrary.ViewModels;

namespace ProAppDistanceAndDirectionModule.ViewModels
{
    public class ProTabBaseViewModel : BaseViewModel
    {
        public ProTabBaseViewModel()
        {
            // commands
            EditPropertiesDialogCommand = new RelayCommand(() => OnEditPropertiesDialog());

        }

        #region Commands

        public RelayCommand EditPropertiesDialogCommand { get; set; }

        /// <summary>
        /// Handler for opening the edit properties dialog
        /// </summary>
        /// <param name="obj"></param>
        private void OnEditPropertiesDialog()
        {
            var dlg = new EditPropertiesView();
            dlg.DataContext = new EditPropertiesViewModel();

            dlg.ShowDialog();
        }
        
        #endregion



        internal async void AddGraphicToMap(Geometry geom, bool IsTempGraphic = false, double size = 1.0)
        {
            if (geom == null || MapView.Active == null)
                return;

            CIMSymbolReference symbol = null;

            if(geom.GeometryType == GeometryType.Point)
            {
                await QueuedTask.Run(() =>
                    {
                        var s = SymbolFactory.ConstructPointSymbol(ColorFactory.Red, size, SimpleMarkerStyle.Circle);
                        symbol = new CIMSymbolReference() { Symbol = s };
                    });
            }
            else if(geom.GeometryType == GeometryType.Polyline)
            {
                await QueuedTask.Run(() =>
                    {
                        var s = SymbolFactory.ConstructLineSymbol(ColorFactory.Red, size);
                        symbol = new CIMSymbolReference() { Symbol = s };
                    });
            }
            else if(geom.GeometryType == GeometryType.Polygon)
            {
                await QueuedTask.Run(() =>
                {
                    var color = CIMColor.CreateRGBColor(255, 0, 0, 25);
                    var outline = SymbolFactory.ConstructStroke(ColorFactory.Black, 1.0, SimpleLineStyle.Solid);
                    var s = SymbolFactory.ConstructPolygonSymbol(color, SimpleFillStyle.Solid, outline);
                    symbol = new CIMSymbolReference() { Symbol = s };
                });
            }

            MapView.Active.AddOverlay(geom, symbol);
        }
    }
}
