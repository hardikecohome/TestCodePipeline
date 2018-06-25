﻿module.exports('basic-information-view', function (require) {
    var clientActions = require('new-client-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;

    var initDob = require('dob-selecters').initDobGroup;

    return function (store) {
        var dispatch = store.dispatch;

        var birth = $('#birth-date');

        birth.on('change', function (e) {
            birth.valid();
            dispatch(createAction(clientActions.SET_BIRTH, e.target.value));
        });
        initDob(birth.parents('.dob-group'));

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