define([
  'dojo/_base/declare',
  'dojo/topic',
  'dijit/form/ValidationTextBox',
  '../models/Coordinate'
], function (
  dojoDeclare,
  dojoTopic,
  dijitValidationTextBox,
  Coord
) {
  var mo = dojoDeclare('test', dijitValidationTextBox, {
    required: true,

    inputCoordinate: null,

    invalidMessage: 'Blah Blah Blah',

    validateOnInput: true,
    _validateOnInputSetter: function (value) {
        this.validateOnInput = (value === 'true');
    },

    clear: function () {
      this.set('validateOnInput', true);
      this.set('value', '');
      this.inputCoordinate.coordinateEsriGeometry = null;
    },
    /**
     *
     **/
    constructor: function () {
        this.inherited(arguments);
        this.inputCoordinate = new Coord({appConfig: arguments[0].appConfig});
    },

    postMixinProperties: function () {
        console.log('Post Create');
    },

    /**
     *
     **/
    validator: function (value, contstraints) {

      if (!this.validateOnInput) {return true;}
      //if (this.get('value').length < 4) return false;

      this.inputCoordinate.set('inputString', value);
      
      //this.inputCoordinate.set('formatString', 'YN XE');

      this.set('invalidMessage', this.inputCoordinate.message);
      this.set('promptMessage', this.inputCoordinate.message);
      
      return true;
    }
  });

  return mo;
});
