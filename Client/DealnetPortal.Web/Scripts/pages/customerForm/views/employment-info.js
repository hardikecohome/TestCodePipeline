module.exports('employment-info-view', function (require) {
    var customerActions = require('customer-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;
    var observe = require('redux').observe;

    return function (store) {
        var dispatch = store.dispatch;

        var status = $('#emp-status');

        var incomeType = $('#income-type');

        var annual = $('#annual');

        var hourly = $('#hourly');

        var years = $('#emp-years');

        var months = $('#emp-months');

        var empType = $('#emp-type');

        var jobTitle = $('#job-title');

        var name = $('#company-name');

        var phone = $('#company-phone');

        var street = $('#company-street');

        var unit = $('#company-unit');

        var city = $('#company-city');

        var province = $('#company-province');

        var postal = $('#company-postal-code');

        var initialStateMap = {
            employStatus: status,
            incomeType: incomeType,
            annualSalary: annual,
            hourlyRate: hourly,
            yearsOfEmploy: years,
            monthsOfEmploy: months,
            employType: empType,
            jobTitle: jobTitle,
            companyName: name,
            companyPhone: phone,
            cstreet: street,
            cunit: unit,
            ccity: city,
            cprovince: province,
            cpostalCode: postal
        };

        dispatch(createAction(customerActions.SET_INITIAL_STATE), readInitialStateFromFields(initialStateMap));

        var observeCustomerFormStore = observe(store);

        observeCustomerFormStore(function (store) {
            return {
                street: state.cstreet,
                unit: state.cunit,
                city: state.ccity,
                province: state.cprovince,
                postalCode: state.cpostalCode
            };
        })(function (props) {
            street.val(props.state);
            unit.val(props.unit);
            city.val(props.city);
            province.val(props.province);
            postal.val(props.postalCode);
        });
    }
});
