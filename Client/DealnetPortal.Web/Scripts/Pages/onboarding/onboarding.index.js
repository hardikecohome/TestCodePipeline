module.exports('onboarding.index', function (require) {
    var company = require('onboarding.company');
    var product = require('onboarding.product');
    var ownerInfo = require('onboarding.owner-info.index');
    var consent = require('onboarding.consent.index');
    var aknowledgement = require('onboarding.ackonwledgment.index');
    var validateAndSubmit = require('onboarding.form.handlers').validateAndSubmit;
    var submitDraft = require('onboarding.form.handlers').submitDraft;
    var resetForm = require('onboarding.common').resetFormValidation;

    function init (model) {
        company.initCompany(model !== undefined ? model.CompanyInfo : {});
        product.initProducts(model !== undefined ? model.ProductInfo : {});
        aknowledgement.init(model !== undefined ? model.Owners : []);
        ownerInfo.init(model !== undefined ? model.Owners : []);
        consent.init();

        $('#submit').on('click', validateAndSubmit);
        $('.save-and-resume').on('click', submitDraft);
    }

    function initAutocomplete () {
        company.initAutocomplete();
        ownerInfo.initAutocomplete();
    }

    window.init = init;
    window.initAutocomplete = initAutocomplete;

    return {
        init: init
    }
})