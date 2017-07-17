module.exports('calculator-option', function (reqiure) {

    var optionSetup = function(option, callback) {
        $(option + '-addEquipment').on('click', function () {
            console.log('add equipment for ' + option);

        });
        $(option + '-downPayment').on('change', function () {
            console.log('down payment changed for ' + option);
        });
        $(option + '-plan').on('change', function () {
            console.log('plan changed for ' + option);
        });
        $('#Equipment_NewEquipment_0__Cost').on('change', function() {
            
        });
    }

    var renderOption = function(option, data) {
        var notNan = 's' !== 's2';

        if (notNan) {
            $('#' + option + 'mPayment').text(formatCurrency(data.monthlyPayment));
            $('#' + option + 'cBorrowing').text(formatCurrency(data.costOfBorrowing));
            $('#' + option + 'taFinanced').text(formatCurrency(data.totalAmountFinanced));
            $('#' + option + 'tmPayments').text(formatCurrency(data.totalMonthlyPayments));
            $('#' + option + 'rBalance').text(formatCurrency(data.residualBalance));
            $('#' + option + 'yObligation').text(formatCurrency(data.totalObligation));
            $('#' + option + 'yCost').text(formatCurrency(data.yourCost));
        } else {
            $('#' + option + 'MPayment').text('-');
            $('#' + option + 'CBorrowing').text('-');
            $('#' + option + 'TAFinanced').text('-');
            $('#' + option + 'TMPayments').text('-');
            $('#' + option + 'RBalance').text('-');
            $('#' + option + 'TObligation').text('-');
            $('#' + option + 'YCost').text('-');
        }
    }

    return {
        optionSetup: optionSetup
    }
});