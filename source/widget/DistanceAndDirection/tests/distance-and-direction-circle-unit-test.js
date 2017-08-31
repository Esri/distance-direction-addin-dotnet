define([
    'intern!object',
    'intern/chai!assert',
    'dojo/dom-construct',
    'dojo/_base/window',
    'esri/map',
    'DD/views/TabCircle',
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
], function(registerSuite, assert, domConstruct, win, Map, TabCircle) {
    // local vars scoped to this module
    var map, circleTab;

    registerSuite({
        name: 'Distance-Direction-Line-Widget',
        // before the suite starts
        setup: function() {
            // load claro and esri css, create a map div in the body, and create the map object and print widget for our tests
            domConstruct.place('<link rel="stylesheet" type="text/css" href="//js.arcgis.com/3.16/esri/css/esri.css">', win.doc.getElementsByTagName("head")[0], 'last');
            domConstruct.place('<link rel="stylesheet" type="text/css" href="//js.arcgis.com/3.16/dijit/themes/claro/claro.css">', win.doc.getElementsByTagName("head")[0], 'last');
            domConstruct.place('<script src="http://js.arcgis.com/3.16/"></script>', win.doc.getElementsByTagName("head")[0], 'last');
            domConstruct.place('<div id="map" style="width:300px;height:200px;" class="claro"></div>', win.body(), 'only');
            domConstruct.place('<div id="circleNode" style="width:300px;" class="claro"></div>', win.body(), 'last');

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
            if (circleTab) {
                circleTab.destroy();
            }
        },

        'Test TabCircle.ctor()': function() {
            console.log('Start CTOR test');

            circleTab = new TabCircle({
                map: map,
                appConfig: {geometryService:""}
            }, domConstruct.create("div")).placeAt("circleNode");
            circleTab.startup();

            assert.ok(circleTab);
            assert.instanceOf(circleTab, TabCircle, 'circleTab should be an instance of TabCircle');

            console.log('End CTOR test');
        },

        'Test Clear Graphics': function() {
            // let the test output console reporter know we are waiting for stuff to load
            console.log('Start clear graphic test');
            if (circleTab) {
                circleTab.clearGraphics();
            }
            console.log('End clear graphic test');
        }
    });
});
