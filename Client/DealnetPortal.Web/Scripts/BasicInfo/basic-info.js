var addAdditionalButton, aditional1Section, aditional2Section, aditional3Section, mailingAddress, mailingAddressCheckbox;
$(document).ready(function () {
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
            $('#age-warning-message').show();
        }
    });
    $.validator.addMethod(
        "date",
        function (value, element) {
            var minDate = Date.parse("1900-01-01");
            var maxDate = new Date(new Date().setFullYear(new Date().getFullYear() - 18));
            var valueEntered = Date.parseExact(value, "M/d/yyyy");
            if (!valueEntered) {
                $.validator.messages.date = "The date must be in correct format";
                return false;
            }
            if (valueEntered < minDate) {
                $.validator.messages.date = "The date must be over 1900 year";
                return false;
            }
            if (valueEntered > maxDate) {
                $.validator.messages.date = "The applicant needs to be over 18 years old";
                return false;
            }
            return true;
        },
        "Please enter a valid date!"
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
    mailingAddress = $("#mailing-address");
    mailingAddressCheckbox = $("#mailing-address-checkbox");
    if (!mailingAddressCheckbox.is(":checked")) {
        mailingAddress.slideUp();
    } else {
        enableMailingAddress();
    }
    mailingAddressCheckbox.click(function () {
        if ($(this).is(":checked")) {
            enableMailingAddress();
        } else {
            disableMailingAddress();
        }
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
        var modal = document.getElementById('camera-modal');
        modal.setAttribute('data-fnToFill', 'additional-first-name-1');
        modal.setAttribute('data-lnToFill', 'additional-last-name-1');
        modal.setAttribute('data-bdToFill', 'additional-birth-date-1');
        modal.setAttribute('data-dlToFill', 'additional-dl-number-1');
        modal.setAttribute('data-stToFill', 'additional-street-1');
        modal.setAttribute('data-ctToFill', 'additional-locality-1');
        modal.setAttribute('data-prToFill', "additional-administrative_area_level_1-1");
        modal.setAttribute('data-pcToFill', "additional-postal_code-1");
    });
    $("#additional2-scan-button").click(function () {
        var modal = document.getElementById('camera-modal');
        modal.setAttribute('data-fnToFill', 'additional-first-name-2');
        modal.setAttribute('data-lnToFill', 'additional-last-name-2');
        modal.setAttribute('data-bdToFill', 'additional-birth-date-2');
        modal.setAttribute('data-dlToFill', 'additional-dl-number-2');
        modal.setAttribute('data-stToFill', 'additional-street-2');
        modal.setAttribute('data-ctToFill', 'additional-locality-2');
        modal.setAttribute('data-prToFill', "additional-administrative_area_level_1-2");
        modal.setAttribute('data-pcToFill', "additional-postal_code-2");
    });
    $("#additional3-scan-button").click(function () {
        var modal = document.getElementById('camera-modal');
        modal.setAttribute('data-fnToFill', 'additional-first-name-3');
        modal.setAttribute('data-lnToFill', 'additional-last-name-3');
        modal.setAttribute('data-bdToFill', 'additional-birth-date-3');
        modal.setAttribute('data-dlToFill', 'additional-dl-number-3');
        modal.setAttribute('data-stToFill', 'additional-street-3');
        modal.setAttribute('data-ctToFill', 'additional-locality-3');
        modal.setAttribute('data-prToFill', "additional-administrative_area_level_1-3");
        modal.setAttribute('data-pcToFill', "additional-postal_code-3");
    });
    $("#add-additional-applicant").click(function () {
        if (!aditional1Section.is(':visible')) {
            showAditional1Section();
        } else if (!aditional2Section.is(':visible')) {
            showAditional2Section();
        } else if (!aditional3Section.is(':visible')) {
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
        }
        if (!isHomeOwner) {
            $("#proceed-homeowner-errormessage").show();
        }
        if (!isHomeOwner || !isApprovalAge) {
            event.preventDefault();
        }
    });
});
function hideAditional1Section() {
    aditional1Section.hide();
    //Needed for validation
    aditional1Section.find('input, select').each(function() {
        $(this).prop("disabled", true);
    });
    addAdditionalButton.show();
}
function hideAditional2Section() {
    aditional2Section.hide();
    //Needed for validation
    aditional2Section.find('input, select').each(function () {
        $(this).prop("disabled", true);
    });
    addAdditionalButton.show();
}
function hideAditional3Section() {
    aditional3Section.hide();
    //Needed for validation
    aditional3Section.find('input, select').each(function () {
        $(this).prop("disabled", true);
    });
    addAdditionalButton.show();
}
function enableMailingAddress() {
    $("#mailing_street").prop("disabled", false);
    $("#mailing_unit_number").prop("disabled", false);
    $("#mailing_locality").prop("disabled", false);
    $("#mailing_administrative_area_level_1").prop("disabled", false);
    $("#mailing_postal_code").prop("disabled", false);
    mailingAddress.slideDown();
}
function disableMailingAddress() {
    mailingAddress.slideUp();
    $("#mailing_street").prop("disabled", true);
    $("#mailing_unit_number").prop("disabled", true);
    $("#mailing_locality").prop("disabled", true);
    $("#mailing_administrative_area_level_1").prop("disabled", true);
    $("#mailing_postal_code").prop("disabled", true);
}
function showAditional1Section() {
    aditional1Section.show();
    aditional1Section.find('input, select').each(function () {
        $(this).prop("disabled", false);
    });
    if (aditional2Section.is(':visible') && aditional3Section.is(':visible')) {
        addAdditionalButton.hide();
    }
}
function showAditional2Section() {
    aditional2Section.show();
    aditional2Section.find('input, select').each(function () {
        $(this).prop("disabled", false);
    });
    if (aditional1Section.is(':visible') && aditional3Section.is(':visible')) {
        addAdditionalButton.hide();
    }
}
function showAditional3Section() {
    aditional3Section.show();
    aditional3Section.find('input, select').each(function () {
        $(this).prop("disabled", false);
    });
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