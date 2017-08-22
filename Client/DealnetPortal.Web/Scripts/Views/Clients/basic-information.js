﻿module.exports('basic-information-view', function (require) {
    var clientActions = require('new-client-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;

    return function (store) {
        var dispatch = store.dispatch;
        var birth = $("#birth-date"); //$('body').is('.ios-device') ? $("#birth-date").siblings('.div-datepicker') :

        inputDateFocus(birth);
        birth.datepicker({
            dateFormat: 'mm/dd/yy',
            changeYear: true,
            changeMonth: (viewport().width < 768) ? true : false,
            yearRange: '1900:' + (new Date().getFullYear() - 18),
            minDate: Date.parse("1900-01-01"),
            maxDate: new Date(new Date().setFullYear(new Date().getFullYear() - 18)),
            onSelect: function (day) {
                dispatch(createAction(clientActions.SET_BIRTH, day));
            }
        });

        birth.on('change', function () {
            var day = birth.val();
            dispatch(createAction(clientActions.SET_BIRTH, day));
        });
        $('#ui-datepicker-div').addClass('cards-datepicker');

        $('#first-name').on('uploadSuccess', dispatchDl);
        $('#camera-modal').on('hidden.bs.modal', dispatchDl);

        function dispatchDl() {
            var obj = {
                firstName: $('#first-name').val(),
                lastName: $('#last-name').val(),
                birthDate: $('#birth-date').val(),
                street: $('#street').val(),
                locality: $('#locality').val(),
                province: $('#province').val(),
                postalCode: $('#postal_code').val()
            }

            dispatch(createAction(clientActions.DRIVER_LICENSE_UPLOADED, obj));
        }

        var name = $('#first-name');
        name.on('change', function (e) {
            dispatch(createAction(clientActions.SET_NAME, e.target.value));
        });
        var lastName = $('#last-name');
        lastName.on('change',
            function (e) {
                dispatch(createAction(clientActions.SET_LAST, e.target.value));
            });

        var initialStateMap = {
            name: name,
            lastName: lastName,
            birthday: birth
        };

        dispatch(createAction(clientActions.SET_INITIAL_STATE, readInitialStateFromFields(initialStateMap)));
    };
});