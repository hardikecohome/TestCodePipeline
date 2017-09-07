﻿module.exports('onboarding.owner-info.index', function (require) {
    var setters = require('onboarding.owner-info.setters');
    var constants = require('onboarding.state').constants;
    var aknwoledgmentSetters = require('onboarding.ackonwledgment.setters');
    var additionalOwner = require('onboarding.owner-info.additional');
    var aknwoledgmentOwner = require('onboarding.ackonwledgment.owners');

    function _setInputHandlers (ownerNumber) {
        $('#' + ownerNumber + '-firstname').on('change', setters.setFirstName(ownerNumber));
        $('#' + ownerNumber + '-firstname').on('change', aknwoledgmentSetters.setFirstName(ownerNumber));
        $('#' + ownerNumber + '-lastname').on('change', setters.setLastName(ownerNumber));
        $('#' + ownerNumber + '-lastname').on('change', aknwoledgmentSetters.setLastName(ownerNumber));
        $('#' + ownerNumber + '-homephone').on('change', setters.setHomePhone(ownerNumber));
        $('#' + ownerNumber + '-cellphone').on('change', setters.setCellPhone(ownerNumber));
        $('#' + ownerNumber + '-email').on('change', setters.setEmailAddress(ownerNumber));
        $('#' + ownerNumber + '-street').on('change', setters.setStreet(ownerNumber));
        $('#' + ownerNumber + '-unit').on('change', setters.setUnit(ownerNumber));
        $('#' + ownerNumber + '-city').on('change', setters.setCity(ownerNumber));
        $('#' + ownerNumber + '-postalcode').on('change', setters.setPostalCode(ownerNumber));
        $('#' + ownerNumber + '-province').on('change', setters.setProvince(ownerNumber));
        $('#' + ownerNumber + '-percentage').on('change', setters.setOwnershipPercentege(ownerNumber));
        $('#' + ownerNumber + '-agreement').on('click', aknwoledgmentSetters.setAgreement(ownerNumber));

        if (google && google.maps)
            initGoogleServices(ownerNumber + '-street',
                ownerNumber + '-city',
                ownerNumber + '-province',
                ownerNumber + '-postalcode');

        $('#' + ownerNumber + '-remove').on('click', function () {
            additionalOwner.remove(ownerNumber);
            aknwoledgmentOwner.remove(ownerNumber);
            if (ownerNumber !== 'owner0') {
                _setInputHandlers(ownerNumber);
                setters.recalculateTotalPercentage();
            }
        });

        var birth = $('#' + ownerNumber + '-birthdate');

        inputDateFocus(birth);
        birth.datepicker({
            dateFormat: 'mm/dd/yy',
            changeYear: true,
            changeMonth: (viewport().width < 768) ? true : false,
            yearRange: '1900:' + (new Date().getFullYear() - 18),
            minDate: Date.parse("1900-01-01"),
            maxDate: new Date(new Date().setFullYear(new Date().getFullYear() - 18)),
            onSelect: function (day) {
                $(this).siblings('input.form-control').val(day);
                $('#' + ownerNumber + '-birthdate').on('change', setters.setBirthDate(ownerNumber, day));
                $(".div-datepicker").removeClass('opened');
            }
        });
    }

    function _initEventHandlers (nubmerOfOwners) {
        for (var i = 0;i < nubmerOfOwners;i++) {
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
        for (var i = 0;Object.keys(state['owner-info'].owners).length > i;i++)
            initGoogleServices('owner' + i + '-street',
                'owner' + i + '-city',
                'owner' + i + '-province',
                'owner' + i + '-postalcode');
    }

    return {
        init: init,
        initAutocomplete: initAutocomplete
    }
})