module.exports('employment-info-view', function (require) {
    var customerActions = require('customer-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;
    var observe = require('redux').observe;

    return function (store) {
        var dispatch = store.dispatch;

        var status = $('#emp-status');
        status.on('change', function (e) {
            dispatch(createAction(customerActions.SET_EMPLOYMENT_STATUS, e.target.value));
        });

        var incomeType = $('#income-type');
        incomeType.on('change', function (e) {
            dispatch(createAction(customerActions.SET_INCOME_TYPE, e.target.value));
        });

        var annual = $('#annual');
        annual.on('change', function (e) {
            dispatch(createAction(customerActions.SET_ANNUAL_SALARY, e.target.value));
        });

        var hourly = $('#hourly');
        hourly.on('change', function (e) {
            dispatch(createAction(customerActions.SET_HOURLY_RATE, e.target.value));
        });

        var years = $('#emp-years');
        years.on('change', function (e) {
            dispatch(createAction(customerActions.SET_YEARS_OF_EMPLOY, e.target.value));
        });

        var months = $('#emp-months');
        months.on('change', function (e) {
            dispatch(createAction(customerActions.SET_MONTHS_OF_EMPLOY, e.target.value));
        });

        var empType = $('#emp-type');
        empType.on('change', function (e) {
            dispatch(createAction(customerActions.SET_EMPLOY_TYPE, e.target.value));
        });

        var jobTitle = $('#job-title');
        jobTitle.on('change', function (e) {
            dispatch(createAction(customerActions.SET_JOB_TITLE, e.target.value));
        });

        var name = $('#company-name');
        name.on('change', function (e) {
            dispatch(createAction(customerActions.SET_COMPANY_NAME, e.target.value));
        });

        var phone = $('#company-phone');
        phone.on('change', function (e) {
            dispatch(createAction(customerActions.SET_COMPANY_PHONE, e.target.value));
        });

        var street = $('#company-street');
        street.on('change', function (e) {
            dispatch(createAction(customerActions.SET_CSTREET, e.target.value));
        });

        var unit = $('#company-unit');
        unit.on('change', function (e) {
            dispatch(createAction(customerActions.SET_CUNIT, e.target.value));
        });

        var city = $('#company-city');
        city.on('change', function (e) {
            dispatch(createAction(customerActions.SET_CCITY, e.target.value));
        });

        var province = $('#company-province');
        province.on('change', function (e) {
            dispatch(createAction(customerActions.SET_CPROVINCE, e.target.value));
        });

        var postal = $('#company-postal-code');
        postal.on('change', function (e) {
            dispatch(createAction(customerActions.SET_CPOSTAL_CODE, e.target.value));
        });

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
