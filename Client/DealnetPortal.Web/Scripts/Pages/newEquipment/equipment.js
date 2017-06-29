module.exports('equipment', function (require) {

    var state = require('state').state;
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
            monthlyCost:''
        };

        var newTemplate = equipmentTemplateFactory($('<div></div>'), { id: id });
        if (id > 0) {
            newTemplate.find('div.additional-remove').attr('id', 'addequipment-remove-' + id);
        }
        state.equipments[newId].template = newTemplate;

        // equipment handlers
        newTemplate.find('.equipment-cost').on('change', updateCost);
        newTemplate.find('#addequipment-remove-' + id).on('click', removeEquipment);
        newTemplate.find('.monthly-cost').on('change', updateMonthlyCost);

        customizeSelect();
        toggleClearInputIcon($(newTemplate).find('textarea, input'));
        resetPlacehoder($(newTemplate).find('textarea, input'));

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

            updateLabelIndex(nextEquipment, nextId);
            updateInputIndex(nextEquipment, nextId);
            updateValidationIndex(nextEquipment, nextId);
            updateButtonIndex(nextEquipment, nextId);

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
        cost = parseFloat($('#NewEquipment_' + i + '__MonthlyCost').val());
        if (state.equipments[i] === undefined) {
            state.equipments[i] = { id: i.toString(), monthlyCost: cost }
        } else {
            state.equipments[i].monthlyCost = cost;
        }

        $('#new-equipment-' + i).find('.equipment-cost').on('change', updateCost);
        $('#new-equipment-' + i).find('.monthly-cost').on('change', updateMonthlyCost);
        customizeSelect();
        //if not first equipment add handler (first equipment should always be visible)
        if (i > 0) {
            $('#addequipment-remove-' + i).on('click', removeEquipment);
        }
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

    function updateMonthlyCost() {
        var mvcId = $(this).attr("id");
        var id = mvcId.split('__MonthlyCost')[0].substr(mvcId.split('__MonthlyCost')[0].lastIndexOf('_') + 1);
        state.equipments[id].monthlyCost = parseFloat($(this).val());
        if (state.agreementType === 0) {
            recalculateValuesAndRender();
        } else {
            recalculateAndRenderRentalValues();
        }
    }

    function updateLabelIndex(selector, index) {
        var labels = selector.find('label');
        labels.each(function () {
            $(this).attr('for', $(this).attr('for').replace('NewEquipment_' + index, 'NewEquipment_' + index - 1));
        });
    }

    function updateInputIndex(selector, index) {
        var inputs = selector.find('input, select, textarea');

        inputs.each(function () {
            $(this).attr('id', $(this).attr('id').replace('NewEquipment_' + index, 'NewEquipment_' + (index - 1)));
            $(this).attr('name', $(this).attr('name').replace('NewEquipment[' + index, 'NewEquipment[' + (index - 1)));
        });
    }

    function updateValidationIndex(selector, index) {
        var spans = selector.find('span');

        spans.each(function () {
            var valFor = $(this).attr('data-valmsg-for');
            if (valFor == null) { return; }
            $(this).attr('data-valmsg-for', valFor.replace('NewEquipment[' + index, 'NewEquipment[' + (index - 1)));
        });
    }

    function updateButtonIndex(selector, index) {
        selector.find('.equipment-number').text('№' + index);
        var removeButton = selector.find('#addequipment-remove-' + index);
        removeButton.attr('id', 'addequipment-remove-' + (index - 1));
        selector.attr('id', 'new-equipment-' + (index - 1));
    }

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