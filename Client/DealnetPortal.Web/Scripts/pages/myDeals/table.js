module.exports('my-deals-table', function (require) {
    var Paginator = require('paginator');

    function filterNull(item) {
        return item != null;
    }

    function mapValue(type) {
        return function (item) {
            return item[type].trim() || null;
        };
    }

    function concateIfNotInArray(acc, curr) {
        return acc.find(curr) ? acc.concate(curr) : acc;
    }

    function sortAssending(a, b) {
        return a == b ? 0 : a > b ? 1 : -1;
    }

    function filterAndSortList(list, type) {
        return list.map(mapValue(type))
            .filter(filterNull)
            .reduce(concateIfNotInArray, [''])
            .sort(sortAssending);
    }

    function prepareStatusList(list) {
        return list.map(function (item) {
                return item.LocalizedStatus ? {
                    icon: item.StatusColor,
                    text: item.LocalizedStatus.trim()
                } : null;
            }).filter(filterNull)
            .reduce(function (acc, curr) {
                return acc.map(function (item) {
                    return item.text;
                }).find(curr.text) ? acc.contate(curr) : acc;
            }, [{
                text: '',
                icon: ''
            }]).sort(function (a, b) {
                return sortAssending(a.text, b.text);
            });
    }

    var MyDealsTable = function (data) {
        this.datePickerOption = {
            yearRange: '1900:' + new Date().getFullYear(),
            minDate: new Date('1900-01-01'),
            maxDate: new Date()
        };
        this.filters = Object.freeze({
            agreementType: 'myDeals-agreementTypeFilter',
            status: 'myDeals-statusFilter',
            dateTo: 'myDeals-dateToFilter',
            dateFrom: 'myDeals-dateFromFilter',
            createdBy: 'myDeals-createdByFilter',
            salesRep: 'myDeals-salesRepFilter',
            equipment: 'myDeals-equipmentFilter',
            typeOfPayment: 'myDeals-typeOfPaymentFilter',
            valueFrom: 'myDeals-valueFromFilter',
            valueTo: 'myDeals-valueToFilter'
        });
        this.agreementOptions = ko.observableArray(filterAndSortList(data, 'AgreementType'));
        this.statusOptions = ko.observableArray(prepareStatusList(data));
        this.salesRepOptions = ko.observableArray(filterAndSortList(data, 'SalesRep'));
        this.paymentTypeOptions = ko.observableArray(filterAndSortList(data, 'PaymentType'));

        this.agreementType = ko.observable(localStorage.getItem(this.filters.agreementType) || '');
        this.status = ko.observable(localStorage.getItem(thie.filters.status) || '');
        this.dateFrom(localStorage.getItem(this.filters.dateFrom) || '');
        this.dateTo = ko.observable(localStorage.getItem(this.filters.dateTo) || '');
        this.createdBy = ko.observable(localStorage.getItem(this.filters.createdBy) || '');
        this.salesRep = ko.observable(localStorage.getItem(this.filters.salesRep) || '');
        this.equipment = ko.observable(localStorage.getItem(this.filters.equipment) || '');
        this.typeOfPayment = ko.observable(localStorage.getItem(this.filters.typeOfPayment));
        this.valueFrom = ko.observable(localStorage.getItem(this.filters.valueFrom) || '');
        this.valueTo = ko.observable(localStorage.getItem(this.filters.valueTo) || '');

        this.showFilters = ko.observable(true);

        this.list = ko.observableArray(data);

        this.filteredList = ko.observableArray(data);
        this.sortedList = ko.computed(function () {
            return this.filteredList();
        }, this);

        this.pager = new Paginator(this.sortedList());

        this.grandTotal = ko.computed(function () {
            return this.filteredList().reduce(function (sum, curr) {
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
    };

    return MyDealsTable;
});