module.exports('newEquipment.clairty.calculation', function(require) {
    var rateCardsCalculator = require('rateCards.index');
    var state = require('state').state;
    var tax = require('financial-functions').tax;
    var totalRentalPrice = require('financial-functions').totalRentalPrice;
    var totalPriceOfEquipment = require('financial-functions').totalPriceOfEquipment;

    var settings = {
        rateCardFields: {
            'displayCostOfBorrow': 'costOfBorrowing',
            'displayTotalAmount': 'totalAmountFinanced',
            'displayAllMonthly': 'totalMonthlyPayments',
            'displayTotalObl': 'totalObligation',
            'displayYourCost': 'yourCost',
            'displayLoanTerm': 'loanTerm',
            'displayAmortTem': 'amortTerm',
            'displayCustRate': 'customerRate',
            'displayBalanceOwning': 'residualBalance'
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
        
        var totalMonthlyData = {
            tax: state.tax,
            equipmentSum: eSum,
            CustomerRate: 6.95,
            AmortizationTerm: 144,
            AdminFee: 0,
            downPayment: state.downPayment
        };

        var notNan = !Object.keys(totalMonthlyData).map(idToValue(totalMonthlyData)).some(function (val) { return isNaN(val); });
        if (notNan && totalMonthlyData.equipmentSum !== 0) {
            $("#totalMonthlyCostNoTax").text(formatCurrency(eSum));
            $("#taxGst").text(formatCurrency(tax(totalMonthlyData)));
            $("#totalMonthlyCostTax").text(formatCurrency(totalRentalPrice(totalMonthlyData)));
            $("#totalPriceEquipment").text(formatCurrency(totalRentalPrice(totalMonthlyData)));
        } else {
            $(settings.totalMonthlyPaymentId).val('');
            $(settings.rentalTaxId).text('-');
            $(settings.rentalTotalMonthlyPaymentId).text('-');
        }

        //_renderTotalPriceInfo(eSumData);

        var data = rateCardsCalculator.calculateClarityValuesForRender($.extend({ equipmentSum: eSum, downPayment: state.downPayment, tax: state.tax }, idToValue(state)('clarity')));
      
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
            Object.keys(settings.rateCardFields).map(function (key) { $('#' + key).text(formatCurrency(data[settings.rateCardFields[key]])); });
        } else {
            if (option === selectedRateCard) {
                $('#displayLoanAmortTerm').text('-');
                $('#displayCustomerRate').text('-');
                Object.keys(settings.displaySectionFields).map(function (key) { $('#' + key).text('-'); });
            }
            Object.keys(settings.rateCardFields).map(function (key) { $('#' + option + key).text('-'); });
        }
    }

    function _renderTotalPriceInfo(data) {
        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
        if (notNan && data.equipmentSum !== 0) {
            $(settings.totalMonthlyPaymentId).val(formatNumber(eSum));
            $(settings.rentalTaxId).text(formatNumber(tax(data)));
            $(settings.rentalTotalMonthlyPaymentId).text(formatNumber(totalRentalPrice(data)));
            $(settings.rentalTotalMonthlyPaymentId).text(formatNumber(totalRentalPrice(data)));
        } else {
            $(settings.totalMonthlyPaymentId).val('');
            $(settings.rentalTaxId).text('-');
            $(settings.rentalTotalMonthlyPaymentId).text('-');
        }
    }

    return {init : init, recalculateClarityValuesAndRender: recalculateClarityValuesAndRender }
});