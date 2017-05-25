module.exports('contact-information-view', function (require) {
    var clientActions = require('new-client-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;

    return function (store) {
        var dispatch = store.dispatch;
       
        var homePhone = $('#home-phone');
        homePhone.on('change', function (e) {
            togglePhone('#cell-phone', clientActions.SET_PHONE, e);
        });

        var cellPhone = $('#cell-phone');
        cellPhone.on('change', function (e) {
            togglePhone('#home-phone', clientActions.SET_CELL_PHONE, e);
        });

        var email = $('#email');
        email.on('change', function (e) {
            dispatch(createAction(clientActions.SET_EMAIL, e.target.value));
        });

        var contactMethod = $('#contact-method');
        contactMethod.on('change', function (e) {
            dispatch(createAction(clientActions.SET_CONTACT_METHOD, e.target.value));
        });

        function togglePhone(selector, action, event) {
            var label = $(selector).parent().parent().find('label');
            if ($(event.currentTarget).valid()) {
                if (label.hasClass('mandatory-field')) {
                    label.removeClass('mandatory-field');
                }
                $(selector).rules("remove", "required");
                $(selector).removeClass('input-validation-error');
                $(selector).next('.text-danger').empty();
                dispatch(createAction(action, event.target.value));
            } else {
                dispatch(createAction(action, event.target.value));

                if (!label.hasClass('mandatory-field')) {
                    label.addClass('mandatory-field');
                }

                $(selector).rules("add", "required");
            }
        }

        var initialStateMap = {
            phone: homePhone,
            cellPhone: cellPhone,
            email: email,
            contactMethod: contactMethod
        };

        dispatch(createAction(clientActions.SET_INITIAL_STATE, readInitialStateFromFields(initialStateMap)));
    };
});