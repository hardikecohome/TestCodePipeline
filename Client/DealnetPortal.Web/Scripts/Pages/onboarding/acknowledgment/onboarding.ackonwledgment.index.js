module.exports('onboarding.ackonwledgment.index', function (require) {
    var setAgreement = require('onboarding.ackonwledgment.setters').setAgreement;


    function _setLoadedData (owners) {
        for (var i = 0;i < owners.length;i++) {
            var owner = 'owner' + i;

            var newOwnerState = {};
            newOwnerState[owner] = {};

            $.extend(state['aknowledgment']['owners'], newOwnerState);
            setAgreement(owner)({ target: { checked: owners[i].Acknowledge } });
        }
    }

    var init = function (owners) {

        if (Array.isArray(owners) && owners.length > 0) {
            _setLoadedData(owners);
        }
    }

    return {
        init: init
    }
})