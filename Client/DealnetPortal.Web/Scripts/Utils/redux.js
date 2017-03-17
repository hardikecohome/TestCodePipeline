module.exports('redux', function (require) {
    var shallowDiff = require('objectUtils').shallowDiff;

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
                    listeners.forEach(function (fn) { fn(); });
                }
                return action;
            },
        };
    };

    var applyMiddleware = function (middlewares) {
        return function (store) {
            middlewares = middlewares.slice();
            middlewares.reverse();

            let dispatch = store.dispatch;
            middlewares.forEach(function (middleware) {
                dispatch = middleware(store)(dispatch);
            });

            return $.extend({}, store, { dispatch: dispatch });
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
            } else {
                return state;
            }
        };
    };

    var createAction = function (type, payload) {
        return {
            type: type,
            payload: payload,
        };
    };

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

                listener(oldState);
                store.subscribe(diffListener);
            };
        };
    };

    return {
        makeStore: makeStore,
        applyMiddleware: applyMiddleware,
        makeReducer: makeReducer,
        createAction: createAction,
        observe: observe,
    };
});