module.exports('calculator-ui', function (require) {
    var state = require('calculator-state').state;
    var constants = require('calculator-state').constants;
    var setters = require('calculator-value-setters');

    var addEquipment = function (option, callback) {
        return function(e) {
            var template = $('#equipment-template').html();
            var equipmentNextNumber = Object.keys(state[option].equipments).length + 1;

            var result = template.split('Equipment.NewEquipment[0]')
                .join('Equipment.NewEquipment[' + state.equipmentNextIndex + ']')
                .split('Equipment_NewEquipment_0').join('Equipment_NewEquipment_' + state.equipmentNextIndex)
                .replace("№1", "№" + equipmentNextNumber);

            var equipmentTemplate = $.parseHTML(result);

            $(equipmentTemplate).find('div.additional-remove').attr('id', 'equipment-remove-' + state.equipmentNextIndex);
            $(equipmentTemplate).attr('id', 'equipment-' + state.equipmentNextIndex);
            $(equipmentTemplate).find('#equipment-remove-' + state.equipmentNextIndex).on('click', setters.removeEquipment(option, callback));
            $(equipmentTemplate).find('.equipment-cost').on('change', setters.setEquipmentCost(option, callback));
            $(equipmentTemplate).find('#Equipment_NewEquipment_' + state.equipmentNextIndex + '__Type').on('change', setters.setEquipmentType(option));
            $(equipmentTemplate).find('#Equipment_NewEquipment_' + state.equipmentNextIndex + '__Description').on('change', setters.setEquipmentDescription(option));
            $('#' + option + '-container').find('.equipments-hold').append(equipmentTemplate);

            $('#equipment-' + state.equipmentNextIndex)
                .find('.clear-input')
                .find('svg')
                .html('<use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-remove"></use>');

            toggleClearInputIcon($('#equipment-' + state.equipmentNextIndex).find('.control-group input, .control-group textarea'));

            setters.setNewEquipment(option, callback);
        }
    }

    var clearFirstOption = function(callback) {
        $('#option1-mPayment').text('-');
        $('#option1-cBorrowing').text('-');
        $('#option1-taFinanced').text('-');
        $('#option1-tmPayments').text('-');
        $('#option1-rBalance').text('-');
        $('#option1-tObligation').text('-');
        $('#option1-yCost').text('-');
        $('#option1-downPayment').val('');

        state['option1'].downPayment = 0;

        var keys = Object.keys(state['option1'].equipments);
        $.grep(keys, function (key) {
            var equipment = state['option1'].equipments[key];
            equipment.cost = '';
            equipment.description = '';

            $('#Equipment_NewEquipment_' + equipment.id + '__Cost').val('');
            $('#Equipment_NewEquipment_' + equipment.id + '__Description').val('');
        });

        callback(['option1']);
    }

    var deleteSecondOption = function(callback) {
        state['option2'] = $.extend(true, {}, state['option3']);
        state.equipmentNextIndex -= Object.keys(state['option3'].equipments).length;
        delete state['option3'];

        var div = $('#option3-container');

        $('#option3-container *').off();

        div.find('[id^="option3"]').each(function () {
            $(this).attr('id', $(this).attr('id').replace('option3', 'option2'));
        });

        $('#option2-header').text($('#option2-header').text().replace('3', '2'));

        div.attr('id', 'option2-container');

        var equipmentsToUpdate = Object.keys(state['option2'].equipments).map(function (k) {
            return state['option2'].equipments[k].id;
        });

        $.grep(equipmentsToUpdate, function (eq) {
            div.find('.equipment-item').find('label[for^="Equipment_NewEquipment_' + eq + '"]').each(function () {
                $(this).attr('for', $(this).attr('for').replace('Equipment_NewEquipment_' + eq, 'Equipment_NewEquipment_' + state.equipmentNextIndex));
            });

            div.find('.equipment-item').find('select[id^="Equipment_NewEquipment_' + eq + '"]').each(function () {
                $(this).attr('id', $(this).attr('id').replace('Equipment_NewEquipment_' + eq, 'Equipment_NewEquipment_' + state.equipmentNextIndex));
                $(this).attr('name', $(this).attr('name').replace('Equipment.NewEquipment[' + eq, 'Equipment_NewEquipment[' + state.equipmentNextIndex));
            });

            div.find('.equipment-item').find('input[id^="Equipment_NewEquipment_' + eq + '"]').each(function () {
                $(this).attr('id', $(this).attr('id').replace('Equipment_NewEquipment_' + eq, 'Equipment_NewEquipment_' + state.equipmentNextIndex));
                $(this).attr('name', $(this).attr('name').replace('Equipment.NewEquipment[' + eq, 'Equipment_NewEquipment[' + state.equipmentNextIndex));
            });

            state['option2'].equipments[eq].id = state.equipmentNextIndex.toString();
            state['option2'].equipments[state.equipmentNextIndex.toString()] = state['option2'].equipments[eq];
            delete state['option2'].equipments[eq];

            state.equipmentNextIndex++;
        });

        div.find('[id*="__Cost"]').each(function () {
            $(this).on('change', setters.setEquipmentCost('option2', callback));
        });
    }


    var copyOption = function() {
        
    }

    var moveButtonByIndex = function (index, isMoveForward) {
        var firstButton = $('#first-add-button').find('button');
        var secondButton = $('#second-add-button').find('button');

        if (index === 1) {
            isMoveForward ? firstButton.addClass('hidden') : firstButton.removeClass('hidden');
            isMoveForward ? secondButton.removeClass('hidden') : secondButton.addClass('hidden');
        } else {
            isMoveForward ? secondButton.addClass('hidden') : secondButton.removeClass('hidden');
        }
    }

    return {
        addEquipment: addEquipment,
        clearFirstOption: clearFirstOption,
        moveButtonByIndex: moveButtonByIndex,
        deleteSecondOption: deleteSecondOption
    }
})