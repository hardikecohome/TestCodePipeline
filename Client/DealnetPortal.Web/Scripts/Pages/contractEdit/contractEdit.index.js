module.exports('contract-edit', function (require) {

    var eSign = require('contract-edit.eSignature');

    var init = function (eSignEnabled) {
        if (eSignEnabled === 1) {
            eSign.init();
        }
    }

    return {
        init: init
    };
});