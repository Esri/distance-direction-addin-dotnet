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

        public bool ConvertLayerToKML(string kmzOutputPath, string tmpShapefilePath, ESRI.ArcGIS.Carto.IMap map, GraphicTypes graphicType)
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

                for (int i = 0; i < map.LayerCount; i++)
                {
                    ILayer layer = map.get_Layer(i);
                    if ((layer.Name == "featureLayer") || (layer.Name == kmzName))
                    {
                        geoLayer = layer as IGeoFeatureLayer;
                        SetRenderer(geoLayer, graphicType);
                        break;
                    }
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

        private void SetRenderer(IGeoFeatureLayer geoLayer, GraphicTypes graphicTypes)
        {
            IUniqueValueRenderer uvRenderer = new UniqueValueRendererClass();
            ISymbol symbol = null;
            switch (graphicTypes)
            {
                case GraphicTypes.Line:
                case GraphicTypes.RangeRing:
                    ESRI.ArcGIS.Display.ISimpleLineSymbol simpleLineSymbol = new ESRI.ArcGIS.Display.SimpleLineSymbol();
                    ESRI.ArcGIS.Display.IRgbColor rgbLineColor = new ESRI.ArcGIS.Display.RgbColorClass() { Red =255, Blue = 0, Green = 0 };
                    simpleLineSymbol.Color = rgbLineColor as IColor;
                    simpleLineSymbol.Width = 3;
                    symbol = simpleLineSymbol as ISymbol;
                    break;
                case GraphicTypes.Circle:
                case GraphicTypes.Ellipse:
                    ESRI.ArcGIS.Display.ISimpleFillSymbol simpleFillSymbol = new ESRI.ArcGIS.Display.SimpleFillSymbol();
                    ESRI.ArcGIS.Display.IRgbColor rgbColor = new ESRI.ArcGIS.Display.RgbColorClass();
                    simpleFillSymbol.Color = rgbColor as IColor;
                                        
                    ISimpleLineSymbol outlineSymbol = new SimpleLineSymbolClass();
                    outlineSymbol.Color = new RgbColorClass() { Red = 255, Blue = 0, Green = 0 } as IColor;
                    outlineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                    outlineSymbol.Width = 3;
                    simpleFillSymbol.Color.Transparency = 100;
                    simpleFillSymbol.Outline = outlineSymbol;
                    symbol = simpleFillSymbol as ISymbol;
                    break;
                case GraphicTypes.Point:
                    break;
                default:
                    break;
            };

            uvRenderer.DefaultSymbol = symbol;
            uvRenderer.UseDefaultSymbol = true;
            geoLayer.Renderer = uvRenderer as IFeatureRenderer;
        }
    }


}
