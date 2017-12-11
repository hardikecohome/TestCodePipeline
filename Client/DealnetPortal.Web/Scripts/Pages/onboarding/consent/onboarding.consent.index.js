module.exports('onboarding.consent.index', function (require) {
    var setters = require('onboarding.consent.setters');

    function _setInputHandlers () {
        $('#contactAgreement').on('click', setters.setContactAgreement);
        $('#creditAgreement').on('click', setters.setCreditAgreement);
    }

    var init = function (contact, credit) {
        setters.setContactAgreement({ target: { checked: contact } });
        setters.setCreditAgreement({ target: { checked: credit } });
        _setInputHandlers();

        $('.j-personal-data-used-modal').on('click', function (e) {
            var data = {
                message: $('#personal-data-used').html(),
                class: "consents-modal",
                cancelBtnText: "OK"
            };
            dynamicAlertModal(data);
            e.preventDefault();
        });
    }

    return {
        init: init
    }
});
