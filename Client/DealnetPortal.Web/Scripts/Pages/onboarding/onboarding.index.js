module.exports('onboarding.index', function (require) {
    var initCompany = require('onboarding.company').initCompany;


    function init() {
        initCompany();
    }

    return {
        init: init
    }
})