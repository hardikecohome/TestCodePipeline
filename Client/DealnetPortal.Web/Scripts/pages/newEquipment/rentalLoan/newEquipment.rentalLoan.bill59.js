module.exports('bill59', function (require) {

    var state = require('state').state;
    var idToValue = require('idToValue');

    var equipmentInList = function (id) {
        return state.bill59Equipment.indexOf(id) > -1;
    };

    var equipmentInListSelected = function () {
        return Object.keys(state.equipments)
            .map(idToValue(state.equipments))
            .map(function (item) {
                return item.type;
            }).reduce(function (acc, item) {
                return acc || equipmentInList(item);
            }, false);
    };

    var enableForAll = function () {
        if (state.isOntario && state.agreementType != 0) {
            var newEquipment = $('div#new-equipments [id^="new-equipment-"]');
            $.each(newEquipment, function (i, el) {
                var $el = $(el);
                if (equipmentInList($el.find('.equipment-select').val())) {
                    enableNewEquipment($el);
                }
            });
        }
    };

    var disableForAll = function () {
        if (state.isOntario && state.agreementType === 0) {
            var newEquipment = $('div#new-equipments [id^="new-equipment-"]');
            $.each(newEquipment, function (i, el) {
                disableNewEquipment(el);
            });
        }
    };

    var enableNewEquipment = function (row) {
        var $row = $(row);
        $row.find('.description-col').removeClass('col-md-6').addClass('col-md-5');
        $row.find('.monthly-cost-col').removeClass('col-md-3').addClass('col-md-2');
        $row.find('.estimated-retail-col').removeClass('hidden')

        var input = $row.find('.estimated-retail');
        input.prop('disabled', false);
        input[0].form && input.rules('add', 'required');
        enableExistingEquipment();
    };

    var disableNewEquipment = function (row) {
        var $row = $(row);
        $row.find('.description-col').removeClass('col-md-5').addClass('col-md-6');
        $row.find('.monthly-cost-col').removeClass('col-md-2').addClass('col-md-3');
        $row.find('.estimated-retail-col').addClass('hidden');

        var input = $row.find('.estimated-retail');
        input.prop('disabled', true);
        input[0].form && input.rules('remove', 'required');
        disableExistingEquipment();
    };

    var enableExistingEquipment = function () {
        if (state.isOntario && state.agreementType !== 0) {
            if (equipmentInListSelected()) {
                Object.keys(state.existingEquipments)
                    .map(idToValue(state.existingEquipments))
                    .forEach(function (equip) {
                        var $equip = $('#existing-equipment-' + equip.id);
                        $equip.find('.responsible-col').removeClass('hidden');
                        var $dropdown = $equip.find('.responsible-dropdown');
                        $dropdown.prop('disabled', false);
                        $dropdown[0].form && $dropdown.rules('add', 'required');
                        $dropdown.change();
                        $equip.find('.note-col').addClass('col-md-pull-6');
                    });
            }
        }
    };

    var disableExistingEquipment = function () {

    };

    var onEquipmentChange = function (e) {
        if (state.isOntario && state.agreementType !== 0) {
            if (equipmentInList(e.target.value)) {
                enableNewEquipment($(e.target).parents('.new-equipment'));
            } else {
                disableNewEquipment($(e.target).parents('.new-equipment'));
            }
        }
    }

    var onResposibilityChange = function (e) {
        var otherCol = $(e.target).parents('.responsible-col').find('.responsible-other-col');
        var $input = otherCol.find('.responsible-other');
        if (e.target.value.toLowerCase() === '3') {
            otherCol.removeClass('hidden');
            $input.attr('disabled', false);
            $input[0].form && $input.rules('add', 'required');
        } else {
            otherCol.addClass('hidden');
            $input.attr('disabled', true);
            $input[0].form && $input.rules('remove', 'required');
        }
    };

    return {
        enableForAll: enableForAll,
        disableForAll: disableForAll,
        onEquipmentChange: onEquipmentChange,
        onResposibilityChange: onResposibilityChange
    };
});