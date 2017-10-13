module.exports('onboarding.owner-info.index', function (require) {
    var setters = require('onboarding.owner-info.setters');
    var constants = require('onboarding.state').constants;
    var aknwoledgmentSetters = require('onboarding.ackonwledgment.setters');
    var additionalOwner = require('onboarding.owner-info.additional');
    var aknwoledgmentOwner = require('onboarding.ackonwledgment.owners');
    var assignOwnerDatepicker = require('onboarding.owner-info.conversion').assignOwnerDatepicker;
    var setLengthLimitedField = require('onboarding.setters').setLengthLimitedField;
    var enableSubmit = require('onboarding.setters').enableSubmit;
    var ownersMoveToNextSection = require('onboarding.owner-info.setters').moveToNextSection;

    function _setInputHandlers (ownerNumber) {
        $('#' + ownerNumber + '-firstname')
            .on('change', setters.setFirstName(ownerNumber));
        $('#' + ownerNumber + '-firstname')
            .on('change', aknwoledgmentSetters.setFirstName(ownerNumber));
        $('#' + ownerNumber + '-lastname')
            .on('change', setters.setLastName(ownerNumber));
        $('#' + ownerNumber + '-lastname')
            .on('change', aknwoledgmentSetters.setLastName(ownerNumber));
        $('#' + ownerNumber + '-homephone')
            .on('change', setters.setHomePhone(ownerNumber))
            .on('keyup', setLengthLimitedField(10));
        $('#' + ownerNumber + '-cellphone')
            .on('change', setters.setCellPhone(ownerNumber))
            .on('keyup', setLengthLimitedField(10));
        $('#' + ownerNumber + '-email')
            .on('change', setters.setEmailAddress(ownerNumber))
            .on('keyup', setLengthLimitedField(50));
        $('#' + ownerNumber + '-street')
            .on('change', setters.setStreet(ownerNumber))
            .on('keyup', setLengthLimitedField(100));
        $('#' + ownerNumber + '-unit')
            .on('change', setters.setUnit(ownerNumber))
            .on('keyup', setLengthLimitedField(10));
        $('#' + ownerNumber + '-city')
            .on('change', setters.setCity(ownerNumber))
            .on('keyup', setLengthLimitedField(50));
        $('#' + ownerNumber + '-postalcode')
            .on('change', setters.setPostalCode(ownerNumber))
            .on('keyup', setLengthLimitedField(6));
        $('#' + ownerNumber + '-province').on('change', setters.setProvince(ownerNumber));
        $('#' + ownerNumber + '-percentage')
            .on('change', setters.setOwnershipPercentege(ownerNumber))
            .on('keyup', setLengthLimitedField(3));
        $('#' + ownerNumber + '-agreement').on('change', aknwoledgmentSetters.setAgreement(ownerNumber));

        var input = assignOwnerDatepicker('#' + ownerNumber + '-birthdate', ownerNumber);

        setDatepickerDate('#' + ownerNumber + '-birthdate', state['owner-info'].owners[ownerNumber].birthdate);

        if (ownerNumber !== 'owner0') {
            initGoogleServices(ownerNumber + '-street',
                ownerNumber + '-city',
                ownerNumber + '-province',
                ownerNumber + '-postalcode');
            $('#' + ownerNumber + '-street').attr('placeholder', '');
            $('#' + ownerNumber + '-city').attr('placeholder', '');
        }

        $('#' + ownerNumber + '-remove').on('click', function () {
            additionalOwner.remove(ownerNumber);
            aknwoledgmentOwner.remove(ownerNumber);
            if (ownerNumber !== 'owner0') {
                for (var i = ownerNumber.substr(-1);i < state['owner-info']['nextOwnerIndex'];i++) {
                    _setInputHandlers('owner' + i);
                }
                setters.recalculateTotalPercentage();
                ownersMoveToNextSection();
                enableSubmit();
            }
        });
    }

    function _initEventHandlers (numberOfOwners) {
        for (var i = 0;i < numberOfOwners;i++) {
            _setInputHandlers('owner' + i);
            state['owner-info']['nextOwnerIndex']++;
        }

        $('#add-additional').on('click', function () {
            var nextOwner = 'owner' + state['owner-info']['nextOwnerIndex'];
            additionalOwner.add(nextOwner);
            aknwoledgmentOwner.add(nextOwner);
            _setInputHandlers(nextOwner);
        });
    }

    function _setLoadedData (owners) {
        for (var i = 0;i < owners.length;i++) {
            var owner = 'owner' + i;
            var newOwnerState = {};
            newOwnerState[owner] = { requiredFields: constants.requiredFields.slice() };
            $.extend(state['owner-info']['owners'], newOwnerState);

            $.grep(constants.requiredFields, function (field) {

                if (field === 'birthdate' && owners[i]['BirthDate'] !== null) {
                    setters.setBirthDate(owner, owners[i]['BirthDate']);
                }
                var $item = $('#' + owner + '-' + field);
                if ($item.val())
                    $item.change();
            });
        }
    }

    function init (owners) {
        _initEventHandlers(owners !== undefined ? owners.length : 1);

        if (Array.isArray(owners) && owners.length > 0) {
            _setLoadedData(owners);
        }
    }

    function initAutocomplete () {
        for (var i = 0;Object.keys(state['owner-info'].owners).length > i;i++) {
            initGoogleServices('owner' + i + '-street',
                'owner' + i + '-city',
                'owner' + i + '-province',
                'owner' + i + '-postalcode');
        }
        for (var i = 0;Object.keys(state['owner-info'].owners).length > i;i++) {
            $('#owner' + i + '-street').attr('placeholder', '');
            $('#owner' + i + '-city').attr('placeholder', '');
        }
    }

    return {
        init: init,
        initAutocomplete: initAutocomplete
    }
})