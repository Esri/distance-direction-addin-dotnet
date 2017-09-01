///////////////////////////////////////////////////////////////////////////
// Copyright (c) 2015 Esri. All Rights Reserved.
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
  'dojo/_base/Deferred',
  'esri/tasks/GeometryService'    
], function (
  dojoDeclare,
  dojoArray,
  Deferred,
  EsriGeometryService
) {
  'use strict';
  return dojoDeclare(null, {

    /**
     *
     **/
    constructor: function (geoServiceURL) {
        if (!geoServiceURL) {
          geoServiceURL = '//utility.arcgisonline.com/arcgis/rest/services/Geometry/GeometryServer';
        }
        this.geomService = new EsriGeometryService(geoServiceURL);
    },

    /**
     *
     **/
    getCleanInput: function (fromstr) {
        fromstr = fromstr.replace(/\n/g,'');
        fromstr = fromstr.replace(/\s+/g, ' ').trim();
        return fromstr.toUpperCase();
    },
    
    /**
     * Send request to get dd coordinates in format string
     **/
    getCoordValues: function (fromInput, toType, numDigits) {
      var deferred = new Deferred();
      var nd = numDigits || 6;
      var tt;
      if (toType.name) {
        tt = toType.name;
      } else {
        tt = toType;
      }
      /**
       * for parameter info
       * http://resources.arcgis.com/en/help/arcgis-rest-api/#/To_GeoCoordinateString/02r30000026w000000/
       **/
      var params = {
          sr: 4326,
          coordinates: [[fromInput.x, fromInput.y]],
          conversionType: tt,
          numOfDigits: nd,
          rounding: true,
          addSpaces: false
      };
            
      switch (toType) {
        case 'DD':
          params.numOfDigits = 6;
          break;
        case 'USNG':
          params.numOfDigits = 5;
          break;            
        case 'MGRS':            
          params.conversionMode = 'mgrsDefault';
          params.numOfDigits = 5;
          break;
        case 'UTM (H)':
          params.conversionType = 'utm';
          params.conversionMode = 'utmNorthSouth';
          params.addSpaces = true;
          break;
        case 'UTM':
          params.conversionType = 'utm';
          params.conversionMode = 'utmDefault';
          params.addSpaces = true;
          break;
        case 'GARS':
          params.conversionMode = 'garsDefault';
          break;
        }         

      this.geomService.toGeoCoordinateString(params).then(function(itm) {
        deferred.resolve(itm); 
      },function() {
        deferred.resolve(null); 
      });
      
      return deferred.promise;
    },

        /**
     *
     **/
    getXYNotation: function (fromStr, toType) {
      var deferred = new Deferred();      
      var a;
      var tt;
      if (toType.name) {
        tt = toType.name;
      } else {
        tt = toType;
      }            
            
      var params = {
        sr: 4326,
        conversionType: tt,
        strings: []
      };

      switch (tt) {
      case 'DD':
      case 'DDM':
      case 'DMS':
        params.numOfDigits = 2;
        a = fromStr.replace(/[°˚º^~*"'′¨˝]/g, '');
        params.strings.push(a);
        break;
      case 'USNG':
        params.strings.push(fromStr);
        params.addSpaces = 'false';
        break;            
      case 'MGRS':            
        params.conversionMode = 'mgrsNewStyle';
        params.strings.push(fromStr);
        params.addSpaces = 'false';
        break;
      case 'UTM (H)':
        params.conversionType = 'utm';
        params.conversionMode = 'utmNorthSouth';
        params.strings.push(fromStr);
        break;
      case 'UTM':
        params.conversionType = 'utm';
        params.conversionMode = 'utmDefault';
        params.strings.push(fromStr);
        break;
      case 'GARS':
        params.conversionMode = 'garsCenter';
        params.strings.push(fromStr);
        break;
      case 'GEOREF':
        params.strings.push(fromStr);
        break;
      }
      
      this.geomService.fromGeoCoordinateString(params).then(function(itm) {
        deferred.resolve(itm); 
      },function() {
        deferred.resolve(null); 
      });
      
      return deferred.promise;
    },
    
    getNotations: function () {    
      var strs = [
        {
          name: 'DD',
          pattern: /^(([NS\+\-\s])*([0-8]?\d([,.]\d*)?|90([,.]0*)?)([°˚º^~*]*)([NS\+\-\s])*)([,:;\s|\/\\]+)(([EW\+\-\s])*([0]?\d?\d([,.]\d*)?|1[0-7]\d([,.]\d*)?|180([,.]0*)?)([°˚º^~*]*)([EW\+\-\s])*)$/,
          notationType: "DD - Latitude/Longitude",
          conversionType: "DD"
        }, {
          name: 'DDrev',
          pattern: /^(([EW\+\-\s])*([0]?\d?\d([,.]\d*)?|1[0-7]\d([,.]\d*)?|180([,.]0*)?)([°˚º^~*]*)([EW\+\-\s])*)([,:;\s|\/\\]+)(([NS\+\-\s])*([0-8]?\d([,.]\d*)?|90([,.]0*)?)([°˚º^~*]*)([NS\+\-\s])*)$/,
          notationType: "DD - Longitude/Latitude",
          conversionType: "DD"
        }, {
          name: 'DDM',
          pattern: /^(([NS\+\-\s])*([0-8]?\d|90)[°˚º^~*\s\-_]+(([0-5]?\d|\d)([,.]\d*)?)['′\s_]*([NS\+\-\s])*)([,:;\s|\/\\]+)(([EW\+\-\s])*([0]?\d?\d|1[0-7]\d|180)[°˚º^~*\s\-_]+(([0-5]\d|\d)([,.]\d*)?)['′\s_]*([EW\+\-\s])*)[\s]*$/,
          notationType: "DDM - Latitude/Longitude",
          conversionType: "DDM"                    
        }, {
          name: 'DDMrev',
          pattern: /^(([EW\+\-\s])*([0]?\d?\d|1[0-7]\d|180)[°˚º^~*\s\-_]+(([0-5]\d|\d)([,.]\d*)?)['′\s_]*([EW\+\-\s])*)([,:;\s|\/\\]+)(([NS\+\-\s])*([0-8]?\d|90)[°˚º^~*\s\-_]+(([0-5]?\d|\d)([,.]\d*)?)['′\s_]*([NS\+\-\s])*)[\s]*$/,
          notationType: "DDM - Longitude/Latitude",
          conversionType: "DDM"                    
        }, {
          name: 'DMS',
          pattern: /^(([NS\+\-\s])*([0-8]?\d|90)[°˚º^~*\s\-_]+([0-5]?\d|\d)['′\s\-_]+(([0-5]?\d|\d)([,.]\d*)?)["¨˝\s_]*([NS\+\-\s])*)([,:;\s|\/\\]+)(([EW\+\-\s])*([0]?\d?\d|1[0-7]\d|180)[°˚º^~*\s\-_]+([0-5]\d|\d)['′\s\-_]+(([0-5]?\d|\d)([,.]\d*)?)["¨˝\s_]*([EW\+\-\s])*)[\s]*$/,
          notationType: "DMS - Latitude/Longitude",
          conversionType: "DMS" 
        }, {
          name: 'DMSrev',
          pattern: /^(([EW\+\-\s])*([0]?\d?\d|1[0-7]\d|180)[°˚º^~*\s\-_]+([0-5]\d|\d)['′\s\-_]+(([0-5]?\d|\d)([,.]\d*)?)["¨˝\s_]*([EW\+\-\s])*)([,:;\s|\/\\]+)(([NS\+\-\s])*([0-8]?\d|90)[°˚º^~*\s\-_]+([0-5]?\d|\d)['′\s\-_]+(([0-5]?\d|\d)([,.]\d*)?)["¨˝\s_]*([NS\+\-\s])*)[\s]*$/,
          notationType: "DMS - Longitude/Latitude",
          conversionType: "DMS" 
        }, {
          name: 'GARS',
          pattern: /^\d{3}[a-zA-Z]{2}[1-4]?[1-9]?$/,
          notationType: "GARS",
          conversionType: "GARS"
        }, {
          name: 'GEOREF',
          pattern: /^[a-zA-Z]{4}\d{1,8}$/,
          notationType: "GEOREF",
          conversionType: "GEOREF"
        }, {
          name: 'MGRS',
          pattern: /^\d{1,2}[-,;:\s]*[C-HJ-NP-X][-,;:\s]*[A-HJ-NP-Z]{2}[-,;:\s]*(\d[-,;:\s]+\d|\d{2}[-,;:\s]+\d{2}|\d{3}[-,;:\s]+\d{3}|\d{4}[-,;:\s]+\d{4}|\d{5}[-,;:\s]+\d{5})$|^(\d{1,2}[-,;:\s]*[C-HJ-NP-X][-,;:\s]*[A-HJ-NP-Z]{2}[-,;:\s]*)(\d{2}|\d{4}|\d{6}|\d{8}|\d{10})?$|^[ABYZ][-,;:\s]*[A-HJ-NP-Z]{2}[-,;:\s]*(\d[-,;:\s]+\d|\d{2}[-,;:\s]+\d{2}|\d{3}[-,;:\s]+\d{3}|\d{4}[-,;:\s]+\d{4}|\d{5}[-,;:\s]+\d{5})$|^[ABYZ][-,;:\s]*[A-HJ-NP-Z]{2}[-,;:\s]*(\d{2}|\d{4}|\d{6}|\d{8}|\d{10})?$/,
          notationType: "MGRS",
          conversionType: "MGRS"
        },
        //not sure if USNG is needed as its exactly the same as MGRS
        /*{
          name: 'USNG',
          pattern: /^\d{1,2}[-,;:\s]*[C-HJ-NP-X][-,;:\s]*[A-HJ-NP-Z]{2}[-,;:\s]*(\d[-,;:\s]+\d|\d{2}[-,;:\s]+\d{2}|\d{3}[-,;:\s]+\d{3}|\d{4}[-,;:\s]+\d{4}|\d{5}[-,;:\s]+\d{5})$|^(\d{1,2}[-,;:\s]*[C-HJ-NP-X][-,;:\s]*[A-HJ-NP-Z]{2}[-,;:\s]*)(\d{2}|\d{4}|\d{6}|\d{8}|\d{10})?$|^[ABYZ][-,;:\s]*[A-HJ-NP-Z]{2}[-,;:\s]*(\d[-,;:\s]+\d|\d{2}[-,;:\s]+\d{2}|\d{3}[-,;:\s]+\d{3}|\d{4}[-,;:\s]+\d{4}|\d{5}[-,;:\s]+\d{5})$|^[ABYZ][-,;:\s]*[A-HJ-NP-Z]{2}[-,;:\s]*(\d{2}|\d{4}|\d{6}|\d{8}|\d{10})?$/,
          notationType: "USNG",
          conversionType: "USNG"
        },*/ 
        {
          name: 'UTM',
          pattern: /^\d{1,2}[-,;:\s]*[c-hj-np-xC-HJ-NP-X][-,;:\s]*\d{1,6}\.?\d*[mM]?[-,;:\s]?\d{1,7}\.?\d*[mM]?$/,
          notationType: "UTM - Band Letter",
          conversionType: "UTM"
        }, {
          name: 'UTM (H)',
          pattern: /^\d{1,2}[-,;:\s]*[NnSs][-,;:\s]*\d{1,6}\.?\d*[mM]?[-,;:\s]+\d{1,7}\.?\d*[mM]?$/,
          notationType: "UTM - Hemisphere (N/S)",
          conversionType: "UTM (H)"
        }
      ];
      
      return strs;
    },

    getCoordinateType: function (fromInput) {
      var clnInput = this.getCleanInput(fromInput);
      var deferred = new Deferred();
      //regexr.com
      
      var strs = this.getNotations();

      var matchedtype = dojoArray.filter(strs, function (itm) {
        return itm.pattern.test(this.v)               
      }, {
        v:clnInput
      });
      
      if (matchedtype.length > 0) {
        deferred.resolve(matchedtype);                
      } else {
        deferred.resolve(null);
      }
      return deferred.promise;          
    },

    /**
     *
     **/
    getFormattedDDStr: function (fromValue, withFormatStr, addSignPrefix) {
      var r = {};
      r.sourceValue = fromValue;
      r.sourceFormatString = withFormatStr;

      var parts = fromValue[0].split(/[ ,]+/);

      r.latdeg = parts[0].replace(/[nNsS]/, '');
      r.londeg = parts[1].replace(/[eEwW]/, '');
      
      if (addSignPrefix) {
        parts[0].slice(-1) === 'N'?r.latdeg = '+' + r.latdeg:r.latdeg = '-' + r.latdeg;         
        parts[1].slice(-1) === "W"?r.londeg = '-' + r.londeg:r.londeg = '+' + r.londeg;
      }

      var s = withFormatStr.replace(/X/, r.londeg);
      s = s.replace(/[eEwW]/, parts[1].slice(-1));
      s = s.replace(/[nNsS]/, parts[0].slice(-1));
      s = s.replace(/Y/, r.latdeg);

      r.formatResult = s;
      return r;
    },

    /**
     *
     **/
    getFormattedDDMStr: function (fromValue, withFormatStr, addSignPrefix) {
      var r = {};
      r.sourceValue = fromValue;
      r.sourceFormatString = withFormatStr;

      r.parts = fromValue[0].split(/[ ,]+/);

      r.latdeg = r.parts[0];            
      r.latmin = r.parts[1].replace(/[nNsS]/, '');
      r.londeg = r.parts[2];
      r.lonmin = r.parts[3].replace(/[eEwW]/, '');
                  
      if (addSignPrefix) {
        r.parts[1].slice(-1) === 'N'?r.latdeg = '+' + r.latdeg:r.latdeg = '-' + r.latdeg;
        r.parts[3].slice(-1) === 'W'?r.londeg = '-' + r.londeg:r.londeg = '+' + r.londeg;
      }

      //A° B'N X° Y'E
      var s = withFormatStr.replace(/[EeWw]/, r.parts[3].slice(-1));
      s = s.replace(/Y/, r.lonmin);
      s = s.replace(/X/, r.londeg);            
      s = s.replace(/[NnSs]/, r.parts[1].slice(-1));
      s = s.replace(/B/, r.latmin);
      s = s.replace(/A/, r.latdeg);

      r.formatResult = s;
      return r;
    },

    /**
     *
     **/
    getFormattedDMSStr: function (fromValue, withFormatStr, addSignPrefix) {
      var r = {};
      r.sourceValue = fromValue;
      r.sourceFormatString = withFormatStr;

      r.parts = fromValue[0].split(/[ ,]+/);

      r.latdeg =  r.parts[0];
      r.latmin =  r.parts[1];
      r.latsec =  r.parts[2].replace(/[NnSs]/, '');

      
      r.londeg = r.parts[3];
      r.lonmin = r.parts[4];
      r.lonsec = r.parts[5].replace(/[EWew]/, ''); 
      
      if (addSignPrefix) {
        r.parts[2].slice(-1) === 'N'?r.latdeg = '+' + r.latdeg:r.latdeg = '-' + r.latdeg;
        r.parts[5].slice(-1) ==='W'?r.londeg = '-' + r.londeg:r.londeg = '+' + r.londeg;            
      }
     
      //A° B' C''N X° Y' Z''E
      var s = withFormatStr.replace(/A/, r.latdeg);
      s = s.replace(/B/, r.latmin);
      s = s.replace(/C/, r.latsec);
      s = s.replace(/X/, r.londeg);
      s = s.replace(/Y/, r.lonmin);
      s = s.replace(/Z/, r.lonsec);
      s = s.replace(/[NnSs]/, r.parts[2].slice(-1));
      s = s.replace(/[EeWw]/, r.parts[5].slice(-1));

      r.formatResult = s;
      return r;
    },        

    /**
     *
     **/
    getFormattedUSNGStr: function (fromValue, withFormatStr, addSignPrefix) {
      var r = {};
      r.sourceValue = fromValue;
      r.sourceFormatString = withFormatStr;
      
      if(fromValue[0].match(/^[ABYZ]/)) {
        r.gzd = fromValue[0].match(/[ABYZ]/)[0].trim();            
      } else {
        r.gzd = fromValue[0].match(/\d{1,2}[C-HJ-NP-X]/)[0].trim(); 
      }
      r.grdsq = fromValue[0].replace(r.gzd, '').match(/[a-hJ-zA-HJ-Z]{2}/)[0].trim();
      r.easting = fromValue[0].replace(r.gzd + r.grdsq, '').match(/^\d{1,5}/)[0].trim();
      r.northing = fromValue[0].replace(r.gzd + r.grdsq, '').match(/\d{1,5}$/)[0].trim();

      //Z S X# Y#
      var s = withFormatStr.replace(/Y/, r.northing);
      s = s.replace(/X/, r.easting);
      s = s.replace(/S/, r.grdsq);
      s = s.replace(/Z/, r.gzd);          
      
      r.formatResult = s;
      return r;
    },

    /**
     *
     **/
    getFormattedMGRSStr: function (fromValue, withFormatStr, addSignPrefix) {
      var r = {};
      r.sourceValue = fromValue;
      r.sourceFormatString = withFormatStr;

      if(fromValue[0].match(/^[ABYZ]/)) {
        r.gzd = fromValue[0].match(/[ABYZ]/)[0].trim();            
      } else {
        r.gzd = fromValue[0].match(/\d{1,2}[C-HJ-NP-X]/)[0].trim(); 
      }
      r.grdsq = fromValue[0].replace(r.gzd, '').match(/[a-hJ-zA-HJ-Z]{2}/)[0].trim();
      r.easting = fromValue[0].replace(r.gzd + r.grdsq, '').match(/^\d{1,5}/)[0].trim();
      r.northing = fromValue[0].replace(r.gzd + r.grdsq, '').match(/\d{1,5}$/)[0].trim();

      //Z S X# Y#
      var s = withFormatStr.replace(/Y/, r.northing);
      s = s.replace(/X/, r.easting);
      s = s.replace(/S/, r.grdsq);
      s = s.replace(/Z/, r.gzd);      
      
      r.formatResult = s;
      return r;
    },

    /**
     *
     **/
    getFormattedGARSStr: function (fromValue, withFormatStr, addSignPrefix) {
      var r = {};
      r.sourceValue = fromValue;
      r.sourceFormatString = withFormatStr;

      r.lon = fromValue[0].match(/\d{3}/);
      r.lat = fromValue[0].match(/[a-zA-Z]{2}/);

      var q = fromValue[0].match(/\d*$/);
      r.quadrant = q[0][0];
      r.key = q[0][1];

      //XYQK
      var s = withFormatStr.replace(/K/, r.key);
      s = s.replace(/Q/, r.quadrant);
      s = s.replace(/Y/, r.lat);
      s = s.replace(/X/, r.lon);

      r.formatResult = s;
      return r;
    },
        
    /**
     *
     **/
    getFormattedGEOREFStr: function (fromValue, withFormatStr, addSignPrefix) {
      var r = {};
      r.sourceValue = fromValue;
      r.sourceFormatString = withFormatStr;

      r.lon = fromValue[0].match(/[a-zA-Z]{1}/)[0].trim();
      r.lat = fromValue[0].replace(r.lon, '').match(/[a-zA-Z]{1}/)[0].trim();
      r.quadrant15lon = fromValue[0].replace(r.lon + r.lat, '').match(/[a-zA-Z]{1}/)[0].trim();
      r.quadrant15lat = fromValue[0].replace(r.lon + r.lat + r.quadrant15lon, '').match(/[a-zA-Z]{1}/)[0].trim();
      
      var q = fromValue[0].replace(r.lon + r.lat + r.quadrant15lon + r.quadrant15lat, '');
      
      r.quadrant1lon = q.substr(0,q.length/2);
      r.quadrant1lat = q.substr(q.length/2, q.length);
     
      //ABCDXY
      var s = withFormatStr.replace(/Y/, r.quadrant1lat);
      s = s.replace(/X/, r.quadrant1lon);
      s = s.replace(/D/, r.quadrant15lat);
      s = s.replace(/C/, r.quadrant15lon);
      s = s.replace(/B/, r.lat);
      s = s.replace(/A/, r.lon);
      
      r.formatResult = s;
      return r;
    },

    /**
     *
     **/
    getFormattedUTMStr: function (fromValue, withFormatStr, addSignPrefix, addDirSuffix) {
      var r = {};
      r.sourceValue = fromValue;
      r.sourceFormatString = withFormatStr;

      r.parts = fromValue[0].split(/[ ,]+/);
      r.zone = r.parts[0].replace(/[A-Z]/,'');
      r.bandLetter = r.parts[0].slice(-1);
      r.easting = r.parts[1];
      r.westing = r.parts[2];

      //ZB Xm Ym'
      var s = withFormatStr.replace(/Y/, r.westing);
      s = s.replace(/X/, r.easting);
      s = s.replace (/B/, r.bandLetter);
      s = s.replace(/Z/, r.zone);
      
      r.formatResult = s;
      return r;
    },
        
    /**
     *
     **/
    getFormattedUTMHStr: function (fromValue, withFormatStr, addSignPrefix, addDirSuffix) {
      var r = {};
      r.sourceValue = fromValue;
      r.sourceFormatString = withFormatStr;

      r.parts = fromValue[0].split(/[ ,]+/);
      r.zone = r.parts[0].replace(/[A-Z]/,'');
      r.hemisphere = r.parts[0].slice(-1);
      
      r.easting = r.parts[1];
      r.westing = r.parts[2];

      //ZH Xm Ym'
      var s = withFormatStr.replace(/Y/, r.westing);
      s = s.replace(/X/, r.easting);
      s = s.replace (/H/, r.hemisphere);
      s = s.replace(/Z/, r.zone);

      r.formatResult = s;
      return r;
    },
  

    /**
     *
     **/
    convertMetersToUnits: function (inMeters, fromUnit) {
      var convLength = 0;
      switch (fromUnit.toLowerCase()) {
        case 'meters':
          convLength = inMeters;
          break;
        case 'feet':
          convLength = inMeters * 3.28084;
          break;
        case 'kilometers':
          convLength = inMeters * 0.001;
          break;
        case 'miles':
          convLength = inMeters * 0.000621371;
          break;
        case 'nautical-miles':
          convLength = inMeters * 0.000539957;
          break;
        case 'yards':
          convLength = inMeters * 1.09361;
          break;
      }
      return convLength;
    },

    /**
     *
     **/
    convertToMeters: function (length, inputUnit) {
      var convertedLength = length;
      switch (inputUnit) {
        case 'meters':
          convertedLength = length;
          break;
        case 'feet':
          convertedLength = length * 0.3048;
          break;
        case 'kilometers':
          convertedLength = length * 1000;
          break;
        case 'miles':
          convertedLength = length * 1609.34;
          break;
        case 'nautical-miles':
          convertedLength = length * 1852.001376036;
          break;
        case 'yards':
          convertedLength = length * 0.9144;
          break;
      }
      return convertedLength;
    }
  });
});
