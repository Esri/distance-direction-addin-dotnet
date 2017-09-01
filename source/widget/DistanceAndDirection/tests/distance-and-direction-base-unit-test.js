define([
  'intern!object',
  'intern/chai!assert',
  'dojo/dom-construct',
  'dojo/_base/window',
  'dojo/number',
  'esri/map',
  'esri/geometry/Extent',
  'DD/util',
  'dojo/promise/all',
  'dojo/_base/lang',
  'dojo/_base/Deferred',
  'jimu/dijit/CheckBox',
  'jimu/BaseWidget',
  'jimu/dijit/Message',
  'dijit/form/Select',
  'dijit/form/TextBox'
], function(registerSuite, assert, domConstruct, win, dojoNumber, Map, Extent, DDUtil, dojoAll, lang, Deferred) {
  // local vars scoped to this module
  var map, ddUtil;
  var dms2,dms3,ds,ds2,dp,ns,pLat,pLon,pss,ms,ss;
  var notations;
  var totalTestCount = 0;
  var latDDArray = [];
  var lonDDArray = [];
  var latDDMArray = [];
  var lonDDMArray = [];
  var latDMSArray = [];
  var lonDMSArray = [];

  registerSuite({
    name: 'Distance Direction Widget',
      // before the suite starts
    setup: function() {
      // load claro and esri css, create a map div in the body, and create map and Coordinate Conversion objects for our tests
      domConstruct.place('<link rel="stylesheet" type="text/css" href="//js.arcgis.com/3.19/esri/css/esri.css">', win.doc.getElementsByTagName("head")[0], 'last');
      domConstruct.place('<link rel="stylesheet" type="text/css" href="//js.arcgis.com/3.19/dijit/themes/claro/claro.css">', win.doc.getElementsByTagName("head")[0], 'last');
      domConstruct.place('<script src="http://js.arcgis.com/3.19/"></script>', win.doc.getElementsByTagName("head")[0], 'last');
      domConstruct.place('<div id="map" style="width:800px;height:600px;" class="claro"></div>', win.body(), 'only');
      domConstruct.place('<div id="ddNode" style="width:300px;" class="claro"></div>', win.body(), 'last');

      map = new Map("map", {
        basemap: "topo",
        center: [-122.45, 37.75],
        zoom: 13,
        sliderStyle: "small",
        extent: new Extent({xmin:-180,ymin:-90,xmax:180,ymax:90,spatialReference:{wkid:4326}})
      });
      
      ddUtil = new DDUtil("https://hgis-ags10-4-1.gigzy.local/ags/rest/services/Utilities/Geometry/GeometryServer");
      
      notations = ddUtil.getNotations();

      //populate the arrays that will be used in the tests       
      //dms2 = degrees/minutes/seconds two figures
      dms2 = ['0','00'];
      //dms3 = degrees/minutes/seconds three figures
      dms3 = ['0','00','000'];
      //ds = degree symbol      
      ds = ['','°','˚','º','^','~','*'];
      //there has to be some seperator between degrees and minute values
      ds2 = [' ','°','˚','º','^','~','*','-','_']; 
      //ms = minute symbol      
      ms = ["","'","′"];       
      //there has to be some seperator between minute and second values
      ms2 = [' ',"'","′"];      
      //ms = second symbol
      ss = ['"','¨','˝'];            
      //dp = decimal place 
      //just test a single decimal place using both comma and decimal point
      dp = ['','.0',',0'];
      //ns = number seperator
      //we know that a comma seperator used with a comma for decimal degrees will fail so do not test for this
      ns = [' ',':',';','|','/','\\'];
      //pLat = prefix / suffix latitude - test lower and upper case
      pLat = ['','n','S','+','-'];
      //pLon = prefix / suffix longitude
      pLon = ['','E','w','+','-'];
      //pss = prefix / suffix spacer
      pss = ['',' '];

       
      //set up an array of each combination of DD latitude values
      for (var a = 0; a < dms2.length; a++) {
        for (var b = 0; b < dp.length; b++) {
          for (var c = 0; c < ds.length; c++) {
            latDDArray.push(dms2[a] + dp[b] + ds[c]);            
          }
        }                   
      }
      //set up an array of each combination of DD longitude values
      for (var a = 0; a < dms3.length; a++) {
        for (var b = 0; b < dp.length; b++) {
          for (var c = 0; c < ds.length; c++) {
            lonDDArray.push(dms3[a] + dp[b] + ds[c]);            
          }
        }                   
      }
      
      //set up an array of each combination of DDM latitude values
      for (var a = 0; a < dms2.length; a++) {
        for (var b = 0; b < ds2.length; b++) {
          for (var c = 0; c < dms2.length; c++) {
            for (var d = 0; d < dp.length; d++) {
              for (var e = 0; e < ms.length; e++) {
                latDDMArray.push(dms2[a] + ds2[b] + dms2[c] + dp[d] + ms[e]);                
              }
            }                   
          }
        }
      }

      //set up an array of each combination of DDM longitude values
      for (var a = 0; a < dms3.length; a++) {
        for (var b = 0; b < ds2.length; b++) {
          for (var c = 0; c < dms2.length; c++) {
            for (var d = 0; d < dp.length; d++) {
              for (var e = 0; e < ms.length; e++) {
                lonDDMArray.push(dms3[a] + ds2[b] + dms2[c] + dp[d] + ms[e]);                
              }
            }                   
          }
        }
      }
      
      //set up an array of each combination of DMS latitude values
      for (var a = 0; a < dms2.length; a++) {
        for (var b = 0; b < ds2.length; b++) {
          for (var c = 0; c < dms2.length; c++) {
            for (var d = 0; d < ms2.length; d++) {
              for (var e = 0; e < dms2.length; e++) {
                for (var f = 0; f < dp.length; f++) {
                  for (var g = 0; g < ss.length; g++) {
                    latDMSArray.push(dms2[a] + ds2[b] + dms2[c] + ms2[d] + dms2[e] + dp[f] + ss[g]);                
                  }
                }
              }
            }                   
          }
        }
      }
      
      //set up an array of each combination of DMS longitude values
      for (var a = 0; a < dms3.length; a++) {
        for (var b = 0; b < ds2.length; b++) {
          for (var c = 0; c < dms2.length; c++) {
            for (var d = 0; d < ms2.length; d++) {
              for (var e = 0; e < dms2.length; e++) {
                for (var f = 0; f < dp.length; f++) {
                  for (var g = 0; g < ss.length; g++) {
                    lonDMSArray.push(dms3[a] + ds2[b] + dms2[c] + ms2[d] + dms2[e] + dp[f] + ss[g]);                
                  }
                }
              }
            }                   
          }
        }
      }
      
      jsonLoader = function loadTests(file, callback) {
          var rawFile = new XMLHttpRequest();
          rawFile.overrideMimeType("application/json");
          rawFile.open("GET", file, false);
          rawFile.onreadystatechange = function() {
              if (rawFile.readyState === 4 && rawFile.status == "200") {
                  callback(rawFile.responseText);
              }
          }
          rawFile.send(null);
      }
      
      roundNumber = function round(value, decimals) {
        return Number(Math.round(value+'e'+decimals)+'e-'+decimals);
      }      
    },

    // before each test executes
    beforeEach: function() {
      // do nothing
    },

    // after the suite is done (all tests)
    teardown: function() {
      // do nothing 
    },
    
    'Test Manual Input: Convert DDM to Lat/Long': function() {
      //test to ensure inputed DDM is converted correctly to Lat/Long (4 Decimal Places)
      //tests held in file: toGeoFromDDM.json
      
      //this.skip('Skip test for now')
      var count = 0;
      var DDM2geo = null;
      var dfd = this.async();
      var returnArray = [];
      
      //read in tests from the json file
      jsonLoader("../../widgets/DistanceAndDirection/tests/toGeoFromDDM.json", lang.hitch(this,function(response){
        DDM2geo = JSON.parse(response);
      }));
       
      for (var i = 0; i < DDM2geo.tests.length; i++) {
        returnArray.push(ddUtil.getXYNotation(DDM2geo.tests[i].testString,'DDM'));          
      }
      
      dojoAll(returnArray).then(dfd.callback(function (itm) {
        for (var i = 0; i < itm.length; i++) {
          assert.equal(roundNumber(itm[i][0][0],4), roundNumber(DDM2geo.tests[i].lon,4), DDM2geo.tests[i].testNumber + " Failed");
          assert.equal(roundNumber(itm[i][0][1],4), roundNumber(DDM2geo.tests[i].lat,4), DDM2geo.tests[i].testNumber + " Failed");
          count++;          
        }
        console.log("The number of manual tests conducted for Convert DDM to Lat/Long conducted was: " + count);
        totalTestCount = totalTestCount + count;
        //clean up Array
        DDM2geo = null;
      }));  
    },
    
  });
});