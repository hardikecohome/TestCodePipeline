﻿module.exports('onboarding.index', function (require) {
    var company = require('onboarding.company');
    var product = require('onboarding.product');
    var ownerInfo = require('onboarding.owner-info.index');
    var consent = require('onboarding.consent.index');
    var aknowledgement = require('onboarding.ackonwledgment.index');
    var validateAndSubmit = require('onboarding.form.handlers').validateAndSubmit;
    var documents = require('onboarding.documents.index');
    var submitDraft = require('onboarding.form.handlers').submitDraft;
    var resetForm = require('onboarding.common').resetFormValidation;
    var detectIe = require('onboarding.common').detectIe;
    var definePolyfill = require('onboarding.polyfill');
    var initTooltip = require('onboarding.common').initTooltip;

    var panelCollapsed = require('panelCollapsed');
    var setEqualHeightRows = require('setEqualHeightRows');
    var clearAddress = require('clearAddress');

    function init (model) {
        initializing = true;
        company.initCompany(model !== undefined ? model.CompanyInfo : {});
        aknowledgement.init(model !== undefined ? model.Owners : []);
        ownerInfo.init(model !== undefined ? model.Owners : []);
        product.initProducts(model !== undefined ? model.ProductInfo : {});
        documents.init(model.DictionariesData.LicenseDocuments, model.AdditionalDocuments, model.RequiredDocuments);
        consent.init(model.AllowCommunicate, model.AllowCreditCheck);

        $('#submitBtn').on('click', validateAndSubmit);
        $('.save-and-resume').on('click', submitDraft);

        if (detectIe()) {
            definePolyfill();
        }
        var salesrep = $('#OnBoardingLink').val();
        if ($(location).attr('href').toLowerCase().indexOf('resumeonboarding') >= 0) {
            gtag('event', 'Dealer Application Start', { 'event_category': 'Dealer Application Start', 'event_action': 'Resume Link open', 'event_label': salesrep });
        }
        else {
            gtag('event', 'Dealer Application Start', { 'event_category': 'Dealer Application Start', 'event_action': 'New Application Link open', 'event_label': salesrep });
        }

        $('.customer-loan-form-panel .panel-heading').on('click', function () {
            panelCollapsed($(this))
        });

        ($(".equal-height-section-1"));

        $(window).on('resize', function () {
            setEqualHeightRows($(".equal-height-section-1"));
        });

        $('.clear-address').on('click', clearAddress);

        $.validator.addMethod('validurl', function (value, element) {
            return $.validator.methods.url.call(this, value, element) || $.validator.methods.url.call(this, 'http://' + value, element);
        }, translations.SiteInvalidFormat);

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
});
