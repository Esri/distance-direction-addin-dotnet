﻿/******************************************************************************* 
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
using System.Collections.Generic;
using System.Windows;
using DistanceAndDirectionLibrary.Helpers;
using System.Xml.Serialization;
using DistanceAndDirectionLibrary.ViewModels;

namespace DistanceAndDirectionLibrary.Models
{
    public class OutputDistanceModel : NotificationObject
    {
        
        private int uniqueRowId;

        public int UniqueRowId
        {
            get { return uniqueRowId; }
            set
            {
                uniqueRowId = value;
                RaisePropertyChanged(() => UniqueRowId);
            }
        }         

        #region OutputDistance
 
        private string outputDistance = "0";

        [XmlIgnore]
        public string OutputDistance
        {
            get { return outputDistance; }
            set
            {

                outputDistance = value;
                RaisePropertyChanged(() => OutputDistance);
             
                if (value == "")
                    throw new ArgumentException(Properties.Resources.AEMustBePositive);
            }
        }

        #endregion

    }
}
