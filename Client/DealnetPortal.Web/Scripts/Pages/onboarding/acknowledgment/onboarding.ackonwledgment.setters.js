module.exports('onboarding.ackonwledgment.setters', function (require) {
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
            enableSubmit();
        }
    }

    return {
        setFirstName: setFirstName,
        setLastName: setLastName,
        setAgreement: setAgreement
    }
});