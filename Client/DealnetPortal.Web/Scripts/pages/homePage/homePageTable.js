module.exports('table', function (require) {

    var Paginator = require('paginator');

    function filterNull(item) {
        return item != null;
    }

    function mapValue(type) {
        return function (item) {
            return item[type] || null;
        }
    }

    function concatIfNotInArray(acc, curr) {
        return acc.indexOf(curr) === -1 ? acc.concat(curr) : acc;
    }

    function sortAssending(a, b) {
        return a == b ? 0 : a > b ? 1 : -1;
    }

    function filterAndSortList(list, type) {
        return [' '].concat(list.map(mapValue(type))
            .filter(filterNull)
            .reduce(concatIfNotInArray, [])
            .sort(sortAssending));
    }

    var HomePageTable = function (list) {
        // properties
        this.agreementOptions = ko.observableArray(filterAndSortList(list, 'AgreementType'));
        this.statusOptions = ko.observableArray(filterAndSortList(list, 'LocalizedStatus'));
        this.salesRepOptions = ko.observableArray(filterAndSortList(list, 'SalesRep'));
        this.equipmentOptions = ko.observableArray(filterAndSortList(list, 'Equipment'));
        this.agreementType = ko.observable('');
        this.status = ko.observable('');
        this.salesRep = ko.observable('');
        this.equipment = ko.observable('');
        this.dateFrom = ko.observable('');
        this.dateTo = ko.observable('');

        this.list = ko.observableArray(list);

        this.pager = new Paginator(this.list());

        // functions
        this.editUrl = function (id) {
            return editContractUrl + '/' + id;
        }

        this.grandTotal = ko.computed(function () {
            return this.list().reduce(function (sum, curr) {
                return sum + (parseFloat(curr.Value.substr(2)) || 0);
            }, 0).toFixed(2);
        }, this);

        this.selectedTotal = ko.computed(function () {
            return this.pager.pagedList().reduce(function (sum, curr) {
                return curr.isSelected() ?
                    sum + (parseFloat(curr.Value.substr(2)) || 0) :
                    sum;
            }, 0).toFixed(2);
        }, this);

        this.setList = function (list) {
            var list = list.map(function (item) {
                return Object.assign({}, item, {
                    isSelected: ko.observable(false)
                });
            });
            this.list(list);
        }

        // subscritions
        this.list.subscribe(function (newValue) {
            this.agreementOptions(filterAndSortList(newValue, 'AgreementType'));
            this.statusOptions(filterAndSortList(newValue, 'LocalizedStatus'));
            this.salesRepOptions(filterAndSortList(newValue, 'SalesRep'));
            this.equipmentOptions(filterAndSortList(newValue, 'Equipment'));
            this.pager.list(newValue);
        }, this);
    }
    return HomePageTable;
});