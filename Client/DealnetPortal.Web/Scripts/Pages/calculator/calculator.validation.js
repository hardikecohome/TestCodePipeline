module.exports('calculator.validation', function (require) {
    var settings = {
        
    }
    
    var setValidationForOption = function(option) {
        $('#' + option + '-downPayment').rules('add', {
            regex: /(^[0]?|(^[1-9]\d{0,11}))([.,][0-9]{1,2})?$/,
            messages: {
                regex: translations.downPaymentInvalidFormat
            }
        });

        $('#' + option + '-customYCostVal').rules('add', {
            required: true,
            regex: /(^[0]?|(^[1-9]\d{0,1}))([.,][0-9]{1,2})?$/,
            number: true,
            min: 0,
            messages: {
                regex: translations.yourCostFormat,
                required: function (ele) {
                    if (!$('#' + option + '-customCRate').val())
                        return translations.ThisFieldIsRequired;
                    return translations.enterZero;
                }
            }
        });

        $('#' + option + '-customCRate').rules('add', {
            required: true,
            regex: /(^[0]?|(^[1-9]\d{0,1}))([.,][0-9]{1,2})?$/,
            min: 0,
            number: true,
            messages: {
                regex: translations.customerRateFormat,
                required: function (ele) {
                    if (!$('#' + option + '-customYCostVal').val())
                        return translations.ThisFieldIsRequired;
                    return translations.enterZero;
                }
            }
        });

        $('#' + option + '-customAmortTerm').rules('add', {
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

        $('#' + option + '-customLoanTerm').rules('add', {
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
        setValidationForOption: setValidationForOption
    }
})