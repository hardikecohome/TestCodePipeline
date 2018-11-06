module.exports('onboarding.setters', function (require) {
    var state = require('onboarding.state').state;
    var initTooltip = require('onboarding.common').initTooltip;
    var removeTooltip = require('onboarding.common').removeTooltip;

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

    var setLengthLimitedField = function (maxLength) {
        return function (e) {
            if (e.target.value.length > maxLength) {
                e.target.value = e.target.value.substr(0, maxLength);
                $(e.target).change();
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
        var current = $('#' + stateSection + '-panel');
        var isValid = state[stateSection].requiredFields.length === 0;
        if (isValid) {

            current.addClass('step-passed')
                .removeClass('active-panel');
            var next = current.next('.panel');
            if (!next.is('.step-passed') && !initializing) {
                next.removeClass('panel-collapsed')
                    .addClass('active-panel');
            }
        } else {
            if (current.is('.step-passed'))
                current.removeClass('step-passed');
        }
    }

    function enableSubmit () {
        var valid = true;
        for (var owner in state['owner-info'].owners) {
            valid = valid && state['owner-info'].owners[owner].requiredFields.length === 0;
        }
        valid = valid && state['owner-info'].totalPercentage >= 50 && state['owner-info'].totalPercentage < 101;

        valid = valid && state.company.requiredFields.length === 0;
        valid = valid && state.product.requiredFields.length === 0;
        // no longer required for submit
        // valid = valid && state.documents['void-cheque-files'].length > 0;
        // valid = valid && state.documents['insurence-files'].length > 0;
        // valid = state.documents.addedLicense.reduce(function (acc, item) {
        //     return acc && item.number.length > 0 && (item.noExpiry || item.date !== null && item.date.length > 0);
        // }, valid);
        valid = valid && state.consent.creditAgreement;
        valid = valid && state.consent.contactAgreement;
        for (var owner in state.aknowledgment.owners) {
            valid = valid && (typeof state.aknowledgment.owners[owner].agreement !== 'undefined' ? state.aknowledgment.owners[owner].agreement : false);
        }

        if (valid) {
            $('#submitBtn').prop('disabled', false);
            removeTooltip();
        } else {
            $('#submitBtn').prop('disabled', true);
            initTooltip();
        }
    }

    return {
        configSetField: configSetField,
        moveToNextSection: moveToNextSection,
        enableSubmit: enableSubmit,
        setLengthLimitedField: setLengthLimitedField
    }
});
