module.exports('onboarding.consent.setters', function (require) {
    var state = require('onboarding.state').state;
    var enableSubmit = require('onboarding.setters').enableSubmit;
    var stateSection = 'consent';

    var setCreditAgreement = function (e) {
        var isChecked = e.target.checked;

        state[stateSection].creditAgreement = isChecked;
        _moveTonextSection();
        enableSubmit();
    }

    var setContactAgreement = function (e) {
        var isChecked = e.target.checked;

        state[stateSection].contactAgreement = isChecked;
        _moveTonextSection();
        enableSubmit();
    }

    function _moveTonextSection () {
        var agreements = Object.keys(state[stateSection]);
        var isValidSection = agreements.every(function (agreement) {
            return state[stateSection][agreement] === true;
        });

        if (isValidSection) {
            $('#client-consent-section')
                .removeClass('active-panel')
                .addClass('panel-collapsed')
                .addClass('step-passed');

            $('#cleint-aknowledgment-section')
                .removeClass('panel-collapsed')
                .addClass('active-panel');
        }
    }

    return {
        setCreditAgreement: setCreditAgreement,
        setContactAgreement: setContactAgreement
    }
});