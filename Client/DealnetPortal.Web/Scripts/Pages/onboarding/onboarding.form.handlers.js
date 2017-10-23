﻿module.exports('onboarding.form.handlers', function (require) {
    var state = require('onboarding.state').state;
    var resetForm = require('onboarding.common').resetFormValidation;

    function submitDraft (e) {
        showLoader();
		var formData = $('#onboard-form').serialize();
		var salesrep = $('#OnBoardingLink').val();
		gtag('event', 'Dealer Application Saved', { 'event_category': 'Dealer Application Saved', 'event_action': 'Save and Resume button clicked', 'event_label': salesrep });
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
        if (state.product.selectedEquipment.length > 1) {
            $('#equipment-error').addClass('hidden');
        } else {
            $('#equipment-error').removeClass('hidden');
            return false;
        }
        return true;
    }

    function validateWorkProvinces () {
        if (state.company.selectedProvinces.length > 1) {
            $('#work-province-error').addClass('hidden');
        } else {
            $('#work-province-error').removeClass('hidden');
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

	function validate(e) {
		
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
