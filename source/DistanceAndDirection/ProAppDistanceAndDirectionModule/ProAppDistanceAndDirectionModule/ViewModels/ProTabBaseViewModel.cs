using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DistanceAndDirectionLibrary.ViewModels;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.CIM;

namespace ProAppDistanceAndDirectionModule.ViewModels
{
    public class ProTabBaseViewModel : BaseViewModel
    {
        public ProTabBaseViewModel()
        { }

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
