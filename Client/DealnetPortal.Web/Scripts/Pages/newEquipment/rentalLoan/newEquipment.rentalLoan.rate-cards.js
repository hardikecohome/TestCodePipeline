'use strict';

module.exports('rate-cards', function (require) {
    var constants = require('state').constants;
    var state = require('state').state;
    var rateCardsCalculator = require('rateCards.index');
    var rateCardsRenderEngine = require('rateCards.render');

    var settings = {
        rateCardFields: {
            'MPayment': 'monthlyPayment',
            'CBorrowing': 'costOfBorrowing',
            'TAFinanced': 'totalAmountFinanced',
            'TMPayments': 'totalMonthlyPayments',
            'RBalance': 'residualBalance',
            'TObligation': 'totalObligation',
            'YCost': 'yourCost'
        },
        totalPriceFields: {
            'totalEquipmentPrice': 'equipmentSum',
            'tax': 'tax',
            'totalPrice': 'totalPrice'
        },
        displaySectionFields: {
            //'displayCustomerRate': 'CustomerRate',
            'displayTFinanced': 'totalAmountFinanced',
            'displayMPayment': 'monthlyPayment'
        },
        numberFields: ['equipmentSum', 'LoanTerm', 'AmortizationTerm', 'CustomerRate', 'DealerCost', 'AdminFee'],
        notCero: ['equipmentSum', 'LoanTerm', 'AmortizationTerm']
    }

    var idToValue = function (obj) {
        return function (id) {
            return obj.hasOwnProperty(id) ? obj[id] : '';
        };
    };

    var submitRateCard = function (option) {
        if (option === 'Deferral') {
            $('#LoanDeferralType').val($('#DeferralPeriodDropdown').val());
        } else {
            $('#LoanDeferralType').val('0');
        }

        //remove percentage symbol
        var amortizationTerm = $('#' + option + '-amortDropdown').val();
        var selectedValuescustom = $('#' + option + '-amortDropdown option:selected').text().split('/');
        var loanTerm = selectedValuescustom[0];
        var slicedCustomerRate = $('#' + option + 'CRate').text().slice(0, -2);
        var slicedAdminFee = $('#' + option + 'AFee').text().replace('$', '').trim();
        var slicedTotalMPayment = $('#' + option + 'TMPayments').text().substring(1);

        var cardType = $.grep(constants.rateCards, function(c) { return c.name === option; })[0].name;
        var deferralPeriod = option === 'Deferral' ? +$('#DeferralPeriodDropdown').val() : 0;
        var amortTerm = Number(amortizationTerm);
        var adminFee = slicedAdminFee.indexOf(',') > -1 ? Globalize.parseNumber(slicedAdminFee) : Number(slicedAdminFee);
        var customerRate = slicedCustomerRate.indexOf(',') > -1 ? Globalize.parseNumber(slicedCustomerRate) : Number(slicedCustomerRate);
        var filtred = rateCardsCalculator.getRateCardOnSubmit(cardType, deferralPeriod, amortTerm, adminFee, customerRate);

        if (filtred !== undefined) {
            $('#SelectedRateCardId').val(filtred.Id);
        }

        $('#AmortizationTerm').val(amortizationTerm);
        $('#LoanTerm').val(loanTerm);
        $('#total-monthly-payment').val(slicedTotalMPayment);
        $('#CustomerRate').val(slicedCustomerRate);
        $('#AdminFee').val(slicedAdminFee);
    };

    /**
     * recalculate all financial values for Loan agreement type
     * @param {Array<string>} options - object of options for recalucaltion, if empty recalculate all values on form 
     *  possible values [FixedRate,Deferral,NoInterst, Custom]
     * @returns {void} 
     */
    var recalculateValuesAndRender = function (options) {
        var optionsToCompute = constants.rateCards;

        if (options !== undefined && options.length > 0) {
            optionsToCompute = options;
        }

        var downPayment = $('#downPayment').val();
        state.downPayment = downPayment ? +downPayment : 0;

        var eSumData = rateCardsCalculator.calculateTotalPrice(state.equipments, state.downPayment, state.tax);

        rateCardsRenderEngine.renderTotalPrice('', eSumData);

        var selectedRateCard = $('#rateCardsBlock').find('div.checked').length > 0
            ? $('#rateCardsBlock').find('div.checked').find('#hidden-option').text()
            : '';

        if (selectedRateCard !== '') {
            if ($("#submit").hasClass('disabled')) {
                $('#submit').removeClass('disabled');
                $('#submit').parent().popover('destroy');
            }
        }

        optionsToCompute.forEach(function (option) {
            var rateCard = rateCardsCalculator.filterRateCard({ rateCardPlan: option.name });

            if (rateCard !== null && rateCard !== undefined) {

                state[option.name] = $.extend(true, {}, rateCard);
                state[option.name].yourCost = '';

                rateCardsRenderEngine.renderAfterFiltration(option.name, { deferralPeriod: state[option.name].DeferralPeriod, adminFee: state[option.name].AdminFee, dealerCost: state[option.name].DealerCost, customerRate: state[option.name].CustomerRate });
            }
            if (option.name !== 'Custom') {
                rateCardsRenderEngine.renderDropdownValues({ rateCardPlan: option.name });
            }

            var data = rateCardsCalculator.calculateValuesForRender($.extend({}, idToValue(state)(option.name)));

            rateCardsRenderEngine.renderOption(option.name, selectedRateCard, data);
        });
    };

    var init = function() {
        rateCardsRenderEngine.init(settings);
    }

    return {
        recalculateValuesAndRender: recalculateValuesAndRender,
        submitRateCard: submitRateCard,
        init: init
    };
});