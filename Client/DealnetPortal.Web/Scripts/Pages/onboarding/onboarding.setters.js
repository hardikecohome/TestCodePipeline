module.exports('onboarding.setters', function (require) {
    var state = require('onboarding.state').state;

    function configSetField (stateSection) {
        return function (field) {
            return function (e) {
                state[stateSection][field] = e.target.value;

                _spliceRequiredFields(stateSection, field);
                moveToNextSection(stateSection);
                enableSubmit();
            }
        }
    }

    function _spliceRequiredFields (stateSection, field) {
        if (!$('#' + field).valid()) {

            var index = state[stateSection].requiredFields.indexOf(field);

            if (index === -1)
                state[stateSection].requiredFields.push(field);

            return;
        }


        var requiredIndex = state[stateSection].requiredFields.indexOf(field);

        if (requiredIndex > -1) {
            state[stateSection].requiredFields.splice(requiredIndex, 1);
        }
    }

    function moveToNextSection (stateSection) {
        var isValid = state[stateSection].requiredFields.length === 0;
        if (isValid) {
            $('#' + stateSection + '-panel')
                .addClass('step-passed')
                .removeClass('active-panel')
                .next()
                .removeClass('panel-collapsed')
                .addClass('active-panel');
        }
    }

    function enableSubmit () {
        var valid = true;
        for (var owner in state['owner-info'].owners) {
            valid = valid && state['owner-info'].owners[owner].requiredFields.length === 0;
        }
        valid = valid && state.company.requiredFields.length === 0;
        valid = valid && state.product.requiredFields.length === 0;
        valid = valid && state.documents['void-cheque-files'].length > 0;
        valid = valid && state.documents['insurence-files'].length > 0;
        valid = state.documents.addedLicense.reduce(function (acc, item) {
            return acc && item.number.length > 0 && (item.noExpiry || item.date.length > 0);
        }, valid);
        valid = valid && state.consent.creditAgreement;
        valid = valid && state.consent.contactAgreement;
        for (var owner in state.aknowledgment.owners) {
            valid = valid && state.aknowledgment.owners[owner].agreement;
        }
        if (valid) {
            $('#submit').prop('disabled', false);
        } else {
            $('#submit').prop('disabled', true);
        }
    }

    return {
        configSetField: configSetField,
        moveToNextSection: moveToNextSection,
        enableSubmit: enableSubmit
    }
});
