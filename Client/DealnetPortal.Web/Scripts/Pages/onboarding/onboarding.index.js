﻿module.exports('onboarding.index', function (require) {
    var company = require('onboarding.company');
    var product = require('onboarding.product');
    var ownerInfo = require('onboarding.owner-info.index');
    var consent = require('onboarding.consent.index');
    var validateAndSubmit = require('onboarding.form.handlers');

    function init() {
        company.initCompany();
        //product.initProducts();
        //ownerInfo.init(model.Owners);
        //consent.init();

        $('#submit').on('click', validateAndSubmit);
    }

    window.init = init;

    return {
        init: init
    }
})