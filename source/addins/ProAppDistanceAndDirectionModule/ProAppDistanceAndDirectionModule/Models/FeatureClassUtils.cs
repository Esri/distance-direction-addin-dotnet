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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// Esri
using ArcGIS.Desktop.Catalog;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Mapping;

using DistanceAndDirectionLibrary;
using System.Windows;

namespace ProAppDistanceAndDirectionModule.Models
{
    class FeatureClassUtils
    {
        private string previousLocation = "";
        private string previousSaveType = "";

        /// <summary>
        /// Prompts the user to save features
        /// 
        /// </summary>
        /// <returns>The path to selected output (fgdb/shapefile)</returns>
        public string PromptUserWithSaveDialog(bool featureChecked, bool shapeChecked, bool kmlChecked)
        {
            //Prep the dialog
            SaveItemDialog saveItemDlg = new SaveItemDialog();
            saveItemDlg.Title = "Select output";
            saveItemDlg.OverwritePrompt = true;
            string saveType = featureChecked ? "gdb" : "file";
            if (string.IsNullOrEmpty(previousSaveType))
                previousSaveType = saveType;
            if (!string.IsNullOrEmpty(previousLocation) && previousSaveType == saveType)
                saveItemDlg.InitialLocation = previousLocation;
            else
            {
                if (featureChecked)
                    saveItemDlg.InitialLocation = ArcGIS.Desktop.Core.Project.Current.DefaultGeodatabasePath;
                else
                    saveItemDlg.InitialLocation = ArcGIS.Desktop.Core.Project.Current.HomeFolderPath;
            }
            previousSaveType = saveType;

            // Set the filters and default extension
            if (featureChecked)
            {
                saveItemDlg.Filter = ItemFilters.geodatabaseItems_all;
            }
            else if (shapeChecked)
            {
                saveItemDlg.Filter = ItemFilters.shapefiles;
                saveItemDlg.DefaultExt = "shp";
            }
            else if (kmlChecked)
            {
                saveItemDlg.Filter = ItemFilters.kml;
                saveItemDlg.DefaultExt = "kmz";
            }

            bool? ok = saveItemDlg.ShowDialog();

            //Show the dialog and get the response
            if (ok == true)
            {
                if (featureChecked == true && saveItemDlg.FilePath.IndexOf(".gdb") == -1)
                {
                    MessageBox.Show(DistanceAndDirectionLibrary.Properties.Resources.FeatureClassNameError,
                        DistanceAndDirectionLibrary.Properties.Resources.DistanceDirectionLabel, MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                } 
                else
                {
                    previousLocation = Path.GetDirectoryName(saveItemDlg.FilePath);                
                    return saveItemDlg.FilePath;
                }            
            }
            return null;
        }

        /// <summary>
        /// Creates the output featureclass, either fgdb featureclass or shapefile
        /// </summary>
        /// <param name="outputPath">location of featureclass</param>
        /// <param name="saveAsType">Type of output selected, either fgdb featureclass or shapefile</param>
        /// <param name="graphicsList">List of graphics for selected tab</param>
        /// <param name="spatialRef">Spatial Reference being used</param>
        /// <param name="mapview">Mapview</param>
        /// <param name="geomType">geomType to create</param>
        /// <param name="isKML">Create KML</param>
        /// <returns>Output featureclass</returns>
        public async Task CreateFCOutput(string outputPath, SaveAsType saveAsType, List<Graphic> graphicsList, SpatialReference spatialRef, MapView mapview, GeomType geomType, bool isKML = false)
        {
            string dataset = System.IO.Path.GetFileName(outputPath);
            string connection = System.IO.Path.GetDirectoryName(outputPath);
            
            try
            {
                await QueuedTask.Run(async () =>
                {
                    await CreateFeatureClass(dataset, geomType, connection, spatialRef, graphicsList, mapview, isKML);
                });  
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Create polyline features from graphics and add to table
        /// </summary>
        /// <param name="graphicsList">List of graphics to add to table</param>
        /// <returns></returns>
        private static async Task CreateFeatures(List<Graphic> graphicsList, bool isKML)
        {
            
            RowBuffer rowBuffer = null;
            bool isLine = false;
                
            try
            {
                await QueuedTask.Run(() =>
                {
                    var layer = MapView.Active.GetSelectedLayers()[0];
                    if (layer is FeatureLayer)
                    {
                        var featureLayer = layer as FeatureLayer;
                        
                        using (var table = featureLayer.GetTable())
                        {
                            TableDefinition definition = table.GetDefinition();
                            int shapeIndex = definition.FindField("Shape");

                            string graphicsType;
                            foreach (Graphic graphic in graphicsList)
                            {
                                rowBuffer = table.CreateRowBuffer();

                                if (graphic.Geometry is Polyline)
                                {
                                    PolylineBuilder pb = new PolylineBuilder((Polyline)graphic.Geometry);
                                    pb.HasZ = false;
                                    rowBuffer[shapeIndex] = pb.ToGeometry();
                                    isLine = true;

                                    // Only add attributes for Esri format

                                    // Add attributes
                                    graphicsType = graphic.p.GetType().ToString().Replace("ProAppDistanceAndDirectionModule.", "");
                                    switch (graphicsType)
                                    {
                                        case "LineAttributes":
                                            {
                                                try
                                                {
                                                    // Add attributes
                                                    rowBuffer[definition.FindField("Distance")] = ((LineAttributes)graphic.p)._distance;
                                                    rowBuffer[definition.FindField("DistUnit")] = ((LineAttributes)graphic.p).distanceunit;
                                                    rowBuffer[definition.FindField("Angle")] = ((LineAttributes)graphic.p).angle;
                                                    rowBuffer[definition.FindField("AngleUnit")] = ((LineAttributes)graphic.p).angleunit;
                                                    rowBuffer[definition.FindField("OriginX")] = ((LineAttributes)graphic.p).originx;
                                                    rowBuffer[definition.FindField("OriginY")] = ((LineAttributes)graphic.p).originy;
                                                    rowBuffer[definition.FindField("DestX")] = ((LineAttributes)graphic.p).destinationx;
                                                    rowBuffer[definition.FindField("DestY")] = ((LineAttributes)graphic.p).destinationy;
                                                    break;
                                                }
                                                // Catch exception likely due to missing fields
                                                // Just skip attempting to write to fields
                                                catch
                                                {
                                                    break;
                                                }
                                            }
                                        case "RangeAttributes":
                                            {
                                                try
                                                {
                                                    rowBuffer[definition.FindField("Rings")] = ((RangeAttributes)graphic.p).numRings;
                                                    rowBuffer[definition.FindField("Distance")] = ((RangeAttributes)graphic.p).distance;
                                                    rowBuffer[definition.FindField("DistUnit")] = ((RangeAttributes)graphic.p).distanceunit;
                                                    rowBuffer[definition.FindField("Radials")] = ((RangeAttributes)graphic.p).numRadials;
                                                    rowBuffer[definition.FindField("CenterX")] = ((RangeAttributes)graphic.p).centerx;
                                                    rowBuffer[definition.FindField("CenterY")] = ((RangeAttributes)graphic.p).centery;
                                                    break;
                                                }
                                                catch
                                                {
                                                    break;
                                                }
                                            }
                                    }

                                }
                                else if (graphic.Geometry is Polygon)
                                {
                                    rowBuffer[shapeIndex] = new PolygonBuilder((Polygon)graphic.Geometry).ToGeometry();

                                    // Only add attributes for Esri format

                                    // Add attributes
                                    graphicsType = graphic.p.GetType().ToString().Replace("ProAppDistanceAndDirectionModule.", "");
                                    switch (graphicsType)
                                    {
                                        case "CircleAttributes":
                                            {
                                                try
                                                {
                                                    rowBuffer[definition.FindField("Distance")] = ((CircleAttributes)graphic.p).distance;
                                                    rowBuffer[definition.FindField("DistUnit")] = ((CircleAttributes)graphic.p).distanceunit;
                                                    rowBuffer[definition.FindField("DistType")] = ((CircleAttributes)graphic.p).circletype;
                                                    rowBuffer[definition.FindField("CenterX")] = ((CircleAttributes)graphic.p).centerx;
                                                    rowBuffer[definition.FindField("CenterY")] = ((CircleAttributes)graphic.p).centery;
                                                    break;
                                                }
                                                catch (Exception ex)
                                                {
                                                    System.Diagnostics.Debug.WriteLine(ex.Message);
                                                    break;
                                                }
                                            }
                                        case "EllipseAttributes":
                                            try
                                            {
                                                rowBuffer[definition.FindField("Minor")] = ((EllipseAttributes)graphic.p).minorAxis;
                                                rowBuffer[definition.FindField("Major")] = ((EllipseAttributes)graphic.p).majorAxis;
                                                rowBuffer[definition.FindField("DistUnit")] = ((EllipseAttributes)graphic.p).distanceunit;
                                                rowBuffer[definition.FindField("CenterX")] = ((EllipseAttributes)graphic.p).centerx;
                                                rowBuffer[definition.FindField("CenterY")] = ((EllipseAttributes)graphic.p).centery;
                                                rowBuffer[definition.FindField("Angle")] = ((EllipseAttributes)graphic.p).angle;
                                                rowBuffer[definition.FindField("AngleUnit")] = ((EllipseAttributes)graphic.p).angleunit;
                                                break;
                                            }
                                            catch
                                            {
                                                break;
                                            }
                                    }

                                }

                                Row row = table.CreateRow(rowBuffer);
                            }
                        }

                        //Get simple renderer from feature layer 
                        CIMSimpleRenderer currentRenderer = featureLayer.GetRenderer() as CIMSimpleRenderer;
                        CIMSymbolReference symbol = currentRenderer.Symbol;

                        var outline = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.RedRGB, 1.0, SimpleLineStyle.Solid);
                        CIMSymbol s;
                        if(isLine)
                            s = SymbolFactory.Instance.ConstructLineSymbol(outline);
                        else
                            s = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.RedRGB, SimpleFillStyle.Null, outline);
                        CIMSymbolReference symbolRef = new CIMSymbolReference() { Symbol = s };
                        currentRenderer.Symbol = symbolRef;

                        featureLayer.SetRenderer(currentRenderer);

                    }
                });

            }
            catch (GeodatabaseException exObj)
            {
                System.Diagnostics.Debug.WriteLine(exObj);
                throw;
            }
            finally
            {
                if (rowBuffer != null)
                    rowBuffer.Dispose();
            }
        }

        private static IReadOnlyList<string> makeValueArray (string featureClass, string fieldName, string fieldType)
        {
            List<object> arguments = new List<object>();
            arguments.Add(featureClass);
            arguments.Add(fieldName);
            arguments.Add(fieldType);
            return Geoprocessing.MakeValueArray(arguments.ToArray());
        }

        /// <summary>
        /// Create a feature class
        /// </summary>
        /// <param name="dataset">Name of the feature class to be created.</param>
        /// <param name="geomType">Type of feature class to be created. Options are:
        /// <list type="bullet">
        /// <item>POINT</item>
        /// <item>MULTIPOINT</item>
        /// <item>POLYLINE</item>
        /// <item>POLYGON</item></list></param>
        /// <param name="connection">connection path</param>
        /// <param name="spatialRef">SpatialReference</param>
        /// <param name="graphicsList">List of graphics</param>
        /// <param name="mapview">MapView object</param>
        /// <param name="isKML">Is this a kml output</param>
        /// <returns></returns>
        private static async Task CreateFeatureClass(string dataset, GeomType geomType, string connection, SpatialReference spatialRef, List<Graphic> graphicsList, MapView mapview, bool isKML = false)
        {
            try
            {
                List<Graphic> list = ClearTempGraphics(graphicsList);

                if ((list == null) || (list.Count == 0))
                    return;

                string strGeomType = geomType == GeomType.PolyLine ? "POLYLINE" : "POLYGON";
                
                List<object> arguments = new List<object>();
                // store the results in the geodatabase
                arguments.Add(connection);
                // name of the feature class
                arguments.Add(dataset);
                // type of geometry
                arguments.Add(strGeomType);
                // no template
                arguments.Add(null);
                // no m values
                arguments.Add("DISABLED");
                // no z values
                arguments.Add("DISABLED");
                arguments.Add(spatialRef.Wkid.ToString());
                
                
                // store the results in the geodatabase
                object[] argArray = arguments.ToArray();

                var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);
                //var valueArray = Geoprocessing.MakeValueArray(argArray);

                IGPResult result = await Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management", 
                    Geoprocessing.MakeValueArray(argArray), 
                    environments, 
                    null, 
                    null);


                // Add additional fields based on type of graphic
                string nameNoExtension = Path.GetFileNameWithoutExtension(dataset);
                string featureClass = "";
                if (isKML)
                {
                    featureClass = connection + "/" + nameNoExtension + ".shp";
                }
                else
                {
                    featureClass = connection + "/" + dataset;
                }
                
                string graphicsType = list[0].p.GetType().ToString().Replace("ProAppDistanceAndDirectionModule.", "");
                switch (graphicsType)
                {
                    case "LineAttributes":
                        {
                            IGPResult result2 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "Distance", "DOUBLE"));
                            IGPResult result3 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "DistUnit", "TEXT"));
                            IGPResult result4 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "OriginX", "DOUBLE"));
                            IGPResult result5 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "OriginY", "DOUBLE"));
                            IGPResult result6 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "DestX", "DOUBLE"));
                            IGPResult result7 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "DestY", "DOUBLE"));
                            IGPResult result8 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "Angle", "DOUBLE"));
                            IGPResult result9 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "AngleUnit", "TEXT"));
                            break;
                        }
                    case "CircleAttributes":
                        {
                            IGPResult result2 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "Distance", "DOUBLE"));
                            IGPResult result3 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "DistUnit", "TEXT"));
                            IGPResult result4 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "DistType", "TEXT"));
                            IGPResult result5 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "CenterX", "DOUBLE"));
                            IGPResult result6 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "CenterY", "DOUBLE"));
                            break;
                        }
                    case "EllipseAttributes":
                        {
                            IGPResult result2 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "Minor", "DOUBLE"));
                            IGPResult result3 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "Major", "DOUBLE"));
                            IGPResult result4 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "DistUnit", "TEXT"));
                            IGPResult result5 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "CenterX", "DOUBLE"));
                            IGPResult result6 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "CenterY", "DOUBLE"));
                            IGPResult result7 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "Angle", "DOUBLE"));
                            IGPResult result8 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "AngleUnit", "TEXT"));
                            break;
                        }
                    case "RangeAttributes":
                        {
                            IGPResult result2 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "Rings", "LONG"));
                            IGPResult result3 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "Distance", "DOUBLE"));
                            IGPResult result4 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "DistUnit", "TEXT"));
                            IGPResult result5 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "Radials", "LONG"));
                            IGPResult result6 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "CenterX", "DOUBLE"));
                            IGPResult result7 = await Geoprocessing.ExecuteToolAsync("AddField_management", makeValueArray(featureClass, "CenterY", "DOUBLE"));
                            break;
                        }
                }


                await CreateFeatures(list, isKML);

                if (isKML)
                {
                    await KMLUtils.ConvertLayerToKML(connection, dataset, MapView.Active);

                    // Delete temporary Shapefile
                    string[] extensionNames = { ".cpg", ".dbf", ".prj", ".shx", ".shp", ".sbn", ".sbx", ".shp.xml" };
                    string datasetNoExtension = Path.GetFileNameWithoutExtension(dataset);
                    foreach (string extension in extensionNames)
                    {
                        string shapeFile = Path.Combine(connection, datasetNoExtension + extension);
                        string shapefileproj = Path.Combine(connection, datasetNoExtension + "_proj" + extension);
                        if (File.Exists(shapeFile))
                            File.Delete(shapeFile);
                        if (File.Exists(shapefileproj))
                            File.Delete(shapefileproj);
                        
                    }
                    DirectoryInfo dir = new DirectoryInfo(connection);
                    FileSystemInfo fsi = dir;
                    fsi.Refresh();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private static List<Graphic> ClearTempGraphics(List<Graphic> graphicsList)
        {

            List<Graphic> list = new List<Graphic>();
            foreach (var item in graphicsList)
            {
                
                if (!item.IsTemp)
                {
                    list.Add(item);
                }
            }
            return list;
        }
    }
}
