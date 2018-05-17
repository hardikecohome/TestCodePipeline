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
                return sortAssending(a.text, b.text);
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
            payment: 'MonthlPayment',
            status: 'LocalizedStatus',
            sign: 'SignatureStatus'
        };
        this.sortDirections = {
            default: 'default',
            asc: 'asc',
            desc: 'desc'
        };
        this.sortDdValues = {
            transactionIdAsc: {
                field: this.sortFields.transactionId,
                dir: this.sortDirections.asc
            },
            transactionIdDesc: {
                field: this.sortFields.transactionId,
                dir: this.sortDirections.desc
            },
            dateAsc: {
                field: this.sortFields.date,
                dir: this.sortDirections.asc
            },
            dateDesc: {
                field: this.sortFields.date,
                dir: this.sortDirections.desc
            },
            applicantNameAsc: {
                field: this.sortFields.applicantName,
                dir: this.sortDirections.asc
            },
            applicantNameDesc: {
                field: this.sortFields.applicantName,
                dir: this.sortDirections.desc
            },
            creditExpiryAsc: {
                field: this.sortFields.creditExpiry,
                dir: this.sortDirections.asc
            },
            creditExpiryDesc: {
                field: this.sortFields.creditExpiry,
                dir: this.sortDirections.desc
            },
            statusAsc: {
                field: this.sortFields.status,
                dir: this.sortDirections.asc
            },
            statusDesc: {
                field: this.sortFields.status,
                dir: this.sortDirections.desc
            },
            signAsc: {
                field: this.sortFields.sign,
                dir: this.sortDirections.asc
            },
            signDesc: {
                field: this.sortFields.sign,
                dir: this.sortDirections.desc
            },
        };
        var filters = {
            agreementType: 'agreementTypeFilter',
            status: 'statusFilter',
            salesRep: 'salesRepFilter',
            equipment: 'equipmentFilter',
            dateTo: 'dateToFilter',
            dateFrom: 'dateFromFilter'
        };
        this.agreementOptions = ko.observableArray(filterAndSortList(list, 'AgreementType'));
        this.statusOptions = ko.observableArray(prepareStatusList(list));
        this.salesRepOptions = ko.observableArray(filterAndSortList(list, 'SalesRep'));
        this.equipmentOptions = ko.observableArray(prepareEquipmentList(list));

        this.agreementType = ko.observable(localStorage.getItem(filters.agreementType) || '');
        this.status = ko.observable(localStorage.getItem(filters.status) || '');
        this.salesRep = ko.observable(localStorage.getItem(filters.salesRepFilter) || '');
        this.equipment = ko.observable(localStorage.getItem(filters.equipment) || '');
        this.dateFrom = ko.observable(localStorage.getItem(filters.dateFrom) || '');
        this.dateTo = ko.observable(localStorage.getItem(filters.dateTo) || '');
        this.singleId = ko.observable('');
        this.sorter = ko.observable('');
        this.sortedColumn = ko.observable('');
        this.sortDirection = ko.observable(this.sortDirections.default);

        this.showFilters = ko.observable(true);
        this.showSorters = ko.observable(false);
        this.showLearnMore = ko.observable(false);

        this.list = ko.observableArray(list);

        this.filteredList = ko.observableArray(this.list());

        this.sortedList = ko.computed(function () {
            var field = this.sortedColumn();
            var dir = this.sortDirection();

            var tempList1 = this.filteredList().slice();
            if (dir == this.sortDirections.default || field == '') {
                return tempList1;
            }
            if (dir == this.sortDirections.asc) {
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

        this.selectedIds = ko.computed(function () {
            return this.list().filter(function (item) {
                return item.isSelected();
            }).map(function (item) {
                return item.Id;
            });
        }, this);

        // functions
        this.toggleFilters = function () {
            this.showSorters(false);
            this.showFilters(!this.showFilters());
        };

        this.toggleSorters = function () {
            this.showFilters(false);
            this.showSorters(!this.showSorters());
        };

        this.configureSortClick = function (field) {
            return function () {
                var dir = this.sortDirection();
                if (field == this.sortedColumn()) {
                    this.sortDirection(
                        dir == this.sortDirections.asc ? this.sortDirections.desc :
                        dir == this.sortDirections.desc ? this.sortDirections.default :
                        this.sortDirections.asc
                    );
                } else {
                    dir = this.sortDirections.asc;
                    this.sortDirection(dir);
                    this.sortedColumn(field);
                }
            }.bind(this);
        };

        this.clearFilters = function () {
            this.agreementType('');
            this.status('');
            this.salesRep('');
            this.equipment('');
            this.dateFrom('');
            this.dateTo('');
            this.filterList();
            localStorage.removeItem(filters.agreementType);
            localStorage.removeItem(filters.status);
            localStorage.removeItem(filters.salesRep);
            localStorage.removeItem(filters.equipment);
            localStorage.removeItem(filters.dateTo);
            localStorage.removeItem(filters.dateFrom);
        };

        this.clearSort = function () {
            this.sortedColumn('');
            this.sortDirection(this.sortDirections.default);
        };

        this.saveFilters = function () {
            this.agreementType() && localStorage.setItem(filters.agreementType, this.agreementType());
            this.status() && localStorage.setItem(filters.status, this.status());
            this.salesRep() && localStorage.setItem(filters.salesRep, this.salesRep());
            this.equipment() && localStorage.setItem(filters.equipment, this.equipment());
            this.dateTo() && localStorage.setItem(filters.dateTo, this.dateTo());
            this.dateFrom() && localStorage.setItem(filters.dateFrom, this.dateFrom());
        };

        this.editUrl = function (id) {
            return editContractUrl + '/' + id;
        };

        this.filterClasses = function (field) {
            if (this.sortedColumn() == field) {
                return 'filter-ico filter-ico--' + this.sortDirection();
            }
            return 'filter-ico filter-ico--' + this.sortDirections.default;
        };

        this.getExpiryText = function (data) {
            return data.CreditExpiry && data.CreditExpiry < 20 ? translations['ExpiresInXdays'].replace('{0}', data.CreditExpiry) : '';
        };

        this.filterList = function () {
            if (this.sorter()) {
                var sort = this.sortDdValues[this.sorter()];
                this.sortDirection(sort.dir);
                this.sortedColumn(sort.field);
                this.sorter('');
            }

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
                    isSelected: ko.observable(false),
                    isMobileOpen: ko.observable(false),
                    showActions: ko.observable(false),
                    showNotes: ko.observable(false)
                });
            });
            this.list(tempList);
        };

        this.exportToExcel = function (id) {
            this.singleId(id);
            $('#export-form-1').submit();
            this.singleId('');
        };

        this.exportSelectedToExcel = function () {
            $('#export-form').submit();
        };

        this.previewContracts = function () {
            var ids = this.selectedIds();
            if (ids.length > 1) {
                $('#preview-form').submit();
            } else {
                window.location.href = contractPreviewUrl + ids[0];
            }
        };

        this.openHelpModal = function (id) {
            sendEmailModel(id);
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