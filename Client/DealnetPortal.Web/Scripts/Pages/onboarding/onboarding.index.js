module.exports('onboarding.index', function (require) {
    var initCompany = require('onboarding.company').initCompany;
    var addProvince = require('onboarding.company').addProvince;


    function init() {
        initCompany();
        $('#province-select').on('change', addProvince);
    }

    return {
        init: init
    }
})