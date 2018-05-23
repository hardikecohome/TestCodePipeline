﻿module.exports('my-deals-table', function (require) {
    var Paginator = require('paginator');

    var filterNull = require('tableFuncs').filterNull;

    var mapValue = require('tableFuncs').mapValue;

    var concateIfNotInArray = require('tableFuncs').concateIfNotInArray;

    var sortAscending = require('tableFuncs').sortAscending;

    var filterAndSortList = require('tableFuncs').filterAndSortList;

    var prepareStatusList = require('tableFuncs').prepareStatusList;

    function stringIncludes(str, value) {
        return str.toLowerCase().includes(value);
    }

    var MyDealsTable = function (data) {
        this.datePickerOptions = {
            yearRange: '1900:' + new Date().getFullYear(),
            minDate: new Date('1900-01-01'),
            maxDate: new Date()
        };
        var filters = Object.freeze({
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
        this.sortDirections = Object.freeze({
            default: 'default',
            asc: 'asc',
            desc: 'desc'
        });
        this.sortFields = Object.freeze({
            transactionId: 'TransactionId',
            date: 'DateVal',
            applicantName: 'CustomerName',
            creditExpiry: 'CreditExpiry',
            loanAmount: 'valueNum',
            term: 'LoanTerm',
            amort: 'Amort',
            payment: 'MonthlPayment',
            status: 'LocalizedStatus',
            sign: 'SignatureStatus'
        });
        this.sortDdValues = Object.freeze({
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
        });

        this.agreementOptions = ko.observableArray(filterAndSortList(data, 'AgreementType'));
        this.statusOptions = ko.observableArray(prepareStatusList(data));
        this.salesRepOptions = ko.observableArray(filterAndSortList(data, 'SalesRep'));
        this.paymentTypeOptions = ko.observableArray(filterAndSortList(data, 'PaymentType'));

        this.agreementType = ko.observable(localStorage.getItem(filters.agreementType) || '');
        this.status = ko.observable(localStorage.getItem(filters.status) || '');
        this.dateFrom = ko.observable(localStorage.getItem(filters.dateFrom) || '');
        this.dateTo = ko.observable(localStorage.getItem(filters.dateTo) || '');
        this.createdBy = ko.observable(localStorage.getItem(filters.createdBy) || '');
        this.salesRep = ko.observable(localStorage.getItem(filters.salesRep) || '');
        this.equipment = ko.observable(localStorage.getItem(filters.equipment) || '');
        this.typeOfPayment = ko.observable(localStorage.getItem(filters.typeOfPayment) || '');
        this.valueFrom = ko.observable(localStorage.getItem(filters.valueFrom) || '');
        this.valueTo = ko.observable(localStorage.getItem(filters.valueTo) || '');
        this.search = ko.observable('');
        this.singleId = ko.observable('');
        this.sorter = ko.observable('');
        this.sortedColumn = ko.observable();
        this.sortDirection = ko.observable(this.sortDirections.default);

        this.showFilters = ko.observable(true);
        this.showSorters = ko.observable(false);
        this.showLearnMore = ko.observable(false);
        this.showDetailedView = ko.observable(false);

        this.list = ko.observableArray(data);

        this.filteredList = ko.observableArray(data);

        this.pager = new Paginator(this.filteredList());

        // computed
        this.sortedList = ko.computed(function () {
            var field = this.sortedColumn();
            var dir = this.sortDirection();

            var tempList1 = this.filteredList().slice();
            if (dir == this.sortDirections.default || field == '') {
                return tempList1;
            }
            if (dir == this.sortDirections.asc) {
                return tempList1.sort(function (a, b) {
                    return sortAscending(a[field], b[field]);
                });
            } else
                return tempList1.sort(function (a, b) {
                    return sortAscending(a[field], b[field]);
                }).reverse();
        }, this);

        this.grandTotal = ko.computed(function () {
            return this.filteredList().reduce(function (sum, curr) {
                return sum + (curr.valueNum || 0);
            }, 0).toFixed(2);
        }, this);

        this.selectedTotal = ko.computed(function () {
            return this.pager.pagedList().reduce(function (sum, curr) {
                return curr.isSelected() ?
                    sum + (curr.valueNum || 0) :
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
                    var newDir = dir == this.sortDirections.asc ? this.sortDirections.desc :
                        dir == this.sortDirections.desc ? this.sortDirections.default :
                        this.sortDirections.asc;
                    if (newDir == this.sortDirections.default)
                        this.sortedColumn('');
                    this.sortDirection(newDir);
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
            this.dateFrom('');
            this.dateTo('');
            this.createdBy('');
            this.salesRep('');
            this.equipment('');
            this.typeOfPayment('');
            this.valueFrom('');
            this.valueTo('');
            localStorage.removeItem(filters.agreementType);
            localStorage.removeItem(filters.status);
            localStorage.removeItem(filters.dateFrom);
            localStorage.removeItem(filters.dateTo);
            localStorage.removeItem(filters.createdBy);
            localStorage.removeItem(filters.salesRep);
            localStorage.removeItem(filters.equipment);
            localStorage.removeItem(filters.typeOfPayment);
            localStorage.removeItem(filters.valueFrom);
            localStorage.removeItem(filters.valueTo);
        };

        this.clearSort = function () {
            this.sortedColumn('');
            this.sortDirection(this.sortDirections.default);
        };

        this.saveFilters = function () {
            this.agreementType() && localStorage.setItem(filters.agreementType, this.agreementType());
            this.status() && localStorage.setItem(filters.status, this.status());
            this.dateFrom() && localStorage.setItem(filters.dateFrom, this.dateFrom());
            this.dateTo() && localStorage.setItem(filters.dateTo, this.dateTo());
            this.createdBy() && localStorage.setItem(filters.createdBy, this.createdBy());
            this.salesRep() && localStorage.setItem(filters.salesRep, this.salesRep());
            this.equipment() && localStorage.setItem(filters.equipment, this.equipment());
            this.typeOfPayment() && localStorage.setItem(filters.typeOfPayment, this.typeOfPayment());
            this.valueFrom() && localStorage.setItem(filters.valueFrom, this.valueFrom());
            this.valueTo() && localStorage.set(filters.valueTo, this.valueTo());
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

        this.filterList = function () {
            var stat = this.status();
            var equip = this.equipment();
            var type = this.agreementType();
            var dFrom = Date.parseExact(this.dateFrom(), 'M/d/yyyy');
            var dTo = Date.parseExact(this.dateTo(), 'M/d/yyyy');
            var created = this.createdBy();
            var createdBool = Boolean(created);
            var sales = this.salesRep();
            var payment = this.typeOfPayment();
            var vFrom = parseFloat(this.valueFrom());
            var vTo = parseFloat(this.valueTo());

            var tempList = this.list().reduce(function (acc, item) {
                if ((!stat || stat === item.LocalizedStatus) &&
                    (!type || type === item.AgreementType) &&
                    (!sales || sales === item.SalesRep) &&
                    (!equip || item.Equipment.match(new RegExp(equip, 'i'))) &&
                    (created == '' || createdBool == item.IsCreatedByCustomer) &&
                    (!dTo || !item.DateVal || item.DateVal <= dTo) &&
                    (!dFrom || !item.DateVal || item.DateVal >= dFrom) &&
                    (!payment || payment === item.PaymentType) &&
                    (isNaN(vTo) || item.valueNum && item.valueNum <= vTo) &&
                    (isNaN(vFrom) || item.valueNum && item.valueNum >= vFrom)) {
                    return acc.concat(item);
                }
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
                    showNotes: ko.observable(false),
                    isExpired: false,
                    valueNum: parseFloat(item.Value.substr(2)) || 0,
                    DateVal: item.Date ? new Date(item.Date) : ''
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

        this.exportAll = function () {
            var selected = this.filteredList().filter(function (item) {
                return item.isSelected();
            }).map(function (item) {
                return item.Id;
            });
            this.filteredList().forEach(function (item) {
                item.isSelected(true);
            });
            $('#export-form').submit();
            this.filteredList().forEach(function (item) {
                item.isSelected(selected.includes(item.Id));
            });
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

        this.removeContract = function () {
            debugger
        };

        //subscriptions
        this.list.subscribe(function (newValue) {
            this.agreementOptions(filterAndSortList(newValue, 'AgreementType'));
            this.statusOptions(prepareStatusList(newValue));
            this.salesRepOptions(filterAndSortList(newValue, 'SalesRep'));
            this.paymentTypeOptions(filterAndSortList(newValue, 'PaymentType'));
            this.filterList();
        }, this);

        this.sortedList.subscribe(function (newValue) {
            this.pager.list(newValue);
        }, this);
    };

    return MyDealsTable;
});