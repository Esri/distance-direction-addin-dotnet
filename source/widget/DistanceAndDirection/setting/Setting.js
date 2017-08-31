///////////////////////////////////////////////////////////////////////////
// Copyright © 2014 Esri. All Rights Reserved.
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

define([
  'dojo/_base/declare',
  'dojo/_base/html',  
  'dojo/_base/lang',
  'dojo/_base/array',  
  'dojo/_base/Color',
  'dojo/dom-geometry',
  'dojo/query',
  'dojo/dom',
  
  'jimu/BaseWidgetSetting',
  'jimu/dijit/ColorPicker',
  'jimu/dijit/SimpleTable',
  
  'dijit/form/NumberSpinner',
  'dijit/registry',
  'jimu/dijit/Message',
  'dijit/_WidgetsInTemplateMixin',
  'dijit/ColorPalette'
   
],
  function(
    dojoDeclare,
    dojoHTML,    
    dojoLang,
    dojoArray,    
    dojoColor,
    dojoDomGeometry,
    dojoQuery,
    dojoDom,
    jimuBaseWidgetSetting,
    jimuColorPicker,
    jimuTable,
    dijitNumberSpinner,
    dijitRegistry,
    dijitMessage,
    dijitWidgetsInTemplateMixin
    ) {

    return dojoDeclare([jimuBaseWidgetSetting, dijitWidgetsInTemplateMixin], {
      baseClass: 'distance-and-direction-setting',

      startup: function () {
      this.inherited(arguments);
      var feedbackTableFields = [{
          name: 'showTab',
          title: 'Show Tab',
          width: '16%',
          type: 'checkbox',
          onChange: dojoLang.hitch(this, this._checkBoxChange),
          'class': 'show'
        }, {
          name: 'index',
          title: 'index',
          type: 'text',
          hidden: true
        }, {
          name: 'feedbackShape',
          title: 'Feedback Shape',
          width: '28%',
          type: 'text'
        }, {
          name: 'lineColor',
          title: 'Line Color',
          create: dojoLang.hitch(this, this._createColorPicker),
          setValue: dojoLang.hitch(this, this._setColorPicker),
          getValue: dojoLang.hitch(this, this._getColorPicker),
          width: '28%',
          type: 'extension'
        }, {
          name: 'lineWidth',
          title: 'Line Width',
          create: dojoLang.hitch(this, this._createNumberSpinner),
          setValue: dojoLang.hitch(this, this._setNumberSpinnerValue),
          getValue: dojoLang.hitch(this, this._getNumberSpinnerValue),
          type: 'extension',
          width: '28%'
        }
      ];

      var feedbackArgs = {
        fields: feedbackTableFields,
        selectable: true,
        autoHeight: false
      };
      
      this.displayFeedbackTable = new jimuTable(feedbackArgs);
      this.displayFeedbackTable.placeAt(this.feedbackTable);
      dojoHTML.setStyle(this.displayFeedbackTable.domNode, {
        'height': '100%'
      });
        
      var labelTableFields = [{
          name: 'index',
          title: 'index',
          type: 'text',
          hidden: true
        }, {
          name: 'feedbackLabel',
          title: 'Feedback Label',
          width: '44%',
          type: 'text'
        }, {
          name: 'textColor',
          title: 'Text Color',
          create: dojoLang.hitch(this, this._createColorPicker),
          setValue: dojoLang.hitch(this, this._setColorPicker),
          getValue: dojoLang.hitch(this, this._getColorPicker),
          width: '28%',
          type: 'extension'
        }, {
          name: 'textSize',
          title: 'Text Size',
          create: dojoLang.hitch(this, this._createTextNumberSpinner),
          setValue: dojoLang.hitch(this, this._setNumberSpinnerValue),
          getValue: dojoLang.hitch(this, this._getNumberSpinnerValue),
          type: 'extension',
          width: '28%'
        }
      ];
      
      var labelArgs = {
        fields: labelTableFields,
        selectable: true,
        autoHeight: false
      };
      
      this.displayLabelTable = new jimuTable(labelArgs);
      this.displayLabelTable.placeAt(this.labelTable);
      dojoHTML.setStyle(this.displayLabelTable.domNode, {
        'height': '100%'
      });
      
        
      this.setConfig(this.config);
    },
    
    
      postCreate: function(){
          
        },
      
          
      _createColorPicker: function(td) {
        var colorPicker = new jimuColorPicker();
        colorPicker.placeAt(td);      
      },
      
      _getColorPicker: function(td) {
        return dijitRegistry.byId(td.childNodes[0].id).getColor();      
      },
      
      _setColorPicker: function(td, color) {
        dijitRegistry.byId(td.childNodes[0].id).setColor(new dojoColor(color));      
      },
      
      _createNumberSpinner: function(td) {
        var numberSpinner = new dijitNumberSpinner({
          value: 2,
          smallDelta: 1,
          constraints: { min:1, max:10, places:0 },
          style: "width:100px"
        });  
        numberSpinner.placeAt(td);
      },
      
      _createTextNumberSpinner: function(td) {
        var numberSpinner = new dijitNumberSpinner({
          value: 12,
          smallDelta: 1,
          constraints: { min:1, max:36, places:0 },
          style: "width:100px"
        });  
        numberSpinner.placeAt(td);
      },
      
      _getNumberSpinnerValue: function(td) {
        return dojoQuery('.dijitInputInner', td)[0].value;        
      },
      
      _setNumberSpinnerValue: function(td, value) {
        dojoQuery('.dijitInputInner', td)[0].value = value;
      },

      setTextSymbol: function () {
        console.log('Set Text Symbol');
      },

      setConfig: function(config){
        
        var feedbacks = [
             {shape: 'Line'},
             {shape: 'Circle'},
             {shape: 'Ellipse'},
             {shape: 'Rings'}
           ];
           
        var configSettings = [{
            showTab: config.feedback.lineSymbol.showTab,
            color: config.feedback.lineSymbol.color,
            width: config.feedback.lineSymbol.width
          }, {
            showTab: config.feedback.circleSymbol.showTab,
            color: config.feedback.circleSymbol.outline.color,
            width: config.feedback.circleSymbol.outline.width
          }, {
            showTab: config.feedback.ellipseSymbol.showTab,
            color: config.feedback.ellipseSymbol.outline.color,
            width: config.feedback.ellipseSymbol.outline.width
          }, {
            showTab: config.feedback.rangeRingSymbol.showTab,
            color: config.feedback.rangeRingSymbol.color,
            width: config.feedback.rangeRingSymbol.width
          }, {
            color: config.feedback.labelSymbol.color,
            width: config.feedback.labelSymbol.font.size
          }
        ];
        
        this._setFeedbackTable(feedbacks, configSettings);
        
        this.displayLabelTable.clear();        
        
        this.displayLabelTable.addRow({
          feedbackLabel: 'Feedback Label',
          index: "0",
          textColor: configSettings[4].color,
          textSize: configSettings[4].width
        });
        
      },
      
      /*
      **
      */
      _setFeedbackTable:function(feedbacks, configSettings){
        this.displayFeedbackTable.clear();
        for (var i = 0; i < feedbacks.length; i++) {
          var rowData = {
            feedbackShape: feedbacks[i].shape,
            index: "" + i,
            showTab: configSettings[i].showTab,
            lineColor: configSettings[i].color,
            lineWidth: configSettings[i].width
          };
          this.displayFeedbackTable.addRow(rowData);
        };
      },

      getConfig: function(){
        
        var feedbackData = this.displayFeedbackTable.getData();
        var labelData = this.displayLabelTable.getData();
        
        var allTabsFalse = 0;
        
        dojoArray.forEach(feedbackData, function(tData) {
          if (tData.showTab) {
            allTabsFalse = allTabsFalse + 1;
          }
        });
        
        
        if(allTabsFalse != 0){
          this.config.feedback = {
            lineSymbol: {
              showTab: feedbackData[0].showTab,
              type: 'esriSLS',
              style: 'esriSLSSolid',
              color: feedbackData[0].lineColor,
              width: feedbackData[0].lineWidth
            },
            circleSymbol: {
              showTab: feedbackData[1].showTab,
              type: 'esriSFS',
              style: 'esriSFSNull',
              color: [255,0,0,0],
              outline: {
                color: feedbackData[1].lineColor,
                width: feedbackData[1].lineWidth,
                type: 'esriSLS',
                style: 'esriSLSSolid'
              }
            },
            ellipseSymbol: {
              showTab: feedbackData[2].showTab,
              type: 'esriSFS',
              style: 'esriSFSNull',
              color: [255,0,0,0],
              outline: {
                color: feedbackData[2].lineColor,
                width: feedbackData[2].lineWidth,
                type: 'esriSLS',
                style: 'esriSLSSolid'
              }
            },
            rangeRingSymbol: {
              showTab: feedbackData[3].showTab,
              type: 'esriSLS',
              style: 'esriSLSSolid',
              color: feedbackData[3].lineColor,
              width: feedbackData[3].lineWidth
            },
            labelSymbol: {
              'type' : 'esriTS',
              'color' : labelData[0].textColor,
              'verticalAlignment' : 'middle',
              'horizontalAlignment' : 'center',
              'xoffset' : 0,
              'yoffset' : 0,
              'kerning' : true,
              'font' : {
                'family' : 'arial',
                'size' : labelData[0].textSize,
                'style' : 'normal',
                'weight' : 'normal',
                'decoration' : 'none'
              }
            }
          };
          return this.config;
        } else {
          new dijitMessage({
            message: 'The widget must be configured with at least one tab shown'
          });
          return false;
        }
      }
    });
  });
