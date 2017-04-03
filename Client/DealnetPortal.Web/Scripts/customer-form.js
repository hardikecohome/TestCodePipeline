module.exports('customer-form', function (require) {
    var makeReducer = require('redux').makeReducer;
    var applyMiddleware = require('redux').applyMiddleware;
    var makeStore = require('redux').makeStore;
    var createAction = require('redux').createAction;
    var observe = require('redux').observe;

    var compose = require('functionUtils').compose;

    var log = require('logMiddleware');

    // your info actions
    var SET_INITIAL_STATE = 'set_initial_state';
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
    var TOGGLE_CREDIT_AGREEMENT = 'toggle_credit_agreement';
    var TOGGLE_CONTACT_AGREEMENT = 'toggle_contact_agreement';
    var SET_LESS_THAN_SIX = 'set_less_than_six';
    var SET_PHONE = 'set_phone';
    var SET_CELL_PHONE = 'set_cell_phone';
    var SET_ADDRESS = 'set_address';
    var SET_PADDRESS = 'set_paddress';

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
        phone: '',
        cellPhone: '',
        captchaCode: '',
        creditAgreement: false,
        contactAgreement: false,
        lessThanSix: false,
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
    reducerObj[SET_INITIAL_STATE] = function(state, action) {
        return $.extend({}, state, action.payload);
    };
    reducerObj[SET_BIRTH] = setFormField('birthday');
    reducerObj[SET_STREET] = setFormField('street');
    reducerObj[SET_UNIT] = setFormField('unit');
    reducerObj[SET_CITY] = setFormField('city');
    reducerObj[SET_PROVINCE] = setFormField('province');
    reducerObj[SET_POSTAL_CODE] = setFormField('postalCode');
    reducerObj[CLEAR_ADDRESS] = function() {
        return {
            street: '',
            unit: '',
            city: '',
            province: '',
            postalCode: '',
        };
    };
    reducerObj[SET_ADDRESS] = function(state, action) {
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
            postalCode: action.payload.postalCode || state.postalCode,
        };
    };
    reducerObj[SET_PSTREET] = setFormField('pstreet');
    reducerObj[SET_PUNIT] = setFormField('punit');
    reducerObj[SET_PCITY] = setFormField('pcity');
    reducerObj[SET_PPROVINCE] = setFormField('pprovince');
    reducerObj[SET_PPOSTAL_CODE] = setFormField('ppostalCode');
    reducerObj[CLEAR_PADDRESS] = function() {
        return {
            pstreet: '',
            punit: '',
            pcity: '',
            pprovince: '',
            ppostalCode: '',
        };
    };
    reducerObj[SET_PADDRESS] = function(state, action) {
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
            ppostalCode: action.payload.postalCode || state.postalCode,
        };
    };
    reducerObj[SET_LESS_THAN_SIX] = setFormField('lessThanSix');
    reducerObj[DISPLAY_SUBMIT_ERRORS] = setFormField('displaySubmitErrors');
    reducerObj[DISPLAY_INSTALLATION] = setFormField('displayInstallation');
    reducerObj[DISPLAY_CONTACT_INFO] = setFormField('displayContactInfo');
    reducerObj[ACTIVATE_INSTALLATION] = function() {
        return {
            displayInstallation: true,
            activePanel: 'installation',
        };
    };
    reducerObj[ACTIVATE_CONTACT_INFO] = function() {
        return {
            displayContactInfo: true,
            activePanel: 'contactInfo',
        };
    };
    reducerObj[TOGGLE_OWNERSHIP] = setFormField('ownership');
    reducerObj[TOGGLE_CREDIT_AGREEMENT] = setFormField('creditAgreement');
    reducerObj[TOGGLE_CONTACT_AGREEMENT] = setFormField('contactAgreement');
    reducerObj[SET_CAPTCHA_CODE] = setFormField('captchaCode');
    reducerObj[SET_PHONE] = setFormField('phone');
    reducerObj[SET_CELL_PHONE] = setFormField('cellPhone');

    var reducer = makeReducer(reducerObj, iniState);

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

        if (!state.creditAgreement) {
            errors.push({
                type: 'agreement',
                messageKey: 'EmptyCreditAgreement'
            });
        }

        if (!state.contactAgreement) {
            errors.push({
                type: 'agreement',
                messageKey: 'EmptyContactAgreement'
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

    var displayErrorsMiddleware = function () {
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

    var readValue = function (element) {
        if (element.attr('type') === 'checkbox') {
            return element.prop('checked') || false;
        }

        return element.val() || '';
    };

    var readInitialStateFromFields = function (map) {
        return Object.keys(map).reduce(function (acc, key) {
            acc[key] = readValue(map[key]);
            return acc;
        }, {});
    };

    window.onLoadCaptcha = function () {
        grecaptcha.render('gcaptcha', {
            sitekey: '6LeqxBgUAAAAAJnAV6vqxzZ5lWOS5kzs3lfxFKEQ',
            callback: function (response) {
                dispatch(createAction(SET_CAPTCHA_CODE, response));
            },
            'expired-callback': function() {
                dispatch(createAction(SET_CAPTCHA_CODE, ''));
            },
        });
    };

    var extend = function(defaults) {
        return function(overrides) {
            return $.extend({}, defaults, overrides);
        };
    };

    var addressForm = {
        street_number: 'short_name',
        route: 'long_name',
        locality: 'long_name',
        administrative_area_level_1: 'short_name',
        country: 'long_name',
        postal_code: 'short_name'
    };

    var getAddress = function(addressForm) {
        return function(addressComponent) {
            var addressType = addressComponent.types[0];
            if (addressForm.hasOwnProperty(addressType)) {
                var addressObj = {};
                addressObj[addressType] = addressComponent[addressForm[addressType]];
                return addressObj;
            }
        };
    };

    var concatObj = function(acc, next) {
        return next ? $.extend(acc, next) : acc;
    };

    var setAutocomplete = function(streetElmId, cityElmId) {
        var extendCommonOpts = extend({
            componentRestrictions: { country: 'ca' },
        });

        var streetElm = document.getElementById(streetElmId);
        var streetAutocomplete = new google.maps.places
            .Autocomplete(streetElm, extendCommonOpts({ types: ['geocode'] }));

        var cityElm = document.getElementById(cityElmId);
        var cityAutocomplete = new google.maps.places
            .Autocomplete(cityElm, extendCommonOpts({ types: ['(cities)'] }));

        return {
            street: streetAutocomplete,
            city: cityAutocomplete,
        };
    };

    window.initAutocomplete = function() {
        configInitialized.then(function() {
            var gAutoCompletes = setAutocomplete('street', 'city');
            var gPAutoCompletes = setAutocomplete('pstreet', 'pcity');

            gAutoCompletes.street.addListener('place_changed',
                function() {
                    var place = gAutoCompletes.street.getPlace().address_components
                        .map(getAddress(addressForm)).reduce(concatObj);

                    dispatch(createAction(SET_ADDRESS,
                    {
                        street: place['route'] || '',
                        number: place['street_number'] || '',
                        city: place['locality'] || '',
                        province: place['administrative_area_level_1'] || '',
                        postalCode: place['postal_code'] || '',
                    }));
                });

            gAutoCompletes.city.addListener('place_changed',
                function() {
                    var place = gAutoCompletes.city.getPlace().address_components
                        .map(getAddress(addressForm)).reduce(concatObj);

                    dispatch(createAction(SET_ADDRESS,
                    {
                        city: place['locality'] || '',
                        province: place['administrative_area_level_1'] || '',
                    }));
                });

            gPAutoCompletes.street.addListener('place_changed',
                function() {
                    var place = gPAutoCompletes.street.getPlace().address_components
                        .map(getAddress(addressForm)).reduce(concatObj);

                    dispatch(createAction(SET_PADDRESS,
                    {
                        street: place['route'] || '',
                        number: place['street_number'] || '',
                        city: place['locality'] || '',
                        province: place['administrative_area_level_1'] || '',
                        postalCode: place['postal_code'] || '',
                    }));
                });

            gPAutoCompletes.city.addListener('place_changed',
                function() {
                    var place = gPAutoCompletes.city.getPlace().address_components
                        .map(getAddress(addressForm)).reduce(concatObj);

                    dispatch(createAction(SET_PADDRESS,
                    {
                        city: place['locality'] || '',
                        province: place['administrative_area_level_1'] || '',
                    }));
                });
        });
    };

    configInitialized
        .then(function () {
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

            var street = $('#street');
            street.on('change', function (e) {
                dispatch(createAction(SET_STREET, e.target.value));
            });

            var unit = $('#unit');
            unit.on('change', function (e) {
                dispatch(createAction(SET_UNIT, e.target.value));
            });

            var city = $('#city');
            city.on('change', function (e) {
                dispatch(createAction(SET_CITY, e.target.value));
            });

            var province = $('#province');
            province.on('change', function (e) {
                dispatch(createAction(SET_PROVINCE, e.target.value));
            });

            var postalCode = $('#postalCode');
            postalCode.on('change', function (e) {
                dispatch(createAction(SET_POSTAL_CODE, e.target.value));
            });

            $('#clearAddress').on('click', function (e) {
                e.preventDefault();
                dispatch(createAction(CLEAR_ADDRESS, e.target.value));
            });

            var homeOwner = $('#homeowner-checkbox');
            homeOwner.on('click', function (e) {
                dispatch(createAction(TOGGLE_OWNERSHIP, homeOwner.prop('checked') ));
            });

            var pstreet = $('#pstreet');
            pstreet.on('change', function (e) {
                dispatch(createAction(SET_PSTREET, e.target.value));
            });

            var punit = $('#punit');
            punit.on('change', function (e) {
                dispatch(createAction(SET_PUNIT, e.target.value));
            });

            var pcity = $('#pcity');
            pcity.on('change', function (e) {
                dispatch(createAction(SET_PCITY, e.target.value));
            });

            var pprovince = $('#pprovince');
            pprovince.on('change', function (e) {
                dispatch(createAction(SET_PPROVINCE, e.target.value));
            });

            var ppostalCode = $('#ppostalCode');
            ppostalCode.on('change', function (e) {
                dispatch(createAction(SET_PPOSTAL_CODE, e.target.value));
            });

            $('#pclearAddress').on('click', function (e) {
                e.preventDefault();
                dispatch(createAction(CLEAR_PADDRESS, e.target.value));
            });

            var creditAgreement = $('#agreement1');
            creditAgreement.on('click', function (e) {
                dispatch(createAction(TOGGLE_CREDIT_AGREEMENT, creditAgreement.prop('checked')));
            });

            var contactAgreement = $('#agreement2');
            contactAgreement.on('click', function (e) {
                dispatch(createAction(TOGGLE_CONTACT_AGREEMENT, contactAgreement.prop('checked')));
            });

            var lessThanSix = $('#living-time-checkbox');
            lessThanSix.on('click', function (e) {
                dispatch(createAction(SET_LESS_THAN_SIX, lessThanSix.prop('checked')));
            });

            var phone = $('#homePhone');
            phone.on('change', function (e) {
                dispatch(createAction(SET_PHONE, e.target.value));
            });

            var cellPhone = $('#cellPhone');
            cellPhone.on('change', function (e) {
                dispatch(createAction(SET_CELL_PHONE, e.target.value));
            });

            var form = $('#mainForm');
            $('#submit').on('click', function (e) {
                dispatch(createAction(SUBMIT));
                var errors = getErrors(customerFormStore.getState());
                if (errors.length > 0 && form.valid()) {
                    e.preventDefault();
                }
            });

            var initialStateMap = {
                street: street,
                unit: unit,
                city: city,
                province: province,
                postalCode: postalCode,
                pstreet: pstreet,
                punit: punit,
                pcity: pcity,
                pprovince: pprovince,
                ppostalCode: ppostalCode,
                homeOwner: homeOwner,
                creditAgreement: creditAgreement,
                contactAgreement: contactAgreement,
                lessThanSix: lessThanSix,
                cellPhone: cellPhone,
                phone: phone,
            };

            dispatch(createAction(SET_INITIAL_STATE, readInitialStateFromFields(initialStateMap)));

             // observers
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

            observeCustomerFormStore(function (state) {
                return {
                    lessThanSix: state.lessThanSix,
                };
            })(function (props) {
                $('#previous-address').find('input, select').each(function () {
                    $(this).prop("disabled", !props.lessThanSix);
                });
            });

            observeCustomerFormStore(function (state) {
                return {
                    phoneRequired: state.cellPhone === '' || (state.cellPhone !== '' && state.phone !== ''),
                    cellPhoneRequired: state.phone === '',
                };
            })(function (props) {
                $('#homePhone').rules(props.phoneRequired ? 'add' : 'remove', 'required');
                $('#cellPhone').rules(props.cellPhoneRequired ? 'add' : 'remove', 'required');

                if (props.phoneRequired) {
                    $('#homePhoneWrapper').addClass('mandatory-field');
                } else {
                    $('#homePhoneWrapper').removeClass('mandatory-field');
                }

                if (props.cellPhoneRequired) {
                    $('#cellPhoneWrapper').addClass('mandatory-field');
                } else {
                    $('#cellPhoneWrapper').removeClass('mandatory-field');
                }
            });
        });
});


