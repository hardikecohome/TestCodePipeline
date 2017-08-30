module.exports('onboarding.ackonwledgment.owners', function(require) {

    var addToAknowdlegment = function(ownerNumber) {
        var newOwnerState = {};
        newOwnerState[ownerNumber] = { };

        $.extend(state['aknowledgment']['owners'], newOwnerState);

        $('#aknowledgmentTemplate')
            .tmpl({ 'owner': ownerNumber })
            .appendTo('#onboard-owners-hold');
    }

    var removeFromAknowledgment = function(ownerNumber) {

    }

    return {
        add: addToAknowdlegment,
        remove: removeFromAknowledgment
    }
})