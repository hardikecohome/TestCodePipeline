module.exports('my-deals-index', function (require) {
    var Table = require('my-deals-table');
    var showLoader = require('loader').showLoader;
    var hideLoader = require('loader').hideLoader;

    var MyDealsTableViewModel = function (data) {
        this.table = ko.observable(new Table(data));

        this.updateTableList = function (list) {
            this.table().setList(list);
        };
    };

    var init = function init() {
        var vm = new MyDealsTableViewModel([]);
        ko.applyBindings(vm, document.getElementById('my-deals-page'));
        showLoader()
        $.ajax(itemsUrl, {
            cache: false,
            mode: 'GET'
        }).done(function (data) {
            vm.updateTableList(data);
        }).always(function () {
            hideLoader();
        });
    };

    return {
        init: init
    };
});