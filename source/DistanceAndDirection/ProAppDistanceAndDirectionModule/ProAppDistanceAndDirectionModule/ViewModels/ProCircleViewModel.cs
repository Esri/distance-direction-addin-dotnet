using ArcGIS.Desktop.Framework;
using DistanceAndDirectionLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProAppDistanceAndDirectionModule.ViewModels
{
    public class ProCircleViewModel : ProTabBaseViewModel
    {
        public ProCircleViewModel() 
        {
            ActivateToolCommand = new ArcGIS.Desktop.Framework.RelayCommand(async () =>
            {
                FrameworkApplication.SetCurrentToolAsync("ProAppDistanceAndDirectionModule_SketchTool");
                Mediator.NotifyColleagues("SET_SKETCH_TOOL_TYPE", ArcGIS.Desktop.Mapping.SketchGeometryType.Circle);
            });

            Mediator.Register("SKETCH_COMPLETE", OnSketchComplete);
        }

        private void OnSketchComplete(object obj)
        {
            AddGraphicToMap(obj as ArcGIS.Core.Geometry.Geometry);
        }

        public ArcGIS.Desktop.Framework.RelayCommand ActivateToolCommand { get; set; }
    }
}
