using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.esriSystem;

namespace ArcMapAddinGeodesyAndRange.ViewModels
{
    class SelectSaveAsFormatViewModel : BaseViewModel
    {

        private bool featureShapeIsChecked = true;
        public bool FeatureShapeIsChecked
        {
            get
            {
                return featureShapeIsChecked;
            }

            set
            {
                featureShapeIsChecked = value;
                RaisePropertyChanged(() => FeatureShapeIsChecked);
            }
        }

        private bool kmlIsChecked = false;
        public bool KmlIsChecked
        {
            get
            {
                return kmlIsChecked;
            }

            set
            {
                kmlIsChecked = value;
                RaisePropertyChanged(() => KmlIsChecked);
            }
        }
    }  
}
