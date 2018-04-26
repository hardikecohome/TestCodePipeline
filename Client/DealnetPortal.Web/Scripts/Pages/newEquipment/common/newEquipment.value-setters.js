module.exports('value-setters', function (require) {

    var state = require('state').state;

    var settings = {
        customRateCardName: 'Custom',
        loanTermErrorId: '#amortLoanTermError',
        recalculateClarityValuesAndRender: {},
        recalculateAndRenderRentalValues: {},
        recalculateRentalTaxAndPrice: {},
        recalculateValuesAndRender: {}
    };

    var init = function(params) {
        if (!params.isClarity) {
            settings.recalculateValuesAndRender = params.recalculateValuesAndRender;
            settings.recalculateAndRenderRentalValues = params.recalculateAndRenderRentalValues;
            settings.recalculateRentalTaxAndPrice = params.recalculateRentalTaxAndPrice;
        } else {
            settings.recalculateClarityValuesAndRender = params.recalculateClarityValuesAndRender;
        }
    }
    var setAgreement = function (e) {
        state.agreementType = Number(e.target.value);
        if (state.agreementType === 1 || state.agreementType === 2) {
            settings.recalculateAndRenderRentalValues();
        } else {
            settings.recalculateValuesAndRender();
        }
    };

    var setLoanAmortTerm = function (optionKey) {
        return function (e) {
            var loanTerm  = e.target.value.split('/')[0];
            var amortTerm = e.target.value.split('/')[1];
            state[optionKey].LoanTerm = Globalize.parseNumber(loanTerm);
            state[optionKey].AmortizationTerm = Globalize.parseNumber(amortTerm);
            settings.recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setReductionCost = function(optionKey) {
        return function(e) {
            var intRate = +e.target.selectedOptions[0].getAttribute('intrate');
            var custRate = +e.target.selectedOptions[0].getAttribute('custRate');
            state[optionKey].InterestRateReduction = intRate;
            state[optionKey].CustomerReduction = custRate;
            settings.recalculateValuesAndRender([{ name: optionKey }]);
        }
    }
    var setLoanTerm = function (optionKey) {
        return function (e) {
            state[optionKey].LoanTerm = Globalize.parseNumber(e.target.value);

            if (optionKey === settings.customRateCardName) {
                validateLoanAmortTerm();
            }
            settings.recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setAmortTerm = function (optionKey) {
        return function (e) {
            state[optionKey].AmortizationTerm = Globalize.parseNumber(e.target.value);

            if (optionKey === settings.customRateCardName) {
                validateLoanAmortTerm();
            }

            settings.recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setAdminFeeIsCovered = function (isCovered) {
        state.isCoveredByCustomer = isCovered;

        settings.recalculateValuesAndRender();
    };

    var setDeferralPeriod = function (optionKey) {
        return function (e) {
            state[optionKey].DeferralPeriod = Globalize.parseNumber(e.target.value);
            settings.recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setCustomerRate = function (optionKey) {
        return function (e) {
            state[optionKey].CustomerRate = Globalize.parseNumber(e.target.value);
            settings.recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setYourCost = function (optionKey) {
        return function (e) {
            state[optionKey].yourCost = Globalize.parseNumber(e.target.value);
            settings.recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setCustomYourCost = function (optionKey) {
        return function (e) {
            state[optionKey].DealerCost = Globalize.parseNumber(e.target.value);
            settings.recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setAdminFee = function (optionKey) {
        return function (e) {
            state[optionKey].AdminFee = Number(e.target.value);
            settings.recalculateValuesAndRender([{ name: optionKey }]);
        };
    };

    var setDownPayment = function (e) {
        state.downPayment = Globalize.parseNumber(e.target.value);
        if (state.agreementType === 3) {
            settings.recalculateClarityValuesAndRender();
        } else {
            settings.recalculateValuesAndRender();
        }
    };

    var setRentalMPayment = function (e) {
        state.rentalMPayment = Globalize.parseNumber(e.target.value);
        settings.recalculateRentalTaxAndPrice();
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
        init: init,
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
        setCustomYourCost: setCustomYourCost,
        setAdminFeeIsCovered: setAdminFeeIsCovered,
        setReductionCost: setReductionCost
    };
});