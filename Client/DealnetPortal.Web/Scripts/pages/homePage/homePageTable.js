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
        return list.map(mapValue(type))
            .filter(filterNull)
            .reduce(concatIfNotInArray, [])
            .sort(sortAssending);
    }

    var HomePageTable = function (list) {
        this.agreementOptions = filterAndSortList(list, 'AgreementType');
        this.statusOptions = filterAndSortList(list, 'LocalizedStatus');
        this.salesRepOptions = filterAndSortList(list, 'SalesRep');
        this.equipmentOptions = filterAndSortList(list, 'Equipment');
        this.agreementType = ko.observable('');
        this.status = ko.observable('');
        this.salesRep = ko.observable('');
        this.equipment = ko.observable('');
        this.dateFrom = ko.observable('');
        this.dateTo = ko.observable('');

        this.list = list;

        this.pager = new Paginator(this.list);

        this.editUrl = function (id) {
            return editContractUrl + '/' + id;
        }

        this.grandTotal = this.list.reduce(function (sum, curr) {
            return sum + (parseFloat(curr.Value.substr(2)) || 0);
        }, 0)

        this.selectedTotal = ko.computed(function () {
            return this.pager.pagedList().reduce(function (sum, curr) {
                return curr.isSelected ?
                    sum + (parseFloat(curr.Value.substr(2)) || 0) :
                    sum;
            }, 0).toFixed(2);
        }, this);
    }
    return HomePageTable;
});