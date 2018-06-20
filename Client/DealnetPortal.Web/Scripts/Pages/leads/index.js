module.exports('leads-page-index', function (require) {

    var Table = require('leads-table');
    var showLoader = require('loader').showLoader;
    var hideLoader = require('loader').hideLoader;

    var LeadsViewModel = function (data) {
        this.table = ko.observable(new Table(data));
        this.loaded = ko.observable(false);

        this.updateTableList = function (list) {
            this.table().setList(list);
        };
    };

    var init = function init() {
        var vm = new LeadsViewModel([]);
        ko.applyBindings(vm, document.getElementById('leads-body'));
        showLoader();

        $.ajax(itemsUrl, {
            cache: false,
            mode: 'GET'
        }).done(function (data) {
            vm.updateTableList(Array.isArray(data) ? data : []);
        }).always(function () {
            vm.loaded(true);
            hideLoader();
        });
    };

    return {
        init: init
    };
});