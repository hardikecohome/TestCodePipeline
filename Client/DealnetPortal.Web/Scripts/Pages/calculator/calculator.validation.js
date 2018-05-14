module.exports('calculator.validation', function (require) {
    var setValidationForOption = function(option) {
        $('#' + option + '-downPayment').rules('add', {
            regex: /(^[0]?|(^[1-9]\d{0,11}))([.,][0-9]{1,2})?$/,
            messages: {
                regex: translations.downPaymentInvalidFormat
            }
        });
    }

    return {
        setValidationForOption: setValidationForOption
    }
})