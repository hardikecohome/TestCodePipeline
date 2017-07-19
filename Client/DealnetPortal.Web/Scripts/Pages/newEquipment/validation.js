module.exports('validation', function (require) {
    var validateOnSelect = function (reset) {
        var isValid = ['CustomCRate', 'CustomAmortTerm', 'CustomLoanTerm', 'CustomYCostVal', 'CustomAFee'].reduce(function (acc, field) {
            var $field = $('#' + field);
            var valid = $field.valid();
            if (reset) {
                $field.removeClass('input-validation-error').parent().find('.field-validation-error').html('');
            }
            return valid && acc;
        }, true);

        return isValid;
    }

    return {
        validateCustomCard: validateOnSelect
    };
});