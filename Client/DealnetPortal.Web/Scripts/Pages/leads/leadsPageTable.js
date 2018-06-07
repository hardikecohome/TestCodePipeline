﻿module.exports('leads-table', function(require) {

    var Paginator = require('paginator');

    var filterNull = require('tableFuncs').filterNull;

    var mapValue = require('tableFuncs').mapValue;

    var concatIfNotInArray = require('tableFuncs').concatIfNotInArray;
    var sortAscending = require('tableFuncs').sortAscending;

    var sortDescending = require('tableFuncs').sortDescending;

    var filterAndSortList = require('tableFuncs').filterAndSortList;

    var LeadsTable = function(list) {
        console.log(list);
        this.datePickerOptions = {
            yearRange: '1900:' + new Date().getFullYear(),
            minDate: new Date("1900-01-01"),
            maxDate: new Date(),
        };

        this.sortFields = Object.freeze({
            date: 'Date',
            postalCode: 'PostalCode',
            preApprovedFor: 'PreApprovalAmount',
            projectType: 'Equipment',
            customerComment: 'CustomerComment'
        });

        this.sortDirections = Object.freeze({
            default: 'default',
            asc: 'asc',
            desc: 'desc'
        });

        this.sortDdValues = Object.freeze({
            dateAsc: {
                field: this.sortFields.date,
                dir: this.sortDirections.asc
            },
            dateDesc: {
                field: this.sortFields.date,
                dir: this.sortDirections.desc
            },
            postalCodeAsc: {
                field: this.sortFields.postalCode,
                dir: this.sortDirections.asc
            },
            postalCodeDesc: {
                field: this.sortFields.postalCode,
                dir: this.sortDirections.desc
            },
            preApprovedForAsc: {
                field: this.sortFields.preApprovedFor,
                dir: this.sortDirections.asc
            },
            preApprovedForDesc: {
                field: this.sortFields.preApprovedFor,
                dir: this.sortDirections.desc
            },
            projectTypeAsc: {
                field: this.sortFields.projectType,
                dir: this.sortDirections.asc
            },
            projectTypeDesc: {
                field: this.sortFields.projectType,
                dir: this.sortDirections.desc
            },
            customerCommentAsc: {
                field: this.sortFields.customerComment,
                dir: this.sortDirections.asc
            },
            customerCommentDesc: {
                field: this.sortFields.customerComment,
                dir: this.sortDirections.desc
            }
        });

        var filters = Object.freeze({
            preApprovedFor: 'agreementTypeFilter',
            postalCode: 'postalCodeFilter',
            dateTo: 'dateToFilter',
            dateFrom: 'dateFromFilter'
        });
        this.postalCodesOptions = ko.observableArray(filterAndSortList(list, 'PostalCode'));
        this.preApprovedForOptions = ko.observableArray(filterAndSortList(list, 'PreApprovalAmount'));

        this.preApprovedFor = ko.observable(localStorage.getItem(filters.preApprovedFor) || '');
        this.postalCode = ko.observable(localStorage.getItem(filters.postalCode) || '');
        this.dateFrom = ko.observable(localStorage.getItem(filters.dateFrom) || '');
        this.dateTo = ko.observable(localStorage.getItem(filters.dateTo) || '');
        this.sorter = ko.observable('');
        this.sortedColumn = ko.observable('');
        this.sortDirection = ko.observable(this.sortDirections.default);

        this.showFilters = ko.observable(true);
        this.showSorters = ko.observable(false);
        this.showLearnMore = ko.observable(false);
        this.selectedLeadId = ko.observable(0);
        this.list = ko.observableArray(list);
        this.isError = ko.observable(false);
        this.errorMessage = ko.observable('');
        this.search = ko.observable('');
        this.showResultMessage = ko.observable(false);
        this.showLeadPopup = ko.observable(false);
        this.filteredList = ko.observableArray(this.list());

        this.pager = new Paginator(this.filteredList());
        this.acceptLead = function() {
            var selectedLeadId = this.selectedLeadId();
            $.ajax({
                type: "POST",
                url: 'leads/acceptLead?id=' + selectedLeadId
            }).done(function(json) {
                this.isError(json.isError);
                if (json.Errors) {
                    this.errorMessage(json.Errors.reduce(function(acc, error) {
                            acc += error + '.\n';
                            return acc;
                        },
                        ''));
                } else {
                    var $message = $('#new-lead-notification-success');
                    $message.html($message.html().replace('{1}', selectedLeadId));
                }
                this.showResultMessage(true);
                var temp = this.filteredList().filter(function(item) { return item.TransactionId != selectedLeadId });
                this.filteredList(temp);
                this.showLeadPopup(false);
            }.bind(this));
        }

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

        this.clearFilters = function () {
            this.preApprovedFor('');
            this.postalCode('');
            this.dateFrom('');
            this.dateTo('');
            this.filterList();
            localStorage.removeItem(filters.preApprovedFor);
            localStorage.removeItem(filters.postalCode);
            localStorage.removeItem(filters.dateTo);
            localStorage.removeItem(filters.dateFrom);
        };

        this.toggleAcceptLeadPopup = function(transactionId) {
            this.selectedLeadId(transactionId || 0);
            this.showLeadPopup(!this.showLeadPopup());
            $('#help-hover').css('display', this.showLeadPopup() ? 'block': 'none');
        }

        this.clearSort = function () {
            this.sortedColumn('');
            this.sortDirection(this.sortDirections.default);
        };

        this.saveFilters = function () {
            this.preApprovedFor() && localStorage.setItem(filters.preApprovedFor, this.preApprovedFor());
            this.postalCode() && localStorage.setItem(filters.postalCode, this.postalCode());
            this.dateTo() && localStorage.setItem(filters.dateTo, this.dateTo());
            this.dateFrom() && localStorage.setItem(filters.dateFrom, this.dateFrom());
        };

        this.toggleFilters = function () {
            this.showSorters(false);
            this.showFilters(!this.showFilters());
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

        this.clearSort = function () {
            this.sortedColumn('');
            this.sortDirection(this.sortDirections.default);
        };

        this.setList = function (list) {
            var tempList = list.map(function (item) {
                return Object.assign({}, item, {
                    isSelected: ko.observable(false),
                    isMobileOpen: ko.observable(false),
                    showActions: ko.observable(false),
                    showInfo: ko.observable(false),
                    valueNum: parseFloat(item.Value.substr(2)) || 0,
                    DateVal: item.Date ? new Date(item.Date) : ''
                });
            });
            this.list(tempList);
        };

        this.filterClasses = function (field) {
            if (this.sortedColumn() == field) {
                return 'filter-ico filter-ico--' + this.sortDirection();
            }
            return 'filter-ico filter-ico--' + this.sortDirections.default;
        };

        this.filterList = function () {
            if (this.sorter()) {
                var sort = this.sortDdValues[this.sorter()];
                this.sortDirection(sort.dir);
                this.sortedColumn(sort.field);
                this.sorter('');
            }

            var postalCode = this.postalCode();
            var preApproved = this.preApprovedFor();
            var to = Date.parseExact(this.dateTo(), 'M/d/yyyy');
            var dFrom = Date.parseExact(this.dateFrom(), 'M/d/yyyy');

            var tempList = this.list().reduce(function (acc, item) {
                if ((!postalCode || postalCode === item.PostalCode) &&
                    (!preApproved || preApproved === item.PreApprovalAmount) &&
                    (!to || !item.DateVal || item.DateVal <= to) &&
                    (!dFrom || !item.DateVal || item.DateVal >= dFrom))
                    return acc.concat(item);

                return acc;
            }, []);
            this.filteredList(tempList);
        };

        // subscriptions
        this.list.subscribe(function (newValue) {
            this.postalCodesOptions(filterAndSortList(newValue, 'PostalCode'));
            this.preApprovedForOptions(filterAndSortList(newValue, 'PreApprovalAmount'));
            this.filterList();
        }, this);

        this.sortedList.subscribe(function (newValue) {
            this.pager.list(newValue);
        }, this);
        this.search.subscribe(function(newValue) {

        });
    }

    return LeadsTable;
})