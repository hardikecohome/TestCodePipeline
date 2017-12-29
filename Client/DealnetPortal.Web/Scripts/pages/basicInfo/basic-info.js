var addAdditionalButton, aditional1Section, aditional2Section, aditional3Section, mailingAddress, mailingAddressCheckbox;

configInitialized
    .then(function () {
        var dynamicAlertModal = module.require('alertModal').dynamicAlertModal;
        var checkApplicantsAge = module.require('customer-validation').checkApplicantsAge;
        var checkHomeOwner = module.require('customer-validation').checkHomeOwner;
        var checkCreditAgree = module.require('customer-validation').checkCreditAgree;
        var scrollPageTo = module.require('scrollPageTo');

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
        });


        $('#agreement-checkbox').change(function () {
            var isValid = checkCreditAgree();
            if (isValid) {
                $('#proceed-error-message').hide();
            }
        });

        $('#administrative_area_level_1').change(function(e) {
            var isQuebec = e.target.value === 'QC';
            if (isQuebec) {
                $('#proceed-qc-dealer').hide();
            }
        });

        $('#agreement-checkbox1').change(function () {
            var isValid = checkCreditAgree();
            if (isValid) {
                $('#proceed-error-message').hide();
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
            if (!checkBox.is(":checked")) {
                disableMailingAddress(correspondingSection);
            } else {
                enableMailingAddress(correspondingSection);
            }
            checkBox.click(function () {
                if ($(this).is(":checked")) {
                    enableMailingAddress(correspondingSection);
                } else {
                    disableMailingAddress(correspondingSection);
                }
            });
        });
        $(".clear-address").on('click', function () {
            var addressInfo2 = $(this).parent().closest('.address-info2');
            var addressInfo1 = addressInfo2.siblings('.address-info1');
            var holders = [];
            holders.push(addressInfo1);
            holders.push(addressInfo2);
            $.each(holders, function (index, item) {
                item.find(':input').each(function () {
                    if ($(this).not('.placeholder')) {
                        $(this).val("");
                    }
                });
            });
            return false;
        });
        $("#owner-scan-button").click(function (e) {
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
        $("#additional1-scan-button").click(function () {
            setDataAttrInModal(1);
        });
        $("#add-additional-applicant").click(function () {
            if (!aditional1Section.data('active')) {
                showAditional1Section();
            }
        });
        $("#additional1-remove").click(function () {
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
    });

function setDataAttrInModal (index) {
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

function hideAditional1Section () {
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
function enableMailingAddress (section) {
    section.find('input, select').each(function () {
        $(this).prop("disabled", false);
    });
    section.slideDown();
}
function disableMailingAddress (section) {
    section.slideUp();
    section.find('input, select').each(function () {
        $(this).prop("disabled", true);
    });
}
function showAditional1Section () {
    aditional1Section.show();
    aditional1Section.find('.personal-info-section').find('input, select').each(function () {
        $(this).prop("disabled", false);
    });
    aditional1Section.data('active', true);
    addAdditionalButton.hide();
}