module.exports('table', function (require) {

    var Paginator = require('paginator');

    function filterNull(item) {
        return item != null;
    }

    function mapValue(type) {
        return function (item) {
            return item[type].trim() || null;
        };
    }

    function concatIfNotInArray(acc, curr) {
        return acc.indexOf(curr) === -1 ? acc.concat(curr) : acc;
    }

    function sortAssending(a, b) {
        return a == b ? 0 : a > b ? 1 : -1;
    }

    function sortDescending(a, b) {
        return a == b ? 0 : a < b ? 1 : -1;
    }

    function filterAndSortList(list, type) {
        return list.map(mapValue(type))
            .filter(filterNull)
            .reduce(concatIfNotInArray, [''])
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
                    }).indexOf(curr.text) === -1 ?
                    acc.concat(curr) :
                    acc;
            }, [{
                text: '',
                icon: ''
            }])
            .sort(function (a, b) {
                return a.text == b.text ? 0 : a.text > b.text ? 1 : -1;
            });
    }

    function prepareEquipmentList(list) {
        return list.map(mapValue('Equipment'))
            .filter(filterNull)
            .reduce(function (acc, item) {
                return acc.concat(item.indexOf(',') > -1 ? item.split(',').map(function (i) {
                    return i.trim();
                }) : item);
            }, [])
            .reduce(concatIfNotInArray, [''])
            .sort(sortAssending);
    }

    var HomePageTable = function (list) {
        // properties
        this.datePickerOptions = {
            yearRange: '1900:' + new Date().getFullYear(),
            minDate: new Date("1900-01-01"),
            maxDate: new Date(),
        };
        this.sortFields = {
            transactionId: 'TransactionId',
            date: 'Date',
            applicantName: 'CustomerName',
            creditExpiry: 'CreditExpiry',
            loanAmount: 'Value',
            term: 'LoanTerm',
            amort: 'Amort',
            payment: 'MonthlPayment'
        };
        var sortDirections = {
            default: 'default',
            asc: 'asc',
            desc: 'desc'
        };
        this.agreementOptions = ko.observableArray(filterAndSortList(list, 'AgreementType'));
        this.statusOptions = ko.observableArray(prepareStatusList(list));
        this.salesRepOptions = ko.observableArray(filterAndSortList(list, 'SalesRep'));
        this.equipmentOptions = ko.observableArray(prepareEquipmentList(list));
        this.agreementType = ko.observable('');
        this.status = ko.observable('');
        this.salesRep = ko.observable('');
        this.equipment = ko.observable('');
        this.dateFrom = ko.observable('');
        this.dateTo = ko.observable('');
        this.sortedColumn = ko.observable('');
        this.sortDirection = ko.observable(sortDirections.default);

        this.list = ko.observableArray(list);

        this.filteredList = ko.observableArray(this.list());

        this.configureSortClick = function (field) {
            return function () {
                var dir = this.sortDirection();
                if (field == this.sortedColumn()) {
                    this.sortDirection(
                        dir == sortDirections.asc ? sortDirections.desc :
                        dir == sortDirections.desc ? sortDirections.default :
                        sortDirections.asc
                    );
                } else {
                    dir = sortDirections.asc;
                    this.sortDirection(dir);
                    this.sortedColumn(field);
                }
            };
        };

        this.sortedList = ko.computed(function () {
            var field = this.sortedColumn();
            var dir = this.sortDirection();

            var tempList1 = this.filteredList().slice();
            if (dir == sortDirections.default || field == '') {
                return tempList1;
            }
            if (dir == sortDirections.asc) {
                return tempList1.sort(function (a, b) {
                    return sortAssending(a[field], b[field]);
                });
            } else
                return tempList1.sort(function (a, b) {
                    return sortAssending(a[field], b[field]);
                }).reverse();
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

        // functions
        this.clearFilters = function () {
            this.agreementType('');
            this.status('');
            this.salesRep('');
            this.equipment('');
            this.dateFrom('');
            this.dateTo('');
            this.filterList();
        };

        this.editUrl = function (id) {
            return editContractUrl + '/' + id;
        };

        this.filterClasses = function (field) {
            if (this.sortedColumn() == field) {
                return 'filter-ico filter-ico--' + this.sortDirection();
            }
            return 'filter-ico filter-ico--' + sortDirections.default;
        }

        this.filterList = function () {
            var stat = this.status();
            var equip = this.equipment();
            var type = this.agreementType();
            var sales = this.salesRep();
            var to = Date.parseExact(this.dateTo(), 'M/d/yyyy');
            var from = Date.parseExact(this.dateFrom(), 'M/d/yyyy');

            var tempList = this.list().reduce(function (acc, item) {

                var itemDate = Date.parseExact(item.Date, 'M/d/yyyy');
                if ((!stat || stat === item.LocalizedStatus) &&
                    (!type || type === item.AgreementType) &&
                    (!sales || sales === item.SalesRep) &&
                    (!equip || item.Equipment.match(new RegExp(equip, 'i'))) &&
                    (!to || !itemDate || itemDate <= to) &&
                    (!from || !itemDate || itemDate >= from))
                    return acc.concat(item);

                return acc;
            }, []);
            this.filteredList(tempList);
        };

        this.setList = function (list) {
            var tempList = list.map(function (item) {
                return Object.assign({}, item, {
                    isSelected: ko.observable(false)
                });
            });
            this.list(tempList);
        };

        // subscriptions
        this.list.subscribe(function (newValue) {
            this.agreementOptions(filterAndSortList(newValue, 'AgreementType'));
            this.statusOptions(prepareStatusList(newValue));
            this.salesRepOptions(filterAndSortList(newValue, 'SalesRep'));
            this.equipmentOptions(prepareEquipmentList(newValue));
            this.filterList();
        }, this);

        this.sortedList.subscribe(function (newValue) {
            this.pager.list(newValue);
        }, this);
    };
    return HomePageTable;
});