using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Desktop.AddIns;
using ArcMapAddinGeodesyAndRange.Helpers;

namespace ArcMapAddinGeodesyAndRange
{
    public class MapPointTool : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        public MapPointTool()
        {
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }

        protected override void OnMouseDown(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
            if (arg.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            try
            {
                //Get the active view from the ArcMap static class.
                IActiveView activeView = ArcMap.Document.FocusMap as IActiveView;

                var point = activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y) as IPoint;

                // always use WGS84
                //var sr = GetSR();

                //if (sr != null)
                //{
                //    point.Project(sr);
                //}

                //var doc = AddIn.FromID<ArcMapAddinGeodesyAndRange.DockableWindowGeodesyAndRange.AddinImpl>(ThisAddIn.IDs.DockableWindowGeodesyAndRange);

                //if (doc != null)
                //{
                //    doc.SetInput(point.X, point.Y);
                //}

                Mediator.NotifyColleagues(Constants.NEW_MAP_POINT, point);
            }
            catch { }
        }

    }

}
