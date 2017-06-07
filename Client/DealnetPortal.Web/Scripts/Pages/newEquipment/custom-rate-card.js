module.exports('custom-rate-card', function (require) {
    var setters = require('value-setters');

    var setValidationOnCustomRateCard = function() {
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
    }

    var validateOnSelect = function () {
        var isValid = ['CustomCRate', 'CustomAmortTerm', 'CustomLoanTerm'].every(function (field) {
            return $("#" + field).valid();
        });

        return isValid;
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

        if (!validateOnSelect()) {
            if (!submit.hasClass('disabled')) {
                submit.addClass('disabled');
            }
        } else {
            if (submit.hasClass('disabled')) {
                submit.removeClass('disabled');
            }
        }
    }
    function numericHandler() {
        // Remove invalid characters
        var sanitized = $(this).val().replace(/[^0-9]/g, '');
        // Update value
        $(this).val(sanitized);
    }

    return {
        setValidationOnCustomRateCard: setValidationOnCustomRateCard
    }
})