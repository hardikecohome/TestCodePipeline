(function () {
    const __DEBUG__ = true;

    var compose = function () {
        var partials = Array.prototype.slice.call(arguments);
        var last = partials.length - 1;
        return function () {
            return partials.reduceRight(function (acc, partial, index) {
                if (index === last) return partial.apply(null, acc);
                return partial(acc);
            }, arguments);
        }
    };

    var mapState = function (fn) {
        return function () {
            var state = arguments[0];
            return $.extend({}, state, fn.apply(null, arguments));
        };
    };

    var shallowDiff = function (oldState, newState) {
        return Object.keys(oldState).some(function (key) {
            return oldState[key] !== newState[key];
        });
    };

    var makeStore = function (reducer) {
        var state = reducer();
        var listeners = [];

        return {
            getState: function () {
                return state;
            },
            subscribe: function (fn) {
                listeners.push(fn);
            },
            unsubscribe: function (fn) {
                var index = listeners.indexOf(fn);
                listeners.splice(index);
            },
            dispatch: function (action) {
                var newState = reducer(state, action);
                if (shallowDiff(state, newState)) {
                    state = newState;
                    if (__DEBUG__) console.log(newState);
                    listeners.forEach(function (fn) { fn(); });
                }
            },
        };
    };

    var makeReducer = function (reducers, initialState) {
        return function (state, action) {
            if (!state && !action) {
                return initialState;
            }

            if (Object.keys(reducers).some(function (key) {
                return key === action.type;
            })) {
                var newState = reducers[action.type](state, action);

                if (newState) {
                    return $.extend({}, state, newState);
                }
            }
        };
    };


    var createAction = function (type, payload) {
        return {
            type: type,
            payload: payload,
        };
    };

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

    var iniState = {
        name: '',
        last: '',
        birth: '',
        sin: '',
        street: '',
        unit: '',
        city: '',
        province: '',
        postalCode: '',
        displayYourInfo: true,
        doneYourInfo: false,
        displayInstallation: false,
        doneInstallation: false,
        displayContactInfo: false,
        doneContactInfo: false,
    };

    // rules
    var displayYourInfoRule = function (state) {
        return !(state.name && state.last && state.birth && state.sin) ? true : false;
    };

    var doneYourInfoRule = function (state) {
        return state.doneYourInfo || state.name && state.last && state.birth ? true : false;
    };

    var displayInstallationRule = doneYourInfoRule;

    var setFormField = function (field) {
        return function (state, action) {
            return $.extend({}, state, {
                [field]: action.payload,
            });
        };
    };

    var hideAndShowPanels = function (state) {
        return $.extend({}, state, {
            displayYourInfo: displayYourInfoRule(state),
            doneYourInfo: doneYourInfoRule(state),
            displayInstallation: displayInstallationRule(state),
        });
    };

    // your info reducer
    var reducer = makeReducer({
        [SET_NAME]: compose(hideAndShowPanels, setFormField('name')),
        [SET_LAST]: compose(hideAndShowPanels, setFormField('last')),
        [SET_BIRTH]: compose(hideAndShowPanels, setFormField('birth')),
        [SET_SIN]: compose(hideAndShowPanels, setFormField('sin')),
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
    }, iniState);

    var customerFormStore = makeStore(reducer);
    var dispatch = customerFormStore.dispatch;

    // view layer


    var observe = function (store) {
        return function (map) {
            return function (listener) {
                var oldState = map(store.getState());

                var diffListener = function () {
                    var newState = map(store.getState());
                    if (shallowDiff(oldState, newState)) {
                        listener(newState, oldState);
                        oldState = newState;
                    }
                };

                store.subscribe(diffListener);
            };
        };
    }


    $(document)
        .ready(function () {
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

            var hideYourInfoFirstTime = true;
            var hideIntallationFirstTime = true;

            observe(customerFormStore)(function (state) {
                return {
                    displayYourInfo: state.displayYourInfo,
                    doneYourInfo: state.doneYourInfo,
                    displayInstallation: state.displayInstallation,
                };
            })(function (props) {
                if (props.displayYourInfo) {
                    $('#yourInfoForm').slideDown();
                } else if (hideYourInfoFirstTime){
                    $('#yourInfoForm').slideUp();
                    hideYourInfoFirstTime = false;
                }

                if (props.displayInstallation) {
                    $('#installationAddressForm').slideDown();
                    $('#yourInfoPanel').removeClass('active-panel');
                    $('#installationAddressPanel').addClass('active-panel');
                } else if (hideInstallationFirstTime) {
                    $('#installationAddressPanel').slideUp();
                    hideInstallationFirstTime = false;
                }
            });

            observe(customerFormStore)(function (state) {
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
        });
})();


