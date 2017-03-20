var addAdditionalButton, aditional1Section, aditional2Section, aditional3Section, mailingAddress, mailingAddressCheckbox;
configInitialized
    .then(function () {
    $(window).keydown(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            return false;
        }
    });
    assignDatepicker($("#birth-date"));
    assignDatepicker($("#additional-birth-date-1"));
    assignDatepicker($("#additional-birth-date-2"));
    assignDatepicker($("#additional-birth-date-3"));
    $('.check-age').change(function () {
        var atLeastOneValid = checkApplicantsAge();
        if (atLeastOneValid) {
            $('#age-warning-message').hide();
            $('#age-error-message').hide();
        } else {
            $('#age-error-message').hide();
            $('#age-warning-message').show();
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
            $("#proceed-homeowner-errormessage").hide();
        }
    });
    $.validator.addMethod(
        "date",
        function (value, element) {
            var minDate = Date.parse("1900-01-01");
            var maxDate = new Date(new Date().setFullYear(new Date().getFullYear() - 18));
            var valueEntered = Date.parseExact(value, "M/d/yyyy");
            if (!valueEntered) {
                $.validator.messages.date = translations['TheDateMustBeInCorrectFormat'];
                return false;
            }
            if (valueEntered < minDate) {
                $.validator.messages.date = translations['TheDateMustBeOver1900'];
                return false;
            }
            if (valueEntered > maxDate) {
                $.validator.messages.date = translations['TheApplicantNeedsToBeOver18'];
                return false;
            }
            return true;
        },
        translations['EnterValidDate']
    );

    addAdditionalButton = $("#add-additional-applicant");
    aditional1Section = $("#additional1-section");
    aditional2Section = $("#additional2-section");
    aditional3Section = $("#additional3-section");
    if (aditional1Section.attr('data-initialized') == "true") {
        showAditional1Section();
    } else {
        hideAditional1Section();
    }
    if (aditional2Section.attr('data-initialized') == "true") {
        showAditional2Section();
    } else {
        hideAditional2Section();
    }
    if (aditional3Section.attr('data-initialized') == "true") {
        showAditional3Section();
    } else {
        hideAditional3Section();
    }
    $('.address-checkbox').each(function() {
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
            item.find('input').each(function () {
                if ($(this).not('.placeholder')) {
                    $(this).val("");
                }
            });
        });
        return false;
    });
    $("#clear-mailing-address").click(function () {
        $('#mailing_street, #mailing_unit_number, #mailing_locality, #mailing_administrative_area_level_1, #mailing_postal_code').each(function () {
            if ($(this).not('.placeholder')) {
                $(this).val("");
            }
        });
        return false;
    });
    $("#owner-scan-button").click(function () {
        var modal = document.getElementById('camera-modal');
        modal.setAttribute('data-fnToFill', 'first-name');
        modal.setAttribute('data-lnToFill', 'last-name');
        modal.setAttribute('data-bdToFill', 'birth-date');
        modal.setAttribute('data-dlToFill', 'dl-number');
        modal.setAttribute('data-stToFill', 'street');
        modal.setAttribute('data-ctToFill', 'locality');
        modal.setAttribute('data-prToFill', "administrative_area_level_1");
        modal.setAttribute('data-pcToFill', "postal_code");
    });
    $("#additional1-scan-button").click(function () {
        setDataAttrInModal(1);
    });
    $("#additional2-scan-button").click(function () {
        setDataAttrInModal(2);
    });
    $("#additional3-scan-button").click(function () {
        setDataAttrInModal(3);
    });
    $("#add-additional-applicant").click(function () {
        if (!aditional1Section.data('active')) {
            showAditional1Section();
        } else if (!aditional2Section.data('active')) {
            showAditional2Section();
        } else if (!aditional3Section.data('active')) {
            showAditional3Section();
        }
    });
    $("#additional1-remove").click(function () {
        hideAditional1Section();
    });
    $("#additional2-remove").click(function () {
        hideAditional2Section();
    });
    $("#additional3-remove").click(function () {
        hideAditional3Section();
    });

    $("#save-and-proceed-button").click(function (event) {
        var isApprovalAge = checkApplicantsAge();
        var isHomeOwner = checkHomeOwner();
        if (!isApprovalAge) {
            $('#age-warning-message').hide();
            $('#age-error-message').show();
            scrollPageTo($('#age-error-message'));
        }
        if (!isHomeOwner) {
            $("#proceed-homeowner-errormessage").show();
            scrollPageTo($("#borrower-is-homeowner"));
        }
        if (!isHomeOwner || !isApprovalAge) {
            if ($('#main-form').valid()) {
                event.preventDefault();
            }
        }
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
function hideAditional1Section() {
    aditional1Section.hide();
    //Needed for validation
    aditional1Section.find('.personal-info-section').find('input, select').each(function () {
        $(this).prop("disabled", true);
    });
    aditional1Section.data('active', false);
    addAdditionalButton.show();
}
function hideAditional2Section() {
    aditional2Section.hide();
    //Needed for validation
    aditional2Section.find('.personal-info-section').find('input, select').each(function () {
        $(this).prop("disabled", true);
    });
    aditional2Section.data('active', false);
    addAdditionalButton.show();
}
function hideAditional3Section() {
    aditional3Section.hide();
    //Needed for validation
    aditional3Section.find('.personal-info-section').find('input, select').each(function () {
        $(this).prop("disabled", true);
    });
    aditional3Section.data('active', false);
    addAdditionalButton.show();
}
function enableMailingAddress(section) {
    section.find('input, select').each(function () {
        $(this).prop("disabled", false);
    });
    section.slideDown();
}
function disableMailingAddress(section) {
    section.slideUp();
    section.find('input, select').each(function () {
        $(this).prop("disabled", true);
    });
}
function showAditional1Section() {
    aditional1Section.show();
    aditional1Section.find('.personal-info-section').find('input, select').each(function () {
        $(this).prop("disabled", false);
    });
    aditional1Section.data('active', true);
    if (aditional2Section.data('active') && aditional3Section.data('active')) {
        addAdditionalButton.hide();
    }
}
function showAditional2Section() {
    aditional2Section.show();
    aditional2Section.find('.personal-info-section').find('input, select').each(function () {
        $(this).prop("disabled", false);
    });
    aditional2Section.data('active', true);
    if (aditional1Section.data('active') && aditional3Section.data('active')) {
        addAdditionalButton.hide();
    }
}
function showAditional3Section() {
    aditional3Section.show();
    aditional3Section.find('.personal-info-section').find('input, select').each(function () {
        $(this).prop("disabled", false);
    });
    aditional3Section.data('active', true);
    addAdditionalButton.hide();
}
function assignDatepicker(input) {
    inputDateFocus(input);

    input.datepicker({
        dateFormat: 'mm/dd/yy',
        changeYear: true,
        changeMonth: (viewport().width < 768) ? true : false,
        yearRange: '1900:' + (new Date().getFullYear()-18),
        minDate: Date.parse("1900-01-01"),
        maxDate: new Date(new Date().setFullYear(new Date().getFullYear() - 18)),
        onClose: function(){
            onDateSelect($(this));
        }
    });
}

function useHomeOwnerAddress(i) {
    $('#additional-street-' + i).val($('#street').val()).removeClass('pac-placeholder').removeClass('placeholder');
    $('#additional-unit_number-' + i).val($('#unit_number').val()).removeClass('pac-placeholder').removeClass('placeholder');
    $('#additional-locality-' + i).val($('#locality').val()).removeClass('pac-placeholder').removeClass('placeholder');
    $('#additional-administrative_area_level_1-' + i).val($('#administrative_area_level_1').val()).removeClass('pac-placeholder').removeClass('placeholder');
    $('#additional-postal_code-' + i).val($('#postal_code').val()).removeClass('pac-placeholder').removeClass('placeholder');
    $('#additional-residence-' + i).val($('#residence').val());
}