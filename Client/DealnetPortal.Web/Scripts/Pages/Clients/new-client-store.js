module.exports('new-client-store', function(require) {
    var applyMiddleware = require('redux').applyMiddleware;
    var flowMiddleware = require('new-client-flow');
    var reducer = require('new-client-reducer');

    var createAction = require('redux').createAction;
    var clientActions = require('new-client-actions');

    var makeStore = require('redux').makeStore;
    var compose = require('functionUtils').compose;

    var displayErrorsMiddleware = function () {
        return function (next) {
            return function (action) {
                var nextAction = next(action);
                if (action.type === clientActions.SUBMIT) {
                    next(createAction(clientActions.DISPLAY_SUBMIT_ERRORS, true));
                }

                return nextAction;
            }
        }
    };

    return compose(applyMiddleware([
        displayErrorsMiddleware,
        flowMiddleware
        //log('store/newCustomer')
    ]), makeStore)(reducer);
});