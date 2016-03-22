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

namespace ArcMapAddinGeodesyAndRange.Models
{
    class FeatureClassUtils
    {
        private IGxDialog m_ipSaveAsGxDialog = null; 

        /// <summary>
        /// Prompts the user to save features
        /// 
        /// Use "this.Handle.ToInt32()" as the parentWindow id 
        /// </summary>
        /// <param name="iParentWindow">The window handle of the parent window</param>
        /// <returns>The path to selected output (fgdb/shapefile)</returns>
        public string PromptUserWithGxDialog(int iParentWindow)
        {
            //Prep the dialog
            if (m_ipSaveAsGxDialog == null)
            {
                m_ipSaveAsGxDialog = new GxDialog();
                IGxObjectFilterCollection ipGxObjFilterCol = (IGxObjectFilterCollection)m_ipSaveAsGxDialog;
                ipGxObjFilterCol.RemoveAllFilters();

                // Add the filters
                ipGxObjFilterCol.AddFilter(new GxFilterFGDBFeatureClasses(), false);
                ipGxObjFilterCol.AddFilter(new GxFilterShapefilesClass(), false);

                m_ipSaveAsGxDialog.AllowMultiSelect = false;
                m_ipSaveAsGxDialog.Title = "Select output";
                m_ipSaveAsGxDialog.ButtonCaption = "OK";
                m_ipSaveAsGxDialog.RememberLocation = true;
            }
            else
            {
                m_ipSaveAsGxDialog.FinalLocation.Refresh();
            }

            //Show the dialog and get the response
            if (m_ipSaveAsGxDialog.DoModalSave(iParentWindow) == false)
                return null;
            else
            {
                IGxObject ipGxObject = m_ipSaveAsGxDialog.FinalLocation;
                string nameString = m_ipSaveAsGxDialog.Name;
                bool replacingObject = m_ipSaveAsGxDialog.ReplacingObject;
                string path = m_ipSaveAsGxDialog.FinalLocation.FullName + "\\" + m_ipSaveAsGxDialog.Name;
                IGxObject ipSelectedObject = m_ipSaveAsGxDialog.InternalCatalog.SelectedObject;

                // user selected an existing featureclass
                if (ipSelectedObject != null && ipSelectedObject is IGxDataset)
                {
                    IGxDataset ipGxDataset = (IGxDataset)ipSelectedObject;
                    IDataset ipDataset = ipGxDataset.Dataset;

                    // User will be prompted if they select an existing shapefile
                    if ( ipDataset.Category.Equals("Shapefile Feature Class"))
                    {
                        return path;
                    }

                    while (DoesFeatureClassExist(ipDataset.Workspace.PathName, ipDataset.BrowseName))
                    {
                        if (System.Windows.Forms.MessageBox.Show("You've selected a feature class that already exists. Do you wish to replace it?", "Overwrite Feature Class", System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                        {
                            return m_ipSaveAsGxDialog.FinalLocation.FullName + "\\" + m_ipSaveAsGxDialog.Name;
                        }

                        if (m_ipSaveAsGxDialog.DoModalSave(iParentWindow) == false)
                        {
                            return null;
                        }

                        if (ipSelectedObject != null && ipSelectedObject is IGxDataset)
                        {
                            ipGxDataset = (IGxDataset)ipSelectedObject;
                            ipDataset = ipGxDataset.Dataset;
                        }
                    }

                    return m_ipSaveAsGxDialog.FinalLocation.FullName + "\\" + m_ipSaveAsGxDialog.Name;
                }
                else
                    return path;
            }
        }

        /// <summary>
        /// Creates the output featureclass, either fgdb featureclass or a shapefile
        /// </summary>
        /// <param name="outputPath">location of featureclass</param>
        /// <param name="saveAsType">Type of output selected, either fgdb featureclass or shapefile</param>
        /// <param name="graphicsList">List of graphics for selected tab</param>
        /// <param name="ipSpatialRef">Spatial Reference being used</param>
        /// <returns>Output featureclass</returns>
        public IFeatureClass CreateFCOutput(string outputPath, SaveAsType saveAsType, List<Graphic> graphicsList, ISpatialReference ipSpatialRef)
        {
            string fcName = System.IO.Path.GetFileName(outputPath);
            string folderName = System.IO.Path.GetDirectoryName(outputPath);
            IFeatureClass fc = null;

            try
            {
                if (saveAsType == SaveAsType.FileGDB)
                {
                    IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactory();
                    IWorkspace workspace = workspaceFactory.OpenFromFile(folderName, 0);
                    IFeatureWorkspace fWorkspace = (IFeatureWorkspace)workspace;

                    if (DoesFeatureClassExist(folderName, fcName))
                    {
                        DeleteFeatureClass(fWorkspace, fcName);
                    }

                    fc = CreatePolylineFeatureClass(fWorkspace, fcName);

                    foreach (Graphic graphic in graphicsList)
                    {
                        IFeature feature = fc.CreateFeature();

                        feature.Shape = graphic.Geometry;
                        feature.Store();
                    }

                }
                else if (saveAsType == SaveAsType.Shapefile)
                {
                    // already asked them for confirmation to overwrite file
                    if (File.Exists(outputPath))
                    {
                        IWorkspaceFactory workspaceFactory = new ShapefileWorkspaceFactory();
                        IWorkspace workspace = workspaceFactory.OpenFromFile(folderName, 0);
                        IFeatureWorkspace fWorkspace = (IFeatureWorkspace)workspace;
                        IDataset ipDs = fWorkspace.OpenFeatureClass(fcName) as IDataset;
                        ipDs.Delete();

                        File.Delete(outputPath);
                    }            

                    fc = ExportToShapefile(outputPath, graphicsList, ipSpatialRef);
                }
                return fc;
            }
            catch (Exception ex)
            {
                return fc;
            }
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

                return featClass;
            }
            catch (Exception ex)
            {
                return featClass;
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

    }

    
}
