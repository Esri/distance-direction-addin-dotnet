using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

namespace ArcMapAddinGeodesyAndRange.Models
{
    public class Graphic
    {
        public Graphic(GraphicTypes _graphicType, string _uniqueid, IGeometry _geometry, bool _isTemp = false)
        {
            graphicType = _graphicType;
            uniqueId = _uniqueid;
            geometry = _geometry;
            isTemp = _isTemp;
        }
        
        // properties   
        public GraphicTypes graphicType = GraphicTypes.Line;
        /// <summary>
        /// Property for the graphic type
        /// </summary>
        public GraphicTypes GraphicType {get;set;}

        public string uniqueId = "";
        /// <summary>
        /// Property for the unique id of the graphic (guid)
        /// </summary>
        public string UniqueId {get;set;}

        public IGeometry geometry = null;
        /// <summary>
        /// Property for the geometry of the graphic
        /// </summary>
        public IGeometry Geometry {get;set;}

        public bool isTemp = false;
        /// <summary>
        /// Property to determine if graphic is temporary or not
        /// </summary>
        public bool IsTemp {get;set;} 
        
    }
}
