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

// System
using System;

// Esri
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Display;
using DistanceAndDirectionLibrary;

namespace ArcMapAddinDistanceAndDirection.Models
{
    class KMLUtils
    {

        public bool ConvertLayerToKML(string kmzOutputPath, string tmpShapefilePath, 
            ESRI.ArcGIS.Carto.IMap map, GraphicTypes graphicType)
        {
            try
            {
                string kmzName = System.IO.Path.GetFileName(kmzOutputPath);

                IGeoProcessor2 gp = new GeoProcessorClass();
                gp.OverwriteOutput = true;
                IGeoFeatureLayer geoLayer = null;

                IVariantArray parameters = new VarArrayClass();
                parameters.Add(tmpShapefilePath);
                parameters.Add(kmzName);
                gp.Execute("MakeFeatureLayer_management", parameters, null);

                string layerFileName = getLayerFileFromGraphicType(graphicType);
                if (!string.IsNullOrEmpty(layerFileName))
                {
                    IVariantArray parametersASM = new VarArrayClass();
                    parametersASM.Add(kmzName);
                    parametersASM.Add(layerFileName);
                    gp.Execute("ApplySymbologyFromLayer_management", parametersASM, null);
                }

                IVariantArray parameters1 = new VarArrayClass();
                // assign  parameters        
                parameters1.Add(kmzName);
                parameters1.Add(kmzOutputPath);

                gp.Execute("LayerToKML_conversion", parameters1, null);

                // Remove the temporary layer from the TOC
                for (int i = 0; i < map.LayerCount; i++ )
                {
                    ILayer layer = map.get_Layer(i);
                    if ((layer.Name == "featureLayer") || (layer.Name == kmzName))
                    {
                        map.DeleteLayer(layer);
                        break;
                    }
                }
                if (geoLayer != null)
                {
                    map.DeleteLayer(geoLayer);
                }

                return true;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private string getLayerFileFromGraphicType(GraphicTypes graphicType)
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            string addinPath = System.IO.Path.GetDirectoryName(
                              Uri.UnescapeDataString(
                                      new Uri(asm.CodeBase).LocalPath));

            string layerFileName = string.Empty;

            switch (graphicType)
            {
                case GraphicTypes.Line:
                    layerFileName = "Lines.lyr";
                    break;
                case GraphicTypes.RangeRing:
                    layerFileName = "Rings.lyr";
                    break;
                case GraphicTypes.Circle:
                    layerFileName = "Circles.lyr";
                    break;
                case GraphicTypes.Ellipse:
                    layerFileName = "Ellipses.lyr";
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(layerFileName))
                return layerFileName;

            string layerPath = System.IO.Path.Combine(addinPath, "Data", layerFileName);

            return layerPath;
        }

    }

}
