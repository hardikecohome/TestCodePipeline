module.exports('home-page-index', function (require) {

    var Table = require('table');
    var showLoader = require('loader').showLoader;
    var hideLoader = require('loader').hideLoader;

    var HomeViewModel = function (data) {
        this.table = ko.observable(new Table(data));
        this.loaded = ko.observable(false);

        this.updateTableList = function (list) {
            this.table().setList(list);
        };
    };

    var init = function init() {
        var vm = new HomeViewModel([]);
        ko.applyBindings(vm, document.getElementById('home-body'));
        showLoader();

        $.ajax(itemsUrl, {
            cache: false,
            mode: 'GET'
        }).done(function (data) {
            vm.updateTableList(data);
        }).always(function () {
            vm.loaded(true);
            hideLoader();
        });
    };

    return {
        init: init
    };
});