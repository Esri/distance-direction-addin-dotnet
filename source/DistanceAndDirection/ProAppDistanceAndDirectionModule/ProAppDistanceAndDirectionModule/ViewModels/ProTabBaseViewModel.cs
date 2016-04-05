using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DistanceAndDirectionLibrary.ViewModels;
using DistanceAndDirectionLibrary.Views;
using ProAppDistanceAndDirectionModule.Models;
using ProAppDistanceAndDirectionModule.Views;

using ArcGIS.Core.Data;
using DistanceAndDirectionLibrary;
using System.Collections.ObjectModel;
using DistanceAndDirectionLibrary.Helpers;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace ProAppDistanceAndDirectionModule.ViewModels
{
    public class ProTabBaseViewModel : BaseViewModel
    {
        #region Properties

        // lists to store GUIDs of graphics, temp feedback and map graphics
        private static ObservableCollection<Graphic> GraphicsList = new ObservableCollection<Graphic>();
        private FeatureClassUtils fcUtils = new FeatureClassUtils();

        #endregion

        #region Commands

        public RelayCommand SaveAsCommand { get; set; }
        
        #endregion

        public ProTabBaseViewModel()
        {

            //commands
            SaveAsCommand = new RelayCommand(OnSaveAs);
        }

        /// <summary>
        /// Saves graphics to file gdb or shp file
        /// </summary>
        /// <param name="obj"></param>
        private void OnSaveAs(object obj)
        {
            var dlg = new ProSaveAsFormatView();
            dlg.DataContext = new ProSaveAsFormatViewModel();
            var vm = dlg.DataContext as ProSaveAsFormatViewModel;

            if (dlg.ShowDialog() == true)
            {
                FeatureClass fc = null;
                //IFeatureClass fc = null;

                // Get the graphics list for the selected tab
                List<Graphic> typeGraphicsList = new List<Graphic>();
                if (this is ProLinesViewModel)
                {
                    typeGraphicsList = GraphicsList.Where(g => g.GraphicType == GraphicTypes.Line).ToList();
                }
                else if (this is ProCircleViewModel)
                {
                    typeGraphicsList = GraphicsList.Where(g => g.GraphicType == GraphicTypes.Circle).ToList();
                }
                else if (this is ProEllipseViewModel)
                {
                    typeGraphicsList = GraphicsList.Where(g => g.GraphicType == GraphicTypes.Ellipse).ToList();
                }
                else if (this is ProRangeViewModel)
                {
                    typeGraphicsList = GraphicsList.Where(g => g.GraphicType == GraphicTypes.RangeRing).ToList();
                }

                string path = null;
                //if (vm.FeatureIsChecked)
                //{
                path = fcUtils.PromptUserWithSaveDialog(vm.FeatureIsChecked, vm.ShapeIsChecked, vm.KmlIsChecked);
                if (path != null)
                {
                    try
                    {
                        string name = System.IO.Path.GetFileName(path);
                        string folderName = System.IO.Path.GetDirectoryName(path);
                        string tempShapeFile = folderName + "\\tmpShapefile.shp";

                        if (vm.FeatureIsChecked)
                        {
                            fcUtils.CreateFCOutput(path, SaveAsType.FileGDB, typeGraphicsList);
                        }
                        else if (vm.ShapeIsChecked)
                        {
                            fcUtils.CreateFCOutput(path, SaveAsType.Shapefile, typeGraphicsList);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    

                    //if (tempFc != null)
                    //{
                    //    kmlUtils.ConvertLayerToKML(path, tempShapeFile, ArcMap.Document.FocusMap);

                    //    // delete the temporary shapefile
                    //    fcUtils.DeleteShapeFile(tempShapeFile);
                    //}
                }

        //            if (path != null)
        //            {
        //                if (System.IO.Path.GetExtension(path).Equals(".shp"))
        //                {
        //                    fc = fcUtils.CreateFCOutput(path, SaveAsType.Shapefile, typeGraphicsList, ArcMap.Document.FocusMap.SpatialReference);
        //                }
        //                else
        //                {
        //                    fc = fcUtils.CreateFCOutput(path, SaveAsType.FileGDB, typeGraphicsList, ArcMap.Document.FocusMap.SpatialReference);
        //                }
        //            }
                //}
        //        else
        //        {
        //            path = PromptSaveFileDialog();
        //            if (path != null)
        //            {
        //                string kmlName = System.IO.Path.GetFileName(path);
        //                string folderName = System.IO.Path.GetDirectoryName(path);
        //                string tempShapeFile = folderName + "\\tmpShapefile.shp";
        //                IFeatureClass tempFc = fcUtils.CreateFCOutput(tempShapeFile, SaveAsType.Shapefile, typeGraphicsList, ArcMap.Document.FocusMap.SpatialReference);

        //                if (tempFc != null)
        //                {
        //                    kmlUtils.ConvertLayerToKML(path, tempShapeFile, ArcMap.Document.FocusMap);

        //                    // delete the temporary shapefile
        //                    fcUtils.DeleteShapeFile(tempShapeFile);
        //                }
        //            }
        //        }

        //        if (fc != null)
        //        {
        //            IFeatureLayer outputFeatureLayer = new FeatureLayerClass();
        //            outputFeatureLayer.FeatureClass = fc;

        //            IGeoFeatureLayer geoLayer = outputFeatureLayer as IGeoFeatureLayer;
        //            geoLayer.Name = fc.AliasName;

        //            ESRI.ArcGIS.Carto.IMap map = ArcMap.Document.FocusMap;
        //            map.AddLayer((ILayer)outputFeatureLayer);
        //        }
            }
        }

        //private string PromptSaveFileDialog()
        //{
        //    if (sfDlg == null)
        //    {
        //        sfDlg = new SaveFileDialog();
        //        sfDlg.AddExtension = true;
        //        sfDlg.CheckPathExists = true;
        //        sfDlg.DefaultExt = "kmz";
        //        sfDlg.Filter = "KMZ File (*.kmz)|*.kmz";
        //        sfDlg.OverwritePrompt = true;
        //        sfDlg.Title = "Choose location to create KMZ file";

        //    }
        //    sfDlg.FileName = "";

        //    if (sfDlg.ShowDialog() == DialogResult.OK)
        //    {
        //        return sfDlg.FileName;
        //    }

        //    return null;
        //}
    }
}
