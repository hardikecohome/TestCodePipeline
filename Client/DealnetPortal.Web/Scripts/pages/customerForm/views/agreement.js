module.exports('agreement-view', function (require) {
    var createAction = require('redux').createAction;
    var customerActions = require('customer-actions');
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;

    return function (store) {
        var dispatch = store.dispatch;

        var creditAgreement = $('#agreement1');
        creditAgreement.on('click', function (e) {
            dispatch(createAction(customerActions.TOGGLE_CREDIT_AGREEMENT, creditAgreement.prop('checked')));
        });

        var contactAgreement = $('#agreement2');
        contactAgreement.on('click', function (e) {
            dispatch(createAction(customerActions.TOGGLE_CONTACT_AGREEMENT, contactAgreement.prop('checked')));
        });

        var initialStateMap = {
            creditAgreement: creditAgreement,
            contactAgreement: contactAgreement
        };

        dispatch(createAction(customerActions.SET_INITIAL_STATE, readInitialStateFromFields(initialStateMap)));
    };
});
