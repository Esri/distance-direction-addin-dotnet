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

using System;
using System.Xml.Serialization;
using ArcGIS.Desktop.Framework.Contracts;

namespace ProAppDistanceAndDirectionModule.Models
{
    public class OutputDistanceModel : PropertyChangedBase
    {
        
        private int _uniqueRowId;

        public int UniqueRowId
        {
            get => _uniqueRowId;
            set => SetProperty(ref _uniqueRowId, value);
        }         

        #region OutputDistance
 
        private string _outputDistance = "0";

        [XmlIgnore]
        public string OutputDistance
        {
            get => _outputDistance;
            set
            {
                SetProperty(ref _outputDistance, value);
             
                if (value == "")
                    throw new ArgumentException(Properties.Resources.AEMustBePositive);
            }
        }

        #endregion

    }
}
