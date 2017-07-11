module.exports('equipment', function (require) {

    var state = require('state').state;
    var templateFactory = require('equipment-template');
    var recalculateValuesAndRender = require('rate-cards').recalculateValuesAndRender;
    var recalculateAndRenderRentalValues = require('rate-cards').recalculateAndRenderRentalValues;

    /**
     * Add new equipment ot list of new equipments
     * Takes template of equipment replace razor generated ids, names with new id index
     * @returns {} 
     */
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

        //create new template with appropiate id and names
        var newTemplate = templateFactory($('<div></div>'),
            {
                id: id,
                templateId: 'new-equipment-base',
                equipmentName: 'NewEquipment',
                equipmentIdPattern: 'new-equipment-',
                equipmentDiv: 'div#new-equipments'
            });

        if (id > 0) {
            newTemplate.find('div.additional-remove').attr('id', 'addequipment-remove-' + id);
        }

        state.equipments[newId].template = newTemplate;

        // equipment handlers
        newTemplate.find('.equipment-cost').on('change', updateCost);
        newTemplate.find('#addequipment-remove-' + id).on('click', setRemoveEquipment(
            {
                name: 'equipments',
                equipmentIdPattern: 'new-equipment-',
                equipmentName: 'NewEquipment',
                equipmentRemovePattern: 'addequipment-remove-'
            }));
            
        newTemplate.find('.monthly-cost').on('change', updateMonthlyCost);

        customizeSelect();
        toggleClearInputIcon($(newTemplate).find('textarea, input'));
        resetPlacehoder($(newTemplate).find('textarea, input'));

        $('#new-equipments').append(newTemplate);

        resetFormValidator("#equipment-form");
    };

    /**
     * Add new equipment ot list of existing equipments
     * Takes template of equipment replace razor generated ids, names with new id index
     * @returns {} 
     */
    var addExistingEquipment = function() {
        var id = $('div#existing-equipments').find('[id^=existing-equipment-]').length;
        var newId = id.toString();
        state.existingEquipments[newId] = {
            id: newId,
            type: '',
            description: '',
            cost: '',
            monthlyCost: ''
        };

        var newTemplate = templateFactory($('<div></div>'),
            {
                id: id,
                templateId: 'existing-equipment-base',
                equipmentName: 'ExistingEquipment',
                equipmentIdPattern: 'existing-equipment-',
                equipmentDiv: 'div#existing-equipments'
            });

        state.existingEquipments[newId].template = newTemplate;

        newTemplate.find('div.additional-remove').attr('id', 'remove-existing-equipment-' + id);

        newTemplate.find('#remove-existing-equipment-' + id).on('click', setRemoveEquipment(
            {
                name: 'existingEquipments',
                equipmentIdPattern: 'existing-equipment-',
                equipmentName: 'ExistingEquipment',
                equipmentRemovePattern: 'remove-existing-equipment-'
            }));

        customizeSelect();
        toggleClearInputIcon($(newTemplate).find('textarea, input'));
        resetPlacehoder($(newTemplate).find('textarea, input'));

        $('#existing-equipments').append(newTemplate);
        resetFormValidator("#equipment-form");
    }

    function setRemoveEquipment(options) {
        return function(e) {
            removeEquipment.call(this, options);
        }
    }

    /**
     * remove equipment form list of new/existing equipments
     * and update indexs, id, names
     * in case of new equipment we recalculate our cost values
     * @param {Object<>} options - object of predefinded values for 
     *  equipment object
     *  options.name - key in global state values [equipments, existingEquipments]
     *  options.equipmentIdPattern - common name of id value for new/existing equipment. 
     *      with this name and id we search for specific equipment [new-equipment-, existing-equipment- ]
     *  options.equipmentRemovePattern - common name of id value for removing new/existing equipment [remove-existing-equipment-, addequipment-remove-]
     *  options.equipmentName - common name of razor generated values for new/existing equipment [NewEquipment, ExistingEquipment]
     * @returns {} 
     */
    function removeEquipment(options) {
        var fullId = $(this).attr('id');
        var id = fullId.substr(fullId.lastIndexOf('-') + 1);
        if (!state[options.name].hasOwnProperty(id)) {
            return;
        }
        $('#' + options.equipmentIdPattern + id).remove();
        delete state[options.name][id];

        var nextId = Number(id);
        while (true) {
            nextId++;
            var nextEquipment = $('#' + options.equipmentIdPattern + nextId);
            if (!nextEquipment.length) { break; }

            updateLabelIndex(nextEquipment, nextId, options.equipmentName);
            updateInputIndex(nextEquipment, nextId, options.equipmentName);
            updateValidationIndex(nextEquipment, nextId, options.equipmentName);
            updateButtonIndex(nextEquipment, nextId, options.equipmentIdPattern, options.equipmentRemovePattern);

            state[options.name][nextId].id = (nextId - 1).toString();
            state[options.name][nextId - 1] = state[options.name][nextId];
            delete state[options.name][nextId];
        }

        if (options.name === 'equipments') {
            if (state.agreementType === 1 || state.agreementType === 2) {
                recalculateAndRenderRentalValues();
            } else {
                recalculateValuesAndRender();
            }
        }
    };

    /**
     * initialize and save new equipments which we get from server
     * to our global state object
     * @param {number} i - new id for new equipment 
     * @returns {} 
     */
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

    /**
     * initialize and save existing equipments which we get from server
     * to our global state object
     * @param {number} i - new id for new equipment 
     * @returns {} 
     */
    function initExistingEquipment(i) {
        state.existingEquipments[i] = { id: i.toString() };
    }

    function resetFormValidator(formId) {
        $(formId).removeData('validator');
        $(formId).removeData('unobtrusiveValidation');
        $.validator.unobtrusive.parse($(formId));
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

    function updateLabelIndex(selector, index, name) {
        var labels = selector.find('label');
        labels.each(function () {
            $(this).attr('for', $(this).attr('for').replace(name + '_' + index, name + '_' + (index - 1)));
        });
    }

    function updateInputIndex(selector, index, name) {
        var inputs = selector.find('input, select, textarea');

        inputs.each(function () {
            $(this).attr('id', $(this).attr('id').replace(name + '_' + index, name + '_' + (index - 1)));
            $(this).attr('name', $(this).attr('name').replace(name + '[' + index, name + '[' + (index - 1)));
        });
    }

    function updateValidationIndex(selector, index, name) {
        var spans = selector.find('span');

        spans.each(function () {
            var valFor = $(this).attr('data-valmsg-for');
            if (valFor == null) { return; }
            $(this).attr('data-valmsg-for', valFor.replace(name + '[' + index, name + '[' + (index - 1)));
        });
    }

    function updateButtonIndex(selector, index, equipmentPatternId, equipmentRemovePattern) {
        selector.find('.equipment-number').text('№' + index);
        var removeButton = selector.find('#' + equipmentRemovePattern + index);
        removeButton.attr('id', equipmentRemovePattern + (index - 1));
        selector.attr('id', equipmentPatternId + (index - 1));
    }

    var equipments = $('div#new-equipments').find('[id^=new-equipment-]').length;
    var existingEquipments = $('div#existing-equipments').find('[id^=existing-equipment-]').length;

    for (var j = 0; j < existingEquipments; j++) {
        initExistingEquipment(j);
    }

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
        addEquipment: addEquipment,
        addExistingEquipment: addExistingEquipment
    }
})