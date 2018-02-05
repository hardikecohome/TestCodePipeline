module.exports('basicInfo.index', function (require) {
    var homeOwnerEmployment = require('basicInfo.homeOwner.employment');
    var additionalEmployment = require('basicInfo.additionalApplicants.employment');

    var dob = require('dob-selecters');
    var checkApplicantsAge = require('customer-validation').checkApplicantsAge;
    var checkHomeOwner = require('customer-validation').checkHomeOwner;
    var checkCreditAgree = require('customer-validation').checkCreditAgree;
    var scrollPageTo = require('scrollPageTo');
    var dynamicAlertModal = require('alertModal').dynamicAlertModal;
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

        $('.dob-group').each(function (index, el) {
            dob.initDobGroup(el);
        });
        $('.dob-input').on('change', function() {
            $(this).valid();
        });

        $('.scanlicence-info-link').on('click', function () {
            $(this).toggleClass('active');
            return false;
        });

        $('.j-personal-data-used-modal').on('click', function (e) {
            var data = {
                message: $('#personal-data-used').html(),
                "class": 'consents-modal',
                cancelBtnText: 'OK'
            };
            dynamicAlertModal(data);
            e.preventDefault();
        });

        $(window).keydown(function (event) {
            if (event.keyCode === 13) {
                event.preventDefault();
                return false;
            }
            return true;
        });

        $('#agreement-checkbox').change(function () {
            var isValid = checkCreditAgree();
            if (isValid) {
                $('#proceed-error-message').hide();
            }
        });

        var province = $('#administrative_area_level_1');
        province.change(function (e) {
            var isQuebec = e.target.value === 'QC';
            var isQuebecDealer = $('#is-quebec-dealer').val().toLowerCase() === 'true';
            var proceedQcDealer = $('#proceed-qc-dealer');
            var proceedNotQcDealer = $('#proceed-not-qc-dealer');

            if (isQuebec) {
                if (!isQuebecDealer) {
                    proceedNotQcDealer.show();
                }
                proceedQcDealer.hide();
            } else {
                if (isQuebecDealer) {
                    $(proceedQcDealer).show();
                }
                $(proceedNotQcDealer).hide();
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

        $('.check-homeowner').change(function () {
            var atLeastOneValid = false;
            $('.check-homeowner').each(function () {
                if ($(this).prop('checked')) {
                    atLeastOneValid = true;
                    return false;
                }
                return true;
            });
            if (atLeastOneValid) {
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
                        $(this).val('');
                    }
                });
            });
            return false;
        });
        $('#owner-scan-button').click(function (e) {
            ga('send', 'event', 'Scan License', 'button_click', 'DrivingLicense', '100');
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

        var navigateToStep = module.require('navigateToStep');
        $('#steps .step-item[data-warning="true"]').on('click', function () {
            if ($(this).attr('href')) {
                navigateToStep($(this));
            }
            return false;
        });

        $('#save-and-proceed-button').click(function (event) {
            var isApprovalAge = checkApplicantsAge();
            var isHomeOwner = checkHomeOwner();
            var isAgreesToCreditCheck = checkCreditAgree();
            var isQuebecDealer = $('#is-quebec-dealer').val().toLowerCase() === 'true';
            var isAddressInQc = province.val().toLowerCase() === 'qc';
            var isValidProcvinceAddress = false;

            //if (!isApprovalAge) {
            //    $('#age-warning-message').hide();
            //    //$('#age-error-message').show();
            //    //scrollPageTo($('#age-error-message'));
            //}

            if (isQuebecDealer) {
                if (!isAddressInQc) {
                    $('#proceed-qc-dealer').show();
                    scrollPageTo($('#proceed-qc-dealer'));
                    isValidProcvinceAddress = false;
                } else {
                    isValidProcvinceAddress = true;
                }
            } else {
                if (isAddressInQc) {
                    $('#proceed-not-qc-dealer').show();
                    scrollPageTo($('#proceed-not-qc-dealer'));
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

            if (!isValidProcvinceAddress|| !isHomeOwner || !isApprovalAge || !isAgreesToCreditCheck) {
                if ($('#main-form').valid()) {
                    event.preventDefault();
                } else {
                    $('.dob-input').each(function(index, el) {
                        dob.validate(el);
                    });
                }
            }
        });

        homeOwnerEmployment.initEmployment(province.val().toLowerCase()!=='qc');
        additionalEmployment.initEmployment(province.val().toLowerCase() !== 'qc' || aditional1Section.is(':hidden'));

        province.on('change', function(e) {
            if (e.target.value.toLowerCase() === 'qc') {
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

        addAdditionalButton.on('click', function() {
            if (province.val().toLowerCase() == 'qc') {
                additionalEmployment.enableEmployment();
            }
        });

        $('#additional1-remove').click(function () {
            additionalEmployment.disableEmployment();
        });
    }

    return {
        init: init
    };
});
