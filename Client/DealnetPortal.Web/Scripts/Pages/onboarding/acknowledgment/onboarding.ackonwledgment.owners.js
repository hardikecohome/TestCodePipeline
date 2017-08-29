module.exports('onboarding.ackonwledgment.owners', function(require) {

    var addToAknowdlegment = function(ownerNumber) {
        var newOwnerState = {};
        newOwnerState[ownerNumber] = { };

        $.extend(state['aknowledgment']['owners'], newOwnerState);

        var template = document.getElementById('aknowledgment-owner-template').innerHTML;
        var nextOwnerIndex = state['owner-info']['nextOwnerIndex'];
    }

    var removeFromAknowledgment = function(ownerNumber) {

    }

    return {
        add: addToAknowdlegment,
        remove: removeFromAknowledgment
    }
})