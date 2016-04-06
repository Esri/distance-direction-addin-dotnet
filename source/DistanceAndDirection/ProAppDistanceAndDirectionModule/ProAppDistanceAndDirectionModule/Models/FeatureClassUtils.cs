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

// Esri


using DistanceAndDirectionLibrary;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Mapping;

namespace ProAppDistanceAndDirectionModule.Models
{
    class FeatureClassUtils
    {
        private SaveItemDialog saveItemDlg = null;
        private string previousLocation = "";

        /// <summary>
        /// Prompts the user to save features
        /// 
        /// </summary>
        /// <returns>The path to selected output (fgdb/shapefile)</returns>
        public string PromptUserWithSaveDialog(bool featureChecked, bool shapeChecked, bool kmlChecked)
        {
            //Prep the dialog
            if (saveItemDlg == null)
            {
                saveItemDlg = new SaveItemDialog();
                saveItemDlg.Title = "Select output";
                saveItemDlg.OverwritePrompt = true;
            }
            else
            {
                saveItemDlg.InitialLocation = previousLocation;
            }

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
                string folderName = System.IO.Path.GetDirectoryName(saveItemDlg.FilePath);
                string fileName = System.IO.Path.GetFileName(saveItemDlg.FilePath);
                previousLocation = folderName;

                return saveItemDlg.FilePath;

                //IGxObject ipGxObject = m_ipSaveAsGxDialog.FinalLocation;
                //string nameString = m_ipSaveAsGxDialog.Name;
                //bool replacingObject = m_ipSaveAsGxDialog.ReplacingObject;
                //string path = m_ipSaveAsGxDialog.FinalLocation.FullName + "\\" + m_ipSaveAsGxDialog.Name;
                //IGxObject ipSelectedObject = m_ipSaveAsGxDialog.InternalCatalog.SelectedObject;

                //// user selected an existing featureclass
                //if (ipSelectedObject != null && ipSelectedObject is IGxDataset)
                //{
                //    IGxDataset ipGxDataset = (IGxDataset)ipSelectedObject;
                //    IDataset ipDataset = ipGxDataset.Dataset;

                //    // User will be prompted if they select an existing shapefile
                //    if ( ipDataset.Category.Equals("Shapefile Feature Class"))
                //    {
                //        return path;
                //    }

                //    while (DoesFeatureClassExist(ipDataset.Workspace.PathName, m_ipSaveAsGxDialog.Name))
                //    {
                //        if (System.Windows.Forms.MessageBox.Show("You've selected a feature class that already exists. Do you wish to replace it?", "Overwrite Feature Class", System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                //        {
                //            return m_ipSaveAsGxDialog.FinalLocation.FullName + "\\" + m_ipSaveAsGxDialog.Name;
                //        }

                //        if (m_ipSaveAsGxDialog.DoModalSave(iParentWindow) == false)
                //        {
                //            return null;
                //        }

                //        if (ipSelectedObject != null && ipSelectedObject is IGxDataset)
                //        {
                //            ipGxDataset = (IGxDataset)ipSelectedObject;
                //            ipDataset = ipGxDataset.Dataset;
                //        }
                //    }

                //    return m_ipSaveAsGxDialog.FinalLocation.FullName + "\\" + m_ipSaveAsGxDialog.Name;
                //}
                //else
                //    return path;
            }
            return null;
        }

