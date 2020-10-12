var Backbone = require('backbone');
var Marionette = require('marionette');
var AboutView = require('./About/AboutView');
var HealthLayout = require('./Health/HealthLayout');
var MoreInfoView = require('./MoreInfo/MoreInfoView');

module.exports = Marionette.Layout.extend({
    template : 'System/Info/SystemInfoLayoutTemplate',

    regions : {
        about     : '#about',
        health    : '#health',
        moreInfo  : '#more-info'
    },

    onRender : function() {
        this.health.show(new HealthLayout());
        this.about.show(new AboutView());
        this.moreInfo.show(new MoreInfoView());
    }
});