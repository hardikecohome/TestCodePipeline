module.exports('bill59', function (require) {

    var state = require('state').state;
    var idToValue = require('idToValue');

    var settings = {
        salesRepTypesId: '#sales-rep-types',
        salesRepTitleId: '#sales-rep-title',
        initiatedContractId: '#initiated-contract',
        initiatedContractCheckboxId: '#initiated-contract-checkbox',
        negotiatedAgreementId: '#negotiated-agreement',
        negotiatedAgreementCheckboxId: '#negotiated-agreement-checkbox',
        concludedAgreementId: '#concluded-agreement',
        concludedAgreementCheckboxId: '#concluded-agreement-checkbox',
        descriptionColClass: '.description-col',
        monthlyColClass: '.monthly-cost-col',
        estimatedRetailColClass: '.estimated-retail-col',
        estimatedRetailClass: '.estimated-retail',
        newEquipmentSelector: 'div#new-equipments [id^="new-equipment-"]',
        equipmentSelectClass: '.equipment-select',
        newEuqipmentClass: '.new-equipment',
        responsibleColClass: '.responsible-col',
        responsibleOtherColClass: '.responsible-other-col',
        responsibleOtherClass: '.responsible-other',
        responsibleDropdownClass: '.responsible-dropdown',
        responsibleDropdownColClass: '.responsible-dropdown-col',
    }

    var enableForAll = function () {
        if (state.isOntario && state.agreementType != 0) {
            var newEquipment = $(settings.newEquipmentSelector);
            $.each(newEquipment, function (i, el) {
                var $el = $(el);
                if (_equipmentInList($el.find(settings.equipmentSelectClass).val())) {
                    _enableNewEquipment($el);
                    if ($(settings.salesRepTypesId).is(':hidden')) _enableSalesRepSection();
                }
            });
        }
    };

    var disableForAll = function () {
        if (state.isOntario && state.agreementType === 0) {
            var newEquipment = $(settings.newEquipmentSelector);
            $.each(newEquipment, function (i, el) {
                _disableNewEquipment(el);
                if (!$(settings.salesRepTypesId).is(':hidden')) _disableSalesRepSection();
            });
        }
    };

    var onEquipmentChange = function (e) {
        if (state.isOntario && state.agreementType !== 0) {
            if (_equipmentInList(e.target.value)) {
                _enableNewEquipment($(e.target).parents(settings.newEuqipmentClass));
                _enableSalesRepSection();
            } else {
                _disableNewEquipment($(e.target).parents(settings.newEuqipmentClass));

                if (!_isBill59EquipmentSelected()) {
                    _disableSalesRepSection();
                }
            }
        }
    }

    var onResposibilityChange = function (e) {
        var otherCol = $(e.target).parents(settings.responsibleColClass).find(settings.responsibleOtherColClass);
        var $input = otherCol.find(settings.responsibleOtherClass);
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

    var onDeleteEquipment = function () {
        if (!_isBill59EquipmentSelected()) {
            _disableSalesRepSection();
            _disableExistingEquipment();
        }
    };

    var enableExistingEquipment = function () {
        if (state.isOntario && state.agreementType !== 0) {
            if (_equipmentInListSelected()) {
                Object.keys(state.existingEquipments)
                    .map(idToValue(state.existingEquipments))
                    .forEach(function (equip) {
                        var $equip = $('#existing-equipment-' + equip.id);
                        $equip.find(settings.responsibleColClass).removeClass('hidden');
                        var $dropdown = $equip.find(settings.responsibleDropdownClass);
                        $dropdown.prop('disabled', false);
                        $dropdown[0].form && $dropdown.rules('add', 'required');
                        $dropdown.change();
                    });
            }
        }
    };

    function _isBill59EquipmentSelected() {
        return Object.keys(state.equipments)
            .map(function (k) {
                return state.equipments[k].type
            })
            .some(function (i) {
                return state.bill59Equipment.indexOf(i) !== -1
            });
    }

    function _equipmentInList(id) {
        return state.bill59Equipment.indexOf(id) > -1;
    };

    function _equipmentInListSelected() {
        return Object.keys(state.equipments)
            .map(idToValue(state.equipments))
            .map(function (item) {
                return item.type;
            }).reduce(function (acc, item) {
                return acc || _equipmentInList(item);
            }, false);
    };

    function _enableNewEquipment(row) {
        var $row = $(row);
        $row.find(settings.descriptionColClass).removeClass('col-md-6').addClass('col-md-5');
        $row.find(settings.monthlyColClass).removeClass('col-md-3').addClass('col-md-2');
        $row.find(settings.estimatedRetailColClass).removeClass('hidden');

        var input = $row.find(settings.estimatedRetailClass);
        input.prop('disabled', false);
        input[0].form && input.rules('add', 'required');
        enableExistingEquipment();
    };

    function _disableNewEquipment(row) {
        var $row = $(row);
        $row.find(settings.descriptionColClass).removeClass('col-md-5').addClass('col-md-6');
        $row.find(settings.monthlyColClass).removeClass('col-md-2').addClass('col-md-3');
        $row.find(settings.estimatedRetailColClass).addClass('hidden');

        var input = $row.find(settings.estimatedRetailClass);
        input.prop('disabled', true);
        input[0].form && input.rules('remove', 'required');
        _disableExistingEquipment();
    };

    function _enableSalesRepSection() {
        $(settings.salesRepTypesId).removeClass('hidden');
        $(settings.salesRepTitleId).removeClass('hidden');
    }

    function _disableSalesRepSection() {
        if ($(settings.salesRepTypesId).is(':visible')) {
            $(settings.salesRepTypesId).addClass('hidden');
            $(settings.salesRepTitleId).addClass('hidden');
            $(settings.initiatedContractId).val(null);
            $(settings.initiatedContractCheckboxId).prop('checked', false);
            $(settings.negotiatedAgreementId).val(null);
            $(settings.negotiatedAgreementCheckboxId).prop('checked', false);
            $(settings.concludedAgreementId).val(null);
            $(settings.concludedAgreementCheckboxId).prop('checked', false);
        }
    }

    function _disableExistingEquipment() {
        if (state.isOntario && state.agreementType === 0 || !_equipmentInListSelected()) {
            Object.keys(state.existingEquipments)
                .map(idToValue(state.existingEquipments))
                .forEach(function (equip) {
                    var $equip = $('#existing-equipment-' + equip.id);
                    $equip.find(settings.responsibleColClass)
                        .addClass('hidden');
                    var $dropdown = $equip.find(settings.responsibleDropdownColClass);
                    $dropdown.val('').change();
                    $dropdown.attr('disabled', true);
                    $dropdown[0].form && $dropdown.rules('remove', 'required');
                });
        }
    };

    return {
        enableForAll: enableForAll,
        disableForAll: disableForAll,
        onEquipmentChange: onEquipmentChange,
        onResposibilityChange: onResposibilityChange,
        enableExistingEquipment: enableExistingEquipment,
        onDeleteEquipment: onDeleteEquipment
    };
});