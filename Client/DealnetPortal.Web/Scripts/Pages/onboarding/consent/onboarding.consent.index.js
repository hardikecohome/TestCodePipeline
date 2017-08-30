module.exports('onboarding.consent.index', function (require) {
    var setters = require('onboarding.consent.setters');

    function _setInputHandlers() {
        $('#contactAgreement').on('click', setters.setContactAgreement);
        $('#creditAgreement').on('click', setters.setCreditAgreement);
    }

    var init = function() {
        _setInputHandlers();
    }

    return {
        init: init
    }
})