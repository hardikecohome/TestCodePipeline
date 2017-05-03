module.exports('contact-information-view', function (require) {
    var clientActions = require('new-client-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;

    return function (store) {
        var dispatch = store.dispatch;

        var homePhone = $('#home-phone');
        homePhone.on('change', function (e) {
            dispatch(createAction(clientActions.SET_PHONE, e.target.value));
        });

        var cellPhone = $('#cell-phone');
        cellPhone.on('change', function (e) {
            dispatch(createAction(clientActions.SET_CELL_PHONE, e.target.value));
        });

        var email = $('#email');
        email.on('change', function (e) {
            dispatch(createAction(clientActions.SET_EMAIL, e.target.value));
        });

        var contactMethod = $('#contact-method');
        contactMethod.on('change', function (e) {
            dispatch(createAction(clientActions.SET_CONTACT_METHOD, e.target.value));
        });

        var initialStateMap = {
            phone: homePhone,
            cellPhone: cellPhone,
            email: email,
            contactMethod: contactMethod
        };

        dispatch(createAction(clientActions.SET_INITIAL_STATE, readInitialStateFromFields(initialStateMap)));
    };
});