module.exports('employment-info-view', function (require) {
    var clientActions = require('new-client-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;

    return function (store) {
        var dispatch = store.dispatch;

        var status = $('#emp-status');

        var incomeType = $('#income-type');

        var annual = $('#annual');

        var hourly = $('#hourly');

        var years = $('#emp-years');

        var months = $('#emp-months');

        var empType = $('#emp-type');

        var jobTitle = $('#job-title');

        var companyName = $('#company-name');

        var companyPhone = $('#company-phone');

        var comapanyStreet = $('#company-street');

        var companyUnit = $('#company-unit');

        var companyCity = $('#company-city');

        var companyProvince = $('#company-province');

        var companyPostal = $('#company-postal-code')
    }
});
