module.exports('newEquipment.clairty.calculation', function(require) {
    var rateCardsCalculator = require('rateCards.index');
    var state = require('state').state;
    var tax = require('financial-functions').tax;
    var totalRentalPrice = require('financial-functions').totalRentalPrice;

    var settings = {
        rateCardCurrencyFields: {
            'displayCostOfBorrow': 'costOfBorrowing',
            'displayTotalAmount': 'totalAmountFinanced',
            'displayAllMonthly': 'totalMonthlyPayments',
            'displayTotalObl': 'totalObligation',
            'displayYourCost': 'yourCost',
            'displayBalanceOwning': 'residualBalance'
        },
        rateCardPercentageFields: {
            'displayCustRate': 'customerRate',
            'displayDealerCost': 'DealerCost'
        },
        rateCardMonthsFields: {
            'displayLoanTerm': 'loanTerm',
            'displayAmortTem': 'amortTerm'
        },
        totalPriceFields: {
            'totalEquipmentPrice': 'equipmentSum',
            'tax': 'tax',
            'totalPrice': 'totalPrice'
        },
        displaySectionFields: {
            'customerRate': 'CustomerRate',
            'total-amount-financed': 'totalAmountFinanced',
            'loanAmortTerm': 'loanAmortTerm'

        },

        numberFields: ['equipmentSum', 'LoanTerm', 'AmortizationTerm', 'CustomerRate', 'DealerCost', 'AdminFee'],
        notCero: ['equipmentSum', 'LoanTerm', 'AmortizationTerm']
    }

    var init = function() {

    }

    var idToValue = function (obj) {
        return function (id) {
            return obj.hasOwnProperty(id) ? obj[id] : '';
        };
    };

    var notNaN = function (num) { return !isNaN(num); };

    var monthlySum = function (equipments) {
        return Object.keys(equipments)
            .map(idToValue(equipments))
            .map(function (equipment) { return parseFloat(equipment.monthlyCost); })
            .filter(notNaN)
            .reduce(function (sum, cost) { return sum + cost; }, 0);
    };

    var recalculateClarityValuesAndRender = function() {

        var downPayment = $('#downPayment').val();
        state.downPayment = downPayment ? +downPayment : 0;

        var eSum = monthlySum(state.equipments);
        var packagesSum = !$.isEmptyObject(state.packages) ? monthlySum(state.packages) : 0;
        var totalMonthlyData = $.extend({ equipmentSum: eSum, downPayment: state.downPayment, tax: state.tax, packagesSum: packagesSum }, idToValue(state)('clarity'));

        _renderTotalPriceInfo(totalMonthlyData);

        var data = rateCardsCalculator.calculateClarityValuesForRender(totalMonthlyData);
      
        _renderCalculatedInfo(data);
    }

    function _renderCalculatedInfo(data) {
        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
        var validateNumber = settings.numberFields.every(function (field) { return typeof data[field] === 'number'; });
        var validateNotEmpty = settings.notCero.every(function (field) { return data[field] !== 0; });

        if (notNan && validateNumber && validateNotEmpty) {
            $('#loanAmortTerm').text(data.loanTerm + '/' + data.amortTerm);
            $('#customerRate').text(data.CustomerRate.toFixed(2) + '%');
            $('#total-amount-financed').text(formatCurrency(data.totalAmountFinanced));
            Object.keys(settings.rateCardCurrencyFields).map(function (key) { $('#' + key).text(formatCurrency(data[settings.rateCardCurrencyFields[key]])); });
            Object.keys(settings.rateCardPercentageFields).map(function (key) { $('#' + key).text(formatNumber(data[settings.rateCardPercentageFields[key]]) + '%'); });
            Object.keys(settings.rateCardMonthsFields).map(function (key) { $('#' + key).text(data[settings.rateCardMonthsFields[key]] + ' ' + translations.months.toLowerCase()); });
        } else {
            $('#loanAmortTerm').text('-');
            $('#customerRate').text('-');
            $('#total-amount-financed').text('-');
            Object.keys(settings.rateCardCurrencyFields).map(function (key) { $('#' + key).text('-'); });
            Object.keys(settings.rateCardPercentageFields).map(function (key) { $('#' + key).text('-'); });
            Object.keys(settings.rateCardMonthsFields).map(function (key) { $('#' + key).text('-'); });
        }
    }

    function _renderTotalPriceInfo(data) {
        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
        if (notNan && data.equipmentSum !== 0) {
            $("#totalMonthlyCostNoTax").text(formatCurrency(data.equipmentSum + data.packagesSum));
            $("#tax").text(formatCurrency(tax(data)));
            $("#totalMonthlyCostTax").text(formatCurrency(totalRentalPrice(data)));
            $("#totalPriceEquipment").text(formatCurrency(totalRentalPrice(data)));
        } else {
            $("#tax").text('-');
            $("#totalMonthlyCostNoTax").text('-');
            $("#totalMonthlyCostTax").text('-');
            $("#totalPriceEquipment").text('-');
        }
    }

    return {init : init, recalculateClarityValuesAndRender: recalculateClarityValuesAndRender }
});