module.exports('value-setters', function (require) {

    var state = require('state').state;

    var recalculateValuesAndRender = require('rate-cards').recalculateValuesAndRender;
    var recalculateAndRenderRentalValues = require('newEquipment.rental').recalculateAndRenderRentalValues;
    var recalculateRentalTaxAndPrice = require('newEquipment.rental').recalculateRentalTaxAndPrice;

    var settings = {
        customRateCardName: 'Custom',
        loanTermErrorId: '#amortLoanTermError'
    };

    var setAgreement = function (e) {
        state.agreementType = Number(e.target.value);
        if (state.agreementType === 1 || state.agreementType === 2) {
            recalculateAndRenderRentalValues();
        } else {
            recalculateValuesAndRender();
        }
    };

    var setLoanAmortTerm = function (optionKey) {
        return function (e) {
            var loanTerm  = e.target.value.split('/')[0];
            var amortTerm = e.target.value.split('/')[1];
            state[optionKey].LoanTerm = Globalize.parseNumber(loanTerm);
            state[optionKey].AmortizationTerm = Globalize.parseNumber(amortTerm);
            recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setLoanTerm = function (optionKey) {
        return function (e) {
            state[optionKey].LoanTerm = Globalize.parseNumber(e.target.value);

            if (optionKey === settings.customRateCardName) {
                validateLoanAmortTerm();
            }
            recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setAmortTerm = function (optionKey) {
        return function (e) {
            state[optionKey].AmortizationTerm = Globalize.parseNumber(e.target.value);

            if (optionKey === settings.customRateCardName) {
                validateLoanAmortTerm();
            }

            recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setDeferralPeriod = function (optionKey) {
        return function (e) {
            state[optionKey].DeferralPeriod = Globalize.parseNumber(e.target.value);
            recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setCustomerRate = function (optionKey) {
        return function (e) {
            state[optionKey].CustomerRate = Globalize.parseNumber(e.target.value);
            recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setYourCost = function (optionKey) {
        return function (e) {
            state[optionKey].yourCost = Globalize.parseNumber(e.target.value);
            recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setCustomYourCost = function (optionKey) {
        return function (e) {
            state[optionKey].DealerCost = Globalize.parseNumber(e.target.value);
            recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setAdminFee = function (optionKey) {
        return function (e) {
            state[optionKey].AdminFee = Number(e.target.value);
            recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setDownPayment = function (e) {
        state.downPayment = Globalize.parseNumber(e.target.value);
        recalculateValuesAndRender();
    };

    var setRentalMPayment = function (e) {
        state.rentalMPayment = Globalize.parseNumber(e.target.value);
        recalculateRentalTaxAndPrice();
    };

    function validateLoanAmortTerm() {
        var amortTerm = state[settings.customRateCardName].AmortizationTerm;
        var loanTerm = state[settings.customRateCardName].LoanTerm;
        if (typeof amortTerm === 'number' && typeof loanTerm === 'number') {
            var $errorSelector = $(settings.loanTermErrorId);

            if (loanTerm > amortTerm) {
                if ($errorSelector.is(':hidden')) {
                    $errorSelector.show();
                    $errorSelector.parent().find('input[type="text"]')
                        .addClass('input-validation-error')
                        .addClass('input-has-error');
                }
            } else {
                if ($errorSelector.is(':visible')) {
                    $errorSelector.hide();
                    $errorSelector.parent().find('input[type="text"]')
                        .removeClass('input-validation-error')
                        .removeClass('input-has-error');
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
        setRentalMPayment: setRentalMPayment,
        setCustomYourCost: setCustomYourCost
    };
});