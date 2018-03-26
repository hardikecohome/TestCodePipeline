configInitialized
    .then(function () {
        var setEqualHeightRows = module.require('setEqualHeightRows');

        $(window).on('resize', function () {
            setEqualHeightRows($('.equal-height-label'));
        });

        setTimeout(function () {
            setEqualHeightRows($('.equal-height-label'));
        }, 0);

        var datepickerOptions = {
            yearRange: '1900:2200',
            minDate: new Date()
        };

        var rateCardValid = $('#RateCardValid').val().toLowerCase() !== 'false' ? true : false;;

        if (!rateCardValid) {
            $('#expired-rate-card-warning').removeClass('hidden');
            $('#submitBtn').addClass('disabled');
        }

        $('.date-input').each(function (index, input) {
            module.require('datepicker').assignDatepicker(input, datepickerOptions)
        });
        var initPaymentTypeForm = $("#payment-type-form").find(":selected").val();
        managePaymentFormElements(initPaymentTypeForm);
        $("#payment-type-form").change(function () {
            managePaymentFormElements($(this).find(":selected").val());
        });

        if (recalculateTotalCash) {
            recalculateTotalCashPrice();
        }

        var navigateToStep = module.require('navigateToStep');

        $('.editToStep1, #steps .step-item[data-warning="true"]').on('click', function () {
            if ($(this).attr('href')) {
                navigateToStep($(this));
            }
            return false;
        });

        $('#edit-existing-equipment-section').on('click', function () {
            copyFormData($('#existing-equipment-section'),
                $('#existing-equipment-section-modal'),
                false);
            var eEModal = $('#existing-equipment-modal')
            eEModal.find('.responsible-input')
                .on('change', changeResponsibilityForRemovalOfExistingEquipment).change();
            eEModal.modal();
        });
        $('#edit-existing-equipment-submit').on('click', function () {
            var url = this.dataset['url'];
            var modal = $('#existing-equipment-section-modal');
            if (saveChanges(modal, $('#existing-equipment-section'), url, $('#existing-equipment-form'))) {
                $('#existing-equipment-modal').modal('hide');
                var input = modal.find('.responsible-input');
                var id = input.attr('id').split('-')[1];
                if (input.val() !== "3") {
                    $('#responsible-display-' + id).val(input.find(':selected').text());
                } else {
                    $('#responsible-display-' + id).val(modal.find('.other-input').val());
                }
            }
        });

        ga('send', 'event', 'Summary And Confirmation', 'button_click', 'Step 5 from Dealer Portal', '100');
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
    var packageSum = 0;
    var isClarity = $('#clarity-dealer').val().toLowerCase() === 'true';
    var isOldClarityDeal = $('#old-clarity-deal').val().toLowerCase() === 'true';
    if (isClarity && !isOldClarityDeal) {
        $(".monthly-cost").each(function () {
            var numberValue = parseFloat(this.value);
            if (!isNaN(numberValue)) {
                sum += numberValue;
            }
        });
        $('.package-cost').each(function () {
            var numberValue = parseFloat(this.value);
            if (!isNaN(numberValue)) {
                packageSum += numberValue;
            }
        });
        $("#package-cash-price").text(formatNumber(packageSum));

    } else {
        $(".equipment-cost").each(function () {
            var numberValue = parseFloat(this.value);
            if (!isNaN(numberValue)) {
                sum += numberValue;
            }
        });
    }

    $("#equipment-cash-price").text(formatNumber(sum));

    if (isClarity && !isOldClarityDeal) {
        calculateClarityTotalCashPrice();
    } else {
        calculateLoanValues();
    }
}

function checkTotalEquipmentCost() {
    var sum = 0;
    var isClarity = $('#clarity-dealer').val().toLowerCase() === 'true';
    var isOldClarityDeal = $('#old-clarity-deal').val().toLowerCase() === 'true';

    if (isClarity && !isOldClarityDeal) {
        $(".monthly-cost").each(function () {
            var numberValue = parseFloat(this.value);
            if (!isNaN(numberValue)) {
                sum += numberValue;
            }
        });
    } else {
        $(".equipment-cost").each(function () {
            var numberValue = parseFloat(this.value);
            if (!isNaN(numberValue)) {
                sum += numberValue;
            }
        });
    }

    if (sum > creditAmount) {
        $('#new-equipment-validation-message').html('<span>' + translations['TotalCostGreaterThanAmount'] + '</span>');
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
        $('#address-info-validation-message').html("<span>" + translations['AfterProvinceChangeTotalMustBeGreater'] + "</span>");
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
    switch (agreementType) {
        case 0:
            calculateLoanValues();
            break;
        case 1:
        case 2:
            recalculateTotalMonthlyPayment();
            break;
    }
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

function calculateClarityTotalsAndRender(sum) {
    var hst = parseFloat($('#hst').text());
    var totalWithTax = sum + hst;

    $('#totalMonthlyCostNoTax').text(formatNumber(sum));
    $('#total-hst').text(formatNumber(hst));
    $('#totalMonthlyCostTax').text(formatNumber(totalWithTax));
}

function changeResponsibilityForRemovalOfExistingEquipment() {
    var mvcId = this.id;
    var id = mvcId.split('-')[1];
    var col = $('#other-col-' + id);
    var input = $('#other-' + id);
    if (this.value === '3') {
        col.removeClass('hidden');
        input.attr('disabled', false);
        input[0].form && input.rules('add', 'required');
    } else {
        col.addClass('hidden');
        input.attr('disabled', true);
        input[0].form && input.rules('remove', 'required');
    }
}