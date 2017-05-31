module.exports('equipment', function (require) {

    var state = require('rate-cards').state;
    var equipmentTemplateFactory = require('equipment-template');
    var recalculateValuesAndRender = require('rate-cards').recalculateValuesAndRender;
    var recalculateAndRenderRentalValues = require('rate-cards').recalculateAndRenderRentalValues;

    var addEquipment = function () {
        var id = $('div#new-equipments').find('[id^=new-equipment-]').length;
        var newId = id.toString();
        state.equipments[newId] = {
            id: newId,
            type: '',
            description: '',
            cost: '',
        };

        var newTemplate = equipmentTemplateFactory($('<div></div>'), { id: id });
        if (id > 0) {
            newTemplate.find('div.additional-remove').attr('id', 'addequipment-remove-' + id);
        }
        state.equipments[newId].template = newTemplate;

        // equipment handlers
        newTemplate.find('.equipment-cost').on('change', updateCost);
        newTemplate.find('#addequipment-remove-' + id).on('click', removeEquipment);

        $('#new-equipments').append(newTemplate);

        resetFormValidator("#equipment-form");
    };

    function removeEquipment() {
        var fullId = $(this).attr('id');
        var id = fullId.substr(fullId.lastIndexOf('-') + 1);

        if (!state.equipments.hasOwnProperty(id)) {
            return;
        }
        $('#new-equipment-' + id).remove();
        delete state.equipments[id];

        var nextId = Number(id);
        while (true) {
            nextId++;
            var nextEquipment = $('#new-equipment-' + nextId);
            if (!nextEquipment.length) { break; }

            var labels = nextEquipment.find('label');
            labels.each(function () {
                $(this).attr('for', $(this).attr('for').replace('NewEquipment_' + nextId, 'NewEquipment_' + nextId - 1));
            });
            var inputs = nextEquipment.find('input, select, textarea');
            inputs.each(function () {
                $(this).attr('id', $(this).attr('id').replace('NewEquipment_' + nextId, 'NewEquipment_' + (nextId - 1)));
                $(this).attr('name', $(this).attr('name').replace('NewEquipment[' + nextId, 'NewEquipment[' + (nextId - 1)));
            });
            var spans = nextEquipment.find('span');
            spans.each(function () {
                var valFor = $(this).attr('data-valmsg-for');
                if (valFor == null) { return; }
                $(this).attr('data-valmsg-for', valFor.replace('NewEquipment[' + nextId, 'NewEquipment[' + (nextId - 1)));
            });
            nextEquipment.find('.equipment-number').text('№' + nextId);
            var removeButton = nextEquipment.find('#addequipment-remove-' + nextId);
            removeButton.attr('id', 'addequipment-remove-' + (nextId - 1));
            nextEquipment.attr('id', 'new-equipment-' + (nextId - 1));

            state.equipments[nextId].id = (nextId - 1).toString();
            state.equipments[nextId - 1] = state.equipments[nextId];
            delete state.equipments[nextId];
        }

        if (state.agreementType === 1 || state.agreementType === 2) {
            recalculateAndRenderRentalValues();
        } else {
            recalculateValuesAndRender();
        }
    };

    function initEquipment(i) {
        var cost = parseFloat($('#NewEquipment_' + i + '__Cost').val());
        if (state.equipments[i] === undefined) {
            state.equipments[i] = { id: i.toString(), cost: cost }
        } else {
            state.equipments[i].cost = cost;
        }

        $('#new-equipment-' + i).find('.equipment-cost').on('change', updateCost);
    }

    function resetFormValidator(formId) {
        $(formId).removeData('validator');
        $(formId).removeData('unobtrusiveValidation');
        $.validator.unobtrusive.parse(formId);
    }

    function updateCost() {
        var mvcId = $(this).attr("id");
        var id = mvcId.split('__Cost')[0].substr(mvcId.split('__Cost')[0].lastIndexOf('_') + 1);
        state.equipments[id].cost = parseFloat($(this).val());
        if (state.agreementType === 1 || state.agreementType === 2) {
            recalculateAndRenderRentalValues();
        } else {
            recalculateValuesAndRender();
        }
    };

    var equipments = $('div#new-equipments').find('[id^=new-equipment-]').length;

    for (var i = 0; i < equipments; i++) {
        // attatch handler to equipments
        initEquipment(i);
        if (state.agreementType === 1 || state.agreementType === 2) {
            recalculateAndRenderRentalValues();
        } else {
            recalculateValuesAndRender();
        }
    }

    if (equipments < 1) {
        addEquipment();
    }

    return {
        addEquipment: addEquipment
    }
})