module.exports('onboarding.ackonwledgment.owners', function(require) {

    var addToAknowdlegment = function(ownerNumber) {
        var newOwnerState = {};
        newOwnerState[ownerNumber] = { };

        $.extend(state['aknowledgment']['owners'], newOwnerState);

        var ownerIndex = ownerNumber.substr(-1);
        $('#aknowledgmentTemplate')
            .tmpl({ 'owner': ownerNumber, 'index': ownerIndex })
            .appendTo('#onboard-owners-hold');
    }

    var removeFromAknowledgment = function(ownerNumber) {
        delete state['aknowledgment']['owners'][ownerNumber];
        $('#' + ownerNumber + '-aknowledgment-holder').off();
        $('#' + ownerNumber + '-aknowledgment-holder').remove();
    }

    return {
        add: addToAknowdlegment,
        remove: removeFromAknowledgment
    }
})