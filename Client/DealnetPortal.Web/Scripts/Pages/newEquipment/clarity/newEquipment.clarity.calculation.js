module.exports('newEquipment.clairty.calculation', function (require) {
    var rateCardsCalculator = require('rateCards.index');
    var state = require('state').state;
    var constants = require('state').constants;
    var tax = require('financial-functions').tax;
    var totalRentalPrice = require('financial-functions').totalRentalPrice;

    var idToValue = require('idToValue');

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

    var init = function () {

    }

    var notNaN = function (num) {
        return !isNaN(num);
    };

    var monthlySum = function (equipments) {
        return Object.keys(equipments)
            .map(idToValue(equipments))
            .map(function (equipment) {
                return parseFloat(equipment.monthlyCost);
            })
            .filter(notNaN)
            .reduce(function (sum, cost) {
                return sum + cost;
            }, 0);
    };

    var countItems = function (equipments) {
        return Object.keys(equipments)
            .reduce(function (count, item) {
                return count + 1;
            }, 0);
    }

    var recalculateClarityValuesAndRender = function () {
        var downPayment = $('#downPayment').val();
        state.downPayment = downPayment ? +downPayment : 0;

        var eSum = monthlySum(state.equipments);
        var packagesSum = !$.isEmptyObject(state.packages) ? monthlySum(state.packages) : 0;
        var totalMonthlyData = $.extend({
            equipmentSum: eSum,
            downPayment: state.downPayment,
            tax: state.tax,
            packagesSum: packagesSum,
            numEquipment: countItems(state.equipments),
            numPackages: !$.isEmptyObject(state.packages) ? countItems(state.packages) : 0
        }, idToValue(state)('clarity'));

        var briefData = rateCardsCalculator.caclulateClarityBriefValues(totalMonthlyData);
        _renderTotalPriceInfo(briefData);

        var data = rateCardsCalculator.calculateClarityValuesForRender(totalMonthlyData);

        _renderCalculatedInfo(data);

        _recalculateAndRenderMCOLessDownPayment(briefData);
    }

    function _renderCalculatedInfo(data) {
        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) {
            return isNaN(val);
        });
        var validateNumber = settings.numberFields.every(function (field) {
            return typeof data[field] === 'number';
        });
        var validateNotEmpty = settings.notCero.every(function (field) {
            return data[field] !== 0;
        });

        if (notNan && validateNumber && validateNotEmpty) {
            $('#loanAmortTerm').text(data.loanTerm + '/' + data.amortTerm);
            $('#customerRate').text(data.CustomerRate.toFixed(2) + '%');
            $('#total-amount-financed').text(formatCurrency(data.totalAmountFinanced));
            $('#totalMonthlyCostTaxDP').text(formatCurrency(data.totalMonthlyCostTaxDP));
            Object.keys(settings.rateCardCurrencyFields).map(function (key) {
                $('#' + key).text(formatCurrency(data[settings.rateCardCurrencyFields[key]]));
            });
            Object.keys(settings.rateCardPercentageFields).map(function (key) {
                $('#' + key).text(formatNumber(data[settings.rateCardPercentageFields[key]]) + '%');
            });
            Object.keys(settings.rateCardMonthsFields).map(function (key) {
                $('#' + key).text(data[settings.rateCardMonthsFields[key]] + ' ' + translations.months.toLowerCase());
            });
        } else {
            $('#loanAmortTerm').text('-');
            $('#customerRate').text('-');
            $('#total-amount-financed').text('-');
            $('#totalMonthlyCostTaxDP').text('-');
            Object.keys(settings.rateCardCurrencyFields).map(function (key) {
                $('#' + key).text('-');
            });
            Object.keys(settings.rateCardPercentageFields).map(function (key) {
                $('#' + key).text('-');
            });
            Object.keys(settings.rateCardMonthsFields).map(function (key) {
                $('#' + key).text('-');
            });
        }
    }

    function _renderTotalPriceInfo(data) {
        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) {
            return isNaN(val);
        });
        if (notNan && data.equipmentSum !== 0) {
            $("#totalMonthlyCostNoTax").text(formatCurrency(data.equipmentSum + data.packagesSum));
            $("#tax").text(formatCurrency(data.tax));
            $("#totalMonthlyCostTax").text(formatCurrency(data.totalMonthlyCostOfOwnership));
            $("#totalPriceEquipment").text(formatCurrency(data.totalPriceOfEquipment));
        } else {
            $("#tax").text('-');
            $("#totalMonthlyCostNoTax").text('-');
            $("#totalMonthlyCostTax").text('-');
            $("#totalPriceEquipment").text('-');
        }
    }

    var _recalculateAndRenderMCOLessDownPayment = function (data) {
        var downPayment = state.downPayment; //D20
        var dpTax = downPayment * constants.clarityPaymentFactor / (1 + state.tax / 100);

        var totalMonthlyCost = formatNumber(data.equipmentSum + data.packagesSum) //d12 - downPayment * constants.clarityPaymentFactor;


        for (var id in state.equipments) {
            var equipment = state.equipments[id];

            equipment.monthlyCostLessDp = equipment.monthlyCost - dpTax / totalMonthlyCost * equipment.monthlyCost;
            $('#NewEquipment_' + id + '__MonthlyCostLessDP')
                .val(formatNumber(equipment.monthlyCostLessDp));
        }

        for (var id in state.packages) {
            var package = state.packages[id];

            package.monthlyCostLessDp = package.monthlyCost - dpTax / totalMonthlyCost * package.monthlyCost;
            $('#InstallationPackages_' + id + '__MonthlyCostLessDP')
                .val(formatNumber(state.packages[id].monthlyCostLessDp));
        }
    }

    return {
        init: init,
        recalculateClarityValuesAndRender: recalculateClarityValuesAndRender
    }
});