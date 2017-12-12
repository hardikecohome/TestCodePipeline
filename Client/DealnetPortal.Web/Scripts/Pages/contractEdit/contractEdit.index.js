module.exports('contract-edit', function (require) {
    var eSign = require('contract-edit.eSignature');
    var navigateToStep = require('navigateToStep');

    var init = function (eSignEnabled) {
        if (eSignEnabled === 1) {
            eSign.init();
        }
        $('.editToStep1').on('click', function () {
            if ($(this).attr('href')) {
                navigateToStep($(this));
            }
            return false;
        });
    }

    return {
        init: init
    };
});