$(document).ready(function () {
    $(window).keydown(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            return false;
        }
    });
});
$("#mailing-adress").hide();
$("#mailing-adress-checkbox").click(function () {
    if ($(this).is(":checked")) {
        $("#mailing-adress").show(300);
    } else {
        $("#mailing-adress").hide(200);
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