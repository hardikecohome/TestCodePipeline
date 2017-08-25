module.exports('onboarding.index', function (require) {
    var ownerInfo = require('onboarding.owner-info.index');
    var initCompany = require('onboarding.company').initCompany;
    var addProvince = require('onboarding.company').addProvince;

    function init() {
        ownerInfo.init();
        initCompany();
        $('#province-select').on('change', addProvince);
    }

    return {
        init: init
    }
})