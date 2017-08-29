module.exports('onboarding.owner-info.setters', function(require) {
    var state = require('onboarding.state').state;

    var stateSection = 'owner-info';

    var setFirstName = function(ownerNumber) {
        return function(e) {
            var firstName = e.target.value;
            state[stateSection]['owners'][ownerNumber].firstName = firstName;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setLastName = function (ownerNumber) {
        return function (e) {
            var lastName = e.target.value;
            state[stateSection]['owners'][ownerNumber].lastName = lastName;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setBirthDate = function(ownerNumber, birthDate) {
        state[stateSection]['owners'][ownerNumber].birthDate = birthDate;

        _spliceRequiredField(ownerNumber, ownerNumber + '-birthdate');
        _moveTonextSection();
    }

    var setHomePhone = function(ownerNumber) {
        return function (e) {
            var homePhone = e.target.value;
            state[stateSection]['owners'][ownerNumber].homePhone = homePhone;
        }
    }

    var setCellPhone = function(ownerNumber) {
        return function (e) {
            var cellPhone = e.target.value;
            state[stateSection]['owners'][ownerNumber].cellPhone = cellPhone;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setEmailAddress = function(ownerNumber) {
        return function (e) {
            var email = e.target.value;
            state[stateSection]['owners'][ownerNumber].email = email;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setStreet = function(ownerNumber) {
        return function (e) {
            var street = e.target.value;
            state[stateSection]['owners'][ownerNumber].street = street;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setPostalCode = function(ownerNumber) {
        return function (e) {
            var postalCode = e.target.value;
            state[stateSection]['owners'][ownerNumber].postalCode = postalCode;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setCity = function(ownerNumber) {
        return function (e) {
            var city = e.target.value;
            state[stateSection]['owners'][ownerNumber].city = city;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setProvince = function(ownerNumber) {
        return function (e) {
            var province = e.target.value;
            state[stateSection]['owners'][ownerNumber].province = province;

            _spliceRequiredField(ownerNumber, e.target.id);
            _moveTonextSection();
        }
    }

    var setUnit = function (ownerNumber) {
        return function (e) {
            var unit = e.target.value;
            state[stateSection]['owners'][ownerNumber].unit = unit;
        }
    }

    var setOwnershipPercentege = function(ownerNumber) {
        return function(e) {
            var percentage = e.target.value;
            state[stateSection]['owners'][ownerNumber].percentage = percentage;
            state[stateSection].totalPercentage = Object.keys(state[stateSection]['owners']).reduce((s, v) => { return s + +state[stateSection]['owners'][v].percentage; }, 0);
            if (ownerNumber === 'owner0') {
                if (+percentage < 50) {
                    $(event.currentTarget)
                        .closest('div.col-sm-4')
                        .siblings('div.col-sm-8')
                        .removeClass('hidden');

                    if ($('#additional-owner-warning').is(':hidden')) {
                        $('#additional-owner-warning').removeClass('hidden');
                    }

                    $('div.action-link-holder').removeClass('hidden');
                } else {
                    $('#additional-owner-warning').addClass('hidden');
                }
            }

            _checkOwnershipPercentage();
            _moveTonextSection();
        }
    }

    var recalculateTotalPercentage = function() {
        state[stateSection].totalPercentage = Object.keys(state[stateSection]['owners']).reduce((s, v) => { return s + +state[stateSection]['owners'][v].percentage; }, 0);
        _checkOwnershipPercentage();
    }

    function _checkOwnershipPercentage() {
        if (state[stateSection].totalPercentage >= 50) {
            $('#add-additional').removeClass('mandatory-field');
            _moveTonextSection();
        } else {
            if (!$('#add-additional').hasClass('mandatory-field')) {
                $('#add-additional').addClass('mandatory-field');
            }
        }
    }

    function _moveTonextSection() {
        var owners = Object.keys(state[stateSection]['owners']);
        var isValidSection = owners.every(function(owner) {
            return state[stateSection]['owners'][owner].requiredFields.length === 0;
        });

        if (isValidSection && state[stateSection].totalPercentage >= 50) {
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
        var index = state[stateSection]['owners'][ownerNumber].requiredFields.indexOf(slicedField);

        if (index >= 0) {
            state[stateSection]['owners'][ownerNumber].requiredFields.splice(index, 1);
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
        setOwnershipPercentege: setOwnershipPercentege,
        recalculateTotalPercentage: recalculateTotalPercentage
    }
})