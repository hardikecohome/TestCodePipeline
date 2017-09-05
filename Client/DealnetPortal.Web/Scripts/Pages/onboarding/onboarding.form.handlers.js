﻿module.exports('onboarding.form.handlers', function (require) {
    var state = require('onboarding.state').state;
    var resetForm = require('onboarding.common').resetFormValidation;
    var isIOS = navigator.userAgent.match(/ipad|ipod|iphone/i);

    function submitDraft (e) {
        var formData = $('form').serialize();
        $.when($.ajax({
            type: 'POST',
            url: saveDraftUrl,
            data: formData
        })).done(function (data) {
            var $modal = $('#save-resume-modal');
            $modal.html(data).modal('show');
            var key = $modal.find('#access-key').val();
            $('#AccessKey').val(key);
            var id = $modal.find('#dealer-info-id').val();
            $('#Id').val(id);
            resetForm('#send-draft-email');
            initSendEmail();
            initCopyLink();
        });
    }

    function initSendEmail () {
        function toggleAgreementError (show) {
            if (show) {
                $('#agreement-email-error').addClass('field-validation-error').text(translations.ThisFieldIsRequired);
            } else {
                $('#agreement-email-error').removeClass('field-validation-error').text('');
            }
        }
        $('#agreement-email').on('change', function () {
            toggleAgreementError(!$(this).prop('checked'));
        })
        $('#send-draft-email').on('submit', function () {
            var $this = $(this);
            var allowCommunicate = $this.find('#agreement-email').prop('checked');
            toggleAgreementError(!allowCommunicate);
            if ($this.valid() && allowCommunicate) {
                $.ajax({
                    url: sendEmailUrl,
                    type: 'POST',
                    data: $this.serialize(),
                    success: function (data) {
                        if (data.success) {
                            $('#sent-success').removeClass('hidden');
                        } else {
                            $('#sent-success').addClass('hidden');
                        }
                    }
                });
            }
        });
    }

    function selectElement (el) {
        if (el.nodeName === "TEXTAREA" || el.nodeName === "INPUT")
            el.select();
        if (el.setSelectionRange && isIOS)
            el.setSelectionRange(0, 999999);
    }

    function copyCommand () {
        if (document.queryCommandSupported("copy")) {
            document.execCommand('copy');
        }
    }

    function initCopyLink () {

        var link = document.getElementById('resume-link');
        if (!isIOS) {

            var activeLink = '';

            $('#copy-resume-link').on('click',
                function () {
                    activeLink = link.value;
                    selectElement(link);
                    copyCommand();
                });

        } else {
            link = $('#resume-link');

            var linkVal = link.val();

            link.parent().append($.parseHTML('<a href="' + linkVal + '" style="word-wrap: break-word;">' + linkVal + '</a>'));

            link.hide();
            $('##copy-resume-link').hide();
        }
    }

    function validateEquipment () {
        if (state.selectedEquipment.length > 1) {
            $('#equipment-error').removeClass('field-validation-error').text('');
        } else {
            $('#equipment-error').addClass('field-validation-error').text(translations.SelectOneProduct);
            return false;
        }
        return true;
    }

    function validateWorkProvinces () {
        if (state.selectedProvinces.length > 1) {
            $('#work-province-error').removeClass('field-validation-error').text('');
        } else {
            $('#work-province-error').addClass('field-validation-error').text(translations.SelectOneProvince);
            return false;
        }
        return true;
    }

    function successCallback (json) {
        hideLoader();
    }

    function errorCallback (xhr, status, p3) {
        hideLoader();
    }

    function validate (e) {
        var $form = $('#onboard-form');
        $('#submit').prop('disabled', true);

        var equipValid = validateEquipment();
        var workProvinceValid = validateWorkProvinces();

        if (!$form.valid()) {
            e.preventDefault();

            $('#submit').prop('disabled', false);
            return;
        }

        if (equipValid && workProvinceValid) {
            showLoader();
            $form.ajaxSubmit({
                type: 'POST',
            });
        } else {
            e.preventDefault();
        }
    }

    return {
        validateAndSubmit: validate,
        submitDraft: submitDraft
    };
});
