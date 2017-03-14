module.exports('customer-form', function (require) {
    var makeReducer = require('redux').makeReducer;
    var applyMiddleware = require('redux').applyMiddleware;
    var makeStore = require('redux').makeStore;
    var createAction = require('redux').createAction;
    var observe = require('redux').observe;

    var shallowDiff = require('objectUtils').shallowDiff;
    var compose = require('functionUtils').compose;

    var log = require('logMiddleware');

    // your info actions
    var SET_NAME = 'set_name';
    var SET_LAST = 'set_last';
    var SET_BIRTH = 'set_birth';
    var SET_SIN = 'set_sin';
    var SET_STREET = 'set_street';
    var SET_UNIT = 'set_unit';
    var SET_CITY = 'set_city';
    var SET_PROVINCE = 'set_province';
    var SET_POSTAL_CODE = 'set_postal_code';
    var CLEAR_ADDRESS = 'clear_address';
    var TOGGLE_OWNERSHIP = 'toggle_ownership';
    var SET_PSTREET = 'set_pstreet';
    var SET_PUNIT = 'set_punit';
    var SET_PCITY = 'set_pcity';
    var SET_PPROVINCE = 'set_pprovince';
    var SET_PPOSTAL_CODE = 'set_ppostal_code';
    var CLEAR_PADDRESS = 'clear_paddress';
    var SUBMIT = 'submit';
    var DISPLAY_SUBMIT_ERRORS = 'display_submit_errors';
    var DISPLAY_INSTALLATION = 'display_installation';
    var DISPLAY_CONTACT_INFO = 'display_contact_info';
    var ACTIVATE_INSTALLATION = 'activate_installation';
    var ACTIVATE_CONTACT_INFO = 'activate_contact_info';
    var SET_CAPTCHA_CODE = 'set_captcha_code';
    var TOGGLE_AGREEMENT = 'toggle_agreement';

    var iniState = {
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
        agreesToPassInfo: false,
        agreesToBeContacted: true,
        displaySubmitErrors: false,
        displayInstallation: false,
        displayContactInfo: false,
        activePanel: 'yourInfo',
        captchaCode: '',
        agreement: false,
    };

    var setFormField = function (field) {
        return function (state, action) {
            return $.extend({}, state, {
                [field]: action.payload,
            });
        };
    };

    // your info reducer
    var reducer = makeReducer({
        [SET_BIRTH]: setFormField('birthday'),
        [SET_STREET]: setFormField('street'),
        [SET_UNIT]: setFormField('unit'),
        [SET_CITY]: setFormField('city'),
        [SET_PROVINCE]: setFormField('province'),
        [SET_POSTAL_CODE]: setFormField('postalCode'),
        [CLEAR_ADDRESS]: function () {
            return {
                street: '',
                unit: '',
                city: '',
                province: '',
                postalCode: '',
            };
        },
        [SET_PSTREET]: setFormField('pstreet'),
        [SET_PUNIT]: setFormField('punit'),
        [SET_PCITY]: setFormField('pcity'),
        [SET_PPROVINCE]: setFormField('pprovince'),
        [SET_PPOSTAL_CODE]: setFormField('ppostalCode'),
        [CLEAR_PADDRESS]: function () {
            return {
                pstreet: '',
                punit: '',
                pcity: '',
                pprovince: '',
                ppostalCode: '',
            };
        },
        [DISPLAY_SUBMIT_ERRORS]: setFormField('displaySubmitErrors'),
        [DISPLAY_INSTALLATION]: setFormField('displayInstallation'),
        [DISPLAY_CONTACT_INFO]: setFormField('displayContactInfo'),
        [ACTIVATE_INSTALLATION]: function () {
            return {
                displayInstallation: true,
                activePanel: 'installation',
            };
        },
        [ACTIVATE_CONTACT_INFO]: function () {
            return {
                displayContactInfo: true,
                activePanel: 'contactInfo',
            };
        },
        [TOGGLE_OWNERSHIP]: setFormField('ownership'),
        [TOGGLE_AGREEMENT]: setFormField('agreement'),
        [SET_CAPTCHA_CODE]: setFormField('captchaCode'),
    }, iniState);

    // selectors
    var getErrors = function (state) {
        var errors = [];

        if (state.birthday !== '') {
            var ageDifMs = Date.now() - Date.parseExact(state.birthday, "M/d/yyyy");
            var ageDate = new Date(ageDifMs);
            var age = Math.abs(ageDate.getUTCFullYear() - 1970);

            if (age > 75) {
                errors.push({
                    type: 'birthday',
                    messageKey: 'ApplicantNeedsToBeUnder75',
                });
            }
        }

        if (!state.ownership) {
            errors.push({
                type: 'ownership',
                messageKey: 'AtLeastOneHomeOwner',
            });
        }

        if (!state.captchaCode) {
            errors.push({
                type: 'captcha',
                messageKey: 'EmptyCaptcha',
            });
        }

        if (!state.agreement) {
            errors.push({
                type: 'agreement',
                messageKey: 'EmptyAgreement'
            });
        }

        return errors;
    };

    var flowMiddleware = function (store) {
        return function (next) {
            var flow1 = [SET_NAME, SET_LAST, SET_BIRTH];
            var flow2 = [SET_STREET, SET_CITY, SET_PROVINCE, SET_POSTAL_CODE, TOGGLE_OWNERSHIP];
            return function (action) {
                var state = store.getState();

                var nextAction = next(action);
                if (state.activePanel === 'yourInfo') {
                    var index1 = flow1.indexOf(action.type);
                    if (index1 >= 0) {
                        flow1.splice(index1, 1);
                    }
                    if (flow1.length === 0) {
                        next(createAction(ACTIVATE_INSTALLATION, true));
                    }
                }

                if (state.activePanel === 'installation') {
                    var index2 = flow2.indexOf(action.type);
                    if (index2 >= 0) {
                        flow2.splice(index2, 1);
                    }
                    if (flow2.length === 0) {
                        next(createAction(ACTIVATE_CONTACT_INFO, true));
                    }
                }

                var stateAfter = store.getState();
                var errors = getErrors(stateAfter)
                var installationErrors = errors.filter(function (error) { return error.type === 'ownership'; });
                if (stateAfter.displaySubmitErrors && installationErrors.length > 0) {
                    next(createAction(DISPLAY_INSTALLATION, true));
                }

                var contactInfoErrors = errors.filter(function (error) {
                    return ['captcha', 'agreement'].some(function (item) {
                        return error.type === item;
                    });
                });

                if (stateAfter.displaySubmitErrors && contactInfoErrors.length > 0) {
                    next(createAction(DISPLAY_CONTACT_INFO, true));
                }

                return nextAction;
            };
        };
    };

    var displayErrorsMiddleware = function (store) {
        return function (next) {
            return function (action) {
                var nextAction = next(action);
                if (action.type === SUBMIT) {
                    next(createAction(DISPLAY_SUBMIT_ERRORS, true));
                }

                return nextAction;
            }
        }
    };

    var customerFormStore = compose(applyMiddleware([
        displayErrorsMiddleware,
        flowMiddleware,
        log('store/customerForm')
    ]), makeStore)(reducer);

    var dispatch = customerFormStore.dispatch;

    // view layer
    var observeCustomerFormStore = observe(customerFormStore);

    $(document)
        .ready(function () {
            $('<option selected value="">- ' + translations['NotSelected'] + ' -</option>').prependTo($('#selectedService'));
            $('#selectedService').val($('#selectedService > option:first').val());
            var input = $("#birth-date-customer");
            inputDateFocus(input);

            input.datepicker({
                dateFormat: 'mm/dd/yy',
                changeYear: true,
                changeMonth: (viewport().width < 768) ? true : false,
                yearRange: '1900:' + (new Date().getFullYear() - 18),
                minDate: Date.parse("1900-01-01"),
                maxDate: new Date(new Date().setFullYear(new Date().getFullYear() - 18)),
                onSelect: function (day) {
                    dispatch(createAction(SET_BIRTH, day));
                }
            });

            window.onLoadCaptcha = function () {
                grecaptcha.render('gcaptcha', {
                    sitekey: '6LeqxBgUAAAAAJnAV6vqxzZ5lWOS5kzs3lfxFKEQ',
                    callback: function (response) {
                        dispatch(createAction(SET_CAPTCHA_CODE, response));
                    },
                });
            };

            // action dispatchers
            $('#firstName').on('change', function (e) {
                dispatch(createAction(SET_NAME, e.target.value));
            });
            $('#lastName').on('change', function (e) {
                dispatch(createAction(SET_LAST, e.target.value));
            });
            $('#sin').on('change', function (e) {
                dispatch(createAction(SET_SIN, e.target.value));
            });
            $('#street').on('change', function (e) {
                dispatch(createAction(SET_STREET, e.target.value));
            });
            $('#unit').on('change', function (e) {
                dispatch(createAction(SET_UNIT, e.target.value));
            });
            $('#city').on('change', function (e) {
                dispatch(createAction(SET_CITY, e.target.value));
            });
            $('#province').on('change', function (e) {
                dispatch(createAction(SET_PROVINCE, e.target.value));
            });
            $('#postalCode').on('change', function (e) {
                dispatch(createAction(SET_POSTAL_CODE, e.target.value));
            });
            $('#clearAddress').on('click', function (e) {
                dispatch(createAction(CLEAR_ADDRESS, e.target.value));
            });
            $('#homeowner-checkbox').on('click', function (e) {
                dispatch(createAction(TOGGLE_OWNERSHIP, $('#homeowner-checkbox').prop('checked') ));
            });
            $('#pstreet').on('change', function (e) {
                dispatch(createAction(SET_PSTREET, e.target.value));
            });
            $('#punit').on('change', function (e) {
                dispatch(createAction(SET_PUNIT, e.target.value));
            });
            $('#pcity').on('change', function (e) {
                dispatch(createAction(SET_PCITY, e.target.value));
            });
            $('#pprovince').on('change', function (e) {
                dispatch(createAction(SET_PPROVINCE, e.target.value));
            });
            $('#ppostalCode').on('change', function (e) {
                dispatch(createAction(SET_PPOSTAL_CODE, e.target.value));
            });
            $('#pclearAddress').on('click', function (e) {
                dispatch(createAction(CLEAR_PADDRESS, e.target.value));
            });
            $('#agreement1').on('click', function (e) {
                dispatch(createAction(TOGGLE_AGREEMENT, $('#agreement1').prop('checked') ));
            });
            $('#submit').on('click', function (e) {
                dispatch(createAction(SUBMIT));
                var errors = getErrors(customerFormStore.getState());
                if (errors.length > 0) {
                    e.preventDefault();
                }
            });

            var hideYourInfoFirstTime = true;
            var hideIntallationFirstTime = true;

            observeCustomerFormStore(function (state) {
                return {
                    displayInstallation: state.displayInstallation,
                    displayContactInfo: state.displayContactInfo,
                    activePanel: state.activePanel,
                };
            })(function (props) {
                if (props.activePanel === 'yourInfo') {
                    $('#yourInfoPanel').addClass('active-panel');
                } else {
                    $('#yourInfoPanel').removeClass('active-panel');
                }

                if (props.displayInstallation) {
                    $('#installationAddressForm').slideDown();
                }

                if (props.activePanel === 'installation') {
                    $('#installationAddressPanel').addClass('active-panel');
                } else {
                    $('#installationAddressPanel').removeClass('active-panel');
                }

                if (props.displayContactInfo) {
                    $('#contactInfoForm').slideDown();
                }

                if (props.activePanel === 'contactInfo') {
                    $('#contactInfoPanel').addClass('active-panel');
                } else {
                    $('#contactInfoPanel').removeClass('active-panel');
                }
            });

            observeCustomerFormStore(function (state) {
                return {
                    street: state.street,
                    unit: state.unit,
                    city: state.city,
                    province: state.province,
                    postalCode: state.postalCode,
                };
            })(function (props) {
                $('#street').val(props.street);
                $('#unit').val(props.unit);
                $('#city').val(props.city);
                $('#province').val(props.province);
                $('#postalCode').val(props.postalCode);
            });

            observeCustomerFormStore(function (state) {
                return {
                    street: state.pstreet,
                    unit: state.punit,
                    city: state.pcity,
                    province: state.pprovince,
                    postalCode: state.ppostalCode,
                };
            })(function (props) {
                $('#pstreet').val(props.street);
                $('#punit').val(props.unit);
                $('#pcity').val(props.city);
                $('#pprovince').val(props.province);
                $('#ppostalCode').val(props.postalCode);
            });

            var createError = function (msg) {
                var err = $('<div class="well danger-well over-aged-well" id="age-error-message"><svg aria-hidden="true" class="icon icon-info-well"><use xlink:href="/Content/images/sprite/sprite.svg#icon-info-well"></use></svg></div>');
                err.append(msg);
                return err;
            };

            observeCustomerFormStore(function (state) {
                return {
                    errors: getErrors(state),
                    displaySubmitErrors: state.displaySubmitErrors,
                }
            })(function (props) {
                $('#yourInfoErrors').empty();
                if (props.errors.length > 0) {
                    props.errors
                        .filter(function (error) { return error.type === 'birthday' })
                        .forEach(function (error) {
                            $('#yourInfoErrors').append(createError(window.translations[error.messageKey]));
                        });
                }

                $('#installationErrors').empty();
                if (props.displaySubmitErrors && props.errors.length > 0) {
                    props.errors
                        .filter(function (error) { return error.type === 'ownership' })
                        .forEach(function (error) {
                            $('#installationErrors').append(createError(window.translations[error.messageKey]));
                        });
                }

                $('#contactInfoErrors').empty();
                if (props.displaySubmitErrors && props.errors.length > 0) {
                    props.errors.filter(function (error) {
                        return ['captcha', 'agreement'].some(function (item) {
                            return error.type === item;
                        })
                    })
                        .forEach(function (error) {
                            $('#contactInfoErrors').append(createError(window.translations[error.messageKey]));
                        });
                }
            });
        });
});


