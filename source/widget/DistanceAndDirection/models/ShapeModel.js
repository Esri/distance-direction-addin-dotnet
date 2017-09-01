///////////////////////////////////////////////////////////////////////////
// Copyright (c) 2016 Esri. All Rights Reserved.
//
// Licensed under the Apache License Version 2.0 (the "License");
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
///////////////////////////////////////////////////////////////////////////

/*global define*/
define([
  'dojo/_base/declare',
  'dojo/_base/array',
  'dojo/_base/lang',
  'dojo/string',
  'dojo/number',
  'esri/geometry/geometryEngine',
  'esri/geometry/webMercatorUtils',
  'esri/geometry/Point',
  'esri/geometry/Polyline',
  'esri/geometry/Circle',
  'esri/units'
], function (
  dojoDeclare,
  dojoArray,
  dojoLang,
  dojoString,
  dojoNumber,
  esriGeometryEngine,
  esriWMUtils,
  esriPoint,
  esriPolyline,
  esriCircle,
  esriUnit
) {
  return dojoDeclare(null, {

    Units: [
      'meters',
      'feet',
      'kilometers',
      'miles',
      'nautical-miles',
      'yards'
    ],

    /**
     *
     **/
    getAngle: function (inUnits) {
      var angle = null;
      if (this.angle === undefined) {
        var delx = this.endPoint.y - this.startPoint.y;
        var dely = this.endPoint.x - this.startPoint.x;

        var azi = Math.atan2(dely, delx) * 180 / Math.PI;
        angle = ((azi + 360) % 360);
      } else {
        angle = Number(this.angle);
      }
      if (inUnits === 'mils') {
        angle *= 17.777777778;
      }

      return angle.toFixed(2);
    },

    /**
     * Returns Geodesic Length of the Geometry
     **/
    getLength: function (withUnit) {

      var u = this.Units[0];
      if (this.Units.indexOf(withUnit.toLowerCase()) > -1){
        u = withUnit.toLowerCase();
      }

      return esriGeometryEngine.geodesicLength(
        this.geographicGeometry,
        u
      );
    },

    /**
     *
     **/
    getFormattedLength: function (withUnit) {
      return dojoNumber.format(this.getLength(withUnit), {
        places:4
      });
    },

    /**
     *
     * @param length
     * @returns {*}
     */
    formatLength: function (length, withUnit) {
      return dojoNumber.format(length, {
        places: 4
      });
    },

    /**
     *
     **/
    constructor: function (args) {
      dojoDeclare.safeMixin(this, args);

      if (this.geometry.type === 'polygon') {
        if (this.geometry.hasOwnProperty('drawType')) {
          if (this.geometry.drawType === 'ellipse') {
            var line = new esriPolyline();
            dojoArray.forEach(this.geometry.geometry.rings, dojoLang.hitch(this, function (ring) {
              line.paths.push(ring);
            }));
            line.spatialReference = this.geometry.spatialReference;
            line = esriWMUtils.webMercatorToGeographic(line);
            this.geographicGeometry = line;
            this.geodesicGeometry = esriGeometryEngine.geodesicDensify(this.geometry.geometry, 10000);
            this.wmGeometry = this.geometry.geometry;
            this.angle = this.geometry.angle;
            this.startPoint = esriWMUtils.webMercatorToGeographic(this.geometry.center);
            this.formattedStartPoint = dojoString.substitute('${xStr}, ${yStr}', {
              xStr: dojoNumber.format(this.startPoint.y, {places:4}),
              yStr: dojoNumber.format(this.startPoint.x, {places:4})
            });
          }
        } else {
          var pLine = new esriPolyline({
            paths: [
              [this.geometry.paths[0][0], this.geometry.paths[0][1]]
            ],
            spatialReference: {
              wkid: this.geometry.spatialReference.wkid
            }
          });
          pLine = esriWMUtils.webMercatorToGeographic(pLine);
          this.geographicGeometry = pLine;
          this.geodesicGeometry = esriGeometryEngine.geodesicDensify(pLine, 10000);
          this.wmGeometry = this.geometry;
          this.startPoint = this.geodesicGeometry.getPoint(0,0);
          this.endPoint = this.geodesicGeometry.getPoint(
            0,
            this.geodesicGeometry.paths[0].length - 1);

          this.formattedStartPoint = dojoString.substitute('${xStr}, ${yStr}', {
            xStr: dojoNumber.format(this.startPoint.y, {places:4}),
            yStr: dojoNumber.format(this.startPoint.x, {places:4})
          });

          this.formattedEndPoint = dojoString.substitute('${xStr}, ${yStr}', {
            xStr: dojoNumber.format(this.endPoint.y, {places:4}),
            yStr: dojoNumber.format(this.endPoint.x, {places:4})
          });
        }
      } else if (this.geometry.type === "point") {
        this.geodesicGeometry = esriGeometryEngine.geodesicBuffer(
          this.geometry,
          this.calculatedDistance,
          'meters'
        );
        this.geographicGeometry = this.lineGeometry !== null ? this.lineGeometry : this.geometry;
        if (this.geodesicGeometry.spatialReference.wkid !== 102100 &&
          this.geodesicGeometry.spatialReference.wkid !== 3857) {
          this.wgsGeometry = this.geodesicGeometry;
          this.wmGeometry = esriWMUtils.geographicToWebMercator(this.geodesicGeometry);
        } else {
          this.wgsGeometry = esriWMUtils.webMercatorToGeographic(this.geodesicGeometry);
          this.wmGeometry = this.geodesicGeometry;
        }

        this.formattedStartPoint = dojoString.substitute("${xStr}, ${yStr}", {
          xStr: dojoNumber.format(this.wgsGeometry.getCentroid().y, {places:4}),
          yStr: dojoNumber.format(this.wgsGeometry.getCentroid().x, {places:4})
        });
      } else {
        this.geodesicGeometry = esriGeometryEngine.geodesicDensify(this.geographicGeometry, 10000);
        this.wmGeometry = esriWMUtils.geographicToWebMercator(this.geodesicGeometry);

        this.startPoint = this.geodesicGeometry.getPoint(0,0);
        this.endPoint = this.geodesicGeometry.getPoint(
          0,
          this.geodesicGeometry.paths[0].length - 1);

        this.formattedStartPoint = dojoString.substitute('${xStr}, ${yStr}', {
          xStr: dojoNumber.format(this.startPoint.y, {places:4}),
          yStr: dojoNumber.format(this.startPoint.x, {places:4})
        });

        this.formattedEndPoint = dojoString.substitute('${xStr}, ${yStr}', {
          xStr: dojoNumber.format(this.endPoint.y, {places:4}),
          yStr: dojoNumber.format(this.endPoint.x, {places:4})
        });
      }
    }
  });
});
