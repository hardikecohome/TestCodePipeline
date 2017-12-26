module.exports('employment-info-view', function (require) {
    var customerActions = require('customer-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;
    var observe = require('redux').observe;

    return function (store) {
        var dispatch = store.dispatch;

        var status = $('#emp-status');
        status.on('change', function (e) {
            changeStatus(e.target.value);
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

        $('#cclear-address').on('click', function (e) {
            e.preventDefault();
            dispatch(createAction(customerActions.CLEAR_CADDRESS, e.target.value));
        });

        function changeStatus (value) {
            switch (value) {
                case '0':
                    changeToEmployed();
                    break;
                case '1':
                case '3':
                    changeToUnemployedOrRetired();
                    break;
                case '2':
                    changeToSelfEmployed();
                    break;
                default:
                    clearStatus();
                    break;
            }
            dispatch(createAction(customerActions.SET_EMPLOYMENT_STATUS, e.target.value));
        }

        function clearStatus () {
            incomeType.prop('disabled', true).parents('.form-group').addClass('hidden');
            annual.prop('disabled', true).parents('.form-group').addClass('hidden');
            hourly.prop('disabled', true).parents('.form-group').addClass('hidden');
            empType.prop('disabled', true).parents('.form-group').addClass('hidden');
            years.prop('disabled', true).parents('.form-group').addClass('hidden');
            months.prop('disabled', true);
            $('#company-info-hold').addClass('hidden');
            jobTitle.prop('disabled', true);
            name.prop('disabled', true);
            phone.prop('disabled', true);
            $('#company-address-hold').addClass('hidden');
            street.prop('disabled', true);
            unit.prop('disabled', true);
            city.prop('disabled', true);
            province.prop('disabled', true);
            postal.prop('disabled', true);
        }

        function changeToEmployed () {
            incomeType.prop('disabled', false).parents('.form-group').removeClass('hidden');
            annual.parents('.form-group').addClass('hidden');
            hourly.parents('.form-group').addClass('hidden');
            years.prop('disabled', false).parents('.form-group').removeClass('hidden');
            months.prop('disabled', false);
            empType.prop('disabled', false).parents('.form-group').removeClass('hidden');
            $('#company-info-hold').removeClass('hidden');
            jobTitle.prop('disabled', false);
            name.prop('disabled', false);
            phone.prop('disabled', false);
            $('#company-address-hold').removeClass('hidden');
            street.prop('disabled', false);
            unit.prop('disabled', false);
            city.prop('disabled', false);
            province.prop('disabled', false);
            postal.prop('disabled', false);
        }

        function changeToUnemployedOrRetired () {
            annual.prop('disabled', false).parents('.form-group').removeClass('hidden');

            incomeType.prop('disabled', true).parents('.form-group').addClass('hidden');
            hourly.prop('disabled', true).parents('.form-group').addClass('hidden');
            empType.prop('disabled', true).parents('.form-group').addClass('hidden');
            years.prop('disabled', true).parents('.form-group').addClass('hidden');
            months.prop('disabled', true);
            $('#company-info-hold').addClass('hidden');
            jobTitle.prop('disabled', true);
            name.prop('disabled', true);
            phone.prop('disabled', true);
            $('#company-address-hold').addClass('hidden');
            street.prop('disabled', true);
            unit.prop('disabled', true);
            city.prop('disabled', true);
            province.prop('disabled', true);
            postal.prop('disabled', true);
        }

        function changeToSelfEmployed () {
            annual.prop('disabled', false).parents('.form-group').removeClass('hidden');
            years.prop('disabled', false).parents('.form-group').removeClass('hidden');
            months.prop('disabled', false);
            $('#company-info-hold').removeClass('hidden');
            jobTitle.prop('disabled', false);
            name.prop('disabled', false);
            phone.prop('disabled', false);
            $('#company-address-hold').removeClass('hidden');
            street.prop('disabled', false);
            unit.prop('disabled', false);
            city.prop('disabled', false);
            province.prop('disabled', false);
            postal.prop('disabled', false);

            incomeType.prop('disabled', true).parents('.form-group').addClass('hidden');
            hourly.prop('disabled', true).parents('.form-group').addClass('hidden');
        }

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

        observeCustomerFormStore(function (state) {
            debugger
            return {
                street: state.cstreet,
                unit: state.cunit,
                city: state.ccity,
                province: state.cprovince,
                postalCode: state.cpostalCode
            };
        })(function (props) {
            debugger
            street.val(props.street);
            unit.val(props.unit);
            city.val(props.city);
            province.val(props.province);
            postal.val(props.postalCode);
        });

        observeCustomerFormStore(function (state) {
            debugger
            return {
                province: state.province
            };
        })(function (props) {
            debugger
            if (props.province.toLowerCase() === 'qc') {
                $('#employmentInfoForm').removeClass('hidden');
                status.val('').change().prop('disabled', false);
            } else {
                $('#employmentInfoForm').addClass('hidden').find('input, select').each(function () {
                    $(this).prop('disabled', true);
                });
            }
        });
    };
});
