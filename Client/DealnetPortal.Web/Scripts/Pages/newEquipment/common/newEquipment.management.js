﻿module.exports('equipment', function (require) {

    var state = require('state').state;
    var conversion = require('newEquipment.conversion');

    var settings = {
        recalculateValuesAndRender: {},
        recalculateAndRenderRentalValues: {},
        recalculateClarityValuesAndRender: {}
    }

    var resetPlaceholder = require('resetPlaceholder');

    /**
     * Add new equipment ot list of new equipments
     * Takes template of equipment replace razor generated ids, names with new id index
     * @returns {void} 
     */
    var addEquipment = function () {
        var id = $('div#new-equipments').find('[id^=new-equipment-]').length;
        var newId = id.toString();
        state.equipments[newId] = {
            id: newId,
            type: '',
            description: '',
            cost: '',
            monthlyCost: '',
            estimatedRetail: ''
        };

        //create new template with appropiate id and names
        var newTemplate = conversion.createItem({
            id: id,
            templateId: 'new-equipment-base',
            equipmentName: 'NewEquipment',
            equipmentIdPattern: 'new-equipment-',
            equipmentDiv: 'div#new-equipments'
        });

        if (id > 0) {
            newTemplate.find('div.additional-remove').attr('id', 'addequipment-remove-' + id);
        }
        if (id === 2) {
            $('.add-equip-link').addClass("hidden");
        }
        state.equipments[newId].template = newTemplate;

        // equipment handlers
        newTemplate.find('.equipment-cost').on('change', updateCost);
        newTemplate.find('#addequipment-remove-' + id).on('click', _initRemoveNewEquipment(id));

        newTemplate.find('.monthly-cost')
            .on('change', updateMonthlyCost);
        newTemplate.find('.estimated-retail')
            .on('change', updateEstimatedRetail);
        var equipSelect = newTemplate.find('.equipment-select');
        equipSelect.on('change', updateType);
        if (!state.isClarity) {
            equipSelect
                .on('change', require('bill59').onEquipmentChange);
        }
        equipSelect.change();

        customizeSelect();
        toggleClearInputIcon($(newTemplate).find('textarea, input'));
        resetPlaceholder($(newTemplate).find('textarea, input'));

        $('#new-equipments').append(newTemplate);
        resetFormValidator("#equipment-form");

        $('#new-equipments').find('.monthly-cost').each(function () {
            $(this).rules('add', {
                required: true,
                messages: {
                    required: function (ele) {
                        return translations.ThisFieldIsRequired;
                    }
                }
            });
        });

    };

    /**
     * Add new equipment ot list of existing equipments
     * Takes template of equipment replace razor generated ids, names with new id index
     * @returns {void} 
     */
    var addExistingEquipment = function () {
        var id = $('div#existing-equipments').find('[id^=existing-equipment-]').length;
        var newId = id.toString();
        state.existingEquipments[newId] = {
            id: newId,
            type: '',
            description: '',
            cost: '',
            monthlyCost: ''
        };

        var newTemplate = conversion.createItem({
            id: id,
            templateId: 'existing-equipment-base',
            equipmentName: 'ExistingEquipment',
            equipmentIdPattern: 'existing-equipment-',
            equipmentDiv: 'div#existing-equipments'
        });

        state.existingEquipments[newId].template = newTemplate;

        newTemplate.find('div.additional-remove').attr('id', 'remove-existing-equipment-' + id);

        newTemplate.find('#remove-existing-equipment-' + id).on('click',
            function () {
                var options = {
                    name: 'existingEquipments',
                    equipmentIdPattern: 'existing-equipment-',
                    equipmentName: 'ExistingEquipment',
                    equipmentRemovePattern: 'remove-existing-equipment-'
                };

                conversion.removeItem.call(this, options);
            });

        newTemplate.find('textarea').keyup(function (e) {
            if (e.keyCode === 13) {
                var textarea = $(this);
                textarea.val(textarea.val() + '\n');
            }
        });

        if (!state.isClarity) {
            newTemplate.find('.responsible-dropdown')
                .on('change', require('bill59').onResposibilityChange);
        }

        customizeSelect();
        toggleClearInputIcon($(newTemplate).find('textarea, input'));
        resetPlaceholder($(newTemplate).find('textarea, input'));

        $('#existing-equipments').append(newTemplate);
        resetFormValidator("#equipment-form");
        if (!state.isClarity) {
            require('bill59').enableExistingEquipment();
        }
    };

    /**
     * initialize and save new equipments which we get from server
     * to our global state object
     * @param {number} i - new id for new equipment 
     * @returns {void} 
     */
    function initEquipment(i) {
        var cost = $('#NewEquipment_' + i + '__Cost').length ?
            Globalize.parseNumber($('#NewEquipment_' + i + '__Cost').val()) :
            0;
        if (state.equipments[i] === undefined) {
            state.equipments[i] = {
                id: i.toString(),
                cost: cost,
                type: $('#NewEquipment_' + i + '__Type').val()
            };
        } else {
            state.equipments[i].cost = cost;
        }
        cost = Globalize.parseNumber($('#NewEquipment_' + i + '__MonthlyCost').val());
        if (state.equipments[i] === undefined) {
            state.equipments[i] = {
                id: i.toString(),
                monthlyCost: cost,
                type: $('#NewEquipment_' + i + '__Type').val()
            };
        } else {
            state.equipments[i].monthlyCost = cost;
        }
        var retail = $('#NewEquipment_' + i + '__EstimatedRetailCost')
        if (retail.length) {
            cost = Globalize.parseNumber(retail.val());
            if (state.equipments[i] === undefined) {
                state.equipments[i] = {
                    id: i.toString(),
                    estimatedRetail: cost,
                    type: $('#NewEquipment_' + i + '__Type').val()
                };
            } else {
                state.equipments[i].estimatedRetail = cost;
            }
        }

        var equipmentRow = $('#new-equipment-' + i);
        state.equipments[i].template = equipmentRow;
        var equipSelect = equipmentRow.find('.equipment-select');

        equipSelect.on('change', updateType);
        if (!state.isClarity) {
            equipSelect.on('change', require('bill59').onEquipmentChange);
        }
        equipmentRow.find('.equipment-cost')
            .on('change', updateCost);
        equipmentRow.find('.monthly-cost')
            .on('change', updateMonthlyCost);
        equipmentRow.find('.estimated-retail')
            .on('change', updateEstimatedRetail);

        customizeSelect();
        //if not first equipment add handler (first equipment should always be visible)
        if (i > 0) {
            $('#addequipment-remove-' + i).on('click', _initRemoveNewEquipment(i));
        }
        if (i === 2) {
            $('.add-equip-link').addClass("hidden");
        }
    }

    /**
     * initialize and save existing equipments which we get from server
     * to our global state object
     * @param {number} i - new id for new equipment 
     * @returns {void} 
     */
    function initExistingEquipment(i) {
        state.existingEquipments[i] = {
            id: i.toString()
        };
        $('#remove-existing-equipment-' + i).on('click', function () {
            var options = {
                name: 'existingEquipments',
                equipmentIdPattern: 'existing-equipment-',
                equipmentName: 'ExistingEquipment',
                equipmentRemovePattern: 'remove-existing-equipment-'
            };

            conversion.removeItem.call(this, options);
        });

        if (!state.isClarity) {
            $('#existing-equipment-' + i + ' .responsible-dropdown')
                .on('change', require('bill59').onResposibilityChange);
        }
    }

    function resetFormValidator(formId) {
        $(formId).removeData('validator');
        $(formId).removeData('unobtrusiveValidation');
        $.validator.unobtrusive.parse($(formId));
    }

    /**
     * update equipment type in global state object
     * needed to update Existing equipment 
     * @returns {void}
     */
    function updateType() {
        var mvcId = $(this).attr('id');
        var id = mvcId.split('__Type')[0].substr(mvcId.split('__Type')[0].lastIndexOf('_') + 1);
        state.equipments[id].type = this.value;
    }

    /**
     * update cost of equipment in our global state object
     * on cost changed we recalulate global cost
     * method uses only for Loan agreement type
     * @returns {void} 
     */
    function updateCost() {
        var mvcId = $(this).attr("id");
        var id = mvcId.split('__Cost')[0].substr(mvcId.split('__Cost')[0].lastIndexOf('_') + 1);
        state.equipments[id].cost = Globalize.parseNumber($(this).val());
        if (state.agreementType === 1 || state.agreementType === 2) {
            settings.recalculateAndRenderRentalValues();
        } else {
            settings.recalculateValuesAndRender();
        }
    }

    /**
     * update monthly cost of equipment in our global state object
     * on cost changed we recalulate global cost
     * method uses only for Rental/RentalHwt agreement type
     * @returns {void} 
     */
    function updateMonthlyCost() {
        var mvcId = $(this).attr("id");
        var id = mvcId.split('__MonthlyCost')[0].substr(mvcId.split('__MonthlyCost')[0].lastIndexOf('_') + 1);
        state.equipments[id].monthlyCost = Globalize.parseNumber($(this).val());
        if (state.agreementType === 3) {
            settings.recalculateClarityValuesAndRender();
        } else {
            if (state.agreementType === 0) {
                settings.recalculateValuesAndRender();
            } else {
                settings.recalculateAndRenderRentalValues();
            }
        }
    }

    /**
     * update estimated retail cost of equipment in our global state object
     * method is used for Rental/RentalHwt agreement type in Ontario 
     * @returns {void}
     */
    function updateEstimatedRetail() {
        var $this = $(this);
        var mvcId = $this.attr("id");
        var id = mvcId.split('__EstimatedRetailCost')[0].substr(mvcId.split('__EstimatedRetailCost')[0].lastIndexOf('_') + 1);
        state.equipments[id].estimatedRetail = Globalize.parseNumber($this.val());
    }

    function _initRemoveNewEquipment(i) {
        return function removeNewEquipment() {
            var options = {
                name: 'equipments',
                equipmentIdPattern: 'new-equipment-',
                equipmentName: 'NewEquipment',
                equipmentRemovePattern: 'addequipment-remove-'
            };
            conversion.removeItem.call(this, options);

            if (state.agreementType !== 3) {
                if (state.agreementType === 1 || state.agreementType === 2) {
                    settings.recalculateAndRenderRentalValues();
                } else {
                    settings.recalculateValuesAndRender();
                }
            } else {
                settings.recalculateClarityValuesAndRender();
            }
            $('.add-equip-link').removeClass("hidden");
            !state.isClarity && require('bill59').onDeleteEquipment();
        }
    }

    function _initExistingEquipment() {
        var existingEquipments = $('div#existing-equipments').find('[id^=existing-equipment-]').length;
        for (var j = 0; j < existingEquipments; j++) {
            initExistingEquipment(j);
        }
    }

    function _initNewEquipment() {
        var equipments = $('div#new-equipments').find('[id^=new-equipment-]').length;
        for (var i = 0; i < equipments; i++) {
            initEquipment(i);
        }

        if (equipments < 1) {
            addEquipment();
        }
    }

    function init(params) {

        if (!params.isClarity) {
            settings.recalculateAndRenderRentalValues = params.recalculateAndRenderRentalValues;
            settings.recalculateValuesAndRender = params.recalculateValuesAndRender;
        } else {
            settings.recalculateClarityValuesAndRender = params.recalculateClarityValuesAndRender;
        }

        _initExistingEquipment();
        _initNewEquipment();
        if (!state.isClarity) {
            require('bill59').enableExistingEquipment();
        }
    }

    return {
        init: init,
        addEquipment: addEquipment,
        addExistingEquipment: addExistingEquipment
    };
});