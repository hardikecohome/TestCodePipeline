module.exports('calculator-conversion', function(require) {
    
    var recalculateEquipmentIndex = function (option, id) {

        delete state[option].equipments[id];

        var nextId = Number(id);
        while (true) {
            nextId++;
            var nextEquipment = $('#' + option + '-equipment-' + nextId);

            if (!nextEquipment.length) {
                break;
            }

            var inputs = nextEquipment.find('input, select, textarea');

            var fullCurrentId = option + '_' + nextId;
            var fullPreviousId = option + '_' + (nextId - 1);
            var currentIdPattern = 'Equipment_NewEquipment_' + fullCurrentId;
            var previousIdPattern = 'Equipment_NewEquipment_' + fullPreviousId;
            var currentNamePattern = 'Equipment.NewEquipment[' + fullCurrentId;
            var previousNamePattern = 'Equipment.NewEquipment[' + fullPreviousId;

            inputs.each(function () {
                $(this).attr('id', $(this).attr('id').replace(currentIdPattern, previousIdPattern));
                $(this).attr('name', $(this).attr('name').replace(currentNamePattern, previousNamePattern));
            });

            var labels = nextEquipment.find('label');

            labels.each(function () {
                $(this).attr('for', $(this).attr('for').replace(currentIdPattern, previousIdPattern));
            });

            var spans = nextEquipment.find('span');

            spans.each(function () {
                var valFor = $(this).attr('data-valmsg-for');
                if (valFor == null) { return; }

                $(this).attr('data-valmsg-for', valFor.replace(currentNamePattern, previousNamePattern));
            });

            nextEquipment.find('.equipment-number').text('№' + nextId);

            var removeButton = nextEquipment.find('#' + option + '-equipment-remove-' + nextId);
            removeButton.attr('id', option + '-equipment-remove-' + (nextId - 1));
            nextEquipment.attr('id', option + '-equipment-' + (nextId - 1));

            state[option].equipments[nextId].id = (nextId - 1).toString();
            state[option].equipments[nextId - 1] = state[option].equipments[nextId];
            delete state[option].equipments[nextId];
        }
    }

    var recalculateEquipmentId = function (optionContainer, optionToReplace, newOption) {

        $(optionContainer).find('[data-valmsg-for^="' + optionToReplace + '"]').each(function () {
            var $this = $(this);
            var newValFor = $this.data('valmsg-for').replace(optionToReplace, newOption);
            $this.attr('data-valmsg-for', newValFor);
        });

        $(optionContainer).find('[id^="Equipment_NewEquipment_' + optionToReplace + '_"]').each(function () {
            $(this).attr('id', $(this).attr('id').replace('Equipment_NewEquipment_' + optionToReplace, 'Equipment_NewEquipment_' + newOption));
        });

        $(optionContainer).find('[name^="Equipment.NewEquipment[' + optionToReplace + '_"]').each(function () {
            $(this).attr('name', $(this).attr('name').replace('"Equipment.NewEquipment[' + optionToReplace, '"Equipment.NewEquipment[' + newOption));
        });

        $(optionContainer).find('[for^="Equipment_NewEquipment_' + optionToReplace + '_"]').each(function () {
            $(this).attr('for', $(this).attr('for').replace('"Equipment_NewEquipment_' + optionToReplace, '"Equipment_NewEquipment_' + newOption));
        });
    }

    return {
        recalculateEquipmentIndex: recalculateEquipmentIndex,
        recalculateEquipmentId: recalculateEquipmentId
    }
})