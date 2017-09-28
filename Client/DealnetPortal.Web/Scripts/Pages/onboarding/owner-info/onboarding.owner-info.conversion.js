module.exports('onboarding.owner-info.conversion', function (require) {

    var datepickerOptions = {
        dateFormat: 'mm/dd/yy',
        changeYear: true,
        changeMonth: (viewport().width < 768) ? true : false,
        yearRange: '1900:' + (new Date().getFullYear() - 18),
        minDate: new Date("1900-01-01"),
        maxDate: new Date(new Date().setFullYear(new Date().getFullYear() - 18))
    };

    var setBirthDate = require('onboarding.owner-info.setters').setBirthDate;
    var resetFormValidation = require('onboarding.common').resetFormValidation;

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
                this.id = this.id.replace(fullCurrentId, fullPreviousId);
                $(this).off();
                this.name = this.name.replace(currentNamePattern, previousNamePattern);
            });

            var spans = nextOwner.find('span');

            spans.each(function () {
                var valFor = $(this).attr('data-valmsg-for');
                if (valFor == null) { return; }

                $(this).attr('data-valmsg-for', valFor.replace(currentNamePattern, previousNamePattern));
            });

            nextOwner.find("#" + fullCurrentId + '-remove').off();
            nextOwner.find("#" + fullCurrentId + '-remove').attr('id', fullPreviousId + '-remove');

            nextOwner.attr('id', 'owner' + (nextId - 1) + '-container');
            nextOwner.attr('id', 'owner' + (nextId - 1) + '-container');

            state['owner-info']['owners']['owner' + (nextId - 1)] = state['owner-info']['owners']['owner' + nextId];
            delete state['owner-info']['owners']['owner' + nextId];
        }
        resetFormValidation('#onboard-form');
    }

    function assignOwnerDatepicker (selector, ownerNumber) {
        var options = $.extend({},
            datepickerOptions,
            {
                onSelect: function (day) {
                    $(this).siblings('input.form-control').val(day);
                    setBirthDate(ownerNumber, day);
                    $(".div-datepicker").removeClass('opened');
                }
            });

        return assignDatepicker(selector, options);
    }

    return {
        recalculateOwnerIndex: recalculateOwnerIndex,
        assignOwnerDatepicker: assignOwnerDatepicker
    };
})