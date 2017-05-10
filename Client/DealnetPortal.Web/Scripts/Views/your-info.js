module.exports('your-info-view', function (require) {
    var customerActions = require('customer-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;

    return function (store) {
        var dispatch = store.dispatch;
        var birth = $("#birth-date-customer");
        inputDateFocus(birth);

        birth.datepicker({
            dateFormat: 'mm/dd/yy',
            changeYear: true,
            changeMonth: (viewport().width < 768) ? true : false,
            yearRange: '1900:' + (new Date().getFullYear() - 18),
            minDate: Date.parse("1900-01-01"),
            maxDate: new Date(new Date().setFullYear(new Date().getFullYear() - 18)),
            onSelect: function (day) {
                dispatch(createAction(customerActions.SET_BIRTH, day));
            }
        });
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
            birthday: birth,
        };

        dispatch(createAction(customerActions.SET_INITIAL_STATE, readInitialStateFromFields(initialStateMap)));
    };
});