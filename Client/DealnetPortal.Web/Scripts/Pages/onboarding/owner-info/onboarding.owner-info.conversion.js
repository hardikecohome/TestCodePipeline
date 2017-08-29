module.exports('onboarding.owner-info.conversion', function (require) {

    var recalculateOwnerIndex = function (ownerNumber) {

        delete state['owner-info']['owners'][ownerNumber];

        var id = ownerNumber.substr(5);

        var nextId = Number(id);
        while (true) {
            nextId++;
            var nextOwner = $('#owner' + nextId + '-container');

            if (!nextOwner.length) {
                break;
            }

            nextOwner.off();

            var inputs = nextOwner.find('input, select, textarea');

            var fullCurrentId = 'owner' + nextId;
            var fullPreviousId = 'owner' + (nextId - 1);
            var currentNamePattern = 'Owners[' + nextId;
            var previousNamePattern = 'Owners[' + (nextId - 1);

            inputs.each(function () {
                $(this).attr('id', $(this).attr('id').replace(fullCurrentId, fullPreviousId));
                $(this).off();
                $(this).attr('name', $(this).attr('name').replace(currentNamePattern, previousNamePattern));
            });

            var labels = nextOwner.find('label');

            //labels.each(function () {
            //    $(this).attr('for', $(this).attr('for').replace(currentIdPattern, previousIdPattern));
            //});

            var spans = nextOwner.find('span');

            spans.each(function () {
                var valFor = $(this).attr('data-valmsg-for');
                if (valFor == null) { return; }

                $(this).attr('data-valmsg-for', valFor.replace(currentNamePattern, previousNamePattern));
            });

            //var removeButton = nextOwner.find('#' + option + '-equipment-remove-' + nextId);
            //removeButton.attr('id', option + '-equipment-remove-' + (nextId - 1));
            nextOwner.attr('id', 'owner' + (nextId - 1) + '-container');
            nextOwner.attr('id', 'owner' + (nextId - 1) + '-container');

            state['owner-info']['owners']['owner' + (nextId - 1)] = state['owner-info']['owners']['owner' + nextId];
            delete state['owner-info']['owners']['owner' + nextId];
        }
    }

    var resetFormValidation = function (option) {
        var $form = $('#' + option + '-container > form');
        $form.removeData("validator");
        $form.removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse('#' + option + '-container > form');
    }

    return {
        recalculateOwnerIndex: recalculateOwnerIndex,
        resetValidation: resetFormValidation
    };
})