module.exports('rateCards.index', function(require) {
    var totalObligation = require('financial-functions').totalObligation;
    var totalPrice = require('financial-functions').totalPrice;
    var totalAmountFinanced = require('financial-functions').totalAmountFinanced;
    var monthlyPayment = require('financial-functions').monthlyPayment;
    var totalMonthlyPayments = require('financial-functions').totalMonthlyPayments;
    var residualBalance = require('financial-functions').residualBalance;
    var totalBorrowingCost = require('financial-functions').totalBorrowingCost;
    var yourCost = require('financial-functions').yourCost;
    var tax = require('financial-functions').tax;
    var notNaN = function (num) { return !isNaN(num); };

    var state = require('rateCards.state').state;
    var constants = require('rateCards.state').constants;

    var idToValue = function (obj) {
        return function (id) {
            return obj.hasOwnProperty(id) ? obj[id] : '';
        };
    };
    
    var equipmentSum = function (equipments) {
        return Object.keys(equipments)
            .map(idToValue(equipments))
            .map(function (equipment) { return parseFloat(equipment.cost); })
            .filter(notNaN)
            .reduce(function (sum, cost) { return sum + cost; }, 0);
    };

    var init = function(ratecards) {
        constants.rateCards.forEach(function (option) { state.rateCards[option.name] = $.grep(ratecards, function (card) { return card.CardType === option.id; }); });
    }

    var getRateCardOnSubmit = function (cardType, deferral, amortTerm, adminFee, customerRate) {
        var items = state.rateCards[cardType];

        if (items === null)
            return undefined;

        return $.grep(items, function (i) {
            return i.DeferralPeriod === deferral && i.AmortizationTerm === amortTerm && i.AdminFee === adminFee && i.CustomerRate === customerRate;
        })[0];
    }

    var filterRateCard = function(dataObject) {
        var totalCash = constants.minimumLoanValue;
        var totalAmount = totalAmountFinanced($.extend({}, { equipmentSum: state.eSum, downPayment: state.downPayment }));
        state[dataObject.rateCardPlan] = $.extend({}, { totalAmountFinanced: totalAmount });

        if (!isNaN(totalAmount)) {
            if (totalAmount > totalCash) {
                totalCash = totalAmount.toFixed(2);
            }
        }

        return _filterRateCardByValues(dataObject, totalCash);
    }

    var calculateTotalPrice = function(equipments, downPayment, regionTax) {
        state.downPayment = downPayment;
        state.tax = regionTax;

        var eSum = equipmentSum(equipments);
        state.eSum = eSum;

        return {
            equipmentSum: eSum !== 0 ? eSum : '-',
            tax: eSum !== 0 ? tax({ equipmentSum: eSum, tax: state.tax }) : '-',
            totalPrice: eSum !== 0 ? totalPrice({ equipmentSum: eSum, tax: state.tax }) : '-'
        }
    }

    var calculateValuesForRender = function(data) {
        data = $.extend({}, data,
            {
                equipmentSum: state.eSum,
                downPayment: state.downPayment,
                tax: state.tax
            });

        return $.extend({}, data,
            {
                monthlyPayment: monthlyPayment(data),
                costOfBorrowing: totalBorrowingCost(data),
                totalAmountFinanced: totalAmountFinanced(data),
                totalMonthlyPayments: totalMonthlyPayments(data),
                residualBalance: residualBalance(data),
                totalObligation: totalObligation(data),
                yourCost: yourCost(data),
                loanTerm: data.LoanTerm,
                amortTerm: data.AmortizationTerm
            });
    }

    /**
     * Select reate cards by current values of dropdown and totalCash
     * @param {string} option - name of the cards [FixedRate,Deferral,NoInterst]
     * @param {number} totalCash - totalAmountFinancedValue for current option
     * @returns {Object<string>} - appropriate rate card object 
     */
    function _filterRateCardByValues(dataObject, totalCash) {
        var selectedValues;

        if (dataObject.hasOwnProperty('standaloneOption')) {
            selectedValues = $('#' + dataObject.standaloneOption + '-amortDropdown option:selected').text().split('/');
        } else {
            selectedValues = $('#' + dataObject.rateCardPlan + '-amortDropdown option:selected').text().split('/');
        }

        var items = state.rateCards[dataObject.rateCardPlan];

        if (!items)
            return null;

        var loanTerm = +selectedValues[0];
        var amortTerm = +selectedValues[1];
        var deferralPeriod = 0;
        if (dataObject.rateCardPlan === 'Deferral') {
            if ($('#DeferralPeriodDropdown').length) {
                deferralPeriod = +$('#DeferralPeriodDropdown').val();
            } else {
                deferralPeriod = +$('#' + dataObject.standaloneOption + '-deferralDropdown').val();
            }
        }

        return $.grep(items, function (i) {
            if (totalCash >= constants.maxRateCardLoanValue) {
                return i.DeferralPeriod === deferralPeriod && i.AmortizationTerm === amortTerm && i.LoanTerm === loanTerm && i.LoanValueFrom <= totalCash && i.LoanValueTo >= constants.maxRateCardLoanValue;
            } else {
                return i.DeferralPeriod === deferralPeriod && i.AmortizationTerm === amortTerm && i.LoanTerm === loanTerm && i.LoanValueFrom <= totalCash && i.LoanValueTo >= totalCash;
            }
        })[0];
    }

    return {
        init: init,
        filterRateCard: filterRateCard,
        calculateTotalPrice: calculateTotalPrice,
        getRateCardOnSubmit: getRateCardOnSubmit,
        calculateValuesForRender: calculateValuesForRender
    };
});