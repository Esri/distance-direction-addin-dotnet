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
using System.Linq;

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

using ProAppDistanceAndDirectionModule.Common;
using System.Windows;
using System.Text.RegularExpressions;

namespace ProAppDistanceAndDirectionModule.Models
{
    class FeatureClassUtils
    {
        private string previousLocation = string.Empty;
        private string previousSaveType = string.Empty;

        /// <summary>
        /// Prompts the user to save features
        /// </summary>
        /// <param name="featureShapeChecked"></param>
        /// <returns></returns>
        public string PromptUserWithSaveDialog(bool featureShapeChecked)
        {
            //Prep the dialog
            SaveItemDialog saveItemDlg = new SaveItemDialog();
            saveItemDlg.Title = "Select output";
            saveItemDlg.OverwritePrompt = true;
            var saveType = (featureShapeChecked) ? "feature-shape" : "kml";
            if (string.IsNullOrEmpty(previousSaveType))
                previousSaveType = saveType;
            if (!string.IsNullOrEmpty(previousLocation) && previousSaveType == saveType)
                saveItemDlg.InitialLocation = previousLocation;
            else
            {
                if (featureShapeChecked)
                    saveItemDlg.InitialLocation = ArcGIS.Desktop.Core.Project.Current.DefaultGeodatabasePath;
                else
                    saveItemDlg.InitialLocation = ArcGIS.Desktop.Core.Project.Current.HomeFolderPath;
            }
            previousSaveType = saveType;

            // Set the filters and default extension
            if (featureShapeChecked)
                saveItemDlg.Filter = ItemFilters.featureClasses_all;
            else
            {
                saveItemDlg.Filter = ItemFilters.kml;
                saveItemDlg.DefaultExt = "kmz";
            }

            bool? ok = saveItemDlg.ShowDialog();

            //Show the dialog and get the response
            if (ok == true)
            {
                if (ContainsInvalidChars(Path.GetFileName(saveItemDlg.FilePath)))
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                        ProAppDistanceAndDirectionModule.Properties.Resources.FeatureClassNameError,
                        ProAppDistanceAndDirectionModule.Properties.Resources.DistanceDirectionLabel,
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
                    return null;
                } 
                previousLocation = Path.GetDirectoryName(saveItemDlg.FilePath);                
                return saveItemDlg.FilePath;           
            }
            return null;
        }


        private static bool CheckResultAndReportMessages(IGPResult result, string toolToReport,
            List<object> toolParameters)
        {
            // Return if no errors
            if (!result.IsFailed)
                return true;

            // If failed, provide feedback of what went wrong
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(toolToReport);
            sb.AppendLine(" - GP Tool FAILED:");
            foreach (var msg in result.ErrorMessages)
                sb.AppendLine(msg.Text);
            foreach (var msg in result.Messages)
                sb.AppendLine(msg.Text);

            if (toolParameters != null)
            {
                sb.Append("Parameters: ");
                int count = 0;
                foreach (var param in toolParameters)
                    sb.Append(string.Format("{0}:{1} ", count++, param));
                sb.AppendLine();
            }

            System.Diagnostics.Debug.WriteLine(sb);

            return false;
        }

        public async Task<bool> ExportLayer(string layerName, string outputPath, SaveAsType saveAsType)
        {
            if (saveAsType == SaveAsType.KML)
                return await ExportKMLLayer(layerName, outputPath);
            else
                return await ExportFeatureLayer(layerName, outputPath);
        }

        public async Task<bool> ExportFeatureLayer(string layerName, string outputPath)
        {
            bool success = false;

            await QueuedTask.Run(async () =>
            {
                List<object> arguments = new List<object>();

                // TODO: if the user moves or renames this group, this layer name may no longer be valid
                arguments.Add("Distance And Direction" + @"\" + layerName);
                arguments.Add(outputPath);

                var parameters = Geoprocessing.MakeValueArray(arguments.ToArray());
                var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);

                string gpTool = "CopyFeatures_management";
                IGPResult result = await Geoprocessing.ExecuteToolAsync(
                    gpTool,
                    parameters,
                    environments,
                    null,
                    null,
                    GPExecuteToolFlags.Default);

                success = CheckResultAndReportMessages(result, gpTool, arguments);
            });

            return success;
        }

        public async Task<bool> ExportKMLLayer(string layerName, string outputPath)
        {
            bool success = false;

            await QueuedTask.Run(async () =>
            {
                List<object> arguments = new List<object>();

                // TODO: if the user moves or renames this group, this layer name may no longer be valid
                arguments.Add("Distance And Direction" + @"\" + layerName);
                arguments.Add(outputPath);

                var parameters = Geoprocessing.MakeValueArray(arguments.ToArray());
                var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);

                string gpTool = "LayerToKML_conversion";
                IGPResult result = await Geoprocessing.ExecuteToolAsync(
                    gpTool,
                    parameters,
                    environments,
                    null,
                    null,
                    GPExecuteToolFlags.Default);

                success = CheckResultAndReportMessages(result, gpTool, arguments);
            });

            return success;
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
        /// Checks if file name has illegal characters
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>       
        private static bool ContainsInvalidChars(string path)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            var regexItem = new Regex("^[A-Za-z_][A-Za-z0-9_]*$");
            var isValidFileName = regexItem.IsMatch(fileName);
            if (!isValidFileName)
                return true;
            else if (fileName.Length > 32)
                return true;
            else if (ValidateReservedWords(fileName))
                return true;
            return false;
        }

        private static bool ValidateReservedWords(string fileName)
        {
            //https://support.esri.com/en/technical-article/000010906 - Used the keyword list mentioned here for 10.1 and above
            var reservedWords = new List<string>() {
                "ADD","ALTER","AND","BETWEEN","BY","COLUMN","CREATE","DELETE","DROP","EXISTS","FOR","FROM","GROUP","IN","INSERT","INTO","IS","LIKE","NOT","NULL","OR","ORDER","SELECT","SET","TABLE","UPDATE","VALUES","WHERE"
            };
            return reservedWords.Where(x => fileName.ToUpper() == x).Any();
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

        public static string AddinAssemblyLocation()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            return System.IO.Path.GetDirectoryName(
                              Uri.UnescapeDataString(
                                      new Uri(asm.CodeBase).LocalPath));
        }


    }
}
