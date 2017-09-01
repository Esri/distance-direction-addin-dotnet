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
  'dojo/topic',
  'esri/graphic',
  'esri/toolbars/draw',
  'esri/geometry/Circle',
  'esri/geometry/Polyline',
  'esri/geometry/geometryEngine',
  './Feedback'
], function (
  dojoDeclare,
  dojoLang,
  dojoConnect,
  dojoTopic,
  esriGraphic,
  esriDraw,
  esriCircle,
  esriPolyline,
  esriGeometryEngine,
  drawFeedback
) {
    var clz = dojoDeclare([drawFeedback], {
        /**
         *
         **/
        constructor: function (map,coordTool) {
            this.syncEvents();
            this.inherited(arguments);
            this.circlePoints = [];
        },
        
        /*

        */
        syncEvents: function () {
            dojoTopic.subscribe(
                'manual-rangering-center-point-input',
                dojoLang.hitch(this, this.onCenterPointManualInputHandler)
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
        
        /**
         *
         **/
        clearGraphics: function (evt) {
            this.map.graphics.clear();
        },
        
        /*
        Handler for the manual input of a center point
        */
        onCenterPointManualInputHandler: function (centerPoint) {
            this._points = [];
            this._points.push(centerPoint.offset(0, 0));
            this.set('startPoint', this._points[0]);
            this.map.centerAt(centerPoint);
        },

        /**
         *
         **/
        _onClickHandler: function (evt) {
            var snapPoint;
            if (this.map.snappingManager) {
                snapPoint = this.map.snappingManager._snappingPoint;
            }

            var start = snapPoint || evt.mapPoint;
            var map = this.map;

            this._points.push(start.offset(0, 0));
            if (this._points.length === 1) {
                this.set('startPoint', this._points[0]);
            }
            this.circlePoints.push(start.offset(0, 0));

            switch (this._geometryType) {

                case esriDraw.POINT:
                    this._drawEnd(start.offset(0, 0));
                    this._setTooltipMessage(0);
                    break;

                case esriDraw.POLYLINE:
                    var pline = new esriPolyline({
                        paths: [[[start.x, start.y], [start.x, start.y]]],
                        spatialReference: map.spatialReference
                    });

                    //var tgra = new esriGraphic(pline, this.lineSymbol);
                    this.lgraphic = new esriGraphic(pline, this.lineSymbol);

                    if (map.snappingManager) {
                        map.snappingManager._setGraphic(this._graphic);
                    }

                    if (this._points.length > 1) {
                        if (this.circleGraphic) {
                            var circleGraphic = new esriGraphic(this.circleGraphic.geometry, this.fillSymbol);
                            this.map.graphics.add(circleGraphic);
                        }
                    }

                    if (this._points.length > 0) {
                        if (!this._onMouseMoveHandler_connect) {
                            this._onMouseMoveHandler_connect = dojoConnect.connect(this.map, 'onMouseMove', this._onMouseMoveHandler);
                        }
                        if (!this._onDoubleClickHandler_connect) {
                            this._onDoubleClickHandler_connect = dojoConnect.connect(this.map, 'onDblClick', dojoLang.hitch(this, this._onDoubleClickHandler));
                        }
                    }
                    break;
            }

            this._setTooltipMessage(this._points.length);
            if (this._points.length > 1) {
                var tooltip = this._tooltip;
                if (tooltip) {
                    tooltip.innerHTML = 'Double-click to finish drawing range rings';
                }
            }
        },

        /**
         *
         **/
        _onMouseMoveHandler: function (evt) {
            var snapPoint;
            if (this.map.snappingManager) {
                snapPoint = this.map.snappingManager._snappingPoint;
            }

            var start = this._points[0];

            var end = snapPoint || evt.mapPoint;
            var tGraphic = this.lgraphic;
            var geom = tGraphic.geometry;

            geom.setPoint(0, 0, { x: start.x, y: start.y });
            geom.setPoint(0, 1, { x: end.x, y: end.y });

            var length = esriGeometryEngine.geodesicLength(geom, 9001);

            var circleGeometry = new esriCircle(start, {
                radius: length,
                geodesic: true,
                numberOfPoints: 360
            });

            if (this.circleGraphic) {
                this.map.graphics.remove(this.circleGraphic);
            }
            circleGeometry = dojoLang.mixin(circleGeometry, {
                distanceDirectionType: "military-tools-range-rings"
            });
            this.circleGraphic = new esriGraphic(circleGeometry, this.fillSymbol);
            this.map.graphics.add(this.circleGraphic);
            //this.lgraphic.setGeometry(geom);
        },

        /**
         *
         **/
        _onDoubleClickHandler: function (evt) {
            this.disconnectOnMouseMoveHandlers();            
            var points = dojoLang.clone(this.circlePoints);
            this.cleanup();
            this._clear();
            this._setTooltipMessage(0);
            var geom = dojoLang.mixin(this.circleGraphic.geometry, {
                circlePoints: points
            });
            this._drawEnd(geom);
        },
        
        /*
         *
         */
        disconnectOnMouseMoveHandlers: function (evt) {
            dojoConnect.disconnect(this._onMouseMoveHandler_connect);
            dojoConnect.disconnect(this._onDoubleClickHandler_connect);
            this._onDoubleClickHandler_connect = null;
        },

        /*
         *
         */
        cleanup: function (evt) {
            for (var i = this.map.graphics.graphics.length - 1; 0 <= i ; i--) {
                if (this.map.graphics.graphics[i].geometry.hasOwnProperty("distanceDirectionType")) {
                    var circleGraphic = this.map.graphics.graphics[i];
                    this.map.graphics.remove(circleGraphic);
                }
            }
            this.circlePoints = [];
        }

    });
    return clz;
});
