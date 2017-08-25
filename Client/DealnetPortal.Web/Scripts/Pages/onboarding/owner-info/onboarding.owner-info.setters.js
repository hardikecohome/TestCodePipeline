module.exports('onboarding.owner-info.setters', function(require) {
    var state = require('onboarding.state').state;
    var stateSection = 'owner-info';

    var setFirstName = function(ownerNumber) {
        return function(e) {
            var firstName = e.target.value;
            state[stateSection][ownerNumber].firstName = firstName;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setLastName = function (ownerNumber) {
        return function (e) {
            var lastName = e.target.value;
            state[stateSection][ownerNumber].lastName = lastName;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setBirthDate = function(ownerNumber, birthDate) {
        state[stateSection][ownerNumber].birthDate = birthDate;

        _spliceRequiredField(ownerNumber, ownerNumber + '-birthdate');
        _moveTonextSection();
    }

    var setHomePhone = function(ownerNumber) {
        return function (e) {
            var homePhone = e.target.value;
            state[stateSection][ownerNumber].homePhone = homePhone;
            _togglePhone('#' + ownerNumber + '-cellphone', e);

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setCellPhone = function(ownerNumber) {
        return function (e) {
            var cellPhone = e.target.value;
            state[stateSection][ownerNumber].cellPhone = cellPhone;
            _togglePhone('#' + ownerNumber + '-homephone', e);

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setEmailAddress = function(ownerNumber) {
        return function (e) {
            var email = e.target.value;
            state[stateSection][ownerNumber].email = email;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setStreet = function(ownerNumber) {
        return function (e) {
            var street = e.target.value;
            state[stateSection][ownerNumber].street = street;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setPostalCode = function(ownerNumber) {
        return function (e) {
            var postalCode = e.target.value;
            state[stateSection][ownerNumber].postalCode = postalCode;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setCity = function(ownerNumber) {
        return function (e) {
            var city = e.target.value;
            state[stateSection][ownerNumber].city = city;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setProvince = function(ownerNumber) {
        return function (e) {
            var province = e.target.value;
            state[stateSection][ownerNumber].province = province;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setUnit = function (ownerNumber) {
        return function (e) {
            var unit = e.target.value;
            state[stateSection][ownerNumber].unit = unit;
        }
    }

    var setOwnershipPercentege = function(ownerNumber) {
        return function(e) {
            var percentage = e.target.value;
            state[stateSection][ownerNumber].percentage = percentage;

            if (ownerNumber === 'owner1') {
                if (+percentage < 50) {
                    $(event.currentTarget)
                        .closest('div.col-sm-4')
                        .siblings('div.col-sm-8')
                        .removeClass('hidden');
                } else {
                    _moveTonextSection();
                }
            }
        }
    }

    function _togglePhone(selector, event) {
        var label = $(selector).parent().siblings('label');

        if ($(event.currentTarget).valid() && event.currentTarget.value !== '') {
            if (label.hasClass('mandatory-field')) {
                label.removeClass('mandatory-field');
            }

            $(selector).rules("remove", "required");
            $(selector).removeClass('input-validation-error');
            $(selector).next('.text-danger').empty();
        } else {
            if (!label.hasClass('mandatory-field')) {
                label.addClass('mandatory-field');
            }

            $(selector).rules("add", "required");
        }
    }

    function _moveTonextSection() {
        var owners = Object.keys(state[stateSection]);
        var isValidSection = owners.every(function(owner) {
            return state[stateSection][owner].requiredFields.length === 0;
        });

        if (isValidSection) {
            $('#owner-info-section')
                .removeClass('active-panel')
                .addClass('panel-collapsed')
                .addClass('step-passed');

            $('#product-information-section')
                .removeClass('panel-collapsed')
                .addClass('active-panel');
        }
    }

    function _spliceRequiredField(ownerNumber, field) {
        if (!$('#' + field).valid()) {
            return;
        }

        var slicedField = field.slice(field.indexOf('-') + 1);
        var index = state[stateSection][ownerNumber].requiredFields.indexOf(slicedField);

        if (index >= 0) {
            state[stateSection][ownerNumber].requiredFields.splice(index, 1);

            if (slicedField === 'homephone' || slicedField === 'cellphone') {
                if (slicedField === 'homephone') {
                    _removePhone(ownerNumber, 'cellphone');
                } else {
                    _removePhone(ownerNumber, 'homephone');
                }
            }
        }
    }

    function _removePhone(ownerNumber, fieldName) {
        var phoneIndex = state[stateSection][ownerNumber].requiredFields.indexOf(fieldName);
        if (phoneIndex) {
            state[stateSection][ownerNumber].requiredFields.splice(phoneIndex, 1);
        }
    }

    return {
        setFirstName : setFirstName,
        setLastName: setLastName,
        setBirthDate: setBirthDate,
        setHomePhone: setHomePhone,
        setCellPhone: setCellPhone,
        setEmailAddress: setEmailAddress,
        setStreet: setStreet,
        setCity: setCity,
        setPostalCode: setPostalCode,
        setProvince: setProvince,
        setUnit: setUnit,
        setOwnershipPercentege: setOwnershipPercentege
    }
})