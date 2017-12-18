'use strict';

module.exports('rate-cards', function (require) {
    var tax = require('financial-functions').tax;
    var constants = require('state').constants;
    var state = require('state').state;
	var totalRentalPrice = require('financial-functions').totalRentalPrice;
    var notNaN = function (num) { return !isNaN(num); };
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
            'displayCustomerRate': 'CustomerRate',
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
        var cards = sessionStorage.getItem(state.contractId + option);
        if (cards !== null) {
            var cardType = $.grep(constants.rateCards, function(c) { return c.name === option; })[0].id;
            var filtred;

            if (option === 'Deferral') {
                var deferralPeriod = +$('#DeferralPeriodDropdown').val();
                filtred = $.parseJSON(cards).filter(
                    function (v) {
                        return v.CardType === cardType &&
                            v.DeferralPeriod === deferralPeriod &&
                            v.AmortizationTerm === Number(amortizationTerm) &&
                            v.AdminFee === (slicedAdminFee.indexOf(',') > -1 ? Globalize.parseNumber(slicedAdminFee) : Number(slicedAdminFee)) &&
                            v.CustomerRate === (slicedCustomerRate.indexOf(',') > -1 ? Globalize.parseNumber(slicedCustomerRate) : Number(slicedCustomerRate));
                    })[0];
            } else {
                filtred = $.parseJSON(cards).filter(
                    function (v) {
                        return v.CardType === cardType &&
                            v.AmortizationTerm === Number(amortizationTerm) &&
                            v.AdminFee === (slicedAdminFee.indexOf(',') > -1 ? Globalize.parseNumber(slicedAdminFee) : Number(slicedAdminFee)) &&
                            v.CustomerRate === (slicedCustomerRate.indexOf(',') > -1 ? Globalize.parseNumber(slicedCustomerRate) : Number(slicedCustomerRate));
                    })[0];
            }

            if (filtred !== undefined) {
                $('#SelectedRateCardId').val(filtred.Id);
            }
        }

        $('#AmortizationTerm').val(amortizationTerm);
        $('#LoanTerm').val(loanTerm);
        $('#total-monthly-payment').val(slicedTotalMPayment);
        $('#CustomerRate').val(slicedCustomerRate);
        $('#AdminFee').val(slicedAdminFee);
    };

    var monthlySum = function (equipments) {
        return Object.keys(equipments)
        .map(idToValue(equipments))
        .map(function (equipment) { return parseFloat(equipment.monthlyCost); })
        .filter(notNaN)
        .reduce(function (sum, cost) { return sum + cost;},0);
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

        rateCardsRenderEngine.renderTotalPrice(eSumData);

        optionsToCompute.forEach(function (option) {
            var rateCard = rateCardsCalculator.filterRateCard({ rateCardPlan: option.name });

            if (rateCard !== null && rateCard !== undefined) {

                state[option.name] = rateCard;
                state[option.name].yourCost = '';

                rateCardsRenderEngine.renderAfterFiltration(option.name, { deferralPeriod: state[option.name].DeferralPeriod, adminFee: state[option.name].AdminFee, dealerCost: state[option.name].DealerCost, customerRate: state[option.name].CustomerRate });
            }
            if (option.name !== 'Custom') {
                rateCardsRenderEngine.renderDropdownValues({ rateCardPlan: option.name });
            }

            var data = rateCardsCalculator.calculateValuesForRender($.extend({}, idToValue(state)(option.name)));
            var selectedRateCard = $('#rateCardsBlock').find('div.checked').length > 0
                ? $('#rateCardsBlock').find('div.checked').find('#hidden-option').text()
                : '';

            rateCardsRenderEngine.renderOption(option.name, selectedRateCard, data);
        });
    };

    /**
     * recalculate all financial values for Rental/RentalHwt agreement type
     * @returns {void} 
     */
    var recalculateAndRenderRentalValues = function () {
        var eSum = monthlySum(state.equipments);
        
        var data = {
            tax: state.tax,
            equipmentSum: eSum
        };

        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
        if (notNan && data.equipmentSum !== 0) {
            $('#total-monthly-payment').val(formatNumber(eSum));
            $('#rentalTax').text(formatNumber(tax(data)));
            $('#rentalTMPayment').text(formatNumber(totalRentalPrice(data)));
        } else {
            $('#total-monthly-payment').val('');
            $('#rentalTax').text('-');
            $('#rentalTMPayment').text('-');
        }
    };

    var recalculateRentalTaxAndPrice = function () {
        var data = {
            tax: state.tax,
            equipmentSum: state.rentalMPayment
        };

        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
        if (notNan && data.equipmentSum !== 0) {
            $('#rentalTax').text(formatNumber(tax(data)));
            $('#rentalTMPayment').text(formatNumber(totalRentalPrice(data)));
        } else {
            $('#rentalTax').text('-');
            $('#rentalTMPayment').text('-');
        }
    };

    var init = function() {
        rateCardsRenderEngine.init(settings);
    }

    return {
        recalculateValuesAndRender: recalculateValuesAndRender,
        recalculateAndRenderRentalValues: recalculateAndRenderRentalValues,
        recalculateRentalTaxAndPrice: recalculateRentalTaxAndPrice,
        submitRateCard: submitRateCard,
        init: init
    };
});