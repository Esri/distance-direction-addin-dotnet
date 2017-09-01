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
    'dojo/dom-attr',
    'dijit/_WidgetBase',
    'dijit/_TemplatedMixin',
    'dijit/_WidgetsInTemplateMixin',
    'dojo/text!./ConfirmNotation.html'
], function (
    dojoDeclare,
    dojoLang,
    dojoOn,
    dojoTopic,
    dojoDomAttr,
    dijitWidgetBase,
    dijitTemplatedMixin,
    dijitWidgetsInTemplate,
    ConfirmNotation
) {
    'use strict';
    return dojoDeclare([dijitWidgetBase, dijitTemplatedMixin, dijitWidgetsInTemplate], {
      templateString: ConfirmNotation,
      numberOfInputs: 0,
      selectOptions: {},
        
        constructor: function (options1) {
            this.numberOfInputs = options1.length; 
            this.selectOptions = options1;
            
        },
        
        postCreate: function () {
          this.label1.innerHTML = "There are " + this.numberOfInputs + " notations that match your input please confirm which you would like to use:";
          for (var i = 0; i < this.selectOptions.length; i++) {
              this.comboOptions.addOption({ value: this.selectOptions[i].name , label: this.selectOptions[i].notationType});
          }
       },
    });
});
