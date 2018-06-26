module.exports('table', function (require) {

    var Paginator = require('paginator');

    var sortAscending = require('tableFuncs').sortAscending;

    var filterAndSortList = require('tableFuncs').filterAndSortList;

    var prepareStatusList = require('tableFuncs').prepareStatusList;

    var HomePageTable = function (list) {
        // properties
        this.datePickerOptions = Object.freeze({
            yearRange: '1900:' + new Date().getFullYear(),
            minDate: new Date("1900-01-01"),
            maxDate: new Date(),
        });
        this.sortFields = Object.freeze({
            transactionId: 'transNum',
            date: 'DateVal',
            applicantName: 'CustomerName',
            creditExpiry: 'CreditExpiry',
            loanAmount: 'valueNum',
            term: 'Term',
            amort: 'Amort',
            payment: 'MonthlPayment',
            status: 'LocalizedStatus',
            sign: 'SignatureStatus'
        });
        this.sortDirections = Object.freeze({
            default: 'default',
            asc: 'asc',
            desc: 'desc'
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
            loanAmountAsc: {
                field: this.sortFields.loanAmount,
                dir: this.sortDirections.asc
            },
            loanAmountDesc: {
                field: this.sortFields.loanAmount,
                dir: this.sortDirections.desc
            },
            termAsc: {
                field: this.sortFields.term,
                dir: this.sortDirections.asc
            },
            termDesc: {
                field: this.sortFields.term,
                dir: this.sortDirections.desc
            },
            amortAsc: {
                field: this.sortFields.amort,
                dir: this.sortDirections.asc
            },
            amortDesc: {
                field: this.sortFields.amort,
                dir: this.sortDirections.desc
            },
        });
        var filters = Object.freeze({
            agreementType: DealerName + '-home-agreementTypeFilter',
            status: DealerName + '-home-statusFilter',
            salesRep: DealerName + '-home-salesRepFilter',
            createdBy: DealerName + '-home-createdByFilter',
            dateTo: DealerName + '-home-dateToFilter',
            dateFrom: DealerName + '-home-dateFromFilter'
        });
        this.agreementOptions = ko.observableArray(filterAndSortList(list, 'AgreementType'));
        this.statusOptions = ko.observableArray(prepareStatusList(list));
        this.salesRepOptions = ko.observableArray(filterAndSortList(list, 'SalesRep'));

        this.agreementType = ko.observable('');
        this.status = ko.observable('');
        this.salesRep = ko.observable('');
        this.createdBy = ko.observable('');
        this.dateFrom = ko.observable('');
        this.dateTo = ko.observable('');
        this.singleId = ko.observable('');
        this.sorter = ko.observable('');
        this.sortedColumn = ko.observable('');
        this.sortDirection = ko.observable(this.sortDirections.default);

        this.showFilters = ko.observable(false);
        this.showSorters = ko.observable(false);
        this.showLearnMore = ko.observable(false);
        this.filtersSaved = ko.observable(false);

        this.list = ko.observableArray(list);

        this.filteredList = ko.observableArray(this.list());
        this.noRecordsFound = ko.pureComputed(function () {
            return this.filteredList().length === 0
        }, this);
        this.pager = new Paginator(this.filteredList());

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
                return sum + curr.valueNum || 0;
            }, 0).toFixed(2);
        }, this);

        this.selectedTotal = ko.computed(function () {
            return this.filteredList().reduce(function (sum, curr) {
                return curr.isSelected() ?
                    sum + curr.valueNum || 0 :
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

        this.allSelected = ko.pureComputed({
            read: function () {
                return this.pager.pagedList().reduce(function (acc, item) {
                    return (item.Id == 0 || item.IsInternal || item.isSelected()) && acc;
                }, true);
            },
            write: function (value) {
                this.pager.pagedList().forEach(function (item) {
                    if (item.Id != 0 && !item.IsInternal) item.isSelected(value);
                });
            },
            owner: this
        });

        // functions
        function clearSavedFilters() {
            localStorage.removeItem(filters.agreementType);
            localStorage.removeItem(filters.status);
            localStorage.removeItem(filters.salesRep);
            localStorage.removeItem(filters.createdBy);
            localStorage.removeItem(filters.dateTo);
            localStorage.removeItem(filters.dateFrom);
        }

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
            this.createdBy('');
            this.dateFrom('');
            this.dateTo('');
            this.filterList();
            clearSavedFilters();
        };

        this.clearSort = function () {
            this.sortedColumn('');
            this.sortDirection(this.sortDirections.default);
        };

        this.saveFilters = function () {
            clearSavedFilters();
            this.agreementType() && localStorage.setItem(filters.agreementType, this.agreementType());
            this.status() && localStorage.setItem(filters.status, this.status());
            this.salesRep() && localStorage.setItem(filters.salesRep, this.salesRep());
            this.createdBy() && localStorage.setItem(filters.createdBy, this.createdBy());
            this.dateTo() && localStorage.setItem(filters.dateTo, this.dateTo());
            this.dateFrom() && localStorage.setItem(filters.dateFrom, this.dateFrom());
            this.filtersSaved(true);
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
            var type = this.agreementType();
            var sales = this.salesRep();
            var created = this.createdBy();
            var createdBool = created == 'true';
            var to = Date.parseExact(this.dateTo(), 'M/d/yyyy');
            var dFrom = Date.parseExact(this.dateFrom(), 'M/d/yyyy');

            var tempList = this.list().reduce(function (acc, item) {
                if ((!stat || stat === item.LocalizedStatus) &&
                    (!type || type === item.AgreementType) &&
                    (!sales || sales === item.SalesRep) &&
                    (created == '' || createdBool == item.IsCreatedByCustomer) &&
                    (!to || !item.DateVal || item.DateVal <= to) &&
                    (!dFrom || !item.DateVal || item.DateVal >= dFrom))
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
                    showNotes: ko.observable(false),
                    valueNum: parseFloat(item.LoanAmount.substr(2)) || 0,
                    DateVal: item.Date ? new Date(item.Date) : '',
                    transNum: Number(item.TransactionId) || 0
                });
            });
            this.list(tempList);
        };

        this.exportToExcel = function (id) {
            exportList([id]);
        };

        this.exportSelectedToExcel = function () {
            var ids = this.filteredList()
                .filter(function (item) {
                    return item.isSelected();
                }).map(function (item) {
                    return item.Id;
                });
            exportList(ids);
        };

        function exportList(list) {
            var $div = $('#export-all-form');
            var inputList = list.map(function (item) {
                return $('<input/>', {
                    type: 'hidden',
                    value: item,
                    name: 'ids'
                });
            });
            $div.append(inputList);
            $('#export-form').submit();
            $div.empty();
        }

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

        this.openSignaturePopup = function (deal) {
            var id = deal.Id;
            $.ajax({
                method: 'GET',
                url: contractSignatureStatusUrl + '?contractId=' + id,
            }).done(function (data) {
                $('#signature-body').html(data);
                $('#contract-signature-modal').modal();
            });
        };

        // subscriptions
        this.list.subscribe(function (newValue) {
            this.agreementOptions(filterAndSortList(newValue, 'AgreementType'));
            this.statusOptions(prepareStatusList(newValue));
            this.salesRepOptions(filterAndSortList(newValue, 'SalesRep'));

            this.agreementType(localStorage.getItem(filters.agreementType) || '');
            this.status(localStorage.getItem(filters.status) || '');
            this.salesRep(localStorage.getItem(filters.salesRep) || '');
            this.createdBy(localStorage.getItem(filters.createdBy) || '');
            this.dateFrom(localStorage.getItem(filters.dateFrom) || '');
            this.dateTo(localStorage.getItem(filters.dateTo) || '');

            this.filterList();
        }, this);

        this.sortedList.subscribe(function (newValue) {
            this.pager.list(newValue);
        }, this);

        this.selectedIds.subscribe(function (newValue) {
            if (newValue.length) {
                $('.floatingHelpBtn, .home-page').addClass('lift');
            } else {
                $('.floatingHelpBtn, .home-page').removeClass('lift');
            }
        }, this);

        $('body').on(($('body.iso-device').length ? 'touchstart' : 'click'), (function (e) {
            var $el = $(e.target);
            var noteId = $el.data('notes');
            var actionId = $el.data('action');

            this.list().forEach(function (item) {
                item.TransactionId != noteId && item.showNotes(false);
                item.TransactionId != actionId && item.showActions(false);
            });
        }).bind(this));
    };
    return HomePageTable;
});