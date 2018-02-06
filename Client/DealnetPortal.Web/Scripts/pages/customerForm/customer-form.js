module.exports('customer-form', function (require) {
    var applyMiddleware = require('redux').applyMiddleware;
    var makeStore = require('redux').makeStore;
    var createAction = require('redux').createAction;
    var observe = require('redux').observe;

    var concatObj = require('objectUtils').concatObj;
    var extendObj = require('objectUtils').extendObj;
    var compose = require('functionUtils').compose;

    var reducer = require('customer-reducer');
    var customerActions = require('customer-actions');
    var configGetErrors = require('customer-selectors').getErrors;

    var initYourInfo = require('your-info-view');
    var initContactInfo = require('contact-info-view');
    var initInstallationAddress = require('installation-address-view');
    var initAgreement = require('agreement-view');
    var initEmployment = require('employment-info-view');

    var log = require('logMiddleware');

    var dynamicAlertModal = require('alertModal').dynamicAlertModal;
    var panelCollapsed = require('panelCollapsed');

    var requiredFields = ['name', 'lastName', 'birthday', 'street', 'province', 'postalCode', 'email', 'creditAgreement', 'contactAgreement', 'ownership', 'captchaCode'];
    var requiredPFields = ['birthday', 'pstreet', 'pprovince', 'ppostalCode'];
    var requiredEmployment = [];

    var getErrors = configGetErrors(requiredFields, requiredPFields, requiredEmployment);

    var flowMiddleware = function (store) {
        return function (next) {
            var flow1 = [customerActions.SET_NAME, customerActions.SET_LAST, customerActions.SET_BIRTH];
            var flow2 = [customerActions.SET_STREET, customerActions.SET_CITY, customerActions.SET_PROVINCE, customerActions.SET_POSTAL_CODE, customerActions.TOGGLE_OWNERSHIP];
            var addressFlow = [customerActions.SET_STREET, customerActions.SET_CITY, customerActions.SET_PROVINCE, customerActions.SET_POSTAL_CODE];

            return function (action) {
                var state = store.getState();

                var nextAction = next(action);
                if (state.activePanel === 'yourInfo') {
                    var index1 = flow1.indexOf(action.type);
                    if (index1 >= 0) {
                        flow1.splice(index1, 1);
                    }
                    if (flow1.length === 0) {
                        next(createAction(customerActions.ACTIVATE_INSTALLATION, true));
                    }
                }

                if (state.activePanel === 'installation') {
                    var index2 = flow2.indexOf(action.type);
                    if (index2 >= 0) {
                        flow2.splice(index2, 1);
                    }
                    if (action.type === customerActions.SET_ADDRESS && action.payload.street !== '') {
                        addressFlow.forEach(function (action) {
                            var index3 = flow2.indexOf(action);
                            if (index3 >= 0) {
                                flow2.splice(index3, 1);
                            }
                        });
                    }

                    if (flow2.length === 0) {
                        next(createAction(customerActions.ACTIVATE_CONTACT_INFO, true));
                    }
                }
                
                var stateAfter = store.getState();
                var errors = getErrors(stateAfter);
                var installationErrors = errors.filter(function (error) { return error.type === 'ownership'; });
                if (stateAfter.displaySubmitErrors && installationErrors.length > 0) {
                    next(createAction(customerActions.DISPLAY_INSTALLATION, true));
                }

                var contactInfoErrors = errors.filter(function (error) {
                    return ['captcha', 'agreement'].some(function (item) {
                        return error.type === item;
                    });
                });

                if (stateAfter.displaySubmitErrors && contactInfoErrors.length > 0) {
                    next(createAction(customerActions.DISPLAY_CONTACT_INFO, true));
                }

                return nextAction;
            };
        };
    };

    var displayErrorsMiddleware = function () {
        return function (next) {
            return function (action) {
                var nextAction = next(action);
                if (action.type === customerActions.SUBMIT) {
                    next(createAction(customerActions.DISPLAY_SUBMIT_ERRORS, true));
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

    if (config.reCaptchaEnabled) {
        window.onLoadCaptcha = function () {
            grecaptcha.render('gcaptcha', {
                sitekey: window.config.reCaptchaKey,
                callback: function (response) {
                    dispatch(createAction(customerActions.SET_CAPTCHA_CODE, response));
                },
                'expired-callback': function () {
                    dispatch(createAction(customerActions.SET_CAPTCHA_CODE, ''));
                }
            });
        };
    }

    var addressForm = {
        street_number: 'short_name',
        route: 'long_name',
        locality: 'long_name',
        administrative_area_level_1: 'short_name',
        country: 'long_name',
        postal_code: 'short_name',
        postal_code_prefix: 'short_name'
    };

    var getAddress = function (addressForm) {
        return function (addressComponent) {
            var addressType = addressComponent.types[0];
            if (addressForm.hasOwnProperty(addressType)) {
                var addressObj = {};
                addressObj[addressType] = addressComponent[addressForm[addressType]];
                return addressObj;
            }
            return null;
        };
    };

    var setAutocomplete = function (streetElmId, cityElmId) {
        var extendCommonOpts = extendObj({
            componentRestrictions: { country: 'ca' }
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

    window.initAutocomplete = function () {
        $(document).ready(function () {
            var gAutoCompletes = setAutocomplete('street', 'city');
            var gPAutoCompletes = setAutocomplete('pstreet', 'pcity');
            var gCAutoCompletes = setAutocomplete('company-street', 'company-city');

            gAutoCompletes.street.addListener('place_changed',
                function () {
                    var place = gAutoCompletes.street.getPlace().address_components
                        .map(getAddress(addressForm)).reduce(concatObj);

                    dispatch(createAction(customerActions.SET_ADDRESS,
                        {
                            street: place['route'] || '',
                            number: place['street_number'] || '',
                            city: place['locality'] || '',
                            province: place['administrative_area_level_1'] || '',
                            postalCode: place['postal_code'] ? place['postal_code'].replace(' ', '') : place['postal_code_prefix'] ? place['postal_code_prefix'].replace(' ', '') : '',
                        }));

                    $('#province').change();
                });

            gAutoCompletes.city.addListener('place_changed',
                function () {
                    var place = gAutoCompletes.city.getPlace().address_components
                        .map(getAddress(addressForm)).reduce(concatObj);

                    dispatch(createAction(customerActions.SET_ADDRESS,
                        {
                            city: place['locality'] || '',
                            province: place['administrative_area_level_1'] || '',
                        }));
                });

            gPAutoCompletes.street.addListener('place_changed',
                function () {
                    var place = gPAutoCompletes.street.getPlace().address_components
                        .map(getAddress(addressForm)).reduce(concatObj);

                    dispatch(createAction(customerActions.SET_PADDRESS,
                        {
                            street: place['route'] || '',
                            number: place['street_number'] || '',
                            city: place['locality'] || '',
                            province: place['administrative_area_level_1'] || '',
                            postalCode: place['postal_code'] ? place['postal_code'].replace(' ', '') : place['postal_code_prefix'] ? place['postal_code_prefix'].replace(' ', '') : '',
                        }));

                    $('#pprovince').change();
                });

            gPAutoCompletes.city.addListener('place_changed',
                function () {
                    var place = gPAutoCompletes.city.getPlace().address_components
                        .map(getAddress(addressForm)).reduce(concatObj);

                    dispatch(createAction(customerActions.SET_PADDRESS,
                        {
                            city: place['locality'] || '',
                            province: place['administrative_area_level_1'] || '',
                        }));
                });

            gCAutoCompletes.street.addListener('place_changed',
                function () {
                    var place = gCAutoCompletes.street.getPlace().address_components
                        .map(getAddress(addressForm)).reduce(concatObj);

                    dispatch(createAction(customerActions.SET_CADDRESS,
                        {
                            street: place['route'] || '',
                            number: place['street_number'] || '',
                            city: place['locality'] || '',
                            province: place['administrative_area_level_1'] || '',
                            postalCode: place['postal_code'] ? place['postal_code'].replace(' ', '') : place['postal_code_prefix'] ? place['postal_code_prefix'].replace(' ', '') : '',
                        }));

                    $('#company-province').change();
                });

            gCAutoCompletes.city.addListener('place_changed',
                function () {
                    var place = gCAutoCompletes.city.getPlace().address_components
                        .map(getAddress(addressForm)).reduce(concatObj);

                    dispatch(createAction(customerActions.SET_CADDRESS,
                        {
                            city: place['locality'] || '',
                            province: place['administrative_area_level_1'] || '',
                        }));
                });
        });
    };

    $(window).keydown(function (event) {
        if (event.keyCode === 13 && event.target.nodeName !== 'TEXTAREA') {
            event.preventDefault();
            return false;
        }
    });

    $(document)
        .ready(function () {
            $('<option selected value="">- ' + translations['NotSelected'] + ' -</option>').prependTo($('#selectedService'));
            $('#selectedService').val($('#selectedService > option:first').val());

            // init views
            initYourInfo(customerFormStore);
            initInstallationAddress(customerFormStore);
            initContactInfo(customerFormStore);
            initAgreement(customerFormStore);
            initEmployment(customerFormStore);

            $('.j-personal-data-used-modal').on('click', function (e) {
                var data = {
                    message: $('#personal-data-used').html(),
                    "class": "consents-modal",
                    cancelBtnText: "OK"
                };
                dynamicAlertModal(data);
                e.preventDefault();
            });

            $('.customer-loan-form-panel .panel-heading').on('click', function () {
                panelCollapsed($(this));
            });

            var form = $('#mainForm');
            form.submit(function (e) {
                $('#submit').prop('disabled', true);

                if (!form.valid()) {
                    e.preventDefault();
                    $('#submit').prop('disabled', false);
                }

                dispatch(createAction(customerActions.SUBMIT));
                var errors = getErrors(customerFormStore.getState());

                if (errors.length > 0 && form.valid()) {
                    e.preventDefault();
                    $('#submit').prop('disabled', false);
                }
            });

            // observers
            observeCustomerFormStore(function (state) {
                return {
                    displayInstallation: state.displayInstallation,
                    displayContactInfo: state.displayContactInfo,
                    activePanel: state.activePanel,
                    isQuebecDealer: state.isQuebecDealer
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
                    $('#additionalInfoForm').slideDown();
                }

                if (props.activePanel === 'contactInfo') {
                    $('#addtionalInfoPanel').addClass('active-panel');
                } else {
                    $('#addtionalInfoPanel').removeClass('active-panel');
                }
                });

            observeCustomerFormStore(function(state) {
                return {
                    province: state.province,
                    employStatus: state.employStatus,
                    incomeType: state.incomeType,
                    isQuebecDealer: state.isQuebecDealer
                }
            })(function (props) {
                requiredEmployment = [];
                if (props.province.toLowerCase() === 'qc' && props.isQuebecDealer) {
                    if (props.employStatus === '') {
                        requiredEmployment.push('employStatus');
                    }
                    if (props.employStatus === '0') {
                        requiredEmployment = requiredEmployment.concat(['employStatus',
                            'incomeType',
                            'yearsOfEmploy',
                            'employType',
                            'jobTitle',
                            'companyName',
                            'companyPhone',
                            'cstreet',
                            'cunit',
                            'ccity',
                            'cprovince',
                            'cpostalCode']);
                        if (props.incomeType === '0') {
                            requiredEmployment.push('annualSalary');
                        }
                        if (props.incomeType === '1') {
                            requiredEmployment.push('hourlyRate');
                        }
                        if (props.yearsOfEmploy !== '10+') {
                            requiredEmployment.push('monthsOfEmploy');
                        }
                    }
                    if (props.employStatus === '1' || props.employStatus === '3') {
                        requiredEmployment = requiredEmployment.concat(['employStatus', 'annualSalary']);
                    }
                    if (props.employStatus === '2') {
                        requiredEmployment = requiredEmployment.concat(['employStatus',
                            'annualSalary',
                            'yearsOfEmploy',
                            'employType',
                            'jobTitle',
                            'companyName',
                            'companyPhone',
                            'cstreet',
                            'cunit',
                            'ccity' ,
                            'cprovince',
                            'cpostalCode']);
                        if (props.yearsOfEmploy !== '10+') {
                            requiredEmployment.push('monthsOfEmploy');
                        }
                    }
                } 
            });

            var createError = function (msg) {
                var err = $('<div class="well danger-well over-aged-well" id="age-error-message"><svg aria-hidden="true" class="icon icon-info-well"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-info-well"></use></svg></div>');
                err.append(msg);
                return err;
            };

            observeCustomerFormStore(function (state) {
                return {
                    errors: getErrors(state),
                    displaySubmitErrors: state.displaySubmitErrors,
                    state: state
                }
            })(function (props) {
                $('#yourInfoErrors').empty();
                $('#qcError').text('');
                $('#province').removeClass('input-custom-err');

                if (props.errors.length > 0) {
                    props.errors
                        .filter(function(error) { return error.type === 'birthday' })
                        .forEach(function(error) {
                            $('#yourInfoErrors').append(createError(window.translations[error.messageKey]));
                        });

                    props.errors
                        .filter(function (error) { return error.type === 'quebec'; })
                        .forEach(function(error) {
                            $('#qcError').text(window.translations[error.messageKey]);
                            $('#province').addClass('input-custom-err');
                        });
                }

                var emptyError = props.errors.filter(function (error) {
                    return error.type === 'empty';
                });

                if (emptyError.length || props.errors.length > 0) {
                    $('#submit').addClass('disabled');
                    $('#submit').parent().popover();
                } else {
                    if ($('#mainForm').valid()) {
                        $('#submit').removeClass('disabled');
                        $('#submit').parent().popover('destroy');
                    } else {
                        if (!$('#submit').hasClass('disabled')) {
                            $('#submit').addClass('disabled');
                            $('#submit').parent().popover();
                        }
                    }
                }
            });
        });
});
