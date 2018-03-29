module.exports('customer-reducer', function (require) {
    var makeReducer = require('redux').makeReducer;
    var customerActions = require('customer-actions');

    var iniState = {
        name: '',
        lastName: '',
        birthday: '',
        street: '',
        unit: '',
        sin: '',
        city: '',
        province: '',
        postalCode: '',
        ownership: false,
        pstreet: '',
        punit: '',
        pcity: '',
        pprovince: '',
        ppostalCode: '',
        displaySubmitErrors: false,
        displayInstallation: false,
        displayContactInfo: false,
        activePanel: 'yourInfo',
        phone: '',
        cellPhone: '',
        email: '',
        comment: '',
        businessPhone: '',
        captchaCode: config.reCaptchaEnabled ? '' : 'empty',
        creditAgreement: false,
        contactAgreement: false,
        lessThanSix: false,
        employStatus: '',
        incomeType: '',
        annualSalary: '',
        hourlyRate: '',
        yearsOfEmploy: '',
        monthsOfEmploy: '',
        employType: '',
        jobTitle: '',
        companyName: '',
        companyPhone: '',
        cstreet: '',
        cunit: '',
        ccity: '',
        cprovince: '',
        cpostalCode: '',
		isQuebecDealer: false,
		monthlyMortgagePayment: 0.00
    };

    var setFormField = function (field) {
        return function (state, action) {
            var fieldObj = {};
            fieldObj[field] = action.payload;

            return $.extend({}, state, fieldObj);
        };
    };

    // your info reducer
    var reducerObj = {}
    reducerObj[customerActions.SET_INITIAL_STATE] = function (state, action) {
        return $.extend({}, state, action.payload);
    };
    reducerObj[customerActions.SET_NAME] = setFormField('name');
    reducerObj[customerActions.SET_LAST] = setFormField('lastName');
    reducerObj[customerActions.SET_BIRTH] = setFormField('birthday');
    reducerObj[customerActions.SET_STREET] = setFormField('street');
    reducerObj[customerActions.SET_UNIT] = setFormField('unit');
    reducerObj[customerActions.SET_CITY] = setFormField('city');
    reducerObj[customerActions.SET_SIN] = setFormField('sin');
    reducerObj[customerActions.SET_PROVINCE] = setFormField('province');
    reducerObj[customerActions.SET_POSTAL_CODE] = setFormField('postalCode');
    reducerObj[customerActions.CLEAR_ADDRESS] = function () {
        return {
            street: '',
            unit: '',
            city: '',
            province: '',
            postalCode: ''
        };
    };
    reducerObj[customerActions.SET_ADDRESS] = function (state, action) {
        var street = '';
        if (action.payload.number) {
            street += action.payload.number;
        }

        if (action.payload.street) {
            street = street + ' ' + action.payload.street;
        }

        if (!street) {
            street = state.street;
        }

        return {
            street: street,
            city: action.payload.city || state.city,
            province: action.payload.province || state.province,
            postalCode: action.payload.postalCode || state.postalCode
        };
    };
    reducerObj[customerActions.SET_PSTREET] = setFormField('pstreet');
    reducerObj[customerActions.SET_PUNIT] = setFormField('punit');
    reducerObj[customerActions.SET_PCITY] = setFormField('pcity');
    reducerObj[customerActions.SET_PPROVINCE] = setFormField('pprovince');
    reducerObj[customerActions.SET_PPOSTAL_CODE] = setFormField('ppostalCode');
    reducerObj[customerActions.CLEAR_PADDRESS] = function () {
        return {
            pstreet: '',
            punit: '',
            pcity: '',
            pprovince: '',
            ppostalCode: ''
        };
    };
    reducerObj[customerActions.SET_PADDRESS] = function (state, action) {
        var street = '';
        if (action.payload.number) {
            street += action.payload.number;
        }

        if (action.payload.street) {
            street = street + ' ' + action.payload.street;
        }

        if (!street) {
            street = state.street;
        }

        return {
            pstreet: street,
            pcity: action.payload.city || state.city,
            pprovince: action.payload.province || state.province,
            ppostalCode: action.payload.postalCode || state.postalCode
        };
    };
    reducerObj[customerActions.SET_LESS_THAN_SIX] = setFormField('lessThanSix');
    reducerObj[customerActions.DISPLAY_SUBMIT_ERRORS] = setFormField('displaySubmitErrors');
    reducerObj[customerActions.DISPLAY_INSTALLATION] = setFormField('displayInstallation');
    reducerObj[customerActions.DISPLAY_CONTACT_INFO] = setFormField('displayContactInfo');
    reducerObj[customerActions.ACTIVATE_INSTALLATION] = function () {
        return {
            displayInstallation: true,
            activePanel: 'installation'
        };
    };
    reducerObj[customerActions.ACTIVATE_CONTACT_INFO] = function () {
        return {
            displayContactInfo: true,
            activePanel: 'contactInfo'
        };
    };
    reducerObj[customerActions.TOGGLE_OWNERSHIP] = setFormField('ownership');
    reducerObj[customerActions.TOGGLE_CREDIT_AGREEMENT] = setFormField('creditAgreement');
    reducerObj[customerActions.TOGGLE_CONTACT_AGREEMENT] = setFormField('contactAgreement');
    reducerObj[customerActions.SET_CAPTCHA_CODE] = setFormField('captchaCode');
    reducerObj[customerActions.SET_PHONE] = setFormField('phone');
    reducerObj[customerActions.SET_CELL_PHONE] = setFormField('cellPhone');
    reducerObj[customerActions.SET_BUSINESS_PHONE] = setFormField('businessPhone');
    reducerObj[customerActions.SET_COMMENT] = setFormField('comment');
    reducerObj[customerActions.SET_EMAIL] = setFormField('email');
    reducerObj[customerActions.SET_EMPLOYMENT_STATUS] = setFormField('employStatus');
    reducerObj[customerActions.SET_INCOME_TYPE] = setFormField('incomeStatus');
    reducerObj[customerActions.SET_ANNUAL_SALARY] = setFormField('annualSalary');
    reducerObj[customerActions.SET_HOURLY_RATE] = setFormField('hourlyRate');
    reducerObj[customerActions.SET_YEARS_OF_EMPLOY] = setFormField('yearsOfEmploy');
    reducerObj[customerActions.SET_MONTHS_OF_EMPLOY] = setFormField('monthsOfEmploy');
    reducerObj[customerActions.SET_EMPLOY_TYPE] = setFormField('employType');
    reducerObj[customerActions.SET_JOB_TITLE] = setFormField('jobTitle');
    reducerObj[customerActions.SET_COMPANY_NAME] = setFormField('companyName');
    reducerObj[customerActions.SET_COMPANY_PHONE] = setFormField('companyPhone');
    reducerObj[customerActions.SET_CSTREET] = setFormField('cstreet');
    reducerObj[customerActions.SET_CUNIT] = setFormField('cunit');
    reducerObj[customerActions.SET_CCITY] = setFormField('ccity');
    reducerObj[customerActions.SET_CPROVINCE] = setFormField('cprovince');
	reducerObj[customerActions.SET_CPOSTAL_CODE] = setFormField('cpostalCode');
	reducerObj[customerActions.SET_MONTHLYMORTGAGEPAYMENT] = setFormField('monthlyMortgagePayment');
    reducerObj[customerActions.CLEAR_CADDRESS] = function () {
        return {
            cstreet: '',
            cunit: '',
            ccity: '',
            cprovince: '',
            cpostalCode: ''
        };
    };
    reducerObj[customerActions.SET_CADDRESS] = function(state, action) {
        var street = '';
        if (action.payload.number) {
            street += action.payload.number;
        }

        if (action.payload.street) {
            street += action.payload.street;
        }

        if (!street) {
            street = state.cstreet;
        }

        return {
            cstreet: street,
            ccity: action.payload.city || state.ccity,
            cprovince: action.payload.province || state.cprovince,
            cpostalCode: action.payload.postalCode || state.cpostalCode
        };
    };

    var reducer = makeReducer(reducerObj, iniState);

    return reducer;
});