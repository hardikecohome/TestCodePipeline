module.exports('onboarding.ackonwledgment.owners', function (require) {

    var resetFormValidation = require('onboarding.common').resetFormValidation;
    var checkAckonwledgments = require('onboarding.ackonwledgment.setters').checkAckonwledgments;

    var recalculateOwnerIndex = function (ownerNumber) {

        var id = ownerNumber.substr(5);

        var nextId = Number(id);
        while (true) {
            nextId++;
            var nextOwner = $('#owner' + nextId + '-aknowledgment-holder');

            if (!nextOwner.length) {
                break;
            }

            nextOwner.off();

            var inputs = nextOwner.find('input, select, textarea');

            var fullCurrentId = 'owner' + nextId;
            var fullPreviousId = 'owner' + (nextId - 1);
            var currentNamePattern = 'Owners[' + nextId;
            var previousNamePattern = 'Owners[' + (nextId - 1);

            nextOwner.attr('id', fullPreviousId + '-aknowledgment-holder');

            inputs.each(function () {
                this.id = this.id.replace(fullCurrentId, fullPreviousId);
                $(this).off();
                this.name = this.name.replace(currentNamePattern, previousNamePattern);
            });

            var spans = nextOwner.find('span');

            spans.each(function () {
                this.id = this.id.replace(fullCurrentId, fullPreviousId);
            });

            var owner = state['aknowledgment']['owners']['owner' + nextId];
            state['aknowledgment']['owners']['owner' + (nextId - 1)] = owner;
            delete state['aknowledgment']['owners']['owner' + nextId];
        }
        resetFormValidation('#onboard-form');
    }

    var addToAknowdlegment = function (ownerNumber) {
        var newOwnerState = {};
        newOwnerState[ownerNumber] = {};

        $.extend(state['aknowledgment']['owners'], newOwnerState);

        var ownerIndex = ownerNumber.substr(-1);
        $('#aknowledgmentTemplate')
            .tmpl({ 'owner': ownerNumber, 'index': ownerIndex })
            .appendTo('#onboard-owners-hold');

        $('#' + ownerNumber + '-agreement').on('change', function () {
            if ($(this).is(':checked')) {
                $(this).attr('value', 'true');
            } else {
                $(this).attr('value', 'false');
            }
        });
        checkAckonwledgments();
    }

    var removeFromAknowledgment = function (ownerNumber) {
        delete state['aknowledgment']['owners'][ownerNumber];
        $('#' + ownerNumber + '-aknowledgment-holder').off();
        $('#' + ownerNumber + '-aknowledgment-holder').remove();
        recalculateOwnerIndex(ownerNumber);
    }

    return {
        add: addToAknowdlegment,
        remove: removeFromAknowledgment
    }
})