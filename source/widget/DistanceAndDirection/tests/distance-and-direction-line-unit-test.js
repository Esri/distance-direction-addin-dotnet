define([
  'intern!object',
  'intern/chai!assert',
  'dojo/dom-construct',
  'dojo/_base/window',
  'esri/map',
  'DD/models/LineFeedback',
  'dijit/_WidgetBase',
  'dijit/_TemplatedMixin',
  'dijit/_WidgetsInTemplateMixin',
  'dojo/_base/declare',
  'dojo/_base/lang',
  'dojo/on',
  'dojo/topic',
  'dojo/dom-attr',
  'dojo/dom-class',
  'dojo/dom-style',
  'dojo/string',
  'dojo/number',
  'dijit/form/Select'    
], function(registerSuite, assert, domConstruct, win, Map, LineFeedback) {
    // local vars scoped to this module
    var map, lineTab, mapPointButton, feedBack, lineSymbol;

    registerSuite({
        name: 'Distance-Direction-Line-Widget',
        // before the suite starts
        setup: function() {
            // load claro and esri css, create a map div in the body, and create the map object and print widget for our tests
            domConstruct.place('<link rel="stylesheet" type="text/css" href="//js.arcgis.com/3.19/esri/css/esri.css">', win.doc.getElementsByTagName("head")[0], 'last');
            domConstruct.place('<link rel="stylesheet" type="text/css" href="//js.arcgis.com/3.19/dijit/themes/claro/claro.css">', win.doc.getElementsByTagName("head")[0], 'last');
            domConstruct.place('<script src="http://js.arcgis.com/3.19/"></script>', win.doc.getElementsByTagName("head")[0], 'last');
            domConstruct.place('<div id="map" style="width:800px;height:600px;" class="claro"></div>', win.body(), 'only');
            domConstruct.place('<div id="lineNode" style="width:300px;" class="claro"></div>', win.body(), 'last');
            domConstruct.place('<div id="buttonNode" style="width:300px;""></div>', win.body(), 'last');

            map = new Map("map", {
                basemap: "topo",
                center: [-122.45, 37.75],
                zoom: 13,
                sliderStyle: "small"
            });
        },

        // before each test executes
        beforeEach: function() {
            // do nothing
        },

        // after the suite is done (all tests)
        teardown: function() {
            if (map.loaded) {
                map.destroy();                    
            }            
            if (lineTab) {
                lineTab.destroy();
            }
        },

        'Test LineFeedback.ctor()': function() {
            console.log('Start CTOR test');

            lineTab = new LineFeedback({
                map: map,
                lineSymbol: {
                    type: 'esriSLS',
                    style: 'esriSLSSolid',
                    color: [255, 50, 50, 255],
                    width: 1.25
            }}, domConstruct.create("div")).placeAt("lineNode"); 
            lineTab.startup();

            assert.ok(lineTab);
            assert.instanceOf(lineTab, TabLine, 'lineTab should be an instance of LineFeedback');

            console.log('End CTOR test');
        },

        'Test Line Creation': function () {
            console.log('Line creation test');

            //Create start point LAT/LONG
            var startPt = new Point({
                x: -122.65,
                y: 45.53,
                spatialReference: {
                  wkid: 4326
                }
            }); 

            //Create end point LAT/LONG
            var endPt = new Point({
                x: -120.65,
                y: 45.53,
                spatialReference: {
                  wkid: 4326
                }
            });     

            feedBack.startPoint = startPt;
            feedBack.endPoint = endPt;

            //Center map on start point
            map.centerAt(startPt);

            //Get screen points from start and end LAT/LONG points
            var screenStartPt = map.toScreen(startPt);
            var screenEndPt = map.toScreen(endPt);

            /*
                Create a line using these steps

                1. Get the HTML page
                2. Wait 5 seconds for the HTML body to load
                3. Get the lineNode element - button
                4. Click on the button
                5. Move the mouse over map to start point in screen units
                6. Press the mouse button
                7. Drag to end point in screen units
                8. Release button
            */
            return this.remote
                .waitForElementByCssSelector('body.loaded', 5000)
                .elementById('lineNode')
                .clickElement()
                .moveMouseTo(map.domNode, screenStartPt.x, screenStartPt.y).sleep(500)
                .pressMouseButton(0).sleep(500)
                .moveMouseTo(map.domNode, screenEndPt.x, screenEndPt.y).sleep(500)
                .releaseMouseButton(0)
                .end();         
        }        
    });
});