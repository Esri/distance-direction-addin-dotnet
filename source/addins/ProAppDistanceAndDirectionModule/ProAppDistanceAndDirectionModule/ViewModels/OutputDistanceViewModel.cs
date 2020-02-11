/******************************************************************************* 
  * Copyright 2015 Esri 
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

using ProAppDistanceAndDirectionModule.Common;
using ProAppDistanceAndDirectionModule.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace ProAppDistanceAndDirectionModule.ViewModels
{
    public class OutputDistanceViewModel : NotificationObject
    {
        public OutputDistanceViewModel()
        {
            AddNewOCCommand = new RelayCommand(OnAddNewOCCommand);
            DeleteCommand = new RelayCommand(OnDeleteCommand);
            TextLostFocus = new RelayCommand(OnTextChangeEvent);
            OutputDistanceListItem = new ObservableCollection<OutputDistanceModel>();
        }

        public static ObservableCollection<OutputDistanceModel> OutputDistanceListItem { get; set; }
        /// <summary>
        /// The bound list.
        /// </summary>
        public ObservableCollection<OutputDistanceModel> OutputDistanceList
        {
            get { return OutputDistanceListItem; }
        }

        #region relay commands
        [XmlIgnore]
        public RelayCommand DeleteCommand { get; set; }

        [XmlIgnore]
        public RelayCommand AddNewOCCommand { get; set; }

        [XmlIgnore]
        public RelayCommand TextLostFocus { get; set; }

        #endregion

        public static int UniqueRowId { get; set; }
        private void OnAddNewOCCommand(object obj)
        {
            if (OutputDistanceListItem.Count > 0)
            {
                foreach (var item in OutputDistanceListItem)
                {
                    if (item.OutputDistance == "" || item.OutputDistance == "0")
                    {
                        return;
                    }
                }                 
            }

            var outputItem = new OutputDistanceModel();
            outputItem.UniqueRowId = UniqueRowId;
            outputItem.OutputDistance = "0";
            OutputDistanceListItem.Add(outputItem);
            UniqueRowId++;
        }

        private void OnDeleteCommand(object obj)
        {            
            var uniqueRowNo = (int)obj;

            foreach (var item in OutputDistanceListItem)
            {
                if (item.UniqueRowId == uniqueRowNo)
                {
                    OutputDistanceListItem.Remove(item);
                    return;
                }

            }
        }

        private void OnTextChangeEvent(object obj)
        {
            if (OutputDistanceListItem != null && OutputDistanceListItem.Count > 1)
            {
                ObservableCollection<OutputDistanceModel> listOfDistances = new ObservableCollection<OutputDistanceModel>();
                foreach (var item in OutputDistanceListItem)
                {
                    if (item.OutputDistance == "")
                    {
                        listOfDistances.Add(item);
                    }
                }

                foreach (var item in listOfDistances)
                {
                    if (item.OutputDistance == "")
                    {
                        OutputDistanceListItem.Remove(item);
                    }
                }
            }
        }



    }
}
