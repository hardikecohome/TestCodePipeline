module.exports('value-setters', function (require) {

    var state = require('rate-cards').state;
    var recalculateValuesAndRender = require('rate-cards').recalculateValuesAndRender;
    var recalculateAndRenderRentalValues = require('rate-cards').recalculateAndRenderRentalValues;
    var recalculateRentalTaxAndPrice = require('rate-cards').recalculateRentalTaxAndPrice;

    var setAgreement = function (e) {
        state.agreementType = Number(e.target.value);
        if (state.agreementType === 1 || state.agreementType === 2) {
            recalculateAndRenderRentalValues();
        } else {
            recalculateValuesAndRender();
        }
    };

    var setLoanAmortTerm = function (optionKey) {
        return function (loanTerm, amortTerm) {
            state[optionKey].LoanTerm = parseFloat(loanTerm);
            state[optionKey].AmortizationTerm = parseFloat(amortTerm);
            recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setLoanTerm = function (optionKey) {
        return function (e) {
            state[optionKey].LoanTerm = parseFloat(e.target.value);

            if (optionKey === 'Custom') {
                validateLoanAmortTerm();
            }
            recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setAmortTerm = function (optionKey) {
        return function (e) {
            state[optionKey].AmortizationTerm = parseFloat(e.target.value);

            if (optionKey === 'Custom') {
                validateLoanAmortTerm();
            }

            recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setDeferralPeriod = function (optionKey) {
        return function (e) {
            state[optionKey].DeferralPeriod = parseFloat(e.target.value);
            recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setCustomerRate = function (optionKey) {
        return function (e) {
            state[optionKey].CustomerRate = parseFloat(e.target.value);
            recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setYourCost = function (optionKey) {
        return function (e) {
            state[optionKey].yourCost = parseFloat(e.target.value);
            recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setAdminFee = function (optionKey) {
        return function (e) {
            state[optionKey].AdminFee = parseFloat(e.target.value);
            recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setDownPayment = function (e) {
        state.downPayment = parseFloat(e.target.value);
        recalculateValuesAndRender();
    };

    var setRentalMPayment = function (e) {
        state.rentalMPayment = parseFloat(e.target.value);
        recalculateRentalTaxAndPrice();
    };

    function validateLoanAmortTerm() {
        var amortTerm = state['Custom'].AmortizationTerm;
        var loanTerm = state['Custom'].LoanTerm;
        if (typeof amortTerm == 'number' && typeof loanTerm == 'number') {
            if (loanTerm > amortTerm) {
                if ($('#amortLoanTermError').is(':hidden')) {
                    $('#amortLoanTermError').show();
                    $('#amortLoanTermError').parent().find('input[type="text"]')
                        .addClass('input-validation-error');
                }
            } else {
                if ($('#amortLoanTermError').is(':visible')) {
                    $('#amortLoanTermError').hide();
                    $('#amortLoanTermError').parent().find('input[type="text"]')
                        .removeClass('input-validation-error');
                }
            }
        }
    }

    return {
        setAgreement: setAgreement,
        setLoanAmortTerm: setLoanAmortTerm,
        setLoanTerm: setLoanTerm,
        setAmortTerm: setAmortTerm,
        setDeferralPeriod: setDeferralPeriod,
        setCustomerRate: setCustomerRate,
        setYourCost: setYourCost,
        setAdminFee: setAdminFee,
        setDownPayment: setDownPayment,
        setRentalMPayment: setRentalMPayment
    }
})