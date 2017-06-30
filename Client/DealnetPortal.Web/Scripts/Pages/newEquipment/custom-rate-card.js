module.exports('custom-rate-card', function (require) {
    var setters = require('value-setters');
    var state = require('state').state;

    var validateOnSelect = function () {
        var isValid = ['CustomCRate', 'CustomAmortTerm', 'CustomLoanTerm', 'CustomYCostVal', 'CustomAFee'].reduce(function (acc, field) {
            var valid = $("#" + field).valid();
            return valid && acc;
        }, true);

        return isValid;
    }

    var toggleDisableClassOnInputs = function (isDisable) {
        ['CustomCRate', 'CustomAmortTerm', 'CustomLoanTerm', 'CustomYCostVal', 'CustomAFee'].forEach(function (field) {
            $('#' + field).prop('disabled', isDisable);
        });
    }

    var submitCustomRateCard = function (event, option) {

        if ($('#amortLoanTermError').is(':visible')) {
            event.preventDefault();
        }

        var customSlicedTotalMPayment = $('#' + option + 'TMPayments').text().substring(1);

        $('#AmortizationTerm').val(state[option].AmortizationTerm);
        $('#LoanTerm').val(state[option].LoanTerm);
        $('#CustomerRate').val(state[option].CustomerRate);
        $('#AdminFee').val(state[option].AdminFee);
        $('#DealerCost').val(state[option].DealerCost);
        $('#total-monthly-payment').val(customSlicedTotalMPayment);
        $('#LoanDeferralType').val(state[option].DeferralPeriod);

        $('#SelectedRateCardId').val(0);
    }

    // custom option
    $('#CustomLoanTerm').on('change', setters.setLoanTerm('Custom'));
    $('#CustomLoanTerm').on('change keyup', validateCustomRateCardOnInput);

    $('#CustomAmortTerm').on('change', setters.setAmortTerm('Custom'));
    $('#CustomAmortTerm').on('change keyup', validateCustomRateCardOnInput);

    $('#CustomDeferralPeriod').on('change', setters.setDeferralPeriod('Custom'));
    $('#CustomCRate').on('change', setters.setCustomerRate('Custom'));
    $('#CustomCRate').on('change', validateCustomRateCardOnInput);

    $('#CustomYCostVal').on('change', setters.setCustomYourCost('Custom'));

    $('#CustomAFee').on('change', setters.setAdminFee('Custom'));

    function validateCustomRateCardOnInput() {
        var submit = $('#submit');
        var selectedRateCard = $('#rateCardsBlock').find('div.checked').length > 0
            ? $('#rateCardsBlock').find('div.checked').find('#hidden-option').text()
            : '';
        if (onlyCustomCard) {
            selectedRateCard = 'Custom';
        }

        if (!validateOnSelect() && selectedRateCard === 'Custom') {
            if (!submit.hasClass('disabled')) {
                submit.addClass('disabled');
                submit.parent().popover();
            }
        } else {
            if (submit.hasClass('disabled') && selectedRateCard === 'Custom') {
                submit.removeClass('disabled');
                submit.parent().popover('destroy');
            }
        }
    }

    $('#CustomYCostVal').rules('add', {
        required: true,
        regex: /^[0-9]\d{0,11}([.,][0-9][0-9]?)?$/,
        number: true,
        min:0,
        messages: {
            regex:translations.yourCostFormat,
            required: function (ele) {
                if (!$('#CustomCRate').val())
                    return translations.customerOrYourCost;
                return translations.enterZero;
            }
        }
    });

    $('#CustomCRate').rules('add', {
        required: true,
        regex: /^[0-9]\d{0,11}([.,][0-9][0-9]?)?$/,
        min:0,
        number: true,
        messages: {
            regex:translations.customerRateFormat,
            required: function (ele) {
                if (!$('#CustomYCostVal').val())
                    return translations.customerOrYourCost;
                return translations.enterZero;
            }
        }
    });

    $('#CustomAFee').rules('add', {
        regex:/(^[0]?|(^[1-9]\d{0,11}))([.,][0-9]{1,2})?$/,
        number: true,
        minlength: 0,
        messages: {
            regex:translations.adminFeeFormat
        }
    });

    $('#CustomAmortTerm').rules('add', {
        required: true,
        regex: /^[1-9]\d{0,2}?$/,
        min: 1,
        max:999,
        messages: {
            required: translations.ThisFieldIsRequired,
            regex: translations.amortTermFormat,
            max: translations.amortTermMax
        }
    });

    $('#CustomLoanTerm').rules('add', {
        required: true,
        regex: /^[1-9]\d{0,2}?$/,
        min: 1,
        max:999,
        messages: {
            required: translations.ThisFieldIsRequired,
            regex: translations.loanTermFormat,
            max: translations.loanTermMax
        }
    });

    return {
        validateOnSelect: validateOnSelect,
        submitCustomRateCard: submitCustomRateCard, 
        toggleDisableClassOnInputs: toggleDisableClassOnInputs
    }
})