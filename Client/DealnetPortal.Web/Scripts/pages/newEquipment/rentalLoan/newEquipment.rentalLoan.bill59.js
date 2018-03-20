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

    var init = function () {
        $('#typeOfAgreementSelect').on('change', onAggrementChange);
        _toggleForAll();
    };

    var onAggrementChange = function (e) {
        var value = +e.target.value;
        value === 0 ? _disableForAll() : _enableForAll();
    }
    var onEquipmentChange = function (e) {
        _toggleNewEquipment($(e.target).parents(settings.newEuqipmentClass));
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
        toggleExistingEquipment();
        _toggleSalesRepSection();
    };

    var toggleExistingEquipment = function () {
        _shouldEnable() ?
            _enableExistingEquipment() :
            _disableExistingEquipment();
    }

    function _shouldEnable() {
        return state.isOntario && _isRentalSelected() && _isBill59EquipmentSelected();
    }

    function _isRentalSelected() {
        return state.agreementType !== 0;
    }

    function _toggleForAll() {
        _shouldEnable() ?
            _enableForAll() :
            _disableForAll();
    }

    function _enableForAll() {
        var newEquipment = $(settings.newEquipmentSelector);
        $.each(newEquipment, function (i, el) {
            var $el = $(el);
            if (_isEquipmentInList($el.find(settings.equipmentSelectClass).val())) {
                _enableNewEquipment($el);
            }
        });
        if (_isBill59EquipmentSelected()) {
            if (_isSalesRepInfoHidden()) _enableSalesRepSection();
            _enableExistingEquipment();
        }
    };

    function _disableForAll() {
        var newEquipment = $(settings.newEquipmentSelector);
        $.each(newEquipment, function (i, el) {
            _disableNewEquipment(el);
        });
        if (!_isSalesRepInfoHidden()) _disableSalesRepSection();
        _disableExistingEquipment();
    };

    function _isBill59EquipmentSelected() {
        return Object.keys(state.equipments)
            .map(function (k) {
                return state.equipments[k].type;
            })
            .some(function (i) {
                return _isEquipmentInList(i);
            });
    }

    function _isEquipmentInList(id) {
        return state.bill59Equipment.indexOf(id) > -1;
    }

    function _toggleNewEquipment(row) {
        state.isOntario && _isRentalSelected() && _isEquipmentInList(row.find(settings.equipmentSelectClass).val()) ?
            _enableNewEquipment(row) :
            _disableNewEquipment(row);
    }

    function _enableNewEquipment(row) {
        var $row = $(row);
        $row.find(settings.descriptionColClass).removeClass('col-md-6').addClass('col-md-5');
        $row.find(settings.monthlyColClass).removeClass('col-md-3').addClass('col-md-2');
        $row.find(settings.estimatedRetailColClass).removeClass('hidden');

        var input = $row.find(settings.estimatedRetailClass);
        input.prop('disabled', false);
        input[0].form && input.rules('add', 'required');
        toggleExistingEquipment();
        _toggleSalesRepSection();
    };

    function _disableNewEquipment(row) {
        var $row = $(row);
        $row.find(settings.descriptionColClass).removeClass('col-md-5').addClass('col-md-6');
        $row.find(settings.monthlyColClass).removeClass('col-md-2').addClass('col-md-3');
        $row.find(settings.estimatedRetailColClass).addClass('hidden');

        var input = $row.find(settings.estimatedRetailClass);
        input.prop('disabled', true);
        input[0].form && input.rules('remove', 'required');
        toggleExistingEquipment();
        _toggleSalesRepSection();
    };

    function _toggleSalesRepSection() {
        _shouldEnable() ?
            _enableSalesRepSection() :
            _disableSalesRepSection();
    }

    function _enableSalesRepSection() {
        $(settings.salesRepTypesId).removeClass('hidden');
        $(settings.salesRepTitleId).removeClass('hidden');
    }

    function _disableSalesRepSection() {
        $(settings.salesRepTypesId).addClass('hidden');
        $(settings.salesRepTitleId).addClass('hidden');
        $(settings.initiatedContractId).val('False');
        $(settings.initiatedContractCheckboxId).prop('checked', false);
        $(settings.negotiatedAgreementId).val('False');
        $(settings.negotiatedAgreementCheckboxId).prop('checked', false);
        $(settings.concludedAgreementId).val('False');
        $(settings.concludedAgreementCheckboxId).prop('checked', false);
    }

    function _isSalesRepInfoHidden() {
        return $(settings.salesRepTypesId).is(':hidden');
    }

    function _enableExistingEquipment() {
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
    };

    function _disableExistingEquipment() {
        Object.keys(state.existingEquipments)
            .map(idToValue(state.existingEquipments))
            .forEach(function (equip) {
                var $equip = $('#existing-equipment-' + equip.id);
                $equip.find(settings.responsibleColClass)
                    .addClass('hidden');
                var $dropdown = $equip.find(settings.responsibleDropdownClass);
                $dropdown.val('').change();
                $dropdown.attr('disabled', true);
                $dropdown[0].form && $dropdown.rules('remove', 'required');
            });
    }

    return {
        init: init,
        onEquipmentChange: onEquipmentChange,
        onResposibilityChange: onResposibilityChange,
        toggleExistingEquipment: toggleExistingEquipment,
        onDeleteEquipment: onDeleteEquipment
    };
});