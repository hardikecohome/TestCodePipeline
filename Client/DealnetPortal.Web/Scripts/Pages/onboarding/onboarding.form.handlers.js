module.exports('onboarding.form.handlers', function (require) {
    var state = require('onboarding.state').state;
    var resetForm = require('onboarding.common').resetFormValidation;

    function submitDraft (e) {
        showLoader();
        var formData = $('#onboard-form').serialize();
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
            hideLoader();
            $('#send-email-submit').prop('disabled', $('#agreement-email').prop('checked') !== true);
        });
    }

    function initSendEmail () {
        $('#agreement-email').on('change', function (e) {
            $('#send-email-submit').prop('disabled', !e.target.checked);
        });
        $('#send-draft-email').on('submit', function (e) {
            var $this = $(this);
            e.preventDefault();
            var allowCommunicate = $this.find('#agreement-email').prop('checked');
            if ($this.valid() && allowCommunicate) {
                e.preventDefault();
                $.ajax({
                    url: sendLinkUrl,
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
        if (el.setSelectionRange && $('body').is('.ios-device'))
            el.setSelectionRange(0, 999999);
    }

    function copyCommand () {
        if (document.queryCommandSupported("copy")) {
            document.execCommand('copy');
        }
    }

    function initCopyLink () {

        var link = document.getElementById('resume-link');
        if (!$('body').is('.ios-device')) {

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

            link.attr('type', 'hidden');
            $('#copy-resume-link').hide();
        }
    }

    function validateEquipment () {
        if (state.product.selectedEquipment.length >= 1) {
            $('#equipment-error').addClass('hidden');
        } else {
            $('#equipment-error').removeClass('hidden');
            return false;
        }
        return true;
    }

    function validateWorkProvinces () {
        if (state.company.selectedProvinces.length >= 1) {
            $('#work-province-error').addClass('hidden');
        } else {
            $('#work-province-error').removeClass('hidden');
            return false;
        }
        return true;
    }

    function validateDocuments() {
        var license = state['documents']['addedLicense'].filter(function (lic) {
            if (lic.noExpiry === undefined) {
                return lic.nubmer === '' || lic.date === '';
            } else {
                if (lic.noExpiry === true) {
                    return lic.nubmer === '';
                } else {
                    return lic.nubmer === '' || lic.date === null;
                }
            }
        }).length === 0;

        var insurence = state['documents']['insurence-files'].length > 0;
        var voidCheque = state['documents']['void-cheque-files'].length > 0;

        return license && insurence && voidCheque;
    }

    function successCallback (json) {
        hideLoader();
    }

    function errorCallback (xhr, status, p3) {
        hideLoader();
    }

    function validate (e) {
        var $form = $('#onboard-form');
        $('#submitBtn').prop('disabled', true);

        var equipValid = validateEquipment();
        var workProvinceValid = validateWorkProvinces();
        var documentsUploaded = validateDocuments();

        if (!$form.valid()) {
            e.preventDefault();

            $('#submitBtn').prop('disabled', false);
            return;
        }

        $('#IsDocumentsUploaded').val(documentsUploaded);

        if (equipValid && workProvinceValid) {
            showLoader();
            $form.submit();
        } else {
            e.preventDefault();
        }
    }

    return {
        validateAndSubmit: validate,
        submitDraft: submitDraft
    };
});
