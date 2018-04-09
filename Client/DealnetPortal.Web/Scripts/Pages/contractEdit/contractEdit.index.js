module.exports('contract-edit', function (require) {
    var showLoader = require('loader').showLoader;
    var hideLoader = require('loader').hideLoader;
    var eSign = require('contract-edit.eSignature');
    var navigateToStep = require('navigateToStep');
    var toggleBackToTopVisibility = require('backToTop').toggleBackToTopVisibility;
    var backToTop = require('backToTop').backToTop;
    var setEqualHeightRows = require('setEqualHeightRows');

    var dynamicAlertModal = require('alertModal').dynamicAlertModal;
    var hideDynamicAlertModal = require('alertModal').hideDynamicAlertModal;

    var init = function (eSignEnabled) {
        if (eSignEnabled === 1) {
            eSign.init();
        }
        $('.editToStep1').on('click', function () {
            if ($(this).attr('href')) {
                navigateToStep($(this));
            }
            return false;
        });

        $(window).on('scroll', toggleBackToTopVisibility)
            .on('resize', function () {
                toggleBackToTopVisibility();
                setEqualHeightRows($('.summary-payment-info .dealnet-field-caption'));
                setEqualHeightRows($('.summary-payment-info .dealnet-field-holder'));
            });

        $('#back-to-top').on('click', function () {
            backToTop();
        });

        setTimeout(function () {
            setEqualHeightRows($('.summary-payment-info .dealnet-field-caption'));
            setEqualHeightRows($('.summary-payment-info .dealnet-field-holder'));
        }, 0);

        $('#send-all-documents-report').on('click', auditConfirmModal);
    }

    function auditConfirmModal() {
        var data = {
            class: "audit-alert-modal",
            message: translations['DidYouUploadAllDocuments'],
            title: translations['FinalCheck'],
            confirmBtnText: translations['Proceed']
        };

        dynamicAlertModal(data);
        $('#confirmAlert').on('click', function () {
            submitAllDocumentsUploaded();

			gtag('event', 'Button', { 'event_category': 'Button', 'event_action': 'Click', 'event_label': 'SendDocuments' });
        });
    }

    function submitAllDocumentsUploaded() {
        showLoader();
        $('#all-documents-uploaded-form').ajaxSubmit({
            method: 'post',
            success: function (result) {
                if (result.isSuccess) {
                    $('.before-all-documents-submitted').hide();
                    $('#all-documents-submitted-message').show();
                    $('.disablable').addClass('disabled');
                    $('button.disabled, input.disabled').attr('disabled', 'disabled');
                    $('.dealnet-section-edit-link').hide();
                    $('.add-applicant-link').hide();
                    $('#esignature-link').addClass('disabled');
                    $('a[id^="signer-btn-"]').addClass('disabled');
                    eSign.disable();
                    isSentToAudit = true;
                } else if (result.isError) {
                    alert(translations['AnErrorWhileSendingReport']);
                }
            },
            error: function () {
            },
            complete: function (xhr) {
                hideLoader();
                hideDynamicAlertModal();

            }
        });
    }


    return {
        init: init
    };
});