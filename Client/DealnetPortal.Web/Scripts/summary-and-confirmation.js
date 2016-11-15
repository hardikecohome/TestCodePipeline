﻿$(document).ready(function () {
    $('.date-input').each(assignDatepicker);
    var initPaymentTypeForm = $("#payment-type-form").find(":selected").val();
    managePaymentFormElements(initPaymentTypeForm);
    $("#payment-type-form").change(function () {
        managePaymentFormElements($(this).find(":selected").val());
    });
});
function managePaymentFormElements(paymentType) {
    switch (paymentType) {
        case '0':
            $(".pap-payment-form").hide();
            $(".enbridge-payment-form").show();
            break;
        case '1':
            $(".enbridge-payment-form").hide();
            $(".pap-payment-form").show();
            break;
    }
}

function assignDatepicker() {
    var input = $(this);
    inputDateFocus(input);
    input.datepicker({
        dateFormat: 'mm/dd/yy',
        changeYear: true,
        changeMonth: (viewport().width < 768) ? true : false,
        yearRange: '1900:2200',
        minDate: new Date(),
        onClose: function(){
            onDateSelect($(this));
        }
    });
}

function assignAutocompletes() {
    $(document)
        .ready(function () {
            initGoogleServices("street", "locality", "administrative_area_level_1", "postal_code");
            initGoogleServices("mailing_street", "mailing_locality", "mailing_administrative_area_level_1", "mailing_postal_code");
        });
}

function submitAsync(url, form, aftervalidateAction) {
    form.validate();
    if (!form.valid()) {
        return;
    };
    if (aftervalidateAction != null) {
        aftervalidateAction();
    }
    $('.sent-email-msg').show();
    $("#send-email-button").text('Resend Emails');
    form.ajaxSubmit({
        type: "POST",
        url: url,
        success: function(json) {
        },
        error: function(xhr, status, p3) {
            hideLoader();
            alert(xhr.responseText);
        }
    });
}