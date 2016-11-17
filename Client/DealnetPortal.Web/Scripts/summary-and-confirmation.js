$(document).ready(function () {
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

function recalculateTotalMonthlyPayment() {
    var sum = 0;
    $(".monthly-cost").each(function () {
        var numberValue = parseFloat(this.value);
        if (!isNaN(numberValue)) {
            sum += numberValue;
        }
    });

    $(".total-monthly-payment").each(function () {
        var sumStr = sum.toFixed(2);
        $(this).val(sumStr);
        $(this).parents(".dealnet-field-holder").find('.dealnet-disabled-input-value').text(sumStr);
    });
    var salesTax = sum * taxRate / 100;
    $("#salex-tax").text(salesTax.toFixed(2));
    $("#total-monthly-payment-wtaxes").text((sum + salesTax).toFixed(2));
}

function recalculateTotalCashPrice() {
    var sum = 0;
    $(".equipment-cost").each(function () {
        var numberValue = parseFloat(this.value);
        if (!isNaN(numberValue)) {
            sum += numberValue;
        }
    });

    $("#equipment-cash-price").text(sum.toFixed(2));
    calculateLoanValues();
}

function checkTotalEquipmentCost() {
    var sum = 0;
    $(".equipment-cost").each(function () {
        var numberValue = parseFloat(this.value);
        if (!isNaN(numberValue)) {
            sum += numberValue;
        }
    });
    if (sum > creditAmount) {
        $('#new-equipment-validation-message').html('<span>Total equipments cost cannot be greater than Credit Amount</span>');
        return false;
    }
    return true;
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
        beforeShow: function (input, inst) {
            if (navigator.userAgent.match(/(iPod|iPhone|iPad)/)){
                var offset = $(input).offset();
                var height = $(input).height();
                window.setTimeout(function () {
                    inst.dpDiv.css({ top: (offset.top + height + 14) + 'px', left: offset.left + 'px' })
                }, 1);
            }
        },
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