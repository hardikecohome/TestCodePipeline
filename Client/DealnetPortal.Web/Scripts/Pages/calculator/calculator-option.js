module.exports('calculator-option', function (require) {
    var setters = require('calculator-value-setters');
    var state = require('calculator-state').state;
    var constants = require('calculator-state').constants;

    var optionSetup = function(option, callback) {
        $('#' + option + '-addEquipment').on('click', function () {
            var template = $('#equipment-template').html();
            var parentNode = $('<div></div>');
            var result = template.split('Equipment.NewEquipment[0]')
                .join('Equipment.NewEquipment[' + state.equipmentNextIndex + ']')
                .split('Equipment_NewEquipment_0').join('Equipment_NewEquipment_' + state.equipmentNextIndex)
                .replace("№1", "№" + (state.equipmentNextIndex + 1));
            parentNode.append($.parseHTML(result));
            // equipment handlers
            $(parentNode).find('.equipment-cost').on('change', setters.setEquipmentCost(option, callback));
            $('#' + option + '-container').find('.equipments-hold').append(parentNode);

            setters.setNewEquipment(option, callback);

        });

        $('#' + option + '-downPayment').on('change', setters.setDownPayment(option, callback));
        $('#' + option + '-plan').on('change', setters.setRateCardPlan(option, callback));
        $('#' + option + '-amortDropdown').on('change', setters.setLoanAmortTerm(option, callback));

        state[option].equipments[state.equipmentNextIndex] = { id: state.equipmentNextIndex.toString(), cost: '' };

        $('#Equipment_NewEquipment_' + state.equipmentNextIndex + '__Cost').on('change', setters.setEquipmentCost(option, callback));
        state.equipmentNextIndex++;
        $('#' + option + '-plan').change();
    }

    var renderOption = function(option, data) {
        var validateNumber = constants.numberFields.every(function (field) {
            var result = typeof data[field] === 'number';
            return result;
        });

        var validateNotEmpty = constants.notCero.every(function (field) {
            return data[field] !== 0;
        });

        $('#' + option + '-aFee').text(data.adminFee);
        $('#' + option + '-cRate').text(data.customerRate + ' %');
        $('#' + option + '-yCostVal').text(data.dealerCost + ' %');

        if (validateNumber && validateNotEmpty) {
            $('#' + option + '-mPayment').text(formatCurrency(data.monthlyPayment));
            $('#' + option + '-cBorrowing').text(formatCurrency(data.costOfBorrowing));
            $('#' + option + '-taFinanced').text(formatCurrency(data.totalAmountFinanced));
            $('#' + option + '-tmPayments').text(formatCurrency(data.totalMonthlyPayments));
            $('#' + option + '-rBalance').text(formatCurrency(data.residualBalance));
            $('#' + option + '-tObligation').text(formatCurrency(data.totalObligation));
            $('#' + option + '-yCost').text(formatCurrency(data.yourCost));
        } else {
            $('#' + option + '-mPayment').text('-');
            $('#' + option + '-cBorrowing').text('-');
            $('#' + option + '-taFinanced').text('-');
            $('#' + option + '-tmPayments').text('-');
            $('#' + option + '-rBalance').text('-');
            $('#' + option + '-tObligation').text('-');
            $('#' + option + '-yCost').text('-');
        }
    }

    var addOption = function () {
        var index = $('#options-container').find('.rate-card-col').length;
        var optionToCopy = $('#option' + index + '-container').html();
        var optionContainer = $('<div class="col-md-4 col-sm-6 rate-card-col"></div>');
        optionContainer.append($.parseHTML(optionToCopy));
        $('#options-container').append(optionContainer);
    }

    return {
        addOption: addOption,
        optionSetup: optionSetup,
        renderOption: renderOption
    }
});