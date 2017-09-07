module.exports('onboarding.owner-info.setters', function (require) {
    var state = require('onboarding.state').state;
    var enableSubmit = require('onboarding.setters').enableSubmit;

    var stateSection = 'owner-info';

    var setFirstName = function (ownerNumber) {
        return function (e) {
            var firstName = e.target.value;
            state[stateSection]['owners'][ownerNumber].firstName = firstName;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
            enableSubmit();
        }
    }

    var setLastName = function (ownerNumber) {
        return function (e) {
            var lastName = e.target.value;
            state[stateSection]['owners'][ownerNumber].lastName = lastName;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
            enableSubmit();
        }
    }

    var setBirthDate = function (ownerNumber, birthDate) {
        state[stateSection]['owners'][ownerNumber].birthDate = birthDate;

        _spliceRequiredField(ownerNumber, ownerNumber + '-birthdate');
        _moveTonextSection();
        enableSubmit();
    }

    var setHomePhone = function (ownerNumber) {
        return function (e) {
            var homePhone = e.target.value;
            state[stateSection]['owners'][ownerNumber].homePhone = homePhone;
        }
    }

    var setCellPhone = function (ownerNumber) {
        return function (e) {
            var cellPhone = e.target.value;
            state[stateSection]['owners'][ownerNumber].cellPhone = cellPhone;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
            enableSubmit();
        }
    }

    var setEmailAddress = function (ownerNumber) {
        return function (e) {
            var email = e.target.value;
            state[stateSection]['owners'][ownerNumber].email = email;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
            enableSubmit();
        }
    }

    var setStreet = function (ownerNumber) {
        return function (e) {
            var street = e.target.value;
            state[stateSection]['owners'][ownerNumber].street = street;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
            enableSubmit();
        }
    }

    var setPostalCode = function (ownerNumber) {
        return function (e) {
            var postalCode = e.target.value;
            state[stateSection]['owners'][ownerNumber].postalCode = postalCode.toUpperCase();

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
            enableSubmit();
        }
    }

    var setCity = function (ownerNumber) {
        return function (e) {
            var city = e.target.value;
            state[stateSection]['owners'][ownerNumber].city = city;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
            enableSubmit();
        }
    }

    var setProvince = function (ownerNumber) {
        return function (e) {
            var province = e.target.value;
            state[stateSection]['owners'][ownerNumber].province = province;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
            enableSubmit();
        }
    }

    var setUnit = function (ownerNumber) {
        return function (e) {
            var unit = e.target.value;
            state[stateSection]['owners'][ownerNumber].unit = unit;
        }
    }

    var setOwnershipPercentege = function (ownerNumber) {
        return function (e) {
            var percentage = e.target.value;
            state[stateSection]['owners'][ownerNumber].percentage = percentage;
            state[stateSection].totalPercentage = Object.keys(state[stateSection]['owners']).reduce(function (s, v) {
                return s + +state[stateSection]['owners'][v].percentage;
            }, 0);

            _spliceRequiredField(ownerNumber, e.target.id);
            _checkOwnershipPercentage();
            _moveTonextSection();
            enableSubmit();
        }
    }

    var recalculateTotalPercentage = function () {
        state[stateSection].totalPercentage = Object.keys(state[stateSection]['owners']).reduce(function (s, v) {
            return s + +state[stateSection]['owners'][v].percentage;
        }, 0);
        _checkOwnershipPercentage();
    }

    function _checkOwnershipPercentage () {
        if (state[stateSection].totalPercentage < 50) {
            $('#owner-notify').removeClass('hidden');

            if ($('#additional-owner-warning').is(':hidden')) {
                $('#additional-owner-warning').removeClass('hidden');
            }

            if (!$('#add-additional').hasClass('mandatory-field')) {
                $('#add-additional').addClass('mandatory-field');
            }
            if ($('#add-additional-div').is(':hidden')) {
                $('#add-additional-div').removeClass('hidden');
            }

            if ($('#owner-info-section .over-100:not(.hidden').length > 0) {
                $('#owner-info-section .over-100:not(.hidden)').removeClass('hidden');
            }

            return;
        }

        if (state[stateSection].totalPercentage > 100) {
            $('#owner-notify').addClass('hidden');

            $('#add-additional-div').addClass('hidden');
            $('#additional-owner-warning').addClass('hidden');

            $('#owner-info-section .over-100:last').removeClass('hidden');
            return;
        }

        if (state[stateSection].totalPercentage >= 50) {
            $('#owner-notify').addClass('hidden');

            $('#add-additional').removeClass('mandatory-field');
            $('#add-additional-div').addClass('hidden');
            $('#additional-owner-warning').addClass('hidden');

            if ($('#owner-info-section .over-100:not(.hidden').length > 0) {
                $('#owner-info-section .over-100:not(.hidden)').removeClass('hidden');
            }

            _moveTonextSection();
            return;
        }
    }

    function _moveTonextSection () {
        var owners = Object.keys(state[stateSection]['owners']);
        var isValidSection = owners.every(function (owner) {
            return state[stateSection]['owners'][owner].requiredFields.length === 0;
        });

        if (isValidSection && state[stateSection].totalPercentage >= 50 && state[stateSection].totalPercentage < 101) {
            $('#owner-info-section')
                .removeClass('active-panel')
                .addClass('step-passed');

            $('#product-panel')
                .removeClass('panel-collapsed')
                .addClass('active-panel');
        } else {
            if ($('#owner-info-section').is('.step-passed'))
                $('#owner-info-section')
                    .removeClass('step-passed');
        }
    }

    function _spliceRequiredField (ownerNumber, field) {
        if (!$('#' + field).valid()) {
            return;
        }

        var slicedField = field.slice(field.indexOf('-') + 1);
        var index = state[stateSection]['owners'][ownerNumber].requiredFields.indexOf(slicedField);

        if (index >= 0) {
            state[stateSection]['owners'][ownerNumber].requiredFields.splice(index, 1);
        }
    }

    return {
        setFirstName: setFirstName,
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
        setOwnershipPercentege: setOwnershipPercentege,
        recalculateTotalPercentage: recalculateTotalPercentage
    }
})