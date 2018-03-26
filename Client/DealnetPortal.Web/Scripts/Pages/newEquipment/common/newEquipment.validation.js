module.exports('validation', function (require) {

    var settings = {
        customLoanTermId: '#CustomLoanTerm',
        customAmortTermId: '#CustomAmortTerm',
        customDeferralPeriodId: '#CustomDeferralPeriod',
        customCustomerRateId: '#CustomCRate',
        customYearCostId: '#CustomYCostVal',
        customAdminFeeId: '#CustomAFee',
        amortizationTermId: '#AmortizationTerm',
        loanTermId: '#LoanTerm'
    }

    var validateOnSelect = function (reset) {
        var isValid = ['CustomCRate', 'CustomAmortTerm', 'CustomLoanTerm', 'CustomYCostVal' ].reduce(function (acc, field) {
            var $field = $('#' + field);
            var valid = $field.valid();
            if (reset) {
                $field.removeClass('input-validation-error').parent().find('.field-validation-error').html('');
            }
            return valid && acc;
        }, true);

        return isValid;
    };

    var initValidators = function () {
        $(settings.customYearCostId).rules('add', {
            required: true,
            regex: /(^[0]?|(^[1-9]\d{0,1}))([.,][0-9]{1,2})?$/,
            number: true,
            min: 0,
            messages: {
                regex: translations.yourCostFormat,
                required: function (ele) {
                    if (!$(settings.customCustomerRateId).val())
                        return translations.ThisFieldIsRequired;
                    return translations.enterZero;
                }
            }
        });

        $(settings.customCustomerRateId).rules('add', {
            required: true,
            regex: /(^[0]?|(^[1-9]\d{0,1}))([.,][0-9]{1,2})?$/,
            min: 0,
            number: true,
            messages: {
                regex: translations.customerRateFormat,
                required: function (ele) {
                    if (!$(settings.customYearCostId).val())
                        return translations.ThisFieldIsRequired;
                    return translations.enterZero;
                }
            }
        });

        //$(settings.customAdminFeeId).rules('add', {
        //    regex: /(^[0]?|(^[1-9]\d{0,11}))([.,][0-9]{1,2})?$/,
        //    number: true,
        //    min: 0,
        //    messages: {
        //        regex: translations.adminFeeFormat
        //    }
        //});

        $(settings.customAmortTermId).rules('add', {
            required: true,
            regex: /^[1-9]\d{0,2}?$/,
            min: 1,
            max: 999,
            messages: {
                required: translations.ThisFieldIsRequired,
                regex: translations.amortTermFormat,
                max: translations.amortTermMax
            }
        });

        $(settings.customLoanTermId).rules('add', {
            required: true,
            regex: /^[1-9]\d{0,2}?$/,
            min: 1,
            max: 999,
            messages: {
                required: translations.ThisFieldIsRequired,
                regex: translations.loanTermFormat,
                max: translations.loanTermMax
            }
        });
    }

    return {
        validateCustomCard: validateOnSelect,
        initValidators: initValidators
    };
});