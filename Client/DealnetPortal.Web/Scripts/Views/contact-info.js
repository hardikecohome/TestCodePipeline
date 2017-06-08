module.exports('contact-info-view', function (require) {
    var customerActions = require('customer-actions');
    var createAction = require('redux').createAction;
    var getRequiredPhones = require('customer-selectors').getRequiredPhones;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;
    var observe = require('redux').observe;

    return function (store) {
        var dispatch = store.dispatch;

        var phone = $('#homePhone');
        phone.on('change', function (e) {
            dispatch(createAction(customerActions.SET_PHONE, e.target.value));
        });

        var cellPhone = $('#cellPhone');
        cellPhone.on('change', function (e) {
            dispatch(createAction(customerActions.SET_CELL_PHONE, e.target.value));
        });

        var comment = $('#comment');
        comment.on('change', function (e) {
            dispatch(createAction(customerActions.SET_COMMENT, e.target.value));
        });

        var email = $('#email');
        email.on('change', function (e) {
            dispatch(createAction(customerActions.SET_EMAIL, e.target.value));
        });


        var initialStateMap = {
            cellPhone: cellPhone,
            phone: phone,
            comment: comment,
            email: email,
        };

        dispatch(createAction(customerActions.SET_INITIAL_STATE, readInitialStateFromFields(initialStateMap)));

        var observeCustomerFormStore = observe(store);

        //observeCustomerFormStore(getRequiredPhones)(function (props) {
        //    $('#homePhone').rules(props.phone ? 'add' : 'remove', 'required');
        //    $('#cellPhone').rules(props.cellPhone ? 'add' : 'remove', 'required');

        //    if (props.phone) {
        //        $('#homePhoneWrapper').addClass('mandatory-field');
        //    } else {
        //        $('#homePhoneWrapper').removeClass('mandatory-field');
        //    }

        //    if (props.cellPhone) {
        //        $('#cellPhoneWrapper').addClass('mandatory-field');
        //    } else {
        //        $('#cellPhoneWrapper').removeClass('mandatory-field');
        //    }
        //});
    };
});
