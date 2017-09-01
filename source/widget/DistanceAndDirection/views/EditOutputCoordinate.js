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
    'dojo/dom-style',
    'dojo/on',
    'dojo/topic',
    'dojo/dom-attr',
    'dijit/_WidgetBase',
    'dijit/_TemplatedMixin',
    'dijit/_WidgetsInTemplateMixin',
    'dojo/text!../templates/EditOutputCoordinate.html',
    'dijit/form/Select',
    'jimu/dijit/CheckBox'
], function (
    dojoDeclare,
    dojoLang,
    dojoDomStyle,
    dojoOn,
    dojoTopic,
    dojoDomAttr,
    dijitWidgetBase,
    dijitTemplatedMixin,
    dijitWidgetsInTemplate,
    edittemplate
) {
    'use strict';
    return dojoDeclare([dijitWidgetBase, dijitTemplatedMixin, dijitWidgetsInTemplate], {
        templateString: edittemplate,

        formats: {

        },

        ct: 'DD',
        _setCtAttr: function (v) {
            this.frmtSelect.set('value', v);
        },

        /**
         *
         **/
        postCreate: function () {
          this.formats = {
            DD: {
                defaultFormat: 'YN XE',
                customFormat: null,
                useCustom: false
            },
            DDM: {
                defaultFormat: 'A° B\'N X° Y\'E',
                customFormat: null,
                useCustom: false
            },
            DMS: {
                defaultFormat: 'A° B\' C\"N X° Y\' Z\"E',
                customFormat: null,
                useCustom: false
            },
            GARS: {
                defaultFormat: 'XYQK',
                customFormat: null,
                useCustom: false
            },
            GEOREF: {
                defaultFormat: 'ABCDXY',
                customFormat: null,
                useCustom: false
            },
            MGRS: {
                defaultFormat: 'ZSXY',
                customFormat: null,
                useCustom: false
            },
            USNG: {
                defaultFormat: 'ZSXY',
                customFormat: null,
                useCustom: false
            },
            UTM: {
                defaultFormat: 'ZB X Y',
                customFormat: null,
                useCustom: false
            },
            'UTM (H)': {
                defaultFormat: 'ZH X Y',
                customFormat: null,
                useCustom: false
            }
          };

          dojoDomAttr.set(this.frmtVal, 'value', this.formats[this.ct].defaultFormat);

          this.own(
            this.frmtSelect.on('change', dojoLang.hitch(
            this,
            this.frmtSelectValueDidChange)
          ));

          this.own(dojoOn(
            this.frmtVal,
            'change',
            dojoLang.hitch(this, this.formatValDidChange)
          ));
          
          this.displayPrefixContainer();
        },

        /**
         *
         *
        startup: function () {
            //this.inherited(arguments);
        },*/
        formatValDidChange: function () {
            var newvalue = dojoDomAttr.get(this.frmtVal, 'value');
            var crdType = this.frmtSelect.get('value');
            this.formats[crdType].customFormat = newvalue;
            this.formats[crdType].useCustom = true;
            this.currentformat = newvalue;
        },

        /**
         *
         **/
        frmtSelectValueDidChange: function () {
            var curval = this.frmtSelect.get('value');
            var selval = this.formats[curval].useCustom ? this.formats[curval].customFormat
              : this.formats[curval].defaultFormat;
            this.ct = curval;
            dojoDomAttr.set(this.frmtVal, 'value', selval);
            this.displayPrefixContainer();
        },
        
        /**
         *
         **/
        displayPrefixContainer: function () {
          switch(this.frmtSelect.get('value')){
            case 'DD':
            case 'DDM':
            case 'DMS':
              dojoDomStyle.set(this.prefixContainer, {display: ""});
              break;
            default:
              dojoDomStyle.set(this.prefixContainer, {display: "none"});
              break;
          }
        }

    });
});
