module.exports('your-info-view', function (require) {
    var customerActions = require('customer-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;

    var initDob = require('dob-selecters').initDobGroup;

    return function (store) {
        var dispatch = store.dispatch;

        var birth = $('#birth-date-customer');
        birth.on('change', function (e) {
            birth.valid();
            dispatch(createAction(customerActions.SET_BIRTH, e.target.value));
        });
        initDob(birth.parents('.dob-group'));

        var name = $('#firstName');
        name.on('change', function (e) {
            dispatch(createAction(customerActions.SET_NAME, e.target.value));
        });
        var lastName = $('#lastName');
        lastName.on('change', function (e) {
            dispatch(createAction(customerActions.SET_LAST, e.target.value));
        });

        $('#sin').on('change', function (e) {
            dispatch(createAction(customerActions.SET_SIN, e.target.value));
        });

        var initialStateMap = {
            name: name,
            lastName: lastName,
            birthday: birth
        };

        dispatch(createAction(customerActions.SET_INITIAL_STATE, readInitialStateFromFields(initialStateMap)));
    };
});