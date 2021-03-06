﻿module.exports('basicInfo.index', function (require) {
    var homeOwnerEmployment = require('basicInfo.homeOwner.employment');
    var additionalEmployment = require('basicInfo.additionalApplicants.employment');

    var dob = require('dob-selecters');
    var checkApplicantsAge = require('customer-validation').checkApplicantsAge;
    var checkCreditAgree = require('customer-validation').checkCreditAgree;
    var scrollPageTo = require('scrollPageTo');
    var dynamicAlertModal = require('alertModal').dynamicAlertModal;
    var hideDynamicAlertModal = require('alertModal').hideDynamicAlertModal;
    var addAdditionalButton, aditional1Section;

    function setDataAttrInModal(index) {
        var modal = document.getElementById('camera-modal');
        modal.setAttribute('data-fnToFill', 'additional-first-name-' + index);
        modal.setAttribute('data-lnToFill', 'additional-last-name-' + index);
        modal.setAttribute('data-bdToFill', 'additional-birth-date-' + index);
        modal.setAttribute('data-dlToFill', 'additional-dl-number-' + index);
        modal.setAttribute('data-stToFill', 'additional-street-' + index);
        modal.setAttribute('data-ctToFill', 'additional-locality-' + index);
        modal.setAttribute('data-prToFill', 'additional-administrative_area_level_1-' + index);
        modal.setAttribute('data-pcToFill', 'additional-postal_code-' + index);
    }

    function hideAditional1Section() {
        aditional1Section.hide();
        //Needed for validation
        aditional1Section.find('.personal-info-section').find('input, select').each(function () {
            $(this).prop("disabled", true);
        });
        aditional1Section.find('.address-checkbox').each(function () {
            var checkBox = $(this);
            var correspondingSection = $('#' + checkBox.data('section'));
            disableMailingAddress(correspondingSection);
        });
        aditional1Section.data('active', false);
        addAdditionalButton.show();
		$('#proceed-error-message').hide();
		//$(this).removeClass('input-custom-err');
		//$('#additional-applicant-qcError').text('');
    }

    function enableMailingAddress(section) {
        section.find('input, select').each(function () {
            $(this).prop('disabled', false);
        });
        section.slideDown();
    }

    function disableMailingAddress(section) {
        section.slideUp();
        section.find('input, select').each(function () {
            $(this).prop('disabled', true);
        });
    }

    function showAditional1Section() {
        aditional1Section.show();
        aditional1Section.find('.personal-info-section').find('input, select').each(function () {
            $(this).prop('disabled', false);
        });
        aditional1Section.data('active', true);
        addAdditionalButton.hide();
    }
	
    function init() {

        configInitialized
            .then(function () {});

        $('input[type="text"]').on('change', function () {
            this.value = this.value.trim();
        });

        $('#save-and-proceed-button').prop('disabled', false);

        $('.dob-group').each(function (index, el) {
            dob.initDobGroup(el);
        });
        $('.dob-input').on('change', function () {
            $(this).valid();
        });

        $('.scanlicence-info-link').on('click', function () {
            $(this).toggleClass('active');
            return false;
        });

        $('.j-personal-data-used-modal').on('click', function (e) {
            var data = {
                message: $('#personal-data-used').html(),
                "class": 'consents-modal basic-info',
                cancelBtnText: '',
                confirmBtnText: 'OK'
            };
            dynamicAlertModal(data);
            $('#confirmAlert').one('click', function (e) {
                hideDynamicAlertModal()
                e.preventDefault();
            });
            e.preventDefault();
        });

        $(window).keydown(function (event) {
            if (event.keyCode === 13) {
                event.preventDefault();
                return false;
            }
            return true;
        });

        $('#agreement-checkbox, #additional-owner-agrees').change(function () {
            var isValid = checkCreditAgree();
            if (isValid) {
                $('#proceed-error-message').hide();
            }
        });

        $('#additional-street-1').on('change', function () {
            var camera = $('#camera-modal');
            if (camera.is('.in') && camera.attr('data-fntofill').indexOf('additional') > -1) {
                $('#mailing-address-checkbox-add-app1').click();
            }
        });

        var province = $('#administrative_area_level_1');
		var additionalApplicantProvince = $('#additional-administrative_area_level_1-1');

		province.change(function (e) {
			var isQuebec = e.target.value === 'QC';
			var isQuebecDealer = $('#is-quebec-dealer').val().toLowerCase() === 'true';

			if (isQuebec) {
				if (!isQuebecDealer) {
					$(this).addClass('input-custom-err');
					$('#qcError').text(translations.InstallationAddressCannotInQuebec);
				} else {
					$(this).removeClass('input-custom-err');
					$('#qcError').text('');
				}
			} else {
				if (isQuebecDealer) {
					$('#qcError').text(translations.InstallationAddressInQuebec);
					$(this).addClass('input-custom-err');
				} else {
					$(this).removeClass('input-custom-err');
					$('#qcError').text('');
				}
			}
		});

		additionalApplicantProvince.change(function(e) {
			var isQuebec = e.target.value === 'QC';
			var isQuebecDealer = $('#is-quebec-dealer').val().toLowerCase() === 'true';

			if (isQuebec) {
				if (!isQuebecDealer) {
					$(this).addClass('input-custom-err');
					$('#additional-applicant-qcError').text(translations.AdditionalApplicantAddressCannotInQuebec);
				} else {
					$(this).removeClass('input-custom-err');
					$('#additional-applicant-qcError').text('');
				}
			} else {
				if (isQuebecDealer) {
					$('#additional-applicant-qcError').text(translations.AdditionalApplicantAddressInQuebec);
					$(this).addClass('input-custom-err');
				} else {
					$(this).removeClass('input-custom-err');
					$('#additional-applicant-qcError').text('');
				}
			}
		});
		
        $('.check-age').change(function () {
            var atLeastOneValid = checkApplicantsAge();
            if (atLeastOneValid) {
                $('#age-warning-message').hide();
                $('#age-error-message').hide();
            } else {
                $('#age-error-message').hide();
                //  $('#age-warning-message').show();
            }
        });

        $('#homeowner-checkbox').change(function () {
            if ($(this).prop('checked')) {
                $('#proceed-homeowner-errormessage').hide();
            }
        });

        addAdditionalButton = $('#add-additional-applicant');
        aditional1Section = $('#additional1-section');
        if (aditional1Section.attr('data-initialized') === 'true') {
            showAditional1Section();
        } else {
            hideAditional1Section();
        }
		
        $('.address-checkbox').each(function () {
            var checkBox = $(this);
            var correspondingSection = $('#' + checkBox.data('section'));
            if (!checkBox.is(':checked')) {
                disableMailingAddress(correspondingSection);
            } else {
                enableMailingAddress(correspondingSection);
            }
            checkBox.click(function () {
                if ($(this).is(':checked')) {
                    enableMailingAddress(correspondingSection);
                } else {
                    disableMailingAddress(correspondingSection);
                }
            });
		});


        $('.clear-address').on('click', function () {
            var addressInfo2 = $(this).parent().closest('.address-info2');
            var addressInfo1 = addressInfo2.siblings('.address-info1');
            var holders = [];
            holders.push(addressInfo1);
            holders.push(addressInfo2);
            $.each(holders, function (index, item) {
                item.find(':input').each(function () {
                    if ($(this).not('.placeholder')) {
                        $(this).val('').change();
                    }
                });
            });
            return false;
        });
        $('#owner-scan-button').click(function (e) {
            gtag('event', 'Scan License', {
                'event_category': 'Scan License',
                'event_action': 'button_click',
                'event_label': 'DrivingLicense'
            });
            if (!(isMobileRequest || typeof isMobileRequest === 'string' && isMobileRequest.toLowerCase() === 'true')) {
                e.preventDefault();
                var modal = document.getElementById('camera-modal');
                modal.setAttribute('data-fnToFill', 'first-name');
                modal.setAttribute('data-lnToFill', 'last-name');
                modal.setAttribute('data-bdToFill', 'birth-date');
                modal.setAttribute('data-dlToFill', 'dl-number');
                modal.setAttribute('data-stToFill', 'street');
                modal.setAttribute('data-ctToFill', 'locality');
                modal.setAttribute('data-prToFill', "administrative_area_level_1");
                modal.setAttribute('data-pcToFill', "postal_code");
            }
            return true;
        });
        $('#additional1-scan-button').click(function () {
            setDataAttrInModal(1);
        });
        addAdditionalButton.click(function () {
            if (!aditional1Section.data('active')) {
                showAditional1Section();
            }
        });
        $('#additional1-remove').click(function () {
            hideAditional1Section();
        });

        //mobile adds required to DL upload input, why???
        if ($('#owner-upload-file').length) {
            $('#owner-upload-file').rules('remove');
        }

        var navigateToStep = require('navigateToStep');
        $('#steps .step-item[data-warning="true"]').on('click', function () {
            if ($(this).attr('href')) {
                navigateToStep($(this));
            }
            return false;
        });

        $('#save-and-proceed-button').click(function (event) {
            var isApprovalAge = checkApplicantsAge();
            var isHomeOwner = $('#homeowner-checkbox').is(':checked');
            var isAgreesToCreditCheck = checkCreditAgree();
			var isQuebecDealer = $('#is-quebec-dealer').val().toLowerCase() === 'true';
			var isAddressInQc = province.val().toLowerCase() === 'qc';
			var isAdditionalApplicantProvinceInQc = additionalApplicantProvince.val().toLocaleLowerCase() === 'qc';
            var isValidProcvinceAddress = false;
			var isValidProvinceForAdditionalApplicant = false;
			var additionalApplicantAddressNotSame = $('#mailing-address-checkbox-add-app1');

			if (!aditional1Section.is(':hidden') && additionalApplicantAddressNotSame.is(':checked')) {
				if (isQuebecDealer) {
					if (!isAdditionalApplicantProvinceInQc) {
						isValidProvinceForAdditionalApplicant = false;
					} else {
						isValidProvinceForAdditionalApplicant = true;
					}
				} else {
					if (isAdditionalApplicantProvinceInQc) {
						isValidProvinceForAdditionalApplicant = false;
					} else {
						isValidProvinceForAdditionalApplicant = true;
					}
				}
			}
			else {
				isValidProvinceForAdditionalApplicant = true
			}
            if (isQuebecDealer) {
                if (!isAddressInQc) {
                    isValidProcvinceAddress = false;
                } else {
                    isValidProcvinceAddress = true;
                }
            } else {
                if (isAddressInQc) {
                    isValidProcvinceAddress = false;
                } else {
                    isValidProcvinceAddress = true;
                }
            }

            if (!isHomeOwner) {
                $('#proceed-homeowner-errormessage').show();
                scrollPageTo($('#borrower-is-homeowner'));
            }

            if (!isAgreesToCreditCheck) {
                $('#proceed-error-message').show();
                scrollPageTo($('#proceed-error-message'));
            }

			if (!isValidProcvinceAddress || !isHomeOwner || !isApprovalAge || !isAgreesToCreditCheck || !isValidProvinceForAdditionalApplicant) {
                if (!isValidProcvinceAddress) {
                    $(this).addClass('input-custom-err');
                    $('#qcError').text(isQuebecDealer ? translations.InstallationAddressInQuebec : translations.InstallationAddressCannotInQuebec);
				}
				if (!isValidProvinceForAdditionalApplicant) {
					$(this).addClass('input-custom-err');
					$('#additional-applicant-qcError').text(isQuebecDealer ? translations.AdditionalApplicantAddressInQuebec : translations.AdditionalApplicantAddressCannotInQuebec);
				}
                if ($('#main-form').valid()) {
                    event.preventDefault();
                } else {
                    $('.dob-input').each(function (index, el) {
                        dob.validate(el);
                    });
                }
            }
        });

        homeOwnerEmployment.initEmployment(province.val().toLowerCase() !== 'qc' || !isQuebecDealer);
        additionalEmployment.initEmployment(province.val().toLowerCase() !== 'qc' || aditional1Section.is(':hidden'));

        province.on('change', function (e) {
            var isQuebecDealer = $('#is-quebec-dealer').val().toLowerCase() === 'true';
            if (e.target.value.toLowerCase() === 'qc' && isQuebecDealer) {
                homeOwnerEmployment.enableEmployment();
                if (!aditional1Section.is(':hidden')) {
                    additionalEmployment.enableEmployment();
                }
            } else {
                homeOwnerEmployment.disableEmployment();
                if (!aditional1Section.is(':hidden')) {
                    additionalEmployment.disableEmployment();
                }
            }
        });

        addAdditionalButton.on('click', function () {
            if (province.val().toLowerCase() == 'qc' && isQuebecDealer) {
                additionalEmployment.enableEmployment();
            }
        });

        $('#additional1-remove').click(function () {
            additionalEmployment.disableEmployment();
        });

        var form = $('#main-form');
        form.submit(function (e) {
            $('#save-and-proceed-button').prop('disabled', true);

            if (!form.valid()) {
                e.preventDefault();
                $('#save-and-proceed-button').prop('disabled', false);
            }


        });
    }

    return {
        init: init
    };
});