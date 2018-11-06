module.exports('new-client-reducer', function (require) {
    var makeReducer = require('redux').makeReducer;
    var clientActions = require('new-client-actions');

    var iniState = {
        selectedEquipment: [],
        name: '',
        lastName: '',
        birthday: '',
        street: '',
        unit: '',
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
        displayAddressInfo: false,
        displayContactInfo: false,
        displayImprovmentInfo: false,
        isValidForm: false,
        unknownAddress: false,
        activePanel: 'basic-information',
        phone: '',
        cellPhone: '',
        email: '',
        comment: '',
        contactMethod: '',
        creditAgreement: false,
        contactAgreement: false,
        lessThanSix: false,
        improvmentOtherAddress: false,
        improvmentCurrentAddress: true,
        improvmentStreet: '',
        improvmentUnit: '',
        improvmentCity: '',
        improvmentProvince: '',
        improvmentPostalCode: '', 
        improvmentMoveInDate: '',
        isChanged: false,
        displayNewAddress: true,
        emailExists: false
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
    reducerObj[clientActions.SET_INITIAL_STATE] = function(state, action) {
        return $.extend({}, state, action.payload);
    };

    reducerObj[clientActions.SET_NAME] = setFormField('name');
    reducerObj[clientActions.SET_LAST] = setFormField('lastName');
    reducerObj[clientActions.SET_BIRTH] = setFormField('birthday');
    reducerObj[clientActions.SET_STREET] = setFormField('street');
    reducerObj[clientActions.SET_UNIT] = setFormField('unit');
    reducerObj[clientActions.SET_CITY] = setFormField('city');
    reducerObj[clientActions.SET_PROVINCE] = setFormField('province');
    reducerObj[clientActions.SET_POSTAL_CODE] = setFormField('postalCode');
    reducerObj[clientActions.SET_EMAIL_EXISTS] = setFormField('emailExists');

    reducerObj[clientActions.SET_IMPROVMENT_STREET] = setFormField('improvmentStreet');
    reducerObj[clientActions.SET_IMPROVMENT_UNIT] = setFormField('improvmentUnit');
    reducerObj[clientActions.SET_IMPROVMENT_CITY] = setFormField('improvmentCity');
    reducerObj[clientActions.SET_IMPROVMENT_PROVINCE] = setFormField('improvmentProvince');
    reducerObj[clientActions.SET_IMPROVMENT_POSTAL_CODE] = setFormField('improvmentPostalCode');
    reducerObj[clientActions.SET_IMPROVMENT_MOVE_DATE] = setFormField('improvmentMoveInDate');

    reducerObj[clientActions.SET_IMPROVMENT_INFO] = setFormField('displayImprovmentInfo');
    reducerObj[clientActions.SET_CURRENT_ADDRESS] = function(state, action) {
        var fieldObj = {};
        fieldObj['improvmentCurrentAddress'] = action.payload;
        fieldObj['improvmentOtherAddress'] = !action.payload;

        return $.extend({}, state, fieldObj);
    }

    reducerObj[clientActions.DRIVER_LICENSE_UPLOADED] = function (state, action) {
        return {
            name: action.payload.firstName,
            lastName: action.payload.firstName,
            street: action.payload.street,
            city: action.payload.locality,
            province: action.payload.province,
            postalCode: action.payload.postalCode,
            birthday: action.payload.birthDate,
            displayContactInfo: true,
            activePanel: 'contact-information'
        }
    }

    reducerObj[clientActions.SET_UNKNOWN_ADDRESS] = function (state, action) {
        return {
            improvmentStreet: '',
            improvmentUnit: '',
            improvmentCity: '',
            improvmentProvince: '',
            improvmentPostalCode: '',
            improvmentMoveInDate: '',
            unknownAddress: action.payload,
            displayNewAddress: !state.displayNewAddress
        }
    }

    reducerObj[clientActions.CLEAR_ADDRESS] = function() {
        return {
            street: '',
            unit: '',
            city: '',
            province: '',
            postalCode: ''
        };
    };
    reducerObj[clientActions.SET_ADDRESS] = function(state, action) {
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
    reducerObj[clientActions.SET_PSTREET] = setFormField('pstreet');
    reducerObj[clientActions.SET_PUNIT] = setFormField('punit');
    reducerObj[clientActions.SET_PCITY] = setFormField('pcity');
    reducerObj[clientActions.SET_PPROVINCE] = setFormField('pprovince');
    reducerObj[clientActions.SET_PPOSTAL_CODE] = setFormField('ppostalCode');
    reducerObj[clientActions.CLEAR_PADDRESS] = function() {
        return {
            pstreet: '',
            punit: '',
            pcity: '',
            pprovince: '',
            ppostalCode: ''
        };
    };

    reducerObj[clientActions.SET_PADDRESS] = function(state, action) {
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

    reducerObj[clientActions.SET_IMPROVMENT_ADDRESS] = function (state, action) {
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
            improvmentStreet: street,
            improvmentCity: action.payload.city || state.city,
            improvmentProvince: action.payload.province || state.province,
            improvmentPostalCode: action.payload.postalCode || state.postalCode
        };
    };

    reducerObj[clientActions.CLEAR_IMPROVMENT_ADDRESS] = function () {
        return {
            improvmentStreet: '',
            improvmentUnit: '',
            improvmentCity: '',
            improvmentProvince: '',
            improvmentPostalCode: ''
        };
    };

    reducerObj[clientActions.SET_IMPROVMENT_OTHER_ADDRESS] = setFormField('improvmentOtherAddress');
    reducerObj[clientActions.SET_LESS_THAN_SIX] = setFormField('lessThanSix');
    reducerObj[clientActions.DISPLAY_SUBMIT_ERRORS] = setFormField('displaySubmitErrors');
    reducerObj[clientActions.DISPLAY_INSTALLATION] = setFormField('displayInstallation');
    reducerObj[clientActions.DISPLAY_CONTACT_INFO] = setFormField('displayContactInfo');
    reducerObj[clientActions.CHECK_VALIDATION] = function (state, action) {
        return {
            isChanged: !state.isChanged
        }
    };
    reducerObj[clientActions.ACTIVATE_ADDRESS_INFO] = function () {
        return {
            displayAddressInfo: true,
            activePanel: 'additional-infomration'
        };
    };
    reducerObj[clientActions.ACTIVATE_CONTACT_INFO] = function () {
        return {
            displayContactInfo: true,
            activePanel: 'contact-information'
        };
    };
    reducerObj[clientActions.ACTIVATE_HOME_IMPROVMENTS] = function () {
        return {
            activePanel: 'home-improvments'
        };
    };
    reducerObj[clientActions.ACTIVATE_CLIENT_CONSENTS] = function () {
        return {
            activePanel: 'client-consents'
        };
    };

    reducerObj[clientActions.TOGGLE_OWNERSHIP] = setFormField('ownership');
    reducerObj[clientActions.TOGGLE_CREDIT_AGREEMENT] = setFormField('creditAgreement');
    reducerObj[clientActions.TOGGLE_CONTACT_AGREEMENT] = setFormField('contactAgreement');
    reducerObj[clientActions.SET_PHONE] = setFormField('phone');
    reducerObj[clientActions.SET_CELL_PHONE] = setFormField('cellPhone');
    reducerObj[clientActions.SET_COMMENT] = setFormField('comment');
    reducerObj[clientActions.SET_EMAIL] = setFormField('email');
    reducerObj[clientActions.SET_CONTACT_METHOD] = setFormField('contactMethod');

    reducerObj[clientActions.REMOVE_EQUIPMENT] = function (state, action) {
        var fieldObj = {};
        fieldObj['selectedEquipment'] = action.payload;

        return $.extend({}, state, fieldObj);
    }

    var reducer = makeReducer(reducerObj, iniState);

    return reducer;
});