module.exports('onboarding.ackonwledgment.setters', function(require) {
    var state = require('onboarding.state').state;
    var stateSection = 'aknowledgment';

    var setFirstName = function (ownerNumber) {
        return function (e) {
            var firstName = e.target.value;
            state[stateSection]['owners'][ownerNumber].firstName = firstName;

            $('#aknowledgment-' + ownerNumber).val(firstName);
        }
    }

    var setLastName = function (ownerNumber) {
        var lastName = e.target.value;
        state[stateSection]['owners'][ownerNumber].lastName = lastName;

        $('#aknowledgment-' + ownerNumber).val(lastName);
    }

    return {
        setFirstName: setFirstName,
        setLastName: setLastName
    }
});