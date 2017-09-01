
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
//
///////////////////////////////////////////////////////////////////////////

define([
  'dojo/_base/declare',
  'dojo/_base/lang',
  'dojo/_base/connect',
  'dojo/has',
  'dojo/number',
  'dojo/string',
  'dojo/Stateful',
  'esri/Color',
  'esri/toolbars/draw',
  'esri/graphic',
  'esri/geometry/Polyline',
  'esri/geometry/Polygon',
  'esri/geometry/Point',
  'esri/geometry/Circle',
  'esri/geometry/screenUtils',
  'esri/graphic',
  'esri/geometry/geometryEngineAsync',
  'esri/geometry/geometryEngine',
  'esri/symbols/TextSymbol',
  'esri/symbols/Font',
  'esri/geometry/webMercatorUtils',
  'esri/units'
], function (
  dojoDeclare,
  dojoLang,
  connect,
  dojoHas,
  dojoNumber,
  dojoString,
  dojoStateful,
  EsriColor,
  esriDraw,
  Graphic,
  EsriPolyLine,
  EsriPolygon,
  EsriPoint,
  EsriCircle,
  esriScreenUtils,
  EsriGraphic,
  esriGeoDUtils,
  esriGeometryEngine,
  EsriTextSymbol,
  EsriFont,
  EsriWebMercatorUtils,
  EsriUnits
  ) {
    var w = dojoDeclare([esriDraw, dojoStateful], {
      startPoint: null,
      _setStartPoint: function (p){
        this._set('startPoint', p);
      },

      endPoint: null,
      _setEndPoint: function (p){
        this._set('endPoint', p);
      },

      lengthUnit: 'meters',
      _setLengthUnit: function (u) {
        this._set('lengthUnit', u);
      },

      angleUnit: 'degrees',
      _setAngle: function (a) {
        this._set('angleUnit', a);
      },

      isDiameter: true,

      /**
       *
       **/
      constructor: function () {
        // force loading of the geometryEngine
        // prevents lag in feedback when used in mousedrag
        esriGeoDUtils.isSimple(new EsriPoint({
          'x': -122.65,
          'y': 45.53,
          'spatialReference': {
            'wkid': 4326
          }
        })).then(function () {
          console.log('Geometry Engine initialized');
        });      
      },

      /**
       *
       **/
      getRadiusUnitType: function () {
        var selectedUnit = EsriUnits.METERS;
        switch (this.lengthUnit) {
          case 'meters':
            selectedUnit = EsriUnits.METERS;
            break;
          case 'feet':
            selectedUnit = EsriUnits.FEET;
            break;
          case 'kilometers':
            selectedUnit = EsriUnits.KILOMETERS;
            break;
          case 'miles':
            selectedUnit = EsriUnits.MILES;
            break;
          case 'nautical-miles':
            selectedUnit = EsriUnits.NAUTICAL_MILES;
            break;
          case 'yards':
            selectedUnit = EsriUnits.YARDS;
            break;
        }
        return selectedUnit;
      },

      /*
       *   add a temporary start point graphic to the map
       */
      addStartGraphic: function (fromGeometry, withSym) {
        this.removeStartGraphic();
        this.startGraphic = new EsriGraphic(fromGeometry, withSym);
        this.map.graphics.add(this.startGraphic);
      },

      removeStartGraphic: function () {
        if (this.startGraphic) {
          this.map.graphics.remove(this.startGraphic);
        }
        this.startGraphic = null;
      }

  });

  return w;
});
