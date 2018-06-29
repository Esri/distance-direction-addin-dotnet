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

using ArcMapAddinDistanceAndDirection.ViewModels;
using DistanceAndDirectionLibrary;
// Esri
using ESRI.ArcGIS.Geometry;
using System.Collections.Generic;

namespace ArcMapAddinDistanceAndDirection.Models
{
    public class Graphic
    {
        public Graphic(GraphicTypes _graphicType, string _uniqueid, IGeometry _geometry, TabBaseViewModel _model, bool _isTemp = false, IDictionary<string, System.Object> attributes = null)
        {
            GraphicType = _graphicType;
            UniqueId = _uniqueid;
            Geometry = _geometry;
            IsTemp = _isTemp;
            ViewModel = _model;
            Attributes = attributes;
        }
        
        // properties   

        /// <summary>
        /// Property for the graphic type
        /// </summary>
        public GraphicTypes GraphicType {get;set;}

        /// <summary>
        /// Property for the unique id of the graphic (guid)
        /// </summary>
        public string UniqueId {get;set;}

        /// <summary>
        /// Property for the geometry of the graphic
        /// </summary>
        public IGeometry Geometry {get;set;}

        /// <summary>
        /// Property to determine if graphic is temporary or not
        /// </summary>
        public bool IsTemp {get;set;}

        /// <summary>
        /// Property to determine what view the graphic was created in
        /// </summary>
        public TabBaseViewModel ViewModel { get; set; }
        /// <summary>
        /// Property to set attributes for different geometries
        /// </summary>
        public IDictionary<string, System.Object> Attributes { get; set; }
    }
}
