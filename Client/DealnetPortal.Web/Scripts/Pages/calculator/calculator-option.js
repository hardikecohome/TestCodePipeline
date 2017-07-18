module.exports('calculator-option', function (require) {
    var setters = require('calculator-value-setters');

    var optionSetup = function(option, callback) {
        $('#' + option + '-addEquipment').on('click', function () {
            console.log('add equipment for ' + option);

        });

        $('#' + option + '-downPayment').on('change', setters.setDownPayment(option, callback));
        $('#' + option + '-plan').on('change', setters.setRateCardPlan(option, callback));

        var equipments = $('#option1-container').find("input[id*='Cost']");
        $('#Equipment_NewEquipment_0__Cost').on('change', function() {
            
        });


    }

    var renderOption = function(option, data) {
        var notNan = 's' !== 's2';

        if (notNan) {
            $('#' + option + '-mPayment').text(formatCurrency(data.monthlyPayment));
            $('#' + option + '-cBorrowing').text(formatCurrency(data.costOfBorrowing));
            $('#' + option + '-taFinanced').text(formatCurrency(data.totalAmountFinanced));
            $('#' + option + '-tmPayments').text(formatCurrency(data.totalMonthlyPayments));
            $('#' + option + '-rBalance').text(formatCurrency(data.residualBalance));
            $('#' + option + '-tObligation').text(formatCurrency(data.totalObligation));
            $('#' + option + '-yCost').text(formatCurrency(data.yourCost));
            $('#' + option + '-aFee').text(data.adminFee);
            $('#' + option + '-cRate').text(data.customerRate + ' %');
            $('#' + option + '-yCostVal').text(data.dealerCost + ' %');
        } else {
            $('#' + option + '-mPayment').text('-');
            $('#' + option + '-cBorrowing').text('-');
            $('#' + option + '-taFinanced').text('-');
            $('#' + option + '-tmPayments').text('-');
            $('#' + option + '-rBalance').text('-');
            $('#' + option + '-tObligation').text('-');
            $('#' + option + '-yCost').text('-');
            $('#' + option + '-aFee').text('-');
            $('#' + option + '-cRate').text('-');
            $('#' + option + '-yCostVal').text('-');
        }
    }

    return {
        optionSetup: optionSetup,
        renderOption: renderOption
    }
});