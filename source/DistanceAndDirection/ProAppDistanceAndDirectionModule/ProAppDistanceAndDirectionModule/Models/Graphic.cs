// Copyright 2016 Esri 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using ArcGIS.Core.Geometry;
using DistanceAndDirectionLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProAppDistanceAndDirectionModule.Models
{
    public class Graphic
    {
        public Graphic(GraphicTypes _graphicType, IDisposable _disposable, Geometry _geometry, bool _isTemp = false)
        {
            GraphicType = _graphicType;
            //UniqueId = _uniqueid;
            Disposable = _disposable;
            Geometry = _geometry;
            IsTemp = _isTemp;
        }

        // properties   

        /// <summary>
        /// Property for the graphic type
        /// </summary>
        public GraphicTypes GraphicType { get; set; }

        /// <summary>
        /// Property for the unique id of the graphic (guid)
        /// </summary>
        //public string UniqueId { get; set; }
        public IDisposable Disposable { get; set; }

        /// <summary>
        /// Property for the geometry of the graphic
        /// </summary>
        public Geometry Geometry { get; set; }

        /// <summary>
        /// Property to determine if graphic is temporary or not
        /// </summary>
        public bool IsTemp { get; set; }

    }
}
