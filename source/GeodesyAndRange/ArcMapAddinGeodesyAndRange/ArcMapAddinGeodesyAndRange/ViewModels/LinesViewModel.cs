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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcMapAddinGeodesyAndRange.Helpers;
using ESRI.ArcGIS.Geometry;

namespace ArcMapAddinGeodesyAndRange.ViewModels
{
    public class LinesViewModel : BaseViewModel
    {
        public LinesViewModel()
        {
            // props
            LineType = LineTypes.Geodesic;
            LineFromType = LineFromTypes.Points;
            LineDistanceType = DistanceTypes.Meters;
            LineAzimuthType = AzimuthTypes.Degrees;

            // commands

            CreateGeoPolylineCommand = new RelayCommand(OnCreateGeoPolylineCommand);

            // lets listen for new points from the map point tool
            Mediator.Register(Constants.NEW_MAP_POINT, OnNewMapPoint);
        }

        #region Properties

        public LineTypes LineType { get; set; }
        public LineFromTypes LineFromType { get; set; }
        public DistanceTypes LineDistanceType { get; set; }
        public AzimuthTypes LineAzimuthType { get; set; }

        public IPoint Point1 { get; set; }
        public IPoint Point2 { get; set; }

        #endregion

        #region Commands

        public RelayCommand CreateGeoPolylineCommand { get; set; }

        private void OnCreateGeoPolylineCommand(object obj)
        {
            //var polycollection = new Polyline() as IGeometryCollection;

            //if (polycollection == null)
            //    return;

            //Point1 = new  as IPoint;
            //var cn = Point1 as IConversionNotation;
            //cn.PutCoordsFromDD("-121 77");
            //Point1.SpatialReference = GetSR();

            ////var point2 = new PointClass();
            ////point2.PutCoords(-77, 44);
            ////point2.SpatialReference = GetSR();

            //// add some test data
            //polycollection.AddGeometry(point);
            ////polycollection.AddGeometry(point2);

            //var pc = polycollection as IPolycurve4;


            ////polyCurveGeo.DensifyGeodetic(esriGeodeticType.esriGeodeticTypeGeodesic, pLU, esriCurveDensifyMethod.esriCurveDensifyByAngle, 1.0);
            //pc.GeodesicDensify(500);
        }

        private ISpatialReference GetSR()
        {
            Type t = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
            System.Object obj = Activator.CreateInstance(t);
            ISpatialReferenceFactory srFact = obj as ISpatialReferenceFactory;

            // Use the enumeration to create an instance of the predefined object.

            IGeographicCoordinateSystem geographicCS =
                srFact.CreateGeographicCoordinateSystem((int)
                esriSRGeoCSType.esriSRGeoCS_WGS1984);

            return geographicCS as ISpatialReference;
        }

        #endregion

        #region Mediator methods

        private void OnNewMapPoint(object obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