        /// <summary>
        /// Creates the output featureclass, either fgdb featureclass or a shapefile
        /// </summary>
        /// <param name="outputPath">location of featureclass</param>
        /// <param name="saveAsType">Type of output selected, either fgdb featureclass or shapefile</param>
        /// <param name="graphicsList">List of graphics for selected tab</param>
        /// <param name="ipSpatialRef">Spatial Reference being used</param>
        /// <returns>Output featureclass</returns>
        public async Task CreateFCOutput(string outputPath, SaveAsType saveAsType, List<Graphic> graphicsList, SpatialReference spatialRef, MapView mapview)
        {
            string dataset = System.IO.Path.GetFileName(outputPath);
            string connection = System.IO.Path.GetDirectoryName(outputPath);
            
            try
            {
                if (saveAsType == SaveAsType.FileGDB || saveAsType == SaveAsType.Shapefile)
                {
                    
                    await QueuedTask.Run(async () =>
                    {
                        await CreateFeatureClass(dataset, "POLYLINE", connection, spatialRef);
                        await CreatePolyLineFeatures(connection, dataset, graphicsList, mapview);
                    });
                }
                else if (saveAsType == SaveAsType.KML)
                {
                    //await QueuedTask.Run(async () =>
                    //{
                    //    await CreateFeatureClass(dataset, "POLYLINE", connection, spatialRef);
                    //    await CreatePolyLineFeatures(connection, dataset, graphicsList, mapview);
                    //});
                }

                    //if (DoesFeatureClassExist(folderName, fcName))
                    //{
                    //    DeleteFeatureClass(fWorkspace, fcName);
                    //}

                    //fc = CreatePolylineFeatureClass(fWorkspace, fcName);

                    //foreach (Graphic graphic in graphicsList)
                    //{
                    //    IFeature feature = fc.CreateFeature();

                    //    feature.Shape = graphic.Geometry;
                    //    feature.Store();
                    //}

                
                //else if (saveAsType == SaveAsType.Shapefile)
                //{
                //    // already asked them for confirmation to overwrite file
                //    if (File.Exists(outputPath))
                //    {
                //        DeleteShapeFile(outputPath);
                //    }            

                //    fc = ExportToShapefile(outputPath, graphicsList, ipSpatialRef);
                //}
                //return fc;
            }
            catch (Exception ex)
            {

            }
        }

        private static async Task CreatePolyLineFeaturesOld(string gdbPath, string dataset, List<Graphic> graphicsList)
        {
            using (Geodatabase fileGeodatabase = new Geodatabase(gdbPath))
            using (FeatureClass featureClass = fileGeodatabase.OpenDataset<FeatureClass>(dataset))
            using (FeatureClassDefinition featureClassDefinition = featureClass.GetDefinition())
            {
                RowBuffer rowBuffer = null;
                Feature feature = null;

                try
                {
                    //EditOperation editOperation = new EditOperation();
                    //editOperation.Callback(context =>
                    //{
                        foreach (Graphic graphic in graphicsList)
                        {
                            //int nameIndex = featureClassDefinition.FindField("NAME");
                            rowBuffer = featureClass.CreateRowBuffer();

                            rowBuffer[featureClassDefinition.GetShapeField()] = new PolylineBuilder(graphic.Geometry as Polyline).ToGeometry();

                            feature = featureClass.CreateRow(rowBuffer);

                            //To Indicate that the Map has to draw this feature and/or the attribute table to be updated
                            //context.Invalidate(feature); 
                        }

                        // Do some other processing with the newly-created feature.

                    //}, featureClass);

                    //bool editResult = editOperation.Execute();

                    // If the table is non-versioned this is a no-op. If it is versioned, we need the Save to be done for the edits to be persisted.
                    bool saveResult = await Project.Current.SaveEditsAsync();
                }
                catch (GeodatabaseException exObj)
                {
                    Console.WriteLine(exObj);
                    throw;
                }
                finally
                {
                    if (rowBuffer != null)
                        rowBuffer.Dispose();

                    if (feature != null)
                        feature.Dispose();
                }
            }
        }

