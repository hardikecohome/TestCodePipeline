module.exports('custom-rate-card', function (require) {
    var setters = require('value-setters');
    var state = require('state').state;

    var validateOnSelect = function () {
        var isValid = ['CustomCRate', 'CustomAmortTerm', 'CustomLoanTerm'].every(function (field) {
            return $("#" + field).valid();
        });

        if (isValid) { return true; }

        var panel = $(this).parent();
        $.grep(panel.find('input[type="text"]'), function (inp) {
            var input = $(inp);
            if (input.val() === '' && !(input.is('#CustomYCostVal') || input.is('#CustomAFee'))) {
                input.addClass('input-validation-error');
            }
        });

        return false;
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
        $('#total-monthly-payment').val(customSlicedTotalMPayment);
        if (state[option].DeferralPeriod === '')
            $('#LoanDeferralType').val(state[option].DeferralPeriod);

        $('#SelectedRateCardId').val(0);
    }

    // custom option
    $('#CustomLoanTerm').on('change', setters.setLoanTerm('Custom'));
    $('#CustomLoanTerm').on('change', validateCustomRateCardOnInput);

    $('#CustomAmortTerm').on('change', setters.setAmortTerm('Custom'));
    $('#CustomAmortTerm').on('change', validateCustomRateCardOnInput);

    $('#CustomDeferralPeriod').on('change', setters.setDeferralPeriod('Custom'));
    $('#CustomCRate').on('change', setters.setCustomerRate('Custom'));
    $('#CustomCRate').on('change', validateCustomRateCardOnInput);

    $('#CustomYCostVal').on('change', setters.setYourCost('Custom'));
    $('#CustomYCostVal').on('change keyup', numericHandler);

    $('#CustomAFee').on('change', setters.setAdminFee('Custom'));
    $('#CustomAFee').on('change keyup', numericHandler);

    function validateCustomRateCardOnInput() {
        var submit = $('#submit');
        var selectedRateCard = $('#rateCardsBlock').find('div.checked').length > 0
            ? $('#rateCardsBlock').find('div.checked').find('#hidden-option').text()
            : '';

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

    function numericHandler() {
        // Remove invalid characters
        var sanitized = $(this).val().replace(/[^-.0-9]/g, '');
        // Remove non-leading minus signs
        sanitized = sanitized.replace(/(.)-+/g, '$1');
        // Remove the first point if there is more than one
        sanitized = sanitized.replace(/\.(?=.*\.)/g, '');

        $(this).val(sanitized);
    }

    $('#CustomYCostVal').rules('add', {
        number: true,
        minlength: 0
    });

    $('#CustomAFee').rules('add', {
        number: true,
        minlength: 0
    });

    $('#CustomCRate').rules('add', {
        required: true,
        minlength: 1,
        regex: /^[0-9]\d{0,11}([.,][0-9][0-9]?)?$/
    });

    $('#CustomAmortTerm').rules('add', {
        required: true,
        minlength: 1,
        regex: /^[0-9]\d{0,11}([.,][0-9][0-9]?)?$/
    });

    $('#CustomLoanTerm').rules('add', {
        required: true,
        minlength: 1,
        regex: /^[0-9]\d{0,11}([.,][0-9][0-9]?)?$/
    });

    return {
        validateOnSelect: validateOnSelect,
        submitCustomRateCard: submitCustomRateCard
    }
})