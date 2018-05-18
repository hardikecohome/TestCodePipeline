module.exports('my-deals-index', function (require) {
    var Table = require('my-deals-table');

    var MyDealsTableViewModel = function (data) {
        this.table = ko.observable(new Table(data));

        this.updateTableData = function (data) {
            this.table().setData(data);
        };
    };

    var init = function init() {
        var vm = new MyDealsTableViewModel([]);
        ko.applyBindings(vm, document.getElementById('my-deals-page'));

        $.ajax(itemsUrl, {
            cache: false,
            mode: 'GET'
        }).done(function (data) {
            vm.updateTableData(data);
        });
    };

    return {
        init: init
    };
});