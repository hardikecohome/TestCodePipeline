module.exports('employment-info-view', function (require) {
    var clientActions = require('new-client-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;

    return function (store) {
        var dispatch = store.dispatch;

        var status = $('#employment-status');

        var incomeType = $('#income-type');

        var annual = $('#annual');

        var hourly = $('#hourly');

        var years = $('#emp-years');

        var months = $('#emp-months');

        var
    }
});
