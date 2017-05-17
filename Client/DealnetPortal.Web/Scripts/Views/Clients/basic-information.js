module.exports('basic-information-view', function (require) {
    var clientActions = require('new-client-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;

    return function (store) {
        var dispatch = store.dispatch;
        var birth = $("#birth-date");
        inputDateFocus(birth);
        birth.datepicker({
            dateFormat: 'mm/dd/yy',
            changeYear: true,
            changeMonth: (viewport().width < 768) ? true : false,
            yearRange: '1900:' + (new Date().getFullYear() - 18),
            minDate: Date.parse("1900-01-01"),
            maxDate: new Date(new Date().setFullYear(new Date().getFullYear() - 18)),
            onSelect: function (day) {
                onDateSelect($(this));
                dispatch(createAction(clientActions.SET_BIRTH, day));
            }
        });
        $('#ui-datepicker-div').addClass('cards-datepicker');

        var name = $('#first-name');
        name.on('change', function (e) {
            if ($(this).valid()) {
                dispatch(createAction(clientActions.SET_NAME, e.target.value));
            }
        });
        var lastName = $('#last-name');
        lastName.on('change', function (e) {
            if ($(this).valid()) {
                dispatch(createAction(clientActions.SET_LAST, e.target.value));
            }
        });

        var initialStateMap = {
            name: name,
            lastName: lastName,
            birthday: birth
        };

        dispatch(createAction(clientActions.SET_INITIAL_STATE, readInitialStateFromFields(initialStateMap)));
    };
});