module.exports('onboarding.index', function (require) {
    var company = require('onboarding.company');
    var product = require('onboarding.product');
    var ownerInfo = require('onboarding.owner-info.index');
    var consent = require('onboarding.consent.index');
    var validateAndSubmit = require('onboarding.form.handlers').validateAndSubmit;
    var submitDraft = require('onboarding.form.handlers').submitDraft;

    function init() {
        company.initCompany();
        product.initProducts();
        ownerInfo.init();
        consent.init();

        $('#submit').on('click', validateAndSubmit);
        $('.save-and-resume').on('click', submitDraft);
    }

    window.init = init;

    return {
        init: init
    }
})