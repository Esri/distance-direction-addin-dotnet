using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcMapAddinGeodesyAndRange.Helpers;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace ArcMapAddinGeodesyAndRange.Models
{
    public class DistanceAndDirectionConfig : NotificationObject
    {
        public DistanceAndDirectionConfig()
        {

        }

        private CoordinateTypes displayCoordinateType = CoordinateTypes.DD;
        public CoordinateTypes DisplayCoordinateType
        {
            get { return displayCoordinateType; }
            set
            {
                displayCoordinateType = value;
                RaisePropertyChanged(() => DisplayCoordinateType);
            }
        }

        public void SaveConfiguration()
        {
            try
            {
                var filename = GetConfigFilename();

                XmlSerializer x = new XmlSerializer(GetType());
                XmlWriter writer = new XmlTextWriter(filename, Encoding.UTF8);
            }
            catch(Exception ex)
            {
                // do nothing
            }
        }

        public void LoadConfiguration()
        {
            try
            {
                var filename = GetConfigFilename();

                if (string.IsNullOrWhiteSpace(filename) || !File.Exists(filename))
                    return;

                XmlSerializer x = new XmlSerializer(GetType());
                TextReader tr = new StreamReader(filename);
                var temp = x.Deserialize(tr) as DistanceAndDirectionConfig;

                if (temp == null)
                    return;

                DisplayCoordinateType = temp.DisplayCoordinateType;
            }
            catch(Exception ex)
            {
                // do nothing
            }
        }

        #region Private methods

        private string GetConfigFilename()
        {
            return this.GetType().Assembly.Location + ".config";
        }

        #endregion Private methods
    }
}
