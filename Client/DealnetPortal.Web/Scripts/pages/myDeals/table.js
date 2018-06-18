module.exports('my-deals-table', function (require) {
    var Paginator = require('paginator');

    var filterNull = require('tableFuncs').filterNull;

    var mapValue = require('tableFuncs').mapValue;

    var concatIfNotInArray = require('tableFuncs').concatIfNotInArray;

    var sortAscending = require('tableFuncs').sortAscending;

    var filterAndSortList = require('tableFuncs').filterAndSortList;

    var prepareStatusList = require('tableFuncs').prepareStatusList;

    var dynamicAlertModal = require('alertModal').dynamicAlertModal;

    var hideDynamicModal = require('alertModal').hideDynamicAlertModal;

    var showLoader = require('loader').showLoader;
    var hideLoader = require('loader').hideLoader;

    function prepareEquipmentList(list) {
        return list.map(mapValue('Equipment'))
            .filter(filterNull)
            .reduce(function (acc, item) {
                return acc.concat(item.indexOf(',') > -1 ? item.split(',').map(function (i) {
                    return i.trim();
                }) : item);
            }, [])
            .reduce(concatIfNotInArray, [''])
            .sort(sortAscending);
    }

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
            agreementType: DealerName + '-myDeals-agreementTypeFilter',
            status: DealerName + '-myDeals-statusFilter',
            dateTo: DealerName + '-myDeals-dateToFilter',
            dateFrom: DealerName + '-myDeals-dateFromFilter',
            createdBy: DealerName + '-myDeals-createdByFilter',
            salesRep: DealerName + '-myDeals-salesRepFilter',
            equipment: DealerName + '-myDeals-equipmentFilter',
            typeOfPayment: DealerName + '-myDeals-typeOfPaymentFilter',
            valueFrom: DealerName + '-myDeals-valueFromFilter',
            valueTo: DealerName + '-myDeals-valueToFilter'
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
        this.equipmentOptions = ko.observableArray(prepareEquipmentList(data));

        this.agreementType = ko.observable('');
        this.status = ko.observable('');
        this.dateFrom = ko.observable('');
        this.dateTo = ko.observable('');
        this.createdBy = ko.observable('');
        this.salesRep = ko.observable('');
        this.equipment = ko.observable('');
        this.typeOfPayment = ko.observable('');
        this.valueFrom = ko.observable('');
        this.valueTo = ko.observable('');
        this.search = ko.observable('');
        this.singleId = ko.observable('');
        this.sorter = ko.observable('');
        this.sortedColumn = ko.observable();
        this.sortDirection = ko.observable(this.sortDirections.default);

        this.showFilters = ko.observable(false);
        this.showSorters = ko.observable(false);
        this.showLearnMore = ko.observable(false);
        this.showDetailedView = ko.observable(false);
        this.filtersSaved = ko.observable(false);

        this.list = ko.observableArray(data);

        this.filteredList = ko.observableArray(data);
        this.noRecordsFound = ko.pureComputed(function () {
            return this.sortedList().length === 0
        }, this);

        this.pager = new Paginator(this.filteredList());

        // computed
        this.searchedList = ko.computed(function () {
            var searchTerm = this.search().toLowerCase();

            function includesSearchTerm(val) {
                return val != null && stringIncludes(val, searchTerm);
            }
            return this.filteredList().filter(function (deal) {
                return includesSearchTerm(deal.CustomerName) ||
                    includesSearchTerm(deal.LocalizedStatus) ||
                    includesSearchTerm(deal.Email) ||
                    includesSearchTerm(deal.Phone) ||
                    includesSearchTerm(deal.TransactionId) ||
                    includesSearchTerm(deal.ProgramOption) ||
                    includesSearchTerm(deal.Value) ||
                    includesSearchTerm(deal.Equipment) ||
                    includesSearchTerm(deal.Address) ||
                    includesSearchTerm(deal.SalesRep) ||
                    includesSearchTerm(deal.EnteredBy) ||
                    includesSearchTerm(deal.CustomerComment);
            });
        }, this);

        this.sortedList = ko.computed(function () {
            var field = this.sortedColumn();
            var dir = this.sortDirection();

            var tempList1 = this.searchedList().slice();
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
            return this.filteredList().reduce(function (sum, curr) {
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

        this.allSelected = ko.pureComputed({
            read: function () {
                return this.pager.pagedList().reduce(function (acc, item) {
                    return (item.Id == 0 || item.IsInternal || item.isSelected()) && acc;
                }, true);
            },
            write: function (value) {
                this.pager.pagedList().forEach(function (item) {
                    if (!value) item.isSelected(value);
                    if (value && item.Id > 0 && !item.IsInternal) item.isSelected(value);
                });
            },
            owner: this
        });

        // functions
        function clearSavedFilters() {
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
            this.search('');
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
            this.dateFrom() && localStorage.setItem(filters.dateFrom, this.dateFrom());
            this.dateTo() && localStorage.setItem(filters.dateTo, this.dateTo());
            this.createdBy() && localStorage.setItem(filters.createdBy, this.createdBy());
            this.salesRep() && localStorage.setItem(filters.salesRep, this.salesRep());
            this.equipment() && localStorage.setItem(filters.equipment, this.equipment());
            this.typeOfPayment() && localStorage.setItem(filters.typeOfPayment, this.typeOfPayment());
            this.valueFrom() && localStorage.setItem(filters.valueFrom, this.valueFrom());
            this.valueTo() && localStorage.setItem(filters.valueTo, this.valueTo());
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
            var dFrom = Date.parseExact(this.dateFrom(), 'M/d/yyyy');
            var dTo = Date.parseExact(this.dateTo(), 'M/d/yyyy');
            var created = this.createdBy();
            var createdBool = created == 'true';
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

        this.exportAll = function () {
            var ids = this.list()
                .filter(function (item) {
                    return item.Id > 0 && !item.IsInternal;
                })
                .map(function (item) {
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

        this.removeContract = function (id) {
            dynamicAlertModal({
                message: translations['AreYouSureYouWantToRemoveThisApplication'] + '?',
                title: translations['Remove'],
                confirmBtnText: translations['Remove']
            });
            var that = this;
            $('#confirmAlert').one('click', function () {
                that.singleId(id);
                showLoader();
                $("#remove-contract-form").ajaxSubmit({
                    method: 'post',
                    success: function (result) {
                        if (result.isSuccess) {
                            that.list.remove(function (item) {
                                return item.Id == id;
                            });
                        } else if (result.isError) {
                            alert(translations['Error']);
                        }
                    },
                    error: function () {
                        alert(translations['Error']);
                    },
                    complete: function (xhr) {
                        hideLoader();
                        hideDynamicAlertModal();
                    }
                });
            });
        };

        //subscriptions
        this.list.subscribe(function (newValue) {
            this.agreementOptions(filterAndSortList(newValue, 'AgreementType'));
            this.statusOptions(prepareStatusList(newValue));
            this.salesRepOptions(filterAndSortList(newValue, 'SalesRep'));
            this.paymentTypeOptions(filterAndSortList(newValue, 'PaymentType'));
            this.equipmentOptions(prepareEquipmentList(newValue));

            this.agreementType(localStorage.getItem(filters.agreementType) || '');
            this.status(localStorage.getItem(filters.status) || '');
            this.dateFrom(localStorage.getItem(filters.dateFrom) || '');
            this.dateTo(localStorage.getItem(filters.dateTo) || '');
            this.createdBy(localStorage.getItem(filters.createdBy) || '');
            this.salesRep(localStorage.getItem(filters.salesRep) || '');
            this.equipment(localStorage.getItem(filters.equipment) || '');
            this.typeOfPayment(localStorage.getItem(filters.typeOfPayment) || '');
            this.valueFrom(localStorage.getItem(filters.valueFrom) || '');
            this.valueTo(localStorage.getItem(filters.valueTo) || '');

            this.filterList();
        }, this);

        this.sortedList.subscribe(function (newValue) {
            this.pager.list(newValue);
        }, this);

        $('body').on('click touch', (function (e) {
            var $el = $(e.target);
            var noteId = $el.data('notes');
            var actionId = $el.data('action');

            this.list().forEach(function (item) {
                item.Id != noteId && item.showNotes(false);
                item.Id != actionId && item.showActions(false);
            });
        }).bind(this));
    };

    return MyDealsTable;
});