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
        private GraphicTypes graphicType = GraphicTypes.Line;
        /// <summary>
        /// Property for the graphic type
        /// </summary>
        public GraphicTypes GraphicType
        {
            get
            {
                return graphicType;
            }
            set
            {
                graphicType = value;
            }
        }

        private string uniqueId = "";
        /// <summary>
        /// Property for the unique id of the graphic (guid)
        /// </summary>
        public string UniqueId
        {
            get
            {
                return uniqueId;
            }
            set
            {
                uniqueId = value;
            }
        }

        private IGeometry geometry = null;
        /// <summary>
        /// Property for the geometry of the graphic
        /// </summary>
        public IGeometry Geometry
        {
            get
            {
                return geometry;
            }
            set
            {
                geometry = value;
            }
        }

        private bool isTemp = false;
        /// <summary>
        /// Property to determine if graphic is temporary or not
        /// </summary>
        public bool IsTemp
        {
            get
            {
                return isTemp;
            }
            set
            {
                isTemp = value;
            }
        }
    }
}
