module.exports('onboarding.documents.index', function (require) {
    var setters = require('onboarding.documents.setters');

    function _setInputHandlers() {
        document.getElementById('void-cheque-upload').addEventListener('change', setters.setVoidChequeFile, false);
        document.getElementById('insurence-upload').addEventListener('change', setters.setInsurenceFile, false);

        $(document).bind('provinceAdded', setters.addLicense);
        $(document).bind('equipmentAdded', setters.addLicense);
    }

    var init = function () {
        _setInputHandlers();
    }

    return {
        init: init
    }
})