$(document).ready(function () {
    $(window).keydown(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            return false;
        }
    });
});
$("#birth-date").datepicker({
    dateFormat: 'mm/dd/yy', changeMonth: true,
    changeYear: true,
    yearRange: '1900:2016',
    minDate: Date.parse("1900-01-01"),
    maxDate: new Date()
});
$("#additional-birth-date-1").datepicker({
    dateFormat: 'mm/dd/yy', changeMonth: true,
    changeYear: true,
    yearRange: '1900:2016',
    minDate: Date.parse("1900-01-01"),
    maxDate: new Date()
});
$("#additional-birth-date-2").datepicker({
    dateFormat: 'mm/dd/yy', changeMonth: true,
    changeYear: true,
    yearRange: '1900:2016',
    minDate: Date.parse("1900-01-01"),
    maxDate: new Date()
});
$("#additional-birth-date-3").datepicker({
    dateFormat: 'mm/dd/yy', changeMonth: true,
    changeYear: true,
    yearRange: '1900:2016',
    minDate: Date.parse("1900-01-01"),
    maxDate: new Date()
});
//
$(function () {
    $.validator.addMethod(
        "date",
        function (value, element) {
            var minDate = Date.parse("1900-01-01");
            var maxDate = new Date();
            var valueEntered = Date.parseExact(value, "M/d/yyyy");
            if (!valueEntered) {
                return false;
            }
            if (valueEntered < minDate || valueEntered > maxDate) {
                return false;
            }
            return true;
        },
        "Please enter a valid date!"
    );
});
//
var addAdditionalButton = $("#add-additional-applicant");
var aditional1Section = $("#additional1-section");
var aditional2Section = $("#additional2-section");
var aditional3Section = $("#additional3-section");
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
//
var mailingAddress = $("#mailing-adress");
var mailingAddressCheckbox = $("#mailing-adress-checkbox");
if (!mailingAddressCheckbox.is(":checked")) {
    mailingAddress.hide();
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

function enableMailingAddress() {
    $("#mailing_street").prop("disabled", false);
    $("#mailing_unit_number").prop("disabled", false);
    $("#mailing_locality").prop("disabled", false);
    $("#mailing_administrative_area_level_1").prop("disabled", false);
    $("#mailing_postal_code").prop("disabled", false);
    mailingAddress.show(300);
}

function disableMailingAddress() {
    mailingAddress.hide(200);
    $("#mailing_street").prop("disabled", true);
    $("#mailing_unit_number").prop("disabled", true);
    $("#mailing_locality").prop("disabled", true);
    $("#mailing_administrative_area_level_1").prop("disabled", true);
    $("#mailing_postal_code").prop("disabled", true);
}


$("#clear-address").click(function () {
    document.getElementById('street').value = '';
    document.getElementById('unit_number').value = '';
    document.getElementById('locality').value = '';
    document.getElementById('administrative_area_level_1').value = '';
    document.getElementById('postal_code').value = '';
}); 
$("#clear-mailing-address").click(function () {
    document.getElementById('mailing_street').value = '';
    document.getElementById('mailing_unit_number').value = '';
    document.getElementById('mailing_locality').value = '';
    document.getElementById('mailing_administrative_area_level_1').value = '';
    document.getElementById('mailing_postal_code').value = '';
});
$("#owner-scan-button").click(function () {
    var modal = document.getElementById('camera-modal');
    modal.setAttribute('data-fnToFill', 'first-name');
    modal.setAttribute('data-lnToFill', 'last-name');
    modal.setAttribute('data-bdToFill', 'birth-date');
    modal.setAttribute('data-fillAddress', "true");
});
$("#additional1-scan-button").click(function () {
    var modal = document.getElementById('camera-modal');
    modal.setAttribute('data-fnToFill', 'additional-first-name-1');
    modal.setAttribute('data-lnToFill', 'additional-last-name-1');
    modal.setAttribute('data-bdToFill', 'additional-birth-date-1');
    modal.setAttribute('data-fillAddress', "false");
});
$("#additional2-scan-button").click(function () {
    var modal = document.getElementById('camera-modal');
    modal.setAttribute('data-fnToFill', 'additional-first-name-2');
    modal.setAttribute('data-lnToFill', 'additional-last-name-2');
    modal.setAttribute('data-bdToFill', 'additional-birth-date-2');
    modal.setAttribute('data-fillAddress', "false");
});
$("#additional3-scan-button").click(function () {
    var modal = document.getElementById('camera-modal');
    modal.setAttribute('data-fnToFill', 'additional-first-name-3');
    modal.setAttribute('data-lnToFill', 'additional-last-name-3');
    modal.setAttribute('data-bdToFill', 'additional-birth-date-3');
    modal.setAttribute('data-fillAddress', "false");
});
$("#add-additional-applicant").click(function() {
    if (!aditional1Section.is(':visible')) {
        showAditional1Section();
    } else if (!aditional2Section.is(':visible')) {
        showAditional2Section();
    } else if (!aditional3Section.is(':visible')) {
        showAditional3Section();
    }
});
function showAditional1Section() {
    aditional1Section.show();
    $("#additional-first-name-1").prop("disabled", false);
    $("#additional-last-name-1").prop("disabled", false);
    $("#additional-birth-date-1").prop("disabled", false);
    if (aditional2Section.is(':visible') && aditional3Section.is(':visible')) {
        addAdditionalButton.hide();
    }
}
function showAditional2Section() {
    aditional2Section.show();
    $("#additional-first-name-2").prop("disabled", false);
    $("#additional-last-name-2").prop("disabled", false);
    $("#additional-birth-date-2").prop("disabled", false);
    if (aditional1Section.is(':visible') && aditional3Section.is(':visible')) {
        addAdditionalButton.hide();
    }
}
function showAditional3Section() {
    aditional3Section.show();
    $("#additional-first-name-3").prop("disabled", false);
    $("#additional-last-name-3").prop("disabled", false);
    $("#additional-birth-date-3").prop("disabled", false);
    addAdditionalButton.hide();
}
$("#additional1-remove").click(function () {
    hideAditional1Section();
});
$("#additional2-remove").click(function () {
    hideAditional2Section();
});
$("#additional3-remove").click(function () {
    hideAditional3Section();
});
function hideAditional1Section() {
    aditional1Section.hide();
    //Needed for validation
    $("#additional-first-name-1").prop("disabled", true);
    $("#additional-last-name-1").prop("disabled", true);
    $("#additional-birth-date-1").prop("disabled", true);
    addAdditionalButton.show();
}
function hideAditional2Section() {
    aditional2Section.hide();
    //Needed for validation
    $("#additional-first-name-2").prop("disabled", true);
    $("#additional-last-name-2").prop("disabled", true);
    $("#additional-birth-date-2").prop("disabled", true);
    addAdditionalButton.show();
}
function hideAditional3Section() {
    aditional3Section.hide();
    //Needed for validation
    $("#additional-first-name-3").prop("disabled", true);
    $("#additional-last-name-3").prop("disabled", true);
    $("#additional-birth-date-3").prop("disabled", true);
    addAdditionalButton.show();
}