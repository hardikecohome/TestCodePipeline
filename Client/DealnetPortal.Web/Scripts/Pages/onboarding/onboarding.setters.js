module.exports('onboarding.setters', function (require) {
    var state = require('onboarding.state').state;

    function configSetField (stateSection) {
        return function (field) {
            return function (e) {
                state[stateSection][field] = e.target.value;

                _spliceRequiredFields(stateSection, field);
                _moveToNextSection(stateSection);
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

        if (requiredIndex >= 0) {
            state[stateSection].requiredFields.splice(requiredIndex, 1);
        }
    }

    function _moveToNextSection (stateSection) {
        var isValid = state[stateSection].requiredFields.length === 0;
        if (isValid) {
            debugger
            $(this)
                .parents('.panel')
                .removeClass('panel-collapsed')
                .addClass('active-panel')
                .addClass('step-passed');
        }
    }

    return {
        configSetField: configSetField
    }
});
