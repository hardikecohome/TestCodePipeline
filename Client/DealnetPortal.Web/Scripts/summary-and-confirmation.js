configInitialized
    .then(function () {
    $('.date-input').each(assignDatepicker);
    var initPaymentTypeForm = $("#payment-type-form").find(":selected").val();
    managePaymentFormElements(initPaymentTypeForm);
    $("#payment-type-form").change(function () {
        managePaymentFormElements($(this).find(":selected").val());
    });

    if (recalculateTotalCash) {
        recalculateTotalCashPrice();
    }
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
        var sumStr = formatNumber(sum);
        $(this).val(sumStr);
        $(this).parents(".dealnet-field-holder").find('.dealnet-disabled-input-value').text(sumStr.replace(/\r?\n/g, '<br />'));
    });
    var salesTax = sum * taxRate / 100;
    $("#salex-tax").text(formatNumber(salesTax));
    $("#total-monthly-payment-wtaxes").text(formatNumber(sum + salesTax));
}

function recalculateTotalCashPrice() {
    var sum = 0;
    $(".equipment-cost").each(function () {
        var numberValue = parseFloat(this.value);
        if (!isNaN(numberValue)) {
            sum += numberValue;
        }
    });

    $("#equipment-cash-price").text(formatNumber(sum));
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
        $('#new-equipment-validation-message').html('<span>' + translations['TotalCostGreaterThanAmount']  + '</span>');
        return false;
    }
    return true;
}

function checkTotalMonthlyPayment() {
    var sum = 0;
    $(".equipment-cost").each(function () {
        var numberValue = parseFloat(this.value);
        if (!isNaN(numberValue)) {
            sum += numberValue;
        }
    });
    if (!checkCalculationValidity(sum)) {
        $('#new-equipment-validation-message').html("<span>" + translations['TotalMonthlyPaymentMustBeGreaterZero'] + "</span>");
        return false;
    }
    return true;
}

function checkProvince() {
    var provinceCode = toProvinceCode($("#administrative_area_level_1").val());
    var provinceTaxRate = provinceTaxRates[provinceCode];
    var rate = typeof provinceTaxRate !== 'undefined' ? provinceTaxRate.rate : 0;
    if (!checkCalculationValidity(null, rate)) {
        $('#address-info-validation-message').html("<span>"+ translations['AfterProvinceChangeTotalMustBeGreater'] +"</span>");
        return false;
    }
    return true;
}

function applyProvinceChange() {
    var provinceCode = toProvinceCode($("#administrative_area_level_1").val());
    var provinceTaxRate = provinceTaxRates[provinceCode];
    var taxDescription = typeof provinceTaxRate !== 'undefined' ? provinceTaxRate.description : translations['Tax'];
    $("#tax-label").text(taxDescription);
    var rate = typeof provinceTaxRate !== 'undefined' ? provinceTaxRate.rate : 0;
    taxRate = rate;
    switch(agreementType) {
        case 0:
            calculateLoanValues();
            break;
        case 1:
        case 2:
            recalculateTotalMonthlyPayment();
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
        showButtonPanel: true,
        closeText: translations['Cancel'],
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
            initGoogleServices("previous_street", "previous_locality", "previous_administrative_area_level_1", "previous_postal_code");
            for (var i = 1; i <= 3; i++) {
                initGoogleServices("additional-street-" + i, "additional-locality-" + i, "additional-administrative_area_level_1-" + i, "additional-postal_code-" + i);
                initGoogleServices("additional-previous-street-" + i, "additional-previous-locality-" + i, "additional-previous-administrative_area_level_1-" + i, "additional-previous-postal_code-" + i);
            }
        });
}