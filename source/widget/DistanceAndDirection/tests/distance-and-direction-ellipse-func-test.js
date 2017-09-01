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
    'create ellipse manually': function() {
      return this.remote
        .get('http://mawidgets/ma2/')
        .sleep(5000)
        .findByXpath('id("dijit__WidgetBase_2")/IMG')
					.moveMouseTo(13, 7)
					.clickMouseButton(0)
					.end()
        .sleep(1000)
				.findByXpath('id("jimu_dijit_TabContainer3_0")/DIV[1]/TABLE/TBODY/TR/TD[3]/DIV')
					.moveMouseTo(20, 20)
					.clickMouseButton(0)
					.end()
        .sleep(1000)
				.findByXpath('id("dijit__WidgetsInTemplateMixin_3")/DIV/DIV[2]/DIV[1]/INPUT[1]')
					.moveMouseTo(145.5, 19)
					.clickMouseButton(0)
        .sleep(1000)
				.pressKeys('32,-90')
					.end()
				.findByXpath('id("dijit__WidgetsInTemplateMixin_3")/DIV/FIELDSET[1]/DIV[1]/DIV[1]/INPUT')
					.moveMouseTo(90.5, 19)
					.clickMouseButton(0)
        .sleep(1000)
				.pressKeys('1500')
					.end()
        .sleep(1000)
				.findByXpath('id("dijit__WidgetsInTemplateMixin_3")/DIV/FIELDSET[1]/DIV[2]/DIV[1]/INPUT')
					.moveMouseTo(39.5, 11)
					.clickMouseButton(0)
        .sleep(1000)
				.pressKeys('500')
					.end()
        .sleep(1000)
				.findByXpath('id("dijit_form_Select_10")/TBODY/TR/TD[1]')
					.moveMouseTo(86.5, 17)
					.clickMouseButton(0)
					.end()
        .sleep(1000)
				.findByXpath('id("dijit_MenuItem_7_text")')
					.moveMouseTo(62.5, 15)
					.clickMouseButton(0)
					.end()
        .sleep(1000)
				.findByXpath('id("dijit__WidgetsInTemplateMixin_3")/DIV/DIV[3]/DIV[1]/INPUT[1]')
					.moveMouseTo(46.5, 22)
					.clickMouseButton(0)
				.pressKeys('45');
      }
    });
});
