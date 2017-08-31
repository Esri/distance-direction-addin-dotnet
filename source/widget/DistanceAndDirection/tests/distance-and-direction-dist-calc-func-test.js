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
    'create circle with calculator': function() {
      return this.remote
        .get('http://mawidgets/ma2/')
        .sleep(5000)
        .findByXpath('id("dijit__WidgetBase_2")/IMG')
          .moveMouseTo(9, 18)
          .clickMouseButton(0)
          .end()
        .sleep(1000)
        .findByXpath('id("jimu_dijit_TabContainer3_0")/DIV[1]/TABLE/TBODY/TR/TD[2]/DIV')
          .moveMouseTo(8, 19)
          .clickMouseButton(0)
          .end()
        .sleep(1000)
        .findByXpath('id("dijit_TitlePane_0_titleBarNode")/DIV/SPAN[3]')
          .moveMouseTo(71, 13)
          .clickMouseButton(0)
          .end()
        .sleep(1000)
        .findByXpath('id("test_0")')
          .moveMouseTo(59, 16)
          .clickMouseButton(0)
        .pressKeys('30,-120')
          .end()
        .sleep(1000)
        .findByXpath('id("dijit_TitlePane_0_pane")/DIV[1]/LABEL/LABEL/DIV/INPUT[1]')
          .moveMouseTo(41, 12)
          .clickMouseButton(0)
        .pressKeys('30')
          .end()
        .sleep(1000)
        .findByXpath('id("dijit_TitlePane_0_pane")/DIV[2]/LABEL/LABEL/DIV/INPUT[1]')
          .moveMouseTo(41, 12)
          .clickMouseButton(0)
        .pressKeys('10');
      }
  });
});
