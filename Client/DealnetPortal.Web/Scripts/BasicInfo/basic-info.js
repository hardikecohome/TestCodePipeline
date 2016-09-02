$(document).ready(function () {
    $(window).keydown(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            return false;
        }
    });
});
$("#birth-date").datepicker({ dateFormat: 'mm/dd/yy' });
$("#additional-birth-date-1").datepicker({ dateFormat: 'mm/dd/yy' });
$("#additional-birth-date-2").datepicker({ dateFormat: 'mm/dd/yy' });
$("#additional-birth-date-3").datepicker({ dateFormat: 'mm/dd/yy' });
var aditional1Section = $("#additional1-section");
var aditional2Section = $("#additional2-section");
var aditional3Section = $("#additional3-section");
aditional1Section.hide();
aditional2Section.hide();
aditional3Section.hide();
var addAdditionalButton = $("#add-additional-applicant");
//
var mailingAddress = $("#mailing-adress");
mailingAddress.hide();
$("#mailing-adress-checkbox").click(function () {
    if ($(this).is(":checked")) {
        mailingAddress.show(300);
    } else {
        mailingAddress.hide(200);
    }
});
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
});
$("#additional1-scan-button").click(function () {
    var modal = document.getElementById('camera-modal');
    modal.setAttribute('data-fnToFill', 'additional-first-name-1');
    modal.setAttribute('data-lnToFill', 'additional-last-name-1');
    modal.setAttribute('data-bdToFill', 'additional-birth-date-1');
});
$("#additional2-scan-button").click(function () {
    var modal = document.getElementById('camera-modal');
    modal.setAttribute('data-fnToFill', 'additional-first-name-2');
    modal.setAttribute('data-lnToFill', 'additional-last-name-2');
    modal.setAttribute('data-bdToFill', 'additional-birth-date-2');
});
$("#additional3-scan-button").click(function () {
    var modal = document.getElementById('camera-modal');
    modal.setAttribute('data-fnToFill', 'additional-first-name-3');
    modal.setAttribute('data-lnToFill', 'additional-last-name-3');
    modal.setAttribute('data-bdToFill', 'additional-birth-date-3');
});
$("#add-additional-applicant").click(function() {
    if (!aditional1Section.is(':visible')) {
        aditional1Section.show();
        if (aditional2Section.is(':visible') && aditional3Section.is(':visible')) {
            addAdditionalButton.hide();
        }
    } else if (!aditional2Section.is(':visible')) {
        aditional2Section.show();
        if (aditional1Section.is(':visible') && aditional3Section.is(':visible')) {
            addAdditionalButton.hide();
        }
    } else if (!aditional3Section.is(':visible')) {
        aditional3Section.show();
        addAdditionalButton.hide();
    }
});
$("#additional1-remove").click(function () {
    aditional1Section.hide();
    addAdditionalButton.show();
});
$("#additional2-remove").click(function () {
    aditional2Section.hide();
    addAdditionalButton.show();
});
$("#additional3-remove").click(function () {
    aditional3Section.hide();
    addAdditionalButton.show();
});