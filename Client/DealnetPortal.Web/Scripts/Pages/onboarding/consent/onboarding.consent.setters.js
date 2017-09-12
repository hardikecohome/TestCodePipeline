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
                .addClass('step-passed');

            var acknowledgement = $('#cleint-aknowledgment-section');
            if (!acknowledgement.is('.step-passed')) {
                acknowledgement.removeClass('panel-collapsed')
                    .addClass('active-panel');
            }
        } else {
            var client = $('#client-consent-section');
            if (client.is('.step-passed'))
                client.removeClass('step-passed');
        }
    }

    return {
        setCreditAgreement: setCreditAgreement,
        setContactAgreement: setContactAgreement
    }
});