        private static async Task CreatePolyLineFeatures(string shapefile, string dataset, List<Graphic> graphicsList, MapView mapview)
        {
            {
                RowBuffer rowBuffer = null;
                Feature feature = null;

                try
                {
                    await QueuedTask.Run(async () =>
                    {
                        var layer = MapView.Active.GetSelectedLayers()[0];
                        if (layer is FeatureLayer)
                        {
                            var featureLayer = layer as FeatureLayer;
                            //if (featureLayer.GetTable().GetDatastore() is UnknownDatastore)
                            //    return;
                            using (var table = featureLayer.GetTable())
                            {
                                TableDefinition definition = table.GetDefinition();
                                int shapeIndex = definition.FindField("Shape");
                                foreach (Graphic graphic in graphicsList)
                                {
                                    //int nameIndex = featureClassDefinition.FindField("NAME");
                                    rowBuffer = table.CreateRowBuffer();

                                    rowBuffer[shapeIndex] = new PolylineBuilder(graphic.Geometry as Polyline).ToGeometry();

                                    table.CreateRow(rowBuffer);

                                    //To Indicate that the Map has to draw this feature and/or the attribute table to be updated
                                    //context.Invalidate(feature);
                                }
                            }
                        }
                    });

                    //EditOperation editOperation = new EditOperation();
                    //editOperation.Callback(context =>
                    //{
                        //foreach (Graphic graphic in graphicsList)
                        //{
                        //    //int nameIndex = featureClassDefinition.FindField("NAME");
                        //    rowBuffer = featureClass.CreateRowBuffer();

                        //    //rowBuffer[featureClassDefinition.GetShapeField()] = new MapPointBuilder(1028367, 1809789).ToGeometry();
                        //    rowBuffer[featureClassDefinition.GetShapeField()] = new PolylineBuilder(graphic.Geometry as Polyline).ToGeometry();

                        //    feature = featureClass.CreateRow(rowBuffer);

                        //    //To Indicate that the Map has to draw this feature and/or the attribute table to be updated
                        //    context.Invalidate(feature);
                        //}

                        // Do some other processing with the newly-created feature.

                    //}, featureClass);

                    //bool editResult = editOperation.Execute();

                    // If the table is non-versioned this is a no-op. If it is versioned, we need the Save to be done for the edits to be persisted.
                    //bool saveResult = await Project.Current.SaveEditsAsync();
                }
                catch (GeodatabaseException exObj)
                {
                    Console.WriteLine(exObj);
                    throw;
                }
                finally
                {
                    if (rowBuffer != null)
                        rowBuffer.Dispose();

                    if (feature != null)
                        feature.Dispose();
                }
            }
        }

