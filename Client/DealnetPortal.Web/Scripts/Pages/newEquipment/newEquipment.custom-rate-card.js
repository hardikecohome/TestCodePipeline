module.exports('custom-rate-card', function (require) {
    var setters = require('value-setters');
    var state = require('state').state;

    var validateOnSelect = require('validation').validateCustomCard;

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

        $(settings.selectedRateCardId).val(0);
    };

    function validateCustomRateCardOnInput() {
        var submit = $(settings.submitButtonId);
        var selectedRateCard = $(settings.rateCardBlockId).find('div.checked').length > 0
            ? $(settings.rateCardBlockId).find('div.checked').find(settings.hiddenOptionId).text()
            : '';
        if (state.onlyCustomRateCard) {
            selectedRateCard = settings.customRateCardName;
        }

        if (!validateOnSelect() && selectedRateCard === settings.customRateCardName) {
            if (!submit.hasClass('disabled')) {
                submit.addClass('disabled');
                submit.parent().popover();
            }
        } else {
            if (submit.hasClass('disabled') && selectedRateCard === settings.customRateCardName) {
                submit.removeClass('disabled');
                submit.parent().popover('destroy');
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

    function _initValidators() {
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

        $(settings.customAdminFeeId).rules('add', {
            regex: /(^[0]?|(^[1-9]\d{0,11}))([.,][0-9]{1,2})?$/,
            number: true,
            min: 0,
            messages: {
                regex: translations.adminFeeFormat
            }
        });

        $(settings.amortizationTermId).rules('add', {
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

        $(settings.loanTermId).rules('add', {
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

    var init = function() {
        _initValidators();
        _initHandlers();
    }

    return {
        init: init,
        validateOnSelect: validateOnSelect,
        submitCustomRateCard: submitCustomRateCard, 
        toggleDisableClassOnInputs: toggleDisableClassOnInputs
    }
})