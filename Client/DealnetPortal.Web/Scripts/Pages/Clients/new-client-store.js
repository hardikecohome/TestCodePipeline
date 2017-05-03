module.exports('new-client-store', function(require) {
    var applyMiddleware = require('redux').applyMiddleware;
    var flowMiddleware = require('new-client-flow');
    var reducer = require('new-client-reducer');

    var makeStore = require('redux').makeStore;
    var compose = require('functionUtils').compose;

    return compose(applyMiddleware([
        //displayErrorsMiddleware,
        flowMiddleware
        //log('store/newCustomer')
    ]), makeStore)(reducer);
});