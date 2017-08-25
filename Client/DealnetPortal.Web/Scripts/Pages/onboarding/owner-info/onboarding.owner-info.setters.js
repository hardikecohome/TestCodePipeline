module.exports('onboarding.owner-info.setters', function(require) {
    var state = require('onboarding.state');
    var stateSection = 'owner-info';

    var setFirstName = function(ownerNumber) {
        return function(e) {
            var firstName = e.target.value;
            state[stateSection][ownerNumber].firstName = firstName;
        }
    }

    var setLastName = function (ownerNumber) {
        return function (e) {
            var lastName = e.target.value;
            state[stateSection][ownerNumber].lastName = lastName;
        }
    }

    var setBirhDate = function(ownerNumber) {
        return function(e) {
            var birthDate = e.target.value;
            state[stateSection][ownerNumber].birthDate = birthDate; 
        }
    }

    var setHomePhone = function(ownerNumber) {
        return function (e) {
            var homePhone = e.target.value;
            state[stateSection][ownerNumber].homePhone = homePhone;
            _togglePhone('#' + ownerNumber + '-cellphone', e);
        }
    }

    var setCellPhone = function(ownerNumber) {
        return function (e) {
            var cellPhone = e.target.value;
            state[stateSection][ownerNumber].cellPhone = cellPhone;
            _togglePhone('#' + ownerNumber + '-homephone', e);
        }
    }

    var setEmailAddress = function(ownerNumber) {
        return function (e) {
            var email = e.target.value;
            state[stateSection][ownerNumber].email = email;
        }
    }

    var setOwnershipPercentege = function() {
        return function(e) {
            
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

    return {
        setFirstName : setFirstName,
        setLastName: setLastName,
        setBirhDate: setBirhDate,
        setHomePhone: setHomePhone,
        setCellPhone: setCellPhone,
        setEmailAddress: setEmailAddress
    }
})