        /// <summary>
        /// Create a feature class
        /// </summary>
        /// <param name="featureclassName">Name of the feature class to be created.</param>
        /// <param name="featureclassType">Type of feature class to be created. Options are:
        /// <list type="bullet">
        /// <item>POINT</item>
        /// <item>MULTIPOINT</item>
        /// <item>POLYLINE</item>
        /// <item>POLYGON</item></list></param>
        /// <returns></returns>
        private static async Task CreateFeatureClass(string featureclassName, string featureclassType, string gdbPath, SpatialReference spatialRef)
        {
            try
            {
                List<object> arguments = new List<object>();
                // store the results in the geodatabase
                arguments.Add(gdbPath);
                // name of the feature class
                arguments.Add(featureclassName);
                // type of geometry
                arguments.Add(featureclassType);
                // no template
                arguments.Add("");
                // no z values
                arguments.Add("DISABLED");
                // no m values
                arguments.Add("DISABLED");
                arguments.Add(spatialRef);

                var valueArray = Geoprocessing.MakeValueArray(arguments.ToArray());
                IGPResult result = await Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management", valueArray);            

            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Create a feature class
        /// </summary>
        /// <param name="featureclassName">Name of the feature class to be created.</param>
        /// <param name="featureclassType">Type of feature class to be created. Options are:
        /// <list type="bullet">
        /// <item>POINT</item>
        /// <item>MULTIPOINT</item>
        /// <item>POLYLINE</item>
        /// <item>POLYGON</item></list></param>
        /// <returns></returns>
        public async Task CreateShapefile(string shapefileName, string featureclassType, string outputPath)
        {
            string folder = System.IO.Path.GetDirectoryName(outputPath);
            try
            {
                List<object> arguments = new List<object>();
                // store the results in a shapefile
                arguments.Add(folder);
                // name of the feature class
                arguments.Add(shapefileName);
                // type of geometry
                arguments.Add(featureclassType);
                // no template
                arguments.Add("");
                // no z values
                arguments.Add("DISABLED");
                // no m values
                arguments.Add("DISABLED");

                await QueuedTask.Run(() =>
                {
                    // spatial reference
                    arguments.Add(SpatialReferenceBuilder.CreateSpatialReference(3857));
                });

                var valueArray = Geoprocessing.MakeValueArray(arguments.ToArray());
                IGPResult result = await Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management", valueArray);
            }
            catch (Exception ex)
            {

            }
        }

        /*
        public void DeleteShapeFile(string shapeFilePath)
        {
            string fcName = System.IO.Path.GetFileName(shapeFilePath);
            string folderName = System.IO.Path.GetDirectoryName(shapeFilePath);

            using (ComReleaser oComReleaser = new ComReleaser())
            {
                IWorkspaceFactory workspaceFactory = new ShapefileWorkspaceFactory();
                IWorkspace workspace = workspaceFactory.OpenFromFile(folderName, 0);
                IFeatureWorkspace fWorkspace = (IFeatureWorkspace)workspace;
                IDataset ipDs = fWorkspace.OpenFeatureClass(fcName) as IDataset;
                ipDs.Delete();

                File.Delete(shapeFilePath);

                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(workspace);
                workspace = null;
                fWorkspace = null;
                ipDs = null;
            }
 
            GC.Collect();
            
        }

        /// <summary>
        /// Export graphics to a shapefile
        /// </summary>
        /// <param name="fileNamePath">Path to shapefile</param>
        /// <param name="graphicsList">List of graphics for selected tab</param>
        /// <param name="ipSpatialRef">Spatial Reference being used</param>
        /// <returns>Created featureclass</returns>
        private IFeatureClass ExportToShapefile(string fileNamePath, List<Graphic> graphicsList, ISpatialReference ipSpatialRef)
        {
            int index = fileNamePath.LastIndexOf('\\');
            string folder = fileNamePath.Substring(0, index);
            string nameOfShapeFile = fileNamePath.Substring(index + 1);
            string shapeFieldName = "Shape";
            IFeatureClass featClass = null;

            using (ComReleaser oComReleaser = new ComReleaser())
            {
                try
                {
                    IWorkspaceFactory workspaceFactory = null;
                    workspaceFactory = new ShapefileWorkspaceFactoryClass();
                    IWorkspace workspace = workspaceFactory.OpenFromFile(folder, 0);
                    IFeatureWorkspace featureWorkspace = workspace as IFeatureWorkspace;
                    IFields fields = null;
                    IFieldsEdit fieldsEdit = null;
                    fields = new Fields();
                    fieldsEdit = (IFieldsEdit)fields;
                    IField field = null;
                    IFieldEdit fieldEdit = null;
                    field = new FieldClass();///###########
                    fieldEdit = (IFieldEdit)field;
                    fieldEdit.Name_2 = "Shape";
                    fieldEdit.Type_2 = (esriFieldType.esriFieldTypeGeometry);
                    IGeometryDef geomDef = null;
                    IGeometryDefEdit geomDefEdit = null;
                    geomDef = new GeometryDefClass();///#########
                    geomDefEdit = (IGeometryDefEdit)geomDef;

                    //This is for line shapefiles
                    geomDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
                    geomDefEdit.SpatialReference_2 = ipSpatialRef;

                    fieldEdit.GeometryDef_2 = geomDef;
                    fieldsEdit.AddField(field);

                    ////Add another miscellaneous text field
                    //field = new FieldClass();
                    //fieldEdit = (IFieldEdit)field;
                    //fieldEdit.Length_2 = 30;
                    //fieldEdit.Name_2 = "TextField";
                    //fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                    //fieldsEdit.AddField(field);

                    featClass = featureWorkspace.CreateFeatureClass(nameOfShapeFile, fields, null, null, esriFeatureType.esriFTSimple, shapeFieldName, "");

                    foreach (Graphic graphic in graphicsList)
                    {
                        IFeature feature = featClass.CreateFeature();

                        feature.Shape = graphic.Geometry;
                        feature.Store();
                    }

                    IFeatureLayer featurelayer = null;
                    featurelayer = new FeatureLayerClass();
                    featurelayer.FeatureClass = featClass;
                    featurelayer.Name = featClass.AliasName;

                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(workspace);
                    workspace = null;
                    GC.Collect();

                    return featClass;
                }
                catch (Exception ex)
                {
                    return featClass;
                }
            }
        }

        /// <summary>
        /// Determines if selected feature class already exists
        /// </summary>
        /// <param name="gdbPath">Path to the file gdb</param>
        /// <param name="fcName">Name of selected feature class</param>
        /// <returns>True if already exists, false otherwise</returns>
        private bool DoesFeatureClassExist(string gdbPath, string fcName)
        {
            List<string> dsNames = GetAllDatasetNames(gdbPath);

            if (dsNames.Contains(fcName))
                return true;

            return false;
        }

        /// <summary>
        /// Retrieves all datasets names from filegdb
        /// </summary>
        /// <param name="gdbFilePath">Path to filegdb</param>
        /// <returns>List of names of all featureclasses in filegdb</returns>
        private List<string> GetAllDatasetNames(string gdbFilePath)
        {
            IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactory();
            IWorkspace workspace = workspaceFactory.OpenFromFile (gdbFilePath, 0);
            IEnumDataset enumDataset = workspace.get_Datasets(esriDatasetType.esriDTAny);
            List<string> names = new List<string>();
            IDataset dataset = null;
            while((dataset = enumDataset.Next())!= null)
            {
                names.Add(dataset.Name);
            }
            return names;
        }

        /// <summary>
        /// Delete a featureclass
        /// </summary>
        /// <param name="fWorkspace">IFeatureWorkspace</param>
        /// <param name="fcName">Name of featureclass to delete</param>
        private void DeleteFeatureClass(IFeatureWorkspace fWorkspace, string fcName)
        {
            IDataset ipDs = fWorkspace.OpenFeatureClass(fcName) as IDataset;
            ipDs.Delete();
        }

        /// <summary> 
        /// Create the polyline feature class 
        /// </summary> 
        /// <param name="featWorkspace">IFeatureWorkspace</param> 
        /// <param name="name">Name of the featureclass</param> 
        /// <returns>IFeatureClass</returns> 
        private IFeatureClass CreatePolylineFeatureClass(IFeatureWorkspace featWorkspace, string name)
        {
            IFieldsEdit pFldsEdt = new FieldsClass();
            IFieldEdit pFldEdt = new FieldClass();

            pFldEdt = new FieldClass();
            pFldEdt.Type_2 = esriFieldType.esriFieldTypeOID;
            pFldEdt.Name_2 = "OBJECTID";
            pFldEdt.AliasName_2 = "OBJECTID";
            pFldsEdt.AddField(pFldEdt);

            IGeometryDefEdit pGeoDef;
            pGeoDef = new GeometryDefClass();
            pGeoDef.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
            pGeoDef.SpatialReference_2 = ArcMap.Document.FocusMap.SpatialReference;

            pFldEdt = new FieldClass();
            pFldEdt.Name_2 = "SHAPE";
            pFldEdt.AliasName_2 = "SHAPE";
            pFldEdt.Type_2 = esriFieldType.esriFieldTypeGeometry;
            pFldEdt.GeometryDef_2 = pGeoDef;
            pFldsEdt.AddField(pFldEdt);

            IFeatureClass pFClass = featWorkspace.CreateFeatureClass(name, pFldsEdt, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");

            return pFClass;
        }
 */

    }

    
}
