module.exports('contact-information-view', function (require) {
    var clientActions = require('new-client-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;

    return function (store) {
        var dispatch = store.dispatch;
       
        var homePhone = $('#home-phone');
        homePhone.on('change', function (e) {
            if ($(this).valid()) {
                $('#cell-phone').rules("remove", "required");
                $('#cell-phone').removeClass('input-validation-error');
                $('#cell-phone').next('.text-danger').empty();
                dispatch(createAction(clientActions.SET_PHONE, e.target.value));
            } else {
                $('#cell-phone').rules("add", "required");
            }
        });

        var cellPhone = $('#cell-phone');
        cellPhone.on('change', function (e) {
            if ($(this).valid()) {
                $('#home-phone').rules("remove", "required");
                $('#home-phone').removeClass('input-validation-error');
                $('#home-phone').next('.text-danger').empty();
                dispatch(createAction(clientActions.SET_CELL_PHONE, e.target.value));
            } else {
                $('#home-phone').rules("add", "required");
            }
        });

        var email = $('#email');
        email.on('change', function (e) {
            if ($(this).valid()) {
                dispatch(createAction(clientActions.SET_EMAIL, e.target.value));
            }
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