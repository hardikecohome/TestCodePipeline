module.exports('client-consents-view', function (require) {
    var clientActions = require('new-client-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;

    return function (store) {
        var dispatch = store.dispatch;

        var creditAgreementCheck = $('#agreement-checkbox-data1');
        creditAgreementCheck.on('click', function (e) {
            dispatch(createAction(clientActions.TOGGLE_CREDIT_AGREEMENT, creditAgreementCheck.prop('checked')));
        });

        var contactAgreementCheck = $('#agreement-checkbox-data2');
        contactAgreementCheck.on('click', function (e) {
            dispatch(createAction(clientActions.TOGGLE_CONTACT_AGREEMENT, contactAgreementCheck.prop('checked')));
        });

        var initialStateMap = {
            creditAgreement: creditAgreementCheck,
            contactAgreement: contactAgreementCheck
        };

        dispatch(createAction(clientActions.SET_INITIAL_STATE, readInitialStateFromFields(initialStateMap)));
    };
});