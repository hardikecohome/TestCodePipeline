module.exports('custom-rate-card', function (require) {
    var setters = require('value-setters');
    var state = require('state').state;
    var constants = require('state').constants;
    var validation = require('validation');

    var settings = Object.freeze({
        customRateCardName: 'Custom',
        amortizationTermId: '#AmortizationTerm',
        loanTermId: '#LoanTerm',
        customerRateId: '#CustomerRate',
        adminFeeId: '#AdminFee',
        delaerCostId: '#DealerCost',
        totalMonthlyPaymentId: '#total-monthly-payment',
        loanDeferralTypeId: '#LoanDeferralType',
        amortLoanTermErrorId: '#amortLoanTermError',
        customLoanTermId: '#CustomLoanTerm',
        customAmortTermId: '#CustomAmortTerm',
        customDeferralPeriodId: '#CustomDeferralPeriod',
        customCustomerRateId: '#CustomCRate',
        customYearCostId: '#CustomYCostVal',
        customAdminFeeId: '#CustomAFee',
        selectedRateCardId: '#SelectedRateCardId',
        submitButtonId: '#submit',
        rateCardBlockId: '#rateCardsBlock',
        hiddenOptionId: '#hidden-option',
        disableInputsArr: ['CustomCRate', 'CustomAmortTerm', 'CustomLoanTerm', 'CustomYCostVal', 'CustomAFee']
    });

    var toggleDisableClassOnInputs = function(isDisable) {
        settings.disableInputsArr.forEach(function(field) { $('#' + field).prop('disabled', isDisable); });
    };

    var setAdminFeeByEquipmentSum = function(eSum) {
        if($.isEmptyObject(state.customRateCardBoundaires)) return;
        var keys = Object.keys(state.customRateCardBoundaires);
        for (var i = 0; i < keys.length; i++) {

            var numbers = keys[i].split('-');
            var lowBound = +numbers[0];
            var highBound = +numbers[1];

            if (eSum <= lowBound || eSum >= highBound) {
                state[settings.customRateCardName].AdminFee = 0;
            }

            if (lowBound <= eSum && highBound >= eSum) {
                state[settings.customRateCardName].AdminFee = +state.customRateCardBoundaires[keys[i]].adminFee;
                break;
            }
        }
    }

    /**
     * On form submit takes values from state and set them to inputs
     * @param {any} event parent javascript event
     * @param {string} option custom rate card name
     */
    var submitCustomRateCard = function(event, option) {

        if ($(settings.amortLoanTermErrorId).is(':visible')) {
            event.preventDefault();
        }

        var customSlicedTotalMPayment = $('#' + option + 'TMPayments').text().replace('$', '').trim();

        $(settings.amortizationTermId).val(state[option].AmortizationTerm);
        $(settings.loanTermId).val(state[option].LoanTerm);
        $(settings.customerRateId).val(state[option].CustomerRate);
        $(settings.adminFeeId).val(state[option].AdminFee);
        $(settings.delaerCostId).val(state[option].DealerCost);
        $(settings.totalMonthlyPaymentId).val(customSlicedTotalMPayment);
        $(settings.loanDeferralTypeId).val(state[option].DeferralPeriod);
        $(settings.selectedRateCardId).val(null);
    };

    /**
     * populate custom rate cards with values
     * @returns {void} 
     */
    var setSelectedCustomRateCard = function () {
        var deferralPeriod = $.grep(constants.customDeferralPeriods, function (period) { return period.name === $(settings.loanDeferralTypeId).val(); })[0];

        state[settings.customRateCardName].LoanTerm = Number($(settings.loanTermId).val());
        state[settings.customRateCardName].AmortizationTerm = Number($(settings.amortizationTermId).val());
        state[settings.customRateCardName].DeferralPeriod = deferralPeriod === undefined ? 0 : deferralPeriod.val;
        state[settings.customRateCardName].CustomerRate = Number($(settings.customerRateId).val());
        state[settings.customRateCardName].AdminFee = Number($(settings.adminFeeId).val());
        state[settings.customRateCardName].DealerCost = Number($(settings.delaerCostId).val());

        $(settings.customLoanTermId).val(state[settings.customRateCardName].LoanTerm);
        $(settings.customAmortTermId).val(state[settings.customRateCardName].AmortizationTerm);
        $(settings.customDeferralPeriodId).val(state[settings.customRateCardName].DeferralPeriod);
        $(settings.customCustomerRateId).val(state[settings.customRateCardName].CustomerRate);
        $(settings.customAdminFeeId).val(state[settings.customRateCardName].AdminFee);
        $(settings.customYearCostId).val(state[settings.customRateCardName].DealerCost);
    }

    /**
     * Check whether input field is valid or not, and disable submit button
     * works for ['#CustomAmortTerm', '#CustomDeferralPeriod', '#CustomCRate', '#CustomYCostVal']
     */
    function validateCustomRateCardOnInput() {
        var $submitBtnSelector = $(settings.submitButtonId);
        var selectedRateCard = $(settings.rateCardBlockId).find('div.checked').length > 0
            ? $(settings.rateCardBlockId).find('div.checked').find(settings.hiddenOptionId).text()
            : '';
        if (state.onlyCustomRateCard) {
            selectedRateCard = settings.customRateCardName;
        }

        if (!validation.validateCustomCard() && selectedRateCard === settings.customRateCardName) {
            if (!$submitBtnSelector.hasClass('disabled')) {
                $submitBtnSelector.addClass('disabled');
                $submitBtnSelector.parent().popover();
            }
        } else {
            if ($submitBtnSelector.hasClass('disabled') && selectedRateCard === settings.customRateCardName) {
                $submitBtnSelector.removeClass('disabled');
                $submitBtnSelector.parent().popover('destroy');
            }
        }
        $(window).resize();
    }

    function _initHandlers() {
        $(settings.customLoanTermId).on('change', setters.setLoanTerm(settings.customRateCardName));
        $(settings.customLoanTermId).on('change keyup', validateCustomRateCardOnInput);
        $(settings.customAmortTermId).on('change', setters.setAmortTerm(settings.customRateCardName));
        $(settings.customAmortTermId).on('change keyup', validateCustomRateCardOnInput);
        $(settings.customDeferralPeriodId).on('change', setters.setDeferralPeriod(settings.customRateCardName));
        $(settings.customCustomerRateId).on('change', setters.setCustomerRate(settings.customRateCardName));
        $(settings.customCustomerRateId).on('change keyup', validateCustomRateCardOnInput);
        $(settings.customYearCostId).on('change', setters.setCustomYourCost(settings.customRateCardName));
        $(settings.customYearCostId).on('change keyup', validateCustomRateCardOnInput);
        $(settings.customAdminFeeId).on('change', setters.setAdminFee(settings.customRateCardName));
    }

    var init = function() {
        validation.initValidators();
        _initHandlers();
    }

    return {
        init: init,
        setAdminFeeByEquipmentSum: setAdminFeeByEquipmentSum,
        validateCustomCard: validation.validateCustomCard,
        submitCustomRateCard: submitCustomRateCard,
        toggleDisableClassOnInputs: toggleDisableClassOnInputs,
        setSelectedCustomRateCard: setSelectedCustomRateCard
    }
})