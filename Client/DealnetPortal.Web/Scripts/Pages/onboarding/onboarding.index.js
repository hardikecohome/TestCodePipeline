module.exports('onboarding.index', function (require) {
    var ownerInfo = require('onboarding.owner-info.index');

    function init() {
        ownerInfo.init();
        console.log('hello');
    }

    return {
        init: init
    }
})