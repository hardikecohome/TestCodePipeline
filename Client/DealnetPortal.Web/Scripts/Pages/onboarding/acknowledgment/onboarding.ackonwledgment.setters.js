﻿module.exports('onboarding.ackonwledgment.setters', function (require) {
    var state = require('onboarding.state').state;
    var enableSubmit = require('onboarding.setters').enableSubmit;
    var stateSection = 'aknowledgment';

    var setFirstName = function (ownerNumber) {
        return function (e) {
            var firstName = e.target.value;
            state[stateSection]['owners'][ownerNumber].firstName = firstName;

            var fullName = ' ' + state[stateSection]['owners'][ownerNumber].firstName + ' ' + state[stateSection]['owners'][ownerNumber].lastName;
            $('#' + ownerNumber + '-name-holder').text(fullName.toUpperCase());
        }
    }

    var setLastName = function (ownerNumber) {
        return function (e) {
            var lastName = e.target.value;
            state[stateSection]['owners'][ownerNumber].lastName = lastName;

            var fullName = state[stateSection]['owners'][ownerNumber].firstName + ' ' + state[stateSection]['owners'][ownerNumber].lastName;
            $('#' + ownerNumber + '-name-holder').text(fullName.toUpperCase());
        }
    }

    var setAgreement = function (ownerNumber) {
        return function (e) {
            var isChecked = e.target.checked;
            state[stateSection]['owners'][ownerNumber].agreement = isChecked;
            _markComplete();
            enableSubmit();
        }
    }

    var _markComplete = function () {
        var valid = true;
        for (var owner in state[stateSection]['owners']) {
            valid = valid && state[stateSection]['owners'][owner].agreement;
        }
        var client = $('#cleint-aknowledgment-section');
        if (valid) {
            client.addClass('step-passed')
            if (!initializing)
                client.removeClass('active-panel');
        } else {
            client.removeClass('step-passed');
        }
    }

    var checkAckonwledgments = function() {
        _markComplete();
    }

    return {
        setFirstName: setFirstName,
        setLastName: setLastName,
        setAgreement: setAgreement,
        checkAckonwledgments: checkAckonwledgments
    }
});