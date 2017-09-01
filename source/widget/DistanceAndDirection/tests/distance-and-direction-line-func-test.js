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
    name: 'Functional Tests for Distance Calculator',
    'create line with bearing-distance': function() {
      return this.remote
        .get('http://mawidgets/ma2/')
        .sleep(5000)
        .findByXpath('id("dijit__WidgetBase_2")/IMG') //open widget
					.moveMouseTo(18, 6)
					.clickMouseButton(0)
					.end()
        .sleep(1000)
				.findByXpath('id("dijit__WidgetsInTemplateMixin_0")/DIV/DIV[3]/DIV[1]/INPUT[1]') // add start point
					.moveMouseTo(69.5, 8)
					.clickMouseButton(0)
				.pressKeys('38.75,-90.46') // manual coord input
					.end()
        .sleep(500)
				.findByXpath('id("dijit__WidgetsInTemplateMixin_0")/DIV/DIV[5]/DIV[1]/INPUT[1]') // move to length textbox
					.moveMouseTo(66.5, 17)
					.clickMouseButton(0)
				.pressKeys('1000')
					.end()
        .sleep(500)
				.findByXpath('id("dijit__WidgetsInTemplateMixin_0")/DIV/DIV[6]/DIV[1]/INPUT[1]') // move to angle and enter number
					.moveMouseTo(57.5, 10)
					.clickMouseButton(0)
				.pressKeys('45');
      }
  });
});
