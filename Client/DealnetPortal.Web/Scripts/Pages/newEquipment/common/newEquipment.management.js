module.exports('equipment', function (require) {

    var state = require('state').state;
    var conversion = require('newEquipment.conversion');

    var settings = {
        recalculateValuesAndRender: {},
        recalculateAndRenderRentalValues: {},
        recalculateClarityValuesAndRender: {},
        updateEquipmentSubTypes: function () {},
        configureMonthlyCostCaps: function () {}
    };

    var resetPlaceholder = require('resetPlaceholder');
    var idToValue = require('idToValue');

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
        newTemplate.find('#addequipment-remove-' + id).on('click', _removeNewEquipment);

        newTemplate.find('.monthly-cost')
            .on('change', updateMonthlyCost);
        newTemplate.find('.estimated-retail')
            .on('change', updateEstimatedRetail);
        var equipSelect = newTemplate.find('.equipment-select');

        equipSelect.html(_getEquipmentTypeSelectList());

        equipSelect.on('change', updateType);
        if (!state.isClarity) {
            equipSelect
                .on('change', require('bill59').onEquipmentChange);
        }
        equipSelect.change();
        newTemplate.find('.sub-type-select')
            .on('change', updateSubType);

        if (state.isStandardRentalTier) {
            settings.configureMonthlyCostCaps(newTemplate.find('.monthly-cost'));
        }

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

        newTemplate.find('#remove-existing-equipment-' + id).on('click', _removeExistingEquipment);

        newTemplate.find('textarea').keyup(function (e) {
            if (e.keyCode === 13) {
                var textarea = $(this);
                textarea.val(textarea.val() + '\n');
            }
        });
        if (id === 2) {
            $('#addExistingEqipment').addClass("hidden");
        }

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
            require('bill59').toggleExistingEquipment();
        }

        _initExistingEquipmentCommonData(id, newTemplate);
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
        var retail = $('#NewEquipment_' + i + '__EstimatedRetailCost');
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
        equipmentRow.find('.sub-type-select')
            .on('change', updateSubType).change();
        equipmentRow.find('.equipment-cost')
            .on('change', updateCost);
        equipmentRow.find('.monthly-cost')
            .on('change', updateMonthlyCost);
        equipmentRow.find('.estimated-retail')
            .on('change', updateEstimatedRetail);

        if (state.isStandardRentalTier) {
            settings.configureMonthlyCostCaps(equipmentRow.find('.monthly-cost'));
        }

        customizeSelect();
        //if not first equipment add handler (first equipment should always be visible)
        if (i > 0) {
            $('#addequipment-remove-' + i).on('click', _removeNewEquipment);
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
        $('#remove-existing-equipment-' + i).on('click', _removeExistingEquipment);
        var $equip = $('#existing-equipment-' + i);
        _initExistingEquipmentCommonData(i, $equip);

        if (!state.isClarity) {
            $equip.find('.responsible-dropdown')
                .on('change', require('bill59').onResposibilityChange);
        }
        if (i === 2) {
            $('#addExistingEqipment').addClass("hidden");
        }
    }

    function _initExistingEquipmentCommonData(id, $equip) {
        if (id > 0) {
            $equip.find('.common-row').addClass('hidden')
                .find('input, select').each(function (index, el) {
                    $(el).prop('disabled', true);
                });

            // update the current template with values from the first existing equipment
            $equip.find('.customer-owned').val($('.customer-owned')[0].value);
            $equip.find('.rental-company').val($('.rental-company')[0].value);
            $equip.find('.responsible-dropdown').val($('.responsible-dropdown')[0].value);
            $equip.find('.responsible-other').val($('.responsible-other')[0].value);
        }
        $equip.find('.customer-owned').on('change', function (e) {
            $('.customer-owned').val(e.target.value);
            var $company = $equip.find('.rental-company');
            if (e.target.value == 'true') {
                $company.val('');
                $company.prop('disabled', true);
                if ($company[0].form) {
                    $company.rules('remove', 'required');
                    $company.removeAttr('required');
                    $company.valid();
                }
                var dropdown = $('.responsible-dropdown');
                if (dropdown.val() == '1') {
                    dropdown.val('');
                }
                dropdown.find('option[value="1"]').prop('disabled', true);
            } else {
                $company.prop('disabled', false);
                $company.attr('required', 'required');
                $company[0].form && $company.rules('add', 'required');
                $('.responsible-dropdown option[value="1"]').prop('disabled', false);
            }
        }).change();
        $equip.find('.rental-company').on('change', function (e) {
            $('.rental-company').val(e.target.value);
        });
        $equip.find('.responsible-dropdown').on('change', function (e) {
            $('.responsible-dropdown').val(e.target.value);
        });
        $equip.find('.responsible-other').on('change', function (e) {
            $('.responsible-other').val(e.target.value);
        });
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
        var equip = state.equipmentTypes[this.value];
        $('#NewEquipment_' + id + '__TypeId').val(equip.Id);

        state.equipments[id].type = this.value;

        settings.updateEquipmentSubTypes($(this).parents('.new-equipment'), this.value);
    }

    function updateSubType() {
        var mvcId = $(this).attr('id');
        var id = mvcId.split('__EquipmentSubTypeId')[0].substr(mvcId.split('__EquipmentSubTypeId')[0].lastIndexOf('_') + 1);
        state.equipments[id].subType = this.value;
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

    function _removeNewEquipment() {
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

    function _removeExistingEquipment() {
        var options = {
            name: 'existingEquipments',
            equipmentIdPattern: 'existing-equipment-',
            equipmentName: 'ExistingEquipment',
            equipmentRemovePattern: 'remove-existing-equipment-'
        };

        conversion.removeItem.call(this, options);

        var i = this.id.split(options.equipmentRemovePattern)[1];

        if (i == 0) {
            var $equip = $('#existing-equipment-' + i);
            $equip.find('.common-row').removeClass('hidden');
            var custOwned = $equip.find('.customer-owned');
            custOwned.prop('disabled', false);
            if (custOwned.val() != 'true') {
                $equip.find('.rental-company').prop('disabled', false);
            }
        }
        $('#addExistingEqipment').removeClass("hidden");
        !state.isClarity && require('bill59').toggleExistingEquipment();
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

    function _getEquipmentTypeSelectList() {
        return Object.keys(state.equipmentTypes)
            .map(idToValue(state.equipmentTypes))
            .filter(function (type) {
                return state.agreementType == 0 || type.Leased;
            }).sort(function (a, b) {
                return a.Description == b.Description ? 0 :
                    a.Description > b.Description ? 1 : -1;
            }).map(function (type) {
                return $('<option/>', {
                    value: type.Type,
                    text: type.Description
                });
            });
    }

    function init(params) {

        if (!params.isClarity) {
            settings.recalculateAndRenderRentalValues = params.recalculateAndRenderRentalValues;
            settings.recalculateValuesAndRender = params.recalculateValuesAndRender;
            settings.updateEquipmentSubTypes = params.updateEquipmentSubTypes;
            settings.configureMonthlyCostCaps = params.configureMonthlyCostCaps;
        } else {
            settings.recalculateClarityValuesAndRender = params.recalculateClarityValuesAndRender;
        }

        _initExistingEquipment();
        _initNewEquipment();
        if (!state.isClarity) {
            require('bill59').init();
        }
    }

    return {
        init: init,
        addEquipment: addEquipment,
        addExistingEquipment: addExistingEquipment
    };
});