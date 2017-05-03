﻿module.exports('new-client-actions', function () {
    return {
        SET_INITIAL_STATE: 'set_initial_state',
        SET_NAME: 'set_name',
        SET_LAST: 'set_last',
        SET_BIRTH: 'set_birth',
        SET_SIN: 'set_sin',
        SET_STREET: 'set_street',
        SET_UNIT: 'set_unit',
        SET_CITY: 'set_city',
        SET_PROVINCE: 'set_province',
        SET_POSTAL_CODE: 'set_postal_code',
        CLEAR_ADDRESS: 'clear_address',
        TOGGLE_OWNERSHIP: 'toggle_ownership',
        SET_PSTREET: 'set_pstreet',
        SET_PUNIT: 'set_punit',
        SET_PCITY: 'set_pcity',
        SET_PPROVINCE: 'set_pprovince',
        SET_PPOSTAL_CODE: 'set_ppostal_code',
        CLEAR_PADDRESS: 'clear_paddress',
        SUBMIT: 'submit',
        DISPLAY_SUBMIT_ERRORS: 'display_submit_errors',
        DISPLAY_ADDRESS_INFO: 'display_address_info',
        DISPLAY_CONTACT_INFO: 'display_contact_info',
        ACTIVATE_ADDRESS_INFO: 'activate_address_info',
        ACTIVATE_CONTACT_INFO: 'activate_contact_info',
        ACTIVATE_HOME_IMPROVMENTS: 'activate_home_improvments',
        TOGGLE_CREDIT_AGREEMENT: 'toggle_credit_agreement',
        TOGGLE_CONTACT_AGREEMENT: 'toggle_contact_agreement',
        SET_LESS_THAN_SIX: 'set_less_than_six',
        SET_PHONE: 'set_phone',
        SET_CELL_PHONE: 'set_cell_phone',
        SET_ADDRESS: 'set_address',
        SET_PADDRESS: 'set_paddress',
        SET_COMMENT: 'set_comment',
        SET_EMAIL: 'set_email',
        SET_CONTACT_METHOD: 'set_contact_method'
    };
});