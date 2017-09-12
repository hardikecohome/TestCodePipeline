﻿module.exports('onboarding.owner-info.conversion', function (require) {

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

            nextOwner.find("#" + fullCurrentId + '-remove').off();//prop("onclick", null);
            nextOwner.find("#" + fullCurrentId + '-remove').attr('id', fullPreviousId + '-remove');

            //var removeButton = nextOwner.find('#' + option + '-equipment-remove-' + nextId);
            //removeButton.attr('id', option + '-equipment-remove-' + (nextId - 1));
            nextOwner.attr('id', 'owner' + (nextId - 1) + '-container');
            nextOwner.attr('id', 'owner' + (nextId - 1) + '-container');

            state['owner-info']['owners']['owner' + (nextId - 1)] = state['owner-info']['owners']['owner' + nextId];
            delete state['owner-info']['owners']['owner' + nextId];
        }
        resetFormValidation('#onboard-form');
    }

    function assignDatepicker (selector, ownerNumber) {

        var input = $('body').is('.ios-device') ? $(selector).siblings('.div-datepicker') : $(selector);

        inputDateFocus(input);

        input.datepicker({
            dateFormat: 'mm/dd/yy',
            changeYear: true,
            changeMonth: (viewport().width < 768) ? true : false,
            yearRange: '1900:' + (new Date().getFullYear() - 18),
            minDate: Date.parse("1900-01-01"),
            maxDate: new Date(new Date().setFullYear(new Date().getFullYear() - 18)),
            onSelect: function (day) {
                $(this).siblings('input.form-control').val(day);
                setBirthDate(ownerNumber, day);
                $(".div-datepicker").removeClass('opened');
            }
        });

        input.siblings('.div-datepicker-value').on('click', function () {
            $('.div-datepicker').removeClass('opened');
            $(this).siblings('.div-datepicker').toggleClass('opened');
            if (!$('.div-datepicker .ui-datepicker-close').length) {
                addCloseButtonForInlineDatePicker();
            }
        });

        return input;
    }

    return {
        recalculateOwnerIndex: recalculateOwnerIndex,
        assignDatepicker: assignDatepicker
    };
})