module.exports('calculator-option', function (require) {
    var setters = require('calculator-value-setters');
    var state = require('calculator-state').state;
    var constants = require('calculator-state').constants;

    var optionSetup = function(option, callback) {
        $('#' + option + '-addEquipment').on('click', function () {
            var template = $('#equipment-template').html();
            var result = template.split('Equipment.NewEquipment[0]')
                .join('Equipment.NewEquipment[' + state.equipmentNextIndex + ']')
                .split('Equipment_NewEquipment_0').join('Equipment_NewEquipment_' + state.equipmentNextIndex)
                .replace("№1", "№" + (state.equipmentNextIndex + 1));

            var equipmentTemplate = $.parseHTML(result);

            $(equipmentTemplate).find('div.additional-remove').attr('id', 'equipment-remove-' + state.equipmentNextIndex);
            $(equipmentTemplate).attr('id', 'equipment-' + state.equipmentNextIndex);
            $(equipmentTemplate).find('#equipment-remove-' + state.equipmentNextIndex).on('click', setters.removeEquipment(option, callback));
            $(equipmentTemplate).find('.equipment-cost').on('change', setters.setEquipmentCost(option, callback));
            $('#' + option + '-container').find('.equipments-hold').append(equipmentTemplate);

            setters.setNewEquipment(option, callback);

        });

        $('#' + option + '-downPayment').on('change', setters.setDownPayment(option, callback));
        $('#' + option + '-plan').on('change', setters.setRateCardPlan(option, callback));
        $('#' + option + '-amortDropdown').on('change', setters.setLoanAmortTerm(option, callback));

        if (option === 'option1') {
            state[option].equipments[state.equipmentNextIndex] = { id: state.equipmentNextIndex.toString(), cost: '' };

            $('#Equipment_NewEquipment_' + state.equipmentNextIndex + '__Cost').on('change', setters.setEquipmentCost(option, callback));
            state.equipmentNextIndex++;
            $('#' + option + '-plan').change();
        }
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

    var addOption = function (callback) {
        var index = $('#options-container').find('.rate-card-col').length;
        if (index === 1) {
            $('#first-add-button').find('button').addClass('hidden');
            $('#second-add-button').find('button').removeClass('hidden');
        } else {
            $('#second-add-button').find('button').addClass('hidden');
        }

        var optionToCopy = $('#option' + index + '-container')
                            .html()
                            .replace("Option " + index, "Option " + (index + 1));;
        var template = $.parseHTML(optionToCopy);

        $(template).find('[id^="option' + index + '-"]').each(function () {
            $(this).attr('id', $(this).attr('id').replace('option' + index, 'option' + (index + 1)));
        });
        var labelIndex = 0;
        $(template).find('.equipment-item').find('label').each(function() {
            $(this).attr('for', $(this).attr('for').replace('Equipment_NewEquipment_' + labelIndex, 'Equipment_NewEquipment_' + state.nextEquipmentId));
            labelIndex++;

        });

        $(template).find('.equipment-item').find('select, input').each(function() {

        });
        var header = $(template).find('h2').text();
        $(header).text($(header).text().replace('Option ' + index, 'Option ' + (index + 1)));

        var optionContainer = $('<div class="col-md-4 col-sm-6 rate-card-col"></div>');
        optionContainer.append(template);
        $('#options-container').append(optionContainer);

        state['option' + (index + 1)] = state['option' + index];

        optionSetup('option' + (index + 1), callback);

    }

    return {
        addOption: addOption,
        optionSetup: optionSetup,
        renderOption: renderOption
    }
});