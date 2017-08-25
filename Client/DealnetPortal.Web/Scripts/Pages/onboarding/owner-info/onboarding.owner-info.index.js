module.exports('onboarding.owner-info.index', function (require) {
    var autocomplete = require('onboarding.autocomplete');
    function init() {
        autocomplete.add('Street', 'City');
    }

    return {
        init: init
    }
})