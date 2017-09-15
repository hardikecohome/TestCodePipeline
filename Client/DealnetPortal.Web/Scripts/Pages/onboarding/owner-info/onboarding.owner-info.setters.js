module.exports('onboarding.owner-info.setters', function (require) {
    var state = require('onboarding.state').state;
    var enableSubmit = require('onboarding.setters').enableSubmit;

    var stateSection = 'owner-info';

    var setFirstName = function (ownerNumber) {
        return function (e) {
            state[stateSection]['owners'][ownerNumber].firstName = e.target.value;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
            enableSubmit();
        }
    }

    var setLastName = function (ownerNumber) {
        return function (e) {
            state[stateSection]['owners'][ownerNumber].lastName = e.target.value;

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
            state[stateSection]['owners'][ownerNumber].homePhone = e.target.value;
        }
    }

    var setCellPhone = function (ownerNumber) {
        return function (e) {
            state[stateSection]['owners'][ownerNumber].cellPhone = e.target.value;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
            enableSubmit();
        }
    }

    var setEmailAddress = function (ownerNumber) {
        return function (e) {
            state[stateSection]['owners'][ownerNumber].email = e.target.value;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
            enableSubmit();
        }
    }

    var setStreet = function (ownerNumber) {
        return function (e) {
            state[stateSection]['owners'][ownerNumber].street = e.target.value;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
            enableSubmit();
        }
    }

    var setPostalCode = function (ownerNumber) {
        return function (e) {
            state[stateSection]['owners'][ownerNumber].postalCode = e.target.value.toUpperCase();

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
            enableSubmit();
        }
    }

    var setCity = function (ownerNumber) {
        return function (e) {
            state[stateSection]['owners'][ownerNumber].city = e.target.value;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
            enableSubmit();
        }
    }

    var setProvince = function (ownerNumber) {
        return function (e) {
            state[stateSection]['owners'][ownerNumber].province = e.target.value;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
            enableSubmit();
        }
    }

    var setUnit = function (ownerNumber) {
        return function (e) {
            state[stateSection]['owners'][ownerNumber].unit = e.target.value;
        }
    }

    var setOwnershipPercentege = function (ownerNumber) {
        return function (e) {
            state[stateSection]['owners'][ownerNumber].percentage = e.target.value;
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

            if (!$('#over-100').is(':hidden')) {
                $('#over-100').addClass('hidden');
            }

            return;
        }

        if (state[stateSection].totalPercentage > 100) {
            $('#owner-notify').addClass('hidden');

            if ($('#add-additional-div').is(':hidden')) {
                $('#add-additional-div').removeClass('hidden');
            }
            $('#additional-owner-warning').addClass('hidden');

            $('#over-100').removeClass('hidden');
            return;
        }

        if (state[stateSection].totalPercentage >= 50) {
            $('#owner-notify').addClass('hidden');

            $('#add-additional').removeClass('mandatory-field');
            if (state[stateSection]['owners'].length <= 1) {
                $('#add-additional-div').addClass('hidden');
            }
            $('#additional-owner-warning').addClass('hidden');

            if (!$('#over-100').is(':hidden')) {
                $('#over-100').addClass('hidden');
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

            var product = $('#product-panel');
            if (!product.is('.step-passed') && !initializing) {
                product.removeClass('panel-collapsed')
                    .addClass('active-panel');
            }
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