module.exports('my-profile-store', function (require) {
    var applyMiddleware = require('redux').applyMiddleware;
    var reducer = require('my-profile-reducer');
    var createAction = require('redux').createAction;
    var profileActions = require('my-profile-actions');
    var makeStore = require('redux').makeStore;
    var compose = require('functionUtils').compose;

    var displayErrorsMiddleware = function () {
        return function (next) {
            return function (action) {
                var nextAction = next(action);
                if (action.type === profileActions.SUBMIT) {
                    next(createAction(profileActions.DISPLAY_SUBMIT_ERRORS, true));
                }
                return nextAction;
            }
        }
    };
    return compose(applyMiddleware([
        displayErrorsMiddleware
        //log('store/newCustomer')
    ]), makeStore)(reducer);
});