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
  'dojo/topic',
  'esri/toolbars/draw',
  'esri/graphic',
  'esri/geometry/Polyline',
  'esri/geometry/geometryEngine',
  './Feedback'
], function (
  dojoDeclare,
  dojoLang,
  dojoConnect,
  dojoHas,
  dojoTopic,
  esriDraw,
  esriGraphic,
  esriPolyLine,
  esriGeometryEngine,
  drawFeedback
) {
    var lf = dojoDeclare([drawFeedback], {

      /**
       *
       **/
      constructor: function (map,coordTool) {
        this.inherited(arguments);
        this._utils = coordTool;
        this.syncEvents();
      },
      
      /*
      * Start up event listeners
      */
      syncEvents: function () {
        dojoTopic.subscribe(
          'manual-startpoint-input',
          dojoLang.hitch(this, this.onLineStartManualInputHandler)
        );        
        dojoTopic.subscribe(
          'manual-line-end-point-input',
          dojoLang.hitch(this, this.onLineEndManualInputHandler)
        ); 
        dojoTopic.subscribe(
          'clear-points',
          dojoLang.hitch(this, this.clearPoints)
        ); 
      },
        
      /*
      Handler for clearing out points
      */
      clearPoints: function (centerPoint) {
          this._points = [];
          this.map.graphics.clear();
      },
      
      /*
      Handler for the manual input of start point
      */
      onLineStartManualInputHandler: function (centerPoint) {
        this._points = [];
        this._points.push(centerPoint.offset(0, 0));
        this.set('startPoint', this._points[0]);
        this.map.centerAt(centerPoint);
      },

      onLineEndManualInputHandler: function (endPoint) {
        this._points.push(endPoint.offset(0, 0));
        this.set('endPoint', this._points[1]);
      },

      /**
       *
       **/
      getAngle: function (stPoint, endPoint) {
        var angle = null;

        var delx = endPoint.y - stPoint.y;
        var dely = endPoint.x - stPoint.x;

        var azi = Math.atan2(dely, delx) * 180 / Math.PI;
        angle = ((azi + 360) % 360);

        if (this.angleUnit === 'mils') {
          angle *= 17.777777778;
        }

        return angle.toFixed(2);
      },

      /**
       *
       **/
      _onClickHandler: function (evt) {
        var snappingPoint;

        if (this.map.snappingManager) {
          snappingPoint = this.map.snappingManager._snappingPoint;
        }

        var start = snappingPoint || evt.mapPoint;
        var  map = this.map;
        var tGraphic;
        var geom;

        this._points.push(start);
        switch (this._geometryType) {
          case esriDraw.POINT:
            this.set('startPoint', this._points[0]);
            this.set('endPoint', this._points[0]);
            this._drawEnd(start);
            this._setTooltipMessage(0);
            break;
          case esriDraw.POLYLINE:
            if (this._points.length === 2) {
              this.set('endPoint', this._points[1]);
              this._onDblClickHandler();
              return;
            }
            if (this._points.length === 1) {
              this.set('startPoint', this._points[0]);
              var polyline = new esriPolyLine(map.spatialReference);
              polyline.addPath(this._points);
              this._graphic = map.graphics.add(new esriGraphic(polyline, this.lineSymbol), true);
              if (map.snappingManager) {
                map.snappingManager._setGraphic(this._graphic);
              }
              this._onMouseMoveHandler_connect = dojoConnect.connect(map, 'onMouseMove', this._onMouseMoveHandler);
              this._tGraphic = map.graphics.add(new esriGraphic(new esriPolyLine({
                paths: [[[start.x, start.y], [start.x, start.y]]],
                spatialReference: map.spatialReference
              }), this.lineSymbol), true);
            } else {
              this.set('endPoint', this._points[1]);
              this._graphic.geometry._insertPoints([start], 0);
              this._graphic.setGeometry(this._graphic.geometry).setSymbol(this.lineSymbol);
              tGraphic = this._tGraphic;
              geom = tGraphic.geometry;
              geom.setPoint(0, 0, start);
              geom.setPoint(0, 1, start);
              tGraphic.setGeometry(geom);
            }
            break;
        }

        this._setTooltipMessage(this._points.length);
        if (this._points.length === 1 && this._geometryType === 'polyline') {
          var tooltip = this._tooltip;
          if (tooltip) {
            tooltip.innerHTML = 'Click to finish drawing line';
          }
        }
      },

      /**
       *
       **/
      _onDblClickHandler: function (evt) {
        var geometry;
        var _pts = this._points;
        var map = this.map;
        var spatialreference = map.spatialReference;

        if (dojoHas('esri-touch') && evt) {
          _pts.push(evt.mapPoint);
        }
        geometry = new esriPolyLine({
          paths: [[[_pts[0].x, _pts[0].y], [_pts[1].x, _pts[1].y]]],
          spatialReference: spatialreference
        });
        geometry.geodesicLength = esriGeometryEngine.geodesicLength(geometry, 9001);

        dojoConnect.disconnect(this._onMouseMoveHandler_connect);

        this._clear();
        this._setTooltipMessage(0);
        this._drawEnd(geometry);
      },

      /**
       *
       **/
      _onMouseMoveHandler: function (evt) {
        var snappingPoint;
        if (this.map.snappingManager) {
          snappingPoint = this.map.snappingManager._snappingPoint;
        }

        var start = (this._geometryType === esriDraw.POLYLINE) ? this._points[0] : this._points[this._points.length - 1];

        var end = snappingPoint || evt.mapPoint;
        
        /**
         * if your network & ArcGIS Server can support a high volume of requests, and you want the end point coordinate to update as
         * the mouse moves then uncomment the line below (use with caution and conduct internal test before making public)
        **/         

        var tGraphic = this._tGraphic;
        var geom = tGraphic.geometry;

        geom.setPoint(0, 0, { x: start.x, y: start.y });
        geom.setPoint(0, 1, { x: end.x, y: end.y });

        var geogeom = esriGeometryEngine.geodesicDensify(geom, 10001);

        var majorAxisLength = esriGeometryEngine.geodesicLength(geom, 9001);

        this._graphic.setGeometry(geogeom);

        var unitlength = this._utils.convertMetersToUnits(majorAxisLength, this.lengthUnit);
        var ang = this.getAngle(start, end);

        dojoTopic.publish('DD_LINE_LENGTH_DID_CHANGE', unitlength);
        dojoTopic.publish('DD_LINE_ANGLE_DID_CHANGE', ang);
      }
    });
    lf.drawnLineLengthDidChange = 'DD_LINE_LENGTH_DID_CHANGE';
    lf.drawnLineAngleDidChange = 'DD_LINE_ANGLE_DID_CHANGE';
    return lf;
});
