module.exports('your-info-view', function (require) {
    var customerActions = require('customer-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;

    return function (store) {
        var dispatch = store.dispatch;
        var birth = $('body').is('.ios-device') ? $("#birth-date-customer").siblings('.div-datepicker') : $("#birth-date-customer");

        inputDateFocus(birth);

        birth.datepicker({
            yearRange: '1900:' + (new Date().getFullYear() - 18),
            minDate: Date.parse("1900-01-01"),
            maxDate: new Date(new Date().setFullYear(new Date().getFullYear() - 18)),
            onSelect: function(day, date){
                dispatch(createAction(customerActions.SET_BIRTH, day));
                //$(this).siblings('.div-datepicker-value').text(day);
                $(this).siblings('input.form-control').val(day);
                $(".div-datepicker").removeClass('opened');
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
            birthday: birth
        };

        dispatch(createAction(customerActions.SET_INITIAL_STATE, readInitialStateFromFields(initialStateMap)));
    };
});