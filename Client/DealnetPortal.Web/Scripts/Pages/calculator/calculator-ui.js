module.exports('calculator-ui', function (require) {
    var state = require('calculator-state').state;
    var constants = require('calculator-state').constants;
    var setters = require('calculator-value-setters');
    var recalculateEquipmentIndex = require('calculator-conversion').recalculateEquipmentIndex;
    var recalculateEquipmentId = require('calculator-conversion').recalculateEquipmentId;
    var resetValidation = require('calculator-conversion').resetValidation;

    var addEquipment = function (option, callback) {
        return function(e) {
            var template = $('#equipment-template').html();
            var equipmentLabel = Object.keys(state[option].equipments).length + 1;
            var nextIndex = state[option].equipmentNextIndex;

            var result = template.split('Equipment.NewEquipment[0]')
                .join('Equipment.NewEquipment[' + option + '_' + nextIndex + ']')
                .split('Equipment_NewEquipment_0').join('Equipment_NewEquipment_' + option + '_' + nextIndex)
                .replace("№1", "№" + equipmentLabel);

            var equipmentTemplate = $.parseHTML(result);

            $(equipmentTemplate).find('div.additional-remove').attr('id', option + '-equipment-remove-' + nextIndex);
            $(equipmentTemplate).attr('id', option + '-equipment-' + nextIndex);
            $(equipmentTemplate).find('#' + option + '-equipment-remove-' + nextIndex).on('click', function (e) {
                var id = e.target.id;
                id = id.substr(id.lastIndexOf('-') + 1);
                recalculateEquipmentIndex(option, id);

                setters.removeEquipment(option, id, callback);
            });

            $(equipmentTemplate).find('.equipment-cost').on('change', setters.setEquipmentCost(option, callback));
            $(equipmentTemplate).find('#Equipment_NewEquipment_' + option + '_' + nextIndex + '__Type').on('change', setters.setEquipmentType(option));
            $(equipmentTemplate).find('#Equipment_NewEquipment_' + option + '_' + nextIndex + '__Description').on('change', setters.setEquipmentDescription(option));

            $('#' + option + '-container').find('.equipments-hold').append(equipmentTemplate);

            $('#equipment-' + nextIndex)
                .find('.clear-input')
                .find('svg')
                .html('<use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-remove"></use>');

            toggleClearInputIcon($('#equipment-' + nextIndex).find('.control-group input, .control-group textarea'));

            setters.setNewEquipment(option, callback);

            resetValidation(option);
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

            $('#Equipment_NewEquipment_option1_' + equipment.id + '__Cost').val('');
            $('#Equipment_NewEquipment_option1_' + equipment.id + '__Description').val('');
        });

        callback(['option1']);
    }

    var deleteSecondOption = function(callback) {
        state['option2'] = $.extend(true, {}, state['option3']);
        state.equipmentNextIndex -= Object.keys(state['option3'].equipments).length;
        delete state['option3'];

        var $container = $('#option3-container');

        $('#option3-container *').off();

        recalculateEquipmentId($container, 'option3', 'option2');

        $container.find('[id^="option3"]').each(function () {
            $(this).attr('id', $(this).attr('id').replace('option3', 'option2'));
        });

        $('#option2-header').text($('#option2-header').text().replace('3', '2'));

        $container.attr('id', 'option2-container');

        $container.find('[id*="__Cost"]').each(function () {
            $(this).on('change', setters.setEquipmentCost('option2', callback));
        });

        $container.find('[id*="__Description"]').each(function () {
            $(this).on('change', setters.setEquipmentDescription('option2'));
        });

        $container.find('[id*="__Type"]').each(function () {
            $(this).on('change', setters.setEquipmentType('option2'));
        });

        $container.find('[id^="option2-equipment-remove-"]').each(function() {
            $(this).on('click', function (e) {
                var id = e.target.id;
                id = id.substr(id.lastIndexOf('-') + 1);
                recalculateEquipmentIndex('option2', id);

                setters.removeEquipment('option2', id, callback);
            });
        });
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