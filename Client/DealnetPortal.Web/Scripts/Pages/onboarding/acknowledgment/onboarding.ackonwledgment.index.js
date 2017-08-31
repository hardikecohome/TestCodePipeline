module.exports('onboarding.ackonwledgment.index', function(require) {

    function _setLoadedData(owners) {
        for (var i = 0; i < owners.length; i++) {
            var owner = 'owner' + i;

            var newOwnerState = {};
            newOwnerState[owner] = {};

            $.extend(state['aknowledgment']['owners'], newOwnerState);
        }
    }

    var init = function (owners) {

        if (Array.isArray(owners) && owners.length > 0) {
            _setLoadedData(owners);
        }
    }

    return {
        init : init
    }
})