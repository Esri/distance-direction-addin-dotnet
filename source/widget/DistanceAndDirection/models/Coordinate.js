define([
  'dojo/_base/declare',
  'dojo/_base/array',
  'dojo/_base/lang',
  'dojo/Stateful',
  'dojo/topic',
  'dojo/Deferred',
  'esri/geometry/Point',
  'esri/SpatialReference',
  'esri/geometry/webMercatorUtils',
  '../util',
  './dialogConfirm',
  './ConfirmNotation'
], function (
  dojoDeclare,
  dojoArray,
  dojoLang,
  dojoStateful,
  dojoTopic,
  DojoDeferred,
  EsriPoint,
  EsriSpatialReference,
  EsriWMUtils,
  CoordinateUtilities,
  dialogConfirm,
  ConfirmNotation
) {

  var mo = dojoDeclare([dojoStateful], {

    formatPrefix: false,
    _formatPrefixSetter: function (value) {
      this.formatPrefix = value;
    },

    inputString: null,
    _inputStringSetter: function (value) {
      this.inputString = value;
    },

    formatString: 'YN XE',
    _formatStringSetter: function (value) {
      this.formatString = value;
    },

    inputType: 'UNKNOWN',

    formatType: 'DD',
    
    _formatTypeSetter: function (value) {
      this.formatType = value;
      this.getFormattedValue();
    },

    outputString: '',    

    coordinateEsriGeometry: null,
    
    _coordinateEsriGeometrySetter: function (value) {
        var pt;
        if (value == null) return;
      if (value.spatialReference.wkid !== 4326) {
        pt = EsriWMUtils.webMercatorToGeographic(value);
      } else {
        pt = value;
      }
      this.coordinateEsriGeometry = pt;
      this.getFormattedValue();
    },

    /**
     *
     **/
    constructor: function (args) {
      dojoDeclare.safeMixin(this, args);
      this.util = new CoordinateUtilities(this.appConfig.geometryService);
    },

    /**
     *
     **/
    getInputType: function () {
      this.inputTypeDef = new DojoDeferred();
      var sanitizedInput = this.util.getCleanInput(this.inputString);
      this.util.getCoordinateType(sanitizedInput).then(dojoLang.hitch(this, function(itm){
        if (itm) {
          if (itm.length == 1) {
            var sortedInput = this.processCoordTextInput(sanitizedInput, itm[0],false);
            this.util.getXYNotation(sortedInput, itm[0].conversionType).then(dojoLang.hitch(this,function(r){
              if (r.length <= 0 || (!r[0][0] && r[0][0] != 0)){
                this.hasError = true;
                this.valid = false;
                this.message = 'Invalid Coordinate';
                this.inputTypeDef.resolve(this);
              } else {
                this.isManual = true;
                this.valid = true;
                this.formatType = itm[0].conversionType;                
                this.inputType = itm[0].conversionType;
                this.coordinateEsriGeometry = new EsriPoint(r[0][0],r[0][1],new EsriSpatialReference({wkid: 4326}));
                this.message = '';
                this.inputTypeDef.resolve(this);
              }              
              })), dojoLang.hitch(this, function (r) {
                this.hasError = true;
                this.valid = false;
                this.inputType = 'UNKNOWN';
                this.message = 'Invalid Coordinate';
                this.inputTypeDef.resolve(this);
              });
          } else {
            var dialog = new dialogConfirm({
               title: 'Confirm Input Notation',
               content: new ConfirmNotation(itm),
               style: "width: 400px",
               hasSkipCheckBox: false
            });
            
            dialog.show().then(dojoLang.hitch(this, function() {                    
              var singleMatch = dojoArray.filter(itm, function (singleItm) {
                return singleItm.name == dialog.content.comboOptions.get('value');
              });
              var withStr = this.processCoordTextInput(sanitizedInput, singleMatch[0],false);
              this.util.getXYNotation(withStr, singleMatch[0].conversionType).then(dojoLang.hitch(this,function(r) {
                  if (r.length <= 0 || (!r[0][0] && r[0][0] != 0)){
                  this.hasError = true;
                  this.valid = false;
                  this.message = 'Invalid Coordinate';
                  this.inputTypeDef.resolve(this);
                } else {
                  this.isManual = true;
                  this.valid = true;
                  this.inputType = itm[0].conversionType;
                  this.formatType = itm[0].conversionType;
                  this.coordinateEsriGeometry = new EsriPoint(r[0][0],r[0][1],new EsriSpatialReference({wkid: 4326}));
                  this.message = '';
                  this.inputTypeDef.resolve(this);
                }              
                })), dojoLang.hitch(this, function (r) {
                  this.hasError = true;
                  this.valid = false;
                  this.inputType = 'UNKNOWN';
                  this.message = 'Invalid Coordinate';
                  this.inputTypeDef.resolve(this);
                });
            }, function() {
               deferred.reject();
            }));
          }
        } else {            
            this.hasError = true;
            this.valid = false;
            this.inputType = 'UNKNOWN';
            this.message = 'Invalid Coordinate';
            this.inputTypeDef.resolve(this);
        }
      }));
      return this.inputTypeDef;
    },
    
    /**
     *
     **/
    processCoordTextInput: function (withStr, asType, testingMode) {
        
        var match = asType.pattern.exec(withStr);            
        
        var northSouthPrefix, northSouthSuffix, eastWestPrefix, eastWestSuffix, latDeg, longDeg, latMin, longMin, latSec, longSec;
        
        var prefixSuffixError = false;
        
        var conversionType = asType.name;
        
        switch (asType.name) {
          case 'DD':
            northSouthPrefix = match[2];
            northSouthSuffix = match[7];
            eastWestPrefix = match[10];
            eastWestSuffix = match[16];
            latDeg = match[3].replace(/[,:]/, '.');
            longDeg = match[11].replace(/[,:]/, '.');   
            conversionType = 'DD'; 
            break; 
          case 'DDrev':
            northSouthPrefix = match[11];
            northSouthSuffix = match[16];
            eastWestPrefix = match[2];
            eastWestSuffix = match[8];
            latDeg = match[12].replace(/[,:]/, '.');
            longDeg = match[3].replace(/[,:]/, '.');  
            conversionType = 'DD';             
            break;            
          case 'DDM':            
            northSouthPrefix = match[2];
            northSouthSuffix = match[7];
            eastWestPrefix = match[10];
            eastWestSuffix = match[15];
            latDeg = match[3];
            latMin = match[4].replace(/[,:]/, '.');
            longDeg = match[11];
            longMin = match[12].replace(/[,:]/, '.');
            conversionType = 'DDM';             
            break;
          case 'DDMrev':
            northSouthPrefix = match[10];
            northSouthSuffix = match[15];
            eastWestPrefix = match[2];
            eastWestSuffix = match[7];
            latDeg = match[11];
            latMin = match[12].replace(/[,:]/, '.');
            longDeg = match[3];
            longMin = match[4].replace(/[,:]/, '.');                
            conversionType = 'DDM';            
            break;
          case 'DMS':
            northSouthPrefix = match[2];
            northSouthSuffix = match[8];
            eastWestPrefix = match[11];
            eastWestSuffix = match[17];
            latDeg = match[3];
            latMin = match[4];
            latSec = match[5].replace(/[,:]/, '.');
            longDeg = match[12];
            longMin = match[13];
            longSec = match[14].replace(/[,:]/, '.');
            conversionType = 'DMS';               
            break;
          case 'DMSrev':
            northSouthPrefix = match[11];
            northSouthSuffix = match[17];
            eastWestPrefix = match[2];
            eastWestSuffix = match[8];
            latDeg = match[12];
            latMin = match[13];
            latSec = match[14].replace(/[,:]/, '.');
            longDeg = match[3];
            longMin = match[4];
            longSec = match[5].replace(/[,:]/, '.');
            conversionType = 'DMS';               
            break;
        }
        
        //check for north/south prefix/suffix
        if(northSouthPrefix && northSouthSuffix) {
              prefixSuffixError = true;                    
              new RegExp(/[Ss-]/).test(northSouthPrefix)?northSouthPrefix = '-':northSouthPrefix = '+';
            } else {
              if(northSouthPrefix && new RegExp(/[Ss-]/).test(northSouthPrefix)){
                northSouthPrefix = '-';
              } else {
                if(northSouthSuffix && new RegExp(/[Ss-]/).test(northSouthSuffix)){
                  northSouthPrefix = '-';
                } else {
                  northSouthPrefix = '+';
                }
              }
            }
            
        //check for east/west prefix/suffix
        if(eastWestPrefix && eastWestSuffix) {
          prefixSuffixError = true;                    
          new RegExp(/[Ww-]/).test(eastWestPrefix)?eastWestPrefix = '-':eastWestPrefix = '+';
        } else {
          if(eastWestPrefix && new RegExp(/[Ww-]/).test(eastWestPrefix)){
            eastWestPrefix = '-';
          } else {
            if(eastWestSuffix && new RegExp(/[Ww-]/).test(eastWestSuffix)){
              eastWestPrefix = '-';
            } else {
              eastWestPrefix = '+';
            }
          }
        }
        
        //give user warning if lat or long is determined as having a prefix and suffix 
        if(prefixSuffixError) {
          if(!testingMode) {
          new JimuMessage({message: 'The input coordinate has been detected as having both a prefix and suffix for the latitude or longitude value, returned coordinate is based on the prefix.'});
          }
        }            
        
        switch (conversionType) {
          case 'DD':               
            withStr = northSouthPrefix + latDeg + "," + eastWestPrefix + longDeg;
            break;              
          case 'DDM':
            withStr = northSouthPrefix + latDeg + " " + latMin + "," + eastWestPrefix + longDeg + " " + longMin;
            break;
          case 'DMS':
            withStr = northSouthPrefix + latDeg + " " + latMin + " " + latSec + "," + eastWestPrefix + longDeg + " " + longMin + " " + longSec;
            break;
          default:
            withStr = withStr;              
            break;
        }
        
        return withStr;
    },

    /**
     *
     **/
    getInputTypeSync: function () {
      var v = this.util.getCoordinateType(this.inputString);
      return v !== null;
    },

    /**
     *
     **/
    getFormattedValue: function () {
      if (!this.coordinateEsriGeometry) {
        return;
      }
      this.util.getCoordValues({
        x: this.coordinateEsriGeometry.x,
        y: this.coordinateEsriGeometry.y
      }, this.formatType, 6).then(dojoLang.hitch(this, function (r) {
        this.set('outputString', this.getCoordUI(r));
        }));
    },

    /**
     * Get coordinate notation in user provided format
     **/
    getCoordUI: function (fromValue) {
      var as = this.get('formatPrefix');
      var r;
      var formattedStr;
      switch (this.formatType) {
      case 'DD':
          r = this.util.getFormattedDDStr(fromValue, this.formatString, as);
          formattedStr = r.formatResult;
          break;
      case 'DDM':
          r = this.util.getFormattedDDMStr(fromValue, this.formatString, as);
          formattedStr = r.formatResult;
          break;
      case 'DMS':
          r = this.util.getFormattedDMSStr(fromValue, this.formatString, as);
          formattedStr = r.formatResult;
          break;
      case 'USNG':
          r = this.util.getFormattedUSNGStr(fromValue, this.formatString, as);
          formattedStr = r.formatResult;
          break;
      case 'MGRS':
          r = this.util.getFormattedMGRSStr(fromValue, this.formatString, as);
          formattedStr = r.formatResult;
          break;
      case 'GARS':
          r = this.util.getFormattedGARSStr(fromValue, this.formatString, as);
          formattedStr = r.formatResult;
          break;
      case 'GEOREF':
          r = this.util.getFormattedGEOREFStr(fromValue, this.formatString, as);
          formattedStr = r.formatResult;
          break;
      case 'UTM':
          r = this.util.getFormattedUTMStr(fromValue, this.formatString, as);
          formattedStr = r.formatResult;
          break;
      case 'UTM (H)':
          r = this.util.getFormattedUTMHStr(fromValue, this.formatString, as);
          formattedStr = r.formatResult;
          break;
      }
      return formattedStr;
    }
  });

  return mo;
});
