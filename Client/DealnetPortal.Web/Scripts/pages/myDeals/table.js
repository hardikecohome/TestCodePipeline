module.exports('my-deals-table', function (require) {
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
        this.sortFields = Object.freeze({
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
        this.sortedColumn = ko.observable();

        this.showFilters = ko.observable(true);
        this.showDetailedView = ko.observable(false);

        this.list = ko.observableArray(data);

        this.filteredList = ko.observableArray(data);

        this.pager = new Paginator(this.filteredList());

        // computed
        this.sortedList = ko.computed(function () {
            return this.filteredList();
        }, this);

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
                var itemDate = Date.parseExact(item.Date, 'M/d/yyyy');

                if ((!stat || stat === item.LocalizedStatus) &&
                    (!type || type === item.AgreementType) &&
                    (!sales || sales === item.SalesRep) &&
                    (!equip || item.Equipment.match(new RegExp(equip, 'i'))) &&
                    (created == '' || createdBool == item.IsCreatedByCustomer) &&
                    (!dTo || !itemDate || itemDate <= dTo) &&
                    (!dFrom || !itemDate || itemDate >= dFrom) &&
                    (!payment || payment === item.PaymentType) &&
                    (isNaN(vTo) || !isNaN(item.Value) && item.Value <= vTo) &&
                    (isNaN(vFrom) || !isNaN(item.Value) && item.Value >= vFrom)) {
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
                    isExpired: false
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