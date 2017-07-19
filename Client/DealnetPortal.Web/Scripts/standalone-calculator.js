module.exports('standalone-calculator', function (require) {
    var makeReducer = require('redux').makeReducer;
    var applyMiddleware = require('redux').applyMiddleware;
    var makeStore = require('redux').makeStore;
    var createAction = require('redux').createAction;
    var observe = require('redux').observe;

    var compose = require('functionUtils').compose;

    var log = require('logMiddleware');

    // your info actions
    var SET_INITIAL_STATE = 'set_initial_state';
    var SET_TYPE = 'set_type';
    var SET_DESCRIPTION = 'set_description';
    var SET_COST= 'set_description';
    var DUPLICATE_OPTION = 'duplicate_option';

    var iniState = {
        options: {
            '0': {
                id: '0',
                equipmentIds: ['0'],
            },
        },
        equipments: {
            '0': {
                id: '0',
                type: '',
                description: '',
                cost: '',
            },
        },
    };

    var isId = function(refId) {
        return function(id) {
            return refId === id;
        };
    };

    var copyAndShiftEquipments = function(base) {
        return function(equipment, index) {
            return $.extend({},
                equipment,
                {
                    id: (index + base).toString(),
                });
        };
    };

    var getId = function(obj) {
        return obj ? obj.id : undefined;
    };

    var concatObj = function(acc, obj) {
        acc[obj.id] = obj;
        return acc;
    };

    var setEquipmentInfo = function (field) {
        return function(state, action) {
            var optionId = action.payload.optionId;
            var equipmentId = action.payload.equipmentId;

            if (!state.options.hasOwnProperty(optionId)) {
                return state;
            }

            var equipments = state.options.equipmentIds;

            if (!equipments.hasOwnProperty(id) || !equipments.some(isId(equipmentId))) {
                return state;
            }

            return {
                equipments: $.extend({},
                    state.equipments,
                    {
                        [equipmentId]: $.extend({},
                            equipments[equipmentId],
                            {
                                [field]: action.payload.value,
                            }),
                    }),
            };
        };
    };

    var idToValue = function(obj) {
        return function(id) {
            return obj.hasOwnProperty(id) ? obj[id] : undefined;
        };
    };


    // your info reducer
    var reducer = makeReducer({
        [SET_INITIAL_STATE]: function (state, action) {
            return $.extend({}, state, action.payload);
        },
        [SET_TYPE]: setEquipmentInfo('type'),
        [SET_DESCRIPTION]: setEquipmentInfo('description'),
        [SET_COST]: setEquipmentInfo('cost'),
        [DUPLICATE_OPTION]: function (state) {
            var options = Object.keys(state.options).map(function(id) {
                return state.options[id];
            });
            var optionsLen = options.length;
            var equipmentLen = Object.keys(state.equipments).length;
            var optionId = options[optionsLen - 1].id;

            if (!state.options.hasOwnProperty(optionId) || optionsLen >= 3) {
                return state;
            }

            var newOptionId = optionsLen;
            var newEquipments = state.options[optionId].equipmentIds
                .map(idToValue(state.equipments))
                .map(copyAndShiftEquipments(equipmentLen));

            return {
                options: $.extend({},
                    state.options,
                    {
                        [newOptionId]: {
                            id: newOptionId,
                            equipmentIds: newEquipments.map(getId),
                        }
                    }),
                equipments: newEquipments.reduce(concatObj, $.extend({}, state.equipments)),
            };
        },
    }, iniState);

    // selectors

    var calculatorStore = compose(applyMiddleware([
        log('store/calculator')
    ]), makeStore)(reducer);

    var dispatch = calculatorStore.dispatch;

    // view layer
    var observeCalculatorStore = observe(calculatorStore);

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

    $(document)
        .ready(function () {
            $('#addOption').on('click', function () {
                dispatch(createAction(DUPLICATE_OPTION));
            });

            observeCalculatorStore(function(state) {
                return {
                    length: Object.keys(state.options).length,
                };
            })(function(props) {
                if (props.length > 1) {
                    $('#cardContainer1').removeClass('hidden');
                } else {
                    $('#cardContainer1').addClass('hidden');
                }

                if (props.length > 2) {
                    $('#cardContainer2').removeClass('hidden');
                } else {
                    $('#cardContainer2').addClass('hidden');
                }
            });
        });
});


