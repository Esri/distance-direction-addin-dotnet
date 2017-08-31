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
  'dojo/_base/lang',
  'dojo/on',
  'dojo/topic',
  'dojo/_base/html',
  'dojo/dom-attr',
  'dojo/dom-class',
  'dojo/string',
  'dojo/mouse',
  'dojo/number',
  'dojo/keys',
  'dijit/_WidgetBase',
  'dijit/_TemplatedMixin',
  'dijit/_WidgetsInTemplateMixin',
  'dijit/TitlePane',
  'dijit/TooltipDialog',
  'dijit/popup',
  'jimu/dijit/Message',
  'esri/layers/FeatureLayer',
  'esri/symbols/SimpleFillSymbol',
  'esri/symbols/SimpleMarkerSymbol',
  'esri/symbols/TextSymbol',
  'esri/graphic',
  'esri/geometry/webMercatorUtils',
  'esri/geometry/Polyline',
  'esri/geometry/Polygon',
  'esri/geometry/Point',
  'esri/geometry/Circle',
  'esri/tasks/FeatureSet',
  'esri/layers/LabelClass',
  '../models/CircleFeedback',
  '../models/ShapeModel',
  '../views/CoordinateInput',
  '../views/EditOutputCoordinate',
  'dojo/text!../templates/TabCircle.html',
  'dijit/form/NumberTextBox',
  'dijit/form/Select',
  'jimu/dijit/CheckBox'  
], function (
  dojoDeclare,
  dojoLang,
  dojoOn,
  dojoTopic,
  dojoHTML,
  dojoDomAttr,
  dojoDomClass,
  dojoString,
  dojoMouse,
  dojoNumber,
  dojoKeys,
  dijitWidgetBase,
  dijitTemplatedMixin,
  dijitWidgetsInTemplate,
  dijitTitlePane,
  DijitTooltipDialog,
  DijitPopup,
  Message,
  EsriFeatureLayer,
  EsriSimpleFillSymbol,
  EsriSimpleMarkerSymbol,
  EsriTextSymbol,
  EsriGraphic,
  EsriWMUtils,
  EsriPolyline,
  EsriPolygon,
  EsriPoint,
  EsriCircle,
  EsriFeatureSet,
  EsriLabelClass,
  DrawFeedBack,
  ShapeModel,
  CoordInput,
  EditOutputCoordinate,
  templateStr
  ) {
  'use strict';
  return dojoDeclare([dijitWidgetBase, dijitTemplatedMixin, dijitWidgetsInTemplate], {
    templateString: templateStr,
    baseClass: 'jimu-widget-TabCircle',

    /*
     * class constructor
     */
    constructor: function (args) {
      dojoDeclare.safeMixin(this, args);
    },

    /*
     * dijit post create
     */
    postCreate: function () {

      this.useCalculatedDistance = false;

      this.currentLengthUnit = this.lengthUnitDD.get('value');

      this._circleSym = new EsriSimpleFillSymbol(this.circleSymbol);

      this._ptSym = new EsriSimpleMarkerSymbol(this.pointSymbol);

      this._labelSym = new EsriTextSymbol(this.labelSymbol);

      this.map.addLayer(this.getLayer());
      
      this.coordTool = new CoordInput({appConfig: this.appConfig}, this.startPointCoords);
      
      this.coordTool.inputCoordinate.formatType = 'DD';

      this.coordinateFormat = new DijitTooltipDialog({
        content: new EditOutputCoordinate(),
        style: 'width: 400px'
      });
      
      if(this.appConfig.theme.name === 'DartTheme')
      {
        dojoDomClass.add(this.coordinateFormat.domNode, 'dartThemeClaroDijitTooltipContainerOverride');
      }

      // add extended toolbar
      this.dt = new DrawFeedBack(this.map,this.coordTool.inputCoordinate.util);
      
      this.dt.setFillSymbol(this._circleSym);

      this.syncEvents();
      
      this.checkValidInputs();
    },

    /*
     * upgrade graphicslayer so we can use the label params
     */
    getLayer: function () {
      if (!this._gl) {
        var layerDefinition = {
          'id': 'circleLayer',
          'geometryType': 'esriGeometryPolygon',
          'fields': [{
              'name': 'Label',
              'type': 'esriFieldTypeString',
              'alias': 'Label'
            }]
          };

          var lblexp = {'labelExpressionInfo': {'value': '{Label}'}};
          var lblClass = new EsriLabelClass(lblexp);
          lblClass.symbol = this._labelSym;

          var featureCollection = {
            layerDefinition: layerDefinition,
            featureSet: new EsriFeatureSet()
          };

          this._gl = new EsriFeatureLayer(featureCollection, { 
            id: 'Distance & Direction - Circle Graphics',
            showLabels: true
          });

          this._gl.setLabelingInfo([lblClass]);

          return this._gl;
      }
    },

    /*
     * Start up event listeners
     */
    syncEvents: function () {
      
      dojoTopic.subscribe('TAB_SWITCHED', dojoLang.hitch(this, this.tabSwitched));

      this.distCalcControl.watch('open',dojoLang.hitch(this, this.distCalcDidExpand));
      
      this.dt.watch('length', dojoLang.hitch(this, function (n, ov, nv) {
        this.circleLengthDidChange(nv);
      }));

      this.dt.watch('startPoint',dojoLang.hitch(this, function (r, ov, nv) {
        this.coordTool.inputCoordinate.set('coordinateEsriGeometry', nv);
        this.coordTool.inputCoordinate.set('inputType',this.coordTool.inputCoordinate.formatType);
        this.dt.addStartGraphic(nv, this._ptSym);
      }));
      
      this.dt.watch('endPoint' , dojoLang.hitch(this, function (r, ov, nv) {
        this.coordTool.inputCoordinate.set('coordinateEsriGeometry',  nv);        
      }));

      this.coordTool.inputCoordinate.watch('outputString', dojoLang.hitch(this, function (r, ov, nv) {
          if(!this.coordTool.manualInput){this.coordTool.set('value', nv);}
      }));

      this.dt.on('draw-complete',dojoLang.hitch(this, this.feedbackDidComplete));
        
              

      this.own(
      
        dojoOn(this.coordTool, 'keyup',dojoLang.hitch(this, this.coordToolKeyWasPressed)),

        this.lengthUnitDD.on('change',dojoLang.hitch(this, this.lengthUnitDDDidChange)),

        this.creationType.on('change',dojoLang.hitch(this, this.creationTypeDidChange)),

        this.distanceUnitDD.on('change',dojoLang.hitch(this, this.distanceInputDidChange)),

        this.timeUnitDD.on('change',dojoLang.hitch(this, this.timeInputDidChange)),

        dojoOn(this.coordinateFormatButton, 'click',dojoLang.hitch(this, this.coordinateFormatButtonWasClicked)),

        dojoOn(this.addPointBtn, 'click',dojoLang.hitch(this, this.pointButtonWasClicked)),

        dojoOn(this.timeInput, 'change',dojoLang.hitch(this, this.timeInputDidChange)),

        dojoOn(this.distanceInput, 'change',dojoLang.hitch(this, this.distanceInputDidChange)),

        dojoOn(this.distanceInput, 'keyup',dojoLang.hitch(this, this.distanceInputKeyWasPressed)),

        dojoOn(this.clearGraphicsButton,'click',dojoLang.hitch(this, this.clearGraphics)),
        
        dojoOn(this.interactiveCircle, 'change',dojoLang.hitch(this, this.interactiveCheckBoxChanged)),

        dojoOn(this.coordinateFormat.content.applyButton, 'click',dojoLang.hitch(this, function () {
          var fs = this.coordinateFormat.content.formats[this.coordinateFormat.content.ct];
          var cfs = fs.defaultFormat;
          var fv = this.coordinateFormat.content.frmtSelect.get('value');
          if (fs.useCustom) {
            cfs = fs.customFormat;
          }
          this.coordTool.inputCoordinate.set('formatPrefix', this.coordinateFormat.content.addSignChkBox.checked);
          this.coordTool.inputCoordinate.set('formatString', cfs);
          this.coordTool.inputCoordinate.set('formatType', fv);
          this.setCoordLabel(fv);

          DijitPopup.close(this.coordinateFormat);
        })),

        dojoOn(this.coordinateFormat.content.cancelButton, 'click',dojoLang.hitch(this, function () {
          DijitPopup.close(this.coordinateFormat);
        })),
        
        dojoOn(this.radiusInputDiv, dojoMouse.leave, dojoLang.hitch(this, this.checkValidInputs)) 
      );
    },

    /*
     *
     */
    circleLengthDidChange: function (l) {
      var fl = dojoNumber.format(l, {places: 2});      
      this.radiusInput.set('value', fl);      
    },
    
    /*
     * checkbox changed
     */
    interactiveCheckBoxChanged: function () {
      this.tabSwitched();
      if(this.interactiveCircle.checked) {
        this.radiusInput.set('disabled', true);
        this.distCalcControl.set('open', false);
        this.distCalcControl.set('open', false);
        this.distCalcControl.set('toggleable', false);
      } else {
        this.radiusInput.set('disabled', false);
        this.distCalcControl.set('disabled', false);
        this.distCalcControl.set('toggleable', true);
      }
      this.checkValidInputs();
    },
    
    /*
     * catch key press in start point
     */
    coordToolKeyWasPressed: function (evt) {
      this.dt.removeStartGraphic();
      if (evt.keyCode === dojoKeys.ENTER) {
        this.coordTool.inputCoordinate.getInputType().then(dojoLang.hitch(this, function (r) {
          if(r.inputType == "UNKNOWN"){
            var alertMessage = new Message({
              message: 'Unable to determine input coordinate type please check your input.'
            });
            this.coordTool.inputCoordinate.coordinateEsriGeometry = null;
            this.checkValidInputs();
          } else {
            dojoTopic.publish(
              'manual-circle-center-point-input',
              this.coordTool.inputCoordinate.coordinateEsriGeometry
            );
            this.setCoordLabel(r.inputType);
            var fs = this.coordinateFormat.content.formats[r.inputType];
            this.coordTool.inputCoordinate.set('formatString', fs.defaultFormat);
            this.coordTool.inputCoordinate.set('formatType', r.inputType);
            this.dt.addStartGraphic(r.coordinateEsriGeometry, this._ptSym);
            this.checkValidInputs();
          }
        }));
      }
      
    },

    /*
     *
     */
    coordinateFormatButtonWasClicked: function () {
      this.coordinateFormat.content.set('ct', this.coordTool.inputCoordinate.formatType);
      DijitPopup.open({
        popup: this.coordinateFormat,
        around: this.coordinateFormatButton
      });
    },

    /*
     *
     */
    distCalcDidExpand: function () {
      this.dt.deactivate();
      this.dt.cleanup();
      this.dt.disconnectOnMouseMoveHandler();
      
      this.coordTool.inputCoordinate.isManual = true;
      
      if (this.distCalcControl.get('open')) {
        this.radiusInput.set('disabled', true);
      } else {
        this.radiusInput.set('disabled', false);
        this.timeInput.set('value', 1);
        this.distanceInput.set('value', 1);
      }
    },    

    /*
     *
     */
    timeInputDidChange: function () {
      this.currentTimeInSeconds = this.timeInput.get('value')  * this.timeUnitDD.get('value');
      this.getCalculatedDistance();
    },

    /*
     * 
     */
    distanceInputKeyWasPressed: function (evt) {      
      this.distanceInputDidChange();
      if (evt.keyCode === dojoKeys.ENTER) {
        if(this.coordTool.inputCoordinate.outputString && this.coordTool.inputCoordinate.inputString != ''){
          this.removeManualGraphic();          
          this.setGraphic(true);
          this.dt._onDoubleClickHandler();
        } else {
          var alertMessage = new Message({
              message: 'No center point set, please check your input.'
            });
        }
      }      
    },
    
    /*
     * 
     */
    okButtonClicked: function (evt) {
      if(!dojoDomClass.contains(this.okButton, "jimu-state-disabled")) {
        this.removeManualGraphic();          
        this.setGraphic(true);
      }       
    },    

    /*
     *
     */
    distanceInputDidChange: function () {
      var currentRateInMetersPerSecond = (
        this.distanceInput.get('value') *
        this.distanceUnitDD.value.split(';')[0]
      ) / this.distanceUnitDD.value.split(';')[1];

      this.currentDistanceInMeters = currentRateInMetersPerSecond;
      this.getCalculatedDistance();
    },

    /*
     *
     */
    getCalculatedDistance: function () {
      if ((this.currentTimeInSeconds && this.currentTimeInSeconds > 0) &&
        (this.currentDistanceInMeters && this.currentDistanceInMeters > 0)) {
        this.calculatedRadiusInMeters = this.currentTimeInSeconds * this.currentDistanceInMeters;
        this.useCalculatedDistance = true;
        var fr = 0;
        switch (this.currentLengthUnit){
          case 'feet':
            fr = this.calculatedRadiusInMeters * 3.2808399;
            break;
          case 'meters':
            fr = this.calculatedRadiusInMeters;
            break;
          case 'yards':
            fr = this.calculatedRadiusInMeters * 1.0936133;
            break;
          case 'kilometers':
            fr = this.calculatedRadiusInMeters * 0.001;
            break;
          case 'miles':
            fr = this.calculatedRadiusInMeters * 0.000621371192;
            break;
          case 'nautical-miles':
            fr = this.calculatedRadiusInMeters * 0.000539957;
            break;
        }
        fr = this.creationType.get('value') === 'Diameter'?fr*2:fr;
        fr = dojoNumber.format(fr, {places: '4'});
        
        this.radiusInput.set('value', fr);
        //this.setGraphic();
      } else {
        this.calculatedRadiusInMeters = null;
        this.useCalculatedDistance = true;
      }
    },

    /*
     * Button click event, activate feedback tool
     */
    pointButtonWasClicked: function () {
      this.coordTool.manualInput = false;
      dojoTopic.publish('clear-points');
      this.map.disableMapNavigation();
      this.dt.set('isDiameter', this.creationType.get('value') === 'Diameter');
      if (this.distCalcControl.get('open')) {
        this.dt.activate('point');
      } else {
        if(!this.interactiveCircle.checked) {
          this.dt.activate('point');
        } else {
          this.dt.activate('polyline');
        }
      }
      dojoDomClass.toggle(this.addPointBtn, 'jimu-state-active');
    },

    /*
     *
     */
    lengthUnitDDDidChange: function () {
      this.currentLengthUnit = this.lengthUnitDD.get('value');
      var currentCreateCircleFrom = this.creationType.get('value');
      this.dt.set('lengthUnit', this.currentLengthUnit);
    },

    /*
     *
     */
    creationTypeDidChange: function() {
      var currentCreateCircleFrom = this.creationType.get('value');
      this.radiusDiameterLabel.innerHTML = currentCreateCircleFrom;
    },

    /*
     *
     */
    feedbackDidComplete: function (results) {
        if(!results.geometry.center){
          dojoDomClass.toggle(this.addPointBtn, 'jimu-state-active');
          this.checkValidInputs();
          return;
        }
        var center = results.geometry.center;
        var edge = new EsriPoint(results.geometry.rings[0][0][0],
          results.geometry.rings[0][0][1],
          results.geometry.center.spatialReference);
        var geom = new EsriPolyline(results.geometry.center.spatialReference);
        geom.addPath([center, edge]);
        this.setGraphic(false, geom);
    },

    /*
     *
     */
    setCoordLabel: function (toType) {
      this.coordInputLabel.innerHTML = dojoString.substitute(
        'Center Point (${crdType})', {
          crdType: toType
        }
      );
    },

    /*
     *
     */
    removeManualGraphic: function () {
        if (this.tempGraphic != null) {
            this._gl.remove(this.tempGraphic);
        }
        this.dt.removeStartGraphic();
    },

    /*
     *
     */
    setGraphic: function (isManual, lineGeom) {
      if(!isManual) {
        dojoDomClass.toggle(this.addPointBtn, 'jimu-state-active');
      }

      var results = {};
      this.map.enableMapNavigation();
      this.dt.deactivate();
      this.dt.removeStartGraphic();      

      
      if (this.creationType.get('value') === 'Diameter') {
        results.calculatedDistance = dojoNumber.parse(this.radiusInput.get('value'), {places: '0,99'})/2;
      } else {
        results.calculatedDistance = dojoNumber.parse(this.radiusInput.get('value'), {places: '0,99'});
      }

      results.calculatedDistance = this.coordTool.inputCoordinate.util.convertToMeters(results.calculatedDistance,this.lengthUnitDD.get('value'));

      results.geometry = this.coordTool.inputCoordinate.coordinateEsriGeometry;
      results.lineGeometry = lineGeom;
      
      var centerPoint;
      this.map.spatialReference.wkid === 4326?centerPoint = results.geometry:centerPoint = EsriWMUtils.geographicToWebMercator(results.geometry);
      
      var newCurrentCircle = new EsriCircle({
          center: centerPoint,
          radius: results.calculatedDistance,
          geodesic: true,
          numberOfPoints: 360
      });
      
      
      
      var newPolygon  = new EsriPolygon(this.map.spatialReference);
      
      newPolygon.addRing(newCurrentCircle.rings[0]);
 
      var cGraphic = new EsriGraphic(
        newPolygon,
        this._circleSym,
        {
          'Label': this.creationType.get('value') + " " + this.radiusInput.get('value').toString() + " " + this.lengthUnitDD.get('value').charAt(0).toUpperCase() + this.lengthUnitDD.get('value').slice(1)
        }
      );      

      this._gl.add(cGraphic);

      this.map.setExtent(newPolygon.getExtent().expand(3));
      

      this.emit('graphic_created', this.currentCircle);
      this.dt.set('startPoint', null);      
    },

    /*
     * Remove graphics and reset values
     */
    clearGraphics: function () {
      if (this._gl) {
        // graphic layers
        this._gl.clear();
        this._gl.refresh();

        // ui controls
        this.clearUI(false);
      }
      this.checkValidInputs();
      //refresh each of the feature/graphic layers to enusre labels are removed
      for(var j = 0; j < this.map.graphicsLayerIds.length; j++) {
        this.map.getLayer(this.map.graphicsLayerIds[j]).refresh();
      }
    },

    /*
     * reset ui controls
     */
    clearUI: function (keepCoords) {
      if (!keepCoords){
        this.coordTool.clear();
      }
      this.dt.set('startPoint', null);
      this.useCalculatedDistance = false;
      this.currentCircle = null;

      dojoDomClass.remove(this.addPointBtn, 'jimu-state-active');
      dojoDomAttr.set(this.startPointCoords, 'value', '');
      this.radiusInput.set('value', 1000);
      this.timeInput.set('value', 1);
      this.distanceInput.set('value', 1);
    },

    /*
     *
     */
    setGraphicsHidden: function () {
      if (this._gl) {
        this._gl.hide();
      }
    },

    /*
     *
     */
    setGraphicsShown: function () {
      if (this._gl) {
        this._gl.show();
      }
    },
    
    /*
    * Activate the ok button if all the requried inputs are valid
    */
    checkValidInputs: function () {
      dojoDomClass.add(this.okButton, 'jimu-state-disabled');
        if(!this.interactiveCircle.checked) {
          if(this.coordTool.inputCoordinate.coordinateEsriGeometry != null && this.radiusInput.isValid()){
            dojoDomClass.remove(this.okButton, 'jimu-state-disabled');
          }            
        }
    },
    
    /*
     * Make sure any active tools are deselected to prevent multiple actions being performed
     */
    tabSwitched: function () {
      this.dt.deactivate();
      this.dt.cleanup();
      this.dt.disconnectOnMouseMoveHandler();
      this.dt.set('startPoint', null);
      this.map.enableMapNavigation();
      this.dt.removeStartGraphic();
      dojoDomClass.remove(this.addPointBtn, 'jimu-state-active');
    }

  });
});
