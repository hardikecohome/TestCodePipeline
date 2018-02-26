﻿module.exports('bill59', function (require) {

    var state = require('state').state;

    var equipmentInList = function (id) {
        return state.bill59Equipment.indexOf(id) > -1;
    }

    var enableForAll = function () {
        if (state.isOntario && state.agreementType != 0) {
            var newEquipment = $('div#new-equipments [id^="new-equipment-"]');
            $.each(newEquipment, function (i, el) {
                var $el = $(el);
                if (equipmentInList($el.find('.equipment-select').val())) {
                    enable($el);
                }
            });
        }
    };

    var disableForAll = function () {
        if (state.isOntario && state.agreementType === 0) {
            var newEquipment = $('div#new-equipments [id^="new-equipment-"]');
            $.each(newEquipment, function (i, el) {
                disable($(el));
            });
        }
    };

    var enable = function ($row) {
        $row.find('.description-col').removeClass('col-md-6').addClass('col-md-5');
        $row.find('.monthly-cost-col').removeClass('col-md-3').addClass('col-md-2');
        $row.find('.estimated-retail-col').removeClass('hidden')

        var input = $row.find('.estimated-retail');
        input.prop('disabled', false);
        input[0].form && input.rules('add', 'required');
    };

    var disable = function ($row) {
        $row.find('.description-col').removeClass('col-md-5').addClass('col-md-6');
        $row.find('.monthly-cost-col').removeClass('col-md-2').addClass('col-md-3');
        $row.find('.estimated-retail-col').addClass('hidden');

        var input = $row.find('.estimated-retail');
        input.prop('disabled', true);
        input[0].form && input.rules('remove', 'required');
    };

    var onEquipmentChange = function (e) {
        if (state.isOntario && state.agreementType !== 0) {
            if (equipmentInList(e.target.value)) {
                enable($(e.target).parents('.new-equipment'));
            } else {
                disable($(e.target).parents('.new-equipment'));
            }
        }
    }

    return {
        enableForAll: enableForAll,
        disableForAll: disableForAll,
        onEquipmentChange: onEquipmentChange
    };
});