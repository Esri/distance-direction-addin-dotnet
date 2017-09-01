define([
  'intern!object',
  'intern/chai!assert',
  'require'
], function (
  registerSuite,
  assert,
  require
) {
  registerSuite({
    name: 'index',
    'create line': function() {
      return this.remote
        .get('http://mawidgets/ma2/')
        .sleep(5000)
        .findById('dijit__WidgetBase_2')
          .click()
          .end()
        .sleep(5000)
        .findAllByClassName('tab-item-div')
        .then(function(t) {
          return t[1].click();
        })
        .end()
        .sleep(1000)
        .findAllByClassName('addPointBtn')
        .then(function (b) {
          console.log('point btn length' + b.length);
          b[1].click();
        })
        .end()
        .sleep(1000)
        .findById('map')
          .moveMouseTo(674, 498)
          .sleep(1000)
          .clickMouseButton(0)
          .sleep(1000)
          .moveMouseTo(675, 500)
          .sleep(1000)
          .clickMouseButton(0)
          .sleep(1000)
          .end()
        .findAllByCssSelector('input[data-dojo-attach-point="textbox,focusNode"]')
        .then(function(t) {
          return t[0].click();
        })
        .getProperty('value')
        .then(function (text){
          assert(text[0] !== null,"Start Point Should be a value");
        });
      }
    });
});
