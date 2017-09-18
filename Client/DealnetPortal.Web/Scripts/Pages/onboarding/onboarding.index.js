module.exports('onboarding.index', function (require) {
    var company = require('onboarding.company');
    var product = require('onboarding.product');
    var ownerInfo = require('onboarding.owner-info.index');
    var consent = require('onboarding.consent.index');
    var aknowledgement = require('onboarding.ackonwledgment.index');
    var validateAndSubmit = require('onboarding.form.handlers');
    var documents = require('onboarding.documents.index');
    var submitDraft = require('onboarding.form.handlers').submitDraft;
    var resetForm = require('onboarding.common').resetFormValidation;
    var detectIe = require('onboarding.common').detectIe;
    var definePolyfill = require('onboarding.polyfill');
    var initTooltip = require('onboarding.common').initTooltip;

    function init (model) {
        initializing = true;
        company.initCompany(model !== undefined ? model.CompanyInfo : {});
        aknowledgement.init(model !== undefined ? model.Owners : []);
        ownerInfo.init(model !== undefined ? model.Owners : []);
        product.initProducts(model !== undefined ? model.ProductInfo : {});
        documents.init(model.DictionariesData.LicenseDocuments, model.AdditionalDocuments, model.RequiredDocuments);
        consent.init(model.AllowCommunicate, model.AllowCreditCheck);

        $('#submit').on('click', validateAndSubmit);
        $('.save-and-resume').on('click', submitDraft);

        if (detectIe()) {
            definePolyfill();
        }

        initializing = false;
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