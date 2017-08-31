module.exports('onboarding.index', function (require) {
    var company = require('onboarding.company');
    var product = require('onboarding.product');
    var ownerInfo = require('onboarding.owner-info.index');
    var consent = require('onboarding.consent.index');
    var validateAndSubmit = require('onboarding.form.handlers');

    function init() {
        company.initCompany();
        product.initProducts();
        ownerInfo.init();
        consent.init();

        $('#submit').on('click', validateAndSubmit);
        $('.save-and-resume').on('click', submitDraft);
    }

    function submitDraft(e) {
        var formData = $('form').serialize();
        $.when($.ajax({
            type: 'POST',
            url: saveDraftUrl,
            data:formData
        })).done(function(data) {
            $('#save-resume-modal').html(data);
            $('#save-resume-modal').modal('show');
            initSendEmail();
        });
    }

    function initSendEmail() {
        $('#send-draft-email').on('submit', function () {

        });
    }

    window.init = init;

    return {
        init: init
    }
})