// System
using System;
using System.Collections.Generic;
using System.IO;

// Esri
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessing;

namespace ArcMapAddinGeodesyAndRange.Models
{
    class KMLUtils
    {
        
        public void CreateKML()
        {
            Console.WriteLine("Initializing Geoprocessor...");
            


        }

        public void ConvertLayerToKML(string fcPath, ESRI.ArcGIS.Carto.IMap map)
        {
            try
            {
                IGeoProcessor2 gp = new GeoProcessorClass();
                IVariantArray parameters = new VarArrayClass();
                parameters.Add(fcPath);
                parameters.Add("featureLayer");
                gp.Execute("MakeFeatureLayer_management", parameters, null);

                IVariantArray parameters1 = new VarArrayClass();
                // assign  parameters        
                parameters1.Add("featureLayer");
                parameters1.Add(@"C:\Hagens\Projects\MilitaryAnalyst\data\test.kmz");

                gp.Execute("LayerToKML_conversion", parameters1, null);

                // Remove the temporary layer from the TOC
                for (int i = 0; i < map.LayerCount; i++ )
                {
                    ILayer layer = map.get_Layer(i);
                    if (layer.Name == "featureLayer")
                    {
                        map.DeleteLayer(layer);
                        break;
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }
    }

    
}
