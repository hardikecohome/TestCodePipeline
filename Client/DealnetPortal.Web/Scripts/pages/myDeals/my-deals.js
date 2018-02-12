var table;

$(document)
    .ready(function () {
        showTable();
        var options = {
            yearRange: '1900:' + new Date().getFullYear(),
            minDate: new Date("1900-01-01"),
            maxDate: new Date()
        };
        var assignDatepicker = module.require('datepicker').assignDatepicker;
        $('.date-input').each(function (index, input) {
            assignDatepicker(input, options);
        });

        $('<option selected value="">- ' + translations['NotSelected'] + ' -</option>').prependTo($('.select-filter'));
        $('.select-filter').val($('.select-filter > option:first').val());

        commonDataTablesSettings();
    });

function showTable () {
    $.ajax(itemsUrl, {
        cache: false,
        mode: 'GET',
        success: function (data) {
            var statusOptions = [];
            var paymentOptions = [];
            var agrTypeOptions = [];
            var salesRepOptions = [];
            //var createdByCustomerCount = 0;//todo: Remove  in hole method if you saw this
            $.each(data, function (i, e) {
                if ($.inArray(e["LocalizedStatus"], statusOptions) == -1)
                    if (e["LocalizedStatus"]) {
                        statusOptions.push(e["LocalizedStatus"]);
                    }
                if ($.inArray(e["PaymentType"], paymentOptions) == -1)
                    if (e["PaymentType"]) {
                        paymentOptions.push(e["PaymentType"]);
                    }
                if ($.inArray(e["AgreementType"], agrTypeOptions) == -1)
                    if (e["AgreementType"]) {
                        agrTypeOptions.push(e["AgreementType"]);
                    }
                if ($.inArray(e["SalesRep"], salesRepOptions) == -1)
                    if (e["SalesRep"]) {
                        salesRepOptions.push(e["SalesRep"]);
                    }

                //if (e["IsCreatedByCustomer"] == true) {
                //    createdByCustomerCount++;
                //}
            });
            //if (createdByCustomerCount) {
            //    $('#new-deals-number').text(createdByCustomerCount);
            //    $('#new-deals-number').show();
            //}
            $.each(statusOptions.sort(function (a, b) {
                if (a === b) return 0;
                if (a > b) return 1;
                return -1;
            }), function (i, e) {
                $("#deal-status").append($("<option />").val(e).text(e));
            });
            $.each(agrTypeOptions.sort(function (a, b) {
                if (a === b) return 0;
                if (a > b) return 1;
                return -1;
            }), function (i, e) {
                $("#agreement-type").append($("<option />").val(e).text(e));
            });
            $.each(paymentOptions.sort(function (a, b) {
                if (a === b) return 0;
                if (a > b) return 1;
                return -1;
            }), function (i, e) {
                $("#payment-type").append($("<option />").val(e).text(e));
            });
            $.each(salesRepOptions.sort(function (a, b) {
                if (a === b) return 0;
                if (a > b) return 1;
                return -1;
            }), function (i, e) {
                $("#sales-rep").append($("<option />").val(e).text(e));
            });

            table = $('#work-items-table')
                .DataTable({
                    data: data,
                    rowId: 'Id',
                    responsive: {
                        details: {
                            type: 'column',
                            target: 1
                        },
                        breakpoints: [
                            { name: 'desktop-lg', width: Infinity },
                            { name: 'desktop', width: 1169 },
                            { name: 'tablet-l', width: $('body').is('.tablet-device') ? 1025 : 1023 },
                            { name: 'tablet', width: 1023 },
                            { name: 'mobile', width: 767 },
                            { name: 'mobile-l', width: 767 },
                            { name: 'mobile-p', width: 480 },
                        ]
                    },
                    oLanguage: {
                        "sSearch": '<span class="label-caption">' + translations['Search'] + '</span> <span class="icon-hold"><svg aria-hidden="true" class="icon icon-search"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-search"></use></svg></span>',
                        "oPaginate": {
                            "sNext": '<i class="glyphicon glyphicon-menu-right"></i>',
                            "sPrevious": '<i class="glyphicon glyphicon-menu-left"></i>'
                        },
                        "sLengthMenu": translations['Show'] + " _MENU_ " + translations['Entries'],
                        "sZeroRecords": translations['NoMatchingRecordsFound']
                    },
                    createdRow: function (row, data, dataIndex) {
                        var status = mapStatusToColorClass(data.Status);
                        $(row).find('.icon-status').addClass(status);

                        var signatureStatus = mapSignatureStatusToColorClass(data.SignatureStatus);
                        $(row).find('.icon-esig-hold').addClass(signatureStatus);

                        if (data.IsNewlyCreated) {
                            $(row)
                                .addClass('unread-deals')
                                .find('.contract-cell')
                                .prepend('<span class="label-new-deal">' + translations['New'] + '</span>');
                        }
                    },
                    columns: [
                        {
                            "render": function (sdata, type, row) {
                                if (row.IsInternal)
                                    return '';
                                if (row.Id != 0) {
                                    return '<label class="custom-checkbox"><input type="checkbox"><span class="checkbox-icon"><svg aria-hidden="true" class="icon icon-checked"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-checked"></use></svg></span></label>';
                                } else {
                                    return '<label class="custom-checkbox"><input type="checkbox" disabled="disabled"><span class="checkbox-icon"><svg aria-hidden="true" class="icon icon-checked"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-checked"></use></svg></span></label>';
                                }
                            },
                            className: 'checkbox-cell',
                            orderable: false
                        },
                        {
                            //"data": 'TransactionId',
                            render: function (sdate, type, row) {
                                var content = row.Id === 0 ? row.TransactionId : '<a href="' + editContractUrl + '/' + row.Id + '" title="' + translations['Edit'] + '">' + row.TransactionId + '</a>';

                                return '<div class="status-hold">' +
                                    '<div class="icon-hold"><span class="icon icon-status"></span></div>' +
                                    '<div class="text-hold"><span class="text">' +
                                    content + '</span></div></div>';
                            },
                            className: 'contract-cell'
                        },
                        { "data": "CustomerName", className: 'customer-cell' },
                        {
                            //"data": 'Status',
                            "render": function (sdata, type, row) {
                                return '<div class="status-hold">' +
                                    '<div class="icon-hold"><span class="icon icon-status"></span>' +
                                    '</div>' +
                                    '<div class="text-hold"><span class="text">' +
                                    row.LocalizedStatus + '</span></div>' +
                                    '<div class="icon-esig-hold"><svg aria-hidden="true" class="icon icon-esignature"><use xlink:href="' +
                                    urlContent +
                                    'Content/images/sprite/sprite.svg#icon-esignature"></use></svg ></div>' +
                                    '</div>';
                            },
                            className: 'status-cell'
                        },
                        { "data": "AgreementType", className: 'type-cell' },
                        { "data": "Email", className: 'email-cell' },
                        { "data": "Phone", className: 'phone-cell' },
                        { "data": "Date", className: 'date-cell' },
                        { "data": "Equipment", className: 'equipment-cell' },
                        { "data": "SalesRep", className: "sales-rep-cell" },
                        { "data": "Value", className: 'value-cell' },
                        {
                            "data": "RemainingDescription",
                            "visible": false
                        },
                        {
                            "data": "PaymentType",
                            "visible": false
                        },
                        {
                            // this is Actions Column
                            "render": function (sdata, type, row) {
                                if (row.Id != 0) {
                                    if (row.IsInternal) {
                                        return '<div class="controls-hold">' +
                                            '<a class="icon-link icon-edit" href=' +
                                            editContractUrl +
                                            '/' +
                                            row.Id +
                                            ' title="' +
                                            translations['Edit'] +
                                            '"><svg aria-hidden="true" class="icon icon-edit"><use xlink:href="' +
                                            urlContent +
                                            'Content/images/sprite/sprite.svg#icon-edit"></use></svg></a><a class="icon-link icon-remove" onclick="removeContract.call(this)" title="' +
                                            translations['Remove'] +
                                            '"><svg aria-hidden="true" class="icon icon-remove"><use xlink:href="' +
                                            urlContent +
                                            'Content/images/sprite/sprite.svg#icon-trash"></use></svg></a></div>';
                                    } else {
                                        return '<div class="controls-hold"><a class="icon-link icon-edit"  href=' + editContractUrl + '/' + row.Id + ' title="' + translations['Edit'] + '"><svg aria-hidden="true" class="icon icon-edit"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-edit"></use></svg></a>' +
                                            '<a class="icon-link export-item" onclick="exportItem.call(this);">' +
                                            '<svg aria-hidden="true" class="icon icon-excel">' +
                                            '<use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-excel"></use></svg></a>' +
                                            '<i onclick= "sendEmailModel(' + row.TransactionId + ');" class="icon-link icon-edit" > ' +
                                            '<svg aria-hidden="true" class="icon icon-edit" > <use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-help-chat"></use></svg >' +
                                            '</i></div>';
                                    }
                                } else {
                                    return '';
                                }
                            },
                            className: 'controls-cell',
                            orderable: false
                        },
                        {
                            "data": "Id",
                            "visible": false
                        },
                        {
                            "data": "IsCreatedByCustomer",
                            "visible": false
                        },
                        { "data": "LocalizedStatus", visible: false }
                    ],
                    dom:
                        "<'row'<'col-md-8''<'#table-title.dealnet-caption'>'><'col-md-4 col-sm-6'>>" +
                        "<'row'<'col-md-12''<'#expand-table-filter'>'l>>" +
                        "<'row'<'col-md-12'tr>>" +
                        "<'table-footer'>" +
                        "<'row'<'col-md-12'p>>" +
                        "<'row'<'col-md-12'i>>",
                    renderer: 'bootstrap',
                    footerCallback: createTableFooter,
                    order: [],
                    drawCallback: function (settings) {
                        var api = this.api();
                        var count = api.rows({ page: 'current' }).data().count();
                        if (count === 0) {
                            $('#export-all-excel').attr("disabled", "disabled");
                        } else {
                            $('#export-all-excel').removeAttr("disabled");
                        }
                        resizeTableStatusCells(this);

                        $('.icon-esignature').off();
                        $('.icon-esignature').on('click', getSignatureDetails);
                    }
                });

            table.on('draw.dt', function () {
                redrawDataTablesSvgIcons();
                resetDataTablesExpandedRows(table);
            });

            table.on('responsive-display', function (e, datatable, row, showHide, update) {
                var status = mapStatusToColorClass(row.data().Status);
                showHide ? $(row.child()).find('.icon-status').addClass(status) : $(row.child()).find('.icon-status').removeClass(status);

                var signatureStatus = mapSignatureStatusToColorClass(row.data().SignatureStatus);
                showHide ? $(row.child()).find('.icon-esig-hold').addClass(signatureStatus) : $(row.child()).find('.icon-status').removeClass(signatureStatus);

                showHide ? $(row.child()).find('.icon-esignature').on('click', getSignatureDetails) : $(row.child()).find('.icon-esignature').off();
            });

            $('#work-items-table th').on('click', function () {
                var el = $(this);
                var attr = el.attr('aria-sort');

                if (attr !== undefined && attr === 'descending') {
                    el.attr('def-sort', 'true');
                }

                if (attr !== undefined && attr === 'ascending') {
                    if (el.attr('def-sort') !== undefined) {
                        el.removeAttr('aria-sort')
                            .removeAttr('def-sort')
                            .removeClass('sorting_asc')
                            .addClass('sorting');

                        table.order.neutral().draw();
                    }
                }
            });

            $('#work-items-table tbody').on('click', ':checkbox', getTotalForSelectedCheckboxes(table));
            createFilter();
            recalculateGrandTotal(table);
            resizeTableStatusCells('#work-items-table');

            table.on('search.dt', function () {
                recalculateGrandTotal(table);
                recalculateTotalForSelected(table);
            });

            table.on('page.dt', function (ev, settings) {
                var rows = table.rows('tr.selected', { page: 'current' }).nodes();
                if (rows.length > 0)
                    $('#check-all').prop('checked', true);
                else
                    $('#check-all').prop('checked', false);
            });

            $('.filter-button').click(function () {
                table.draw();
            });

            $('#clear-filters').click(function () {
                $('.filter-input').val("");
                table.search('').draw();
            });

            $('#export-excel').click(function () {
                var ids = $.map(table.rows('tr.selected', { search: 'applied' }).nodes(), function (tr) {
                    return tr.id;
                });
                submitExportRequest(ids);
            });
            $('#export-all-excel').click(function () {
                var ids = $.map(table.rows('tr', { search: 'applied' }).nodes(), function (tr) {
                    return tr.id ? tr.id : null;
                });
                submitExportRequest(ids);
            });
            $('#preview-button').click(function () {
                var ids = $.map(table.rows('tr.selected', { search: 'applied' }).nodes(), function (tr) {
                    return tr.id;
                });
                if (ids.length > 1) {
                    submitMultiplePreviewRequest(ids);
                } else {
                    submitSinglePreviewRequest(ids[0]);
                }
            });

            $('#check-all').on('click', function () {
                var checked = this.checked;
                var rows = table.rows('tr', { page: 'current' }).nodes();
                $(rows).find('input[type="checkbox"]')
                    .prop('checked', !checked).click();
            });
        }
    });
};

function exportItem () {
    var tr = $(this).parents('tr');
    //var id = $(tr).find(':nth-child(2)').text();
    var id = $(tr)[0].id;
    var arr = [];
    arr[0] = id;
    submitExportRequest(arr);
};

function getSignatureDetails () {
    var tr = $(this).parents('tr');
    var id;
    if ($(tr).hasClass('child')) {
        id = $('tr.parent').attr('id');
    } else {
        id = $(tr).attr('id');
    }
    $.ajax({
        method: "GET",
        url: contractSignatureStatusUrl + '?contractId=' + id,
    }).done(function (data) {
        $('#signature-body').html(data);
        $('#contract-signature-modal').modal();
    });
}

function previewItem () {
    var tr = $(this).parents('tr');
    var id = $(tr)[0].id;
    submitSinglePreviewRequest(id);
};

$.fn.dataTable.Api.register('order.neutral()', function () {
    return this.iterator('table', function (s) {
        s.aaSorting.length = 0;
        s.aiDisplay.sort(function (a, b) {
            return a - b;
        });
        s.aiDisplayMaster.sort(function (a, b) {
            return a - b;
        });
    });
});

$.fn.dataTable.ext.search.push(
    function () {
        var statusEl = $("#deal-status");
        var agreementTypeEl = $("#agreement-type");
        var salesRepEl = $("#sales-rep");
        var createdByEl = $("#created-by");
        var dateToEl = $("#date-to");
        var dateFromEl = $("#date-from");
        var equipmentEl = $('#equipment-input');
        var dealValueFromEl = $("#deal-value-from");
        var dealValueToEl = $("#deal-value-to");
        var paymentEl = $("#payment-type");

        return function (settings, data, dataIndex) {
            var status = statusEl.val();
            var agreementType = agreementTypeEl.val();
            var salesRep = salesRepEl.val();
            var createdBy = createdByEl.val();
            var equipment = equipmentEl.val();

            var dateFrom = Date.parseExact(dateFromEl.val(), "M/d/yyyy");
            var dateTo = Date.parseExact(dateToEl.val(), "M/d/yyyy");
            var valueEntered = Date.parseExact(data[7], "M/d/yyyy");

            var value = parseFloat(data[10].replace(/[\$,]/g, ''));
            var valueOfDealFrom = parseFloat(dealValueFromEl.val());
            var valueOfDealTo = parseFloat(dealValueToEl.val());
            var paymentType = paymentEl.val();

            // check dropdown status against LocalizedStatus
            if ((!status || status === data[16]) &&
                (!agreementType || agreementType === data[4]) &&
                (!salesRep || salesRep === data[9]) &&
                (!dateTo || valueEntered <= dateTo) &&
                (!dateFrom || valueEntered >= dateFrom) &&
                (!paymentType || paymentType === data[12]) &&
                (!equipment || data[8].match(new RegExp(equipment, "i"))) &&
                (isNaN(valueOfDealFrom) || !isNaN(value) && value >= valueOfDealFrom) &&
                (isNaN(valueOfDealTo) || !isNaN(value) && value <= valueOfDealTo) &&
                (createdBy === '' || createdBy == data[15])) {
                return true;
            }
            return false;
        }
    }());

function submitExportRequest (ids) {
    $("#export-ids").empty();
    $.each(ids, function (index, item) {
        $("#export-ids").append($('<input>', {
            'name': 'ids',
            'value': item,
            'type': 'hidden'
        }));
    });
    $("#export-form").submit();
}

function submitSinglePreviewRequest (id) {
    window.location.href = contractPreviewUrl + id;
}

function submitMultiplePreviewRequest (ids) {
    $("#contract-preview-ids").empty();
    $.each(ids, function (index, item) {
        $("#contract-preview-ids").append($('<input>', {
            'name': 'ids',
            'value': item,
            'type': 'hidden'
        }));
    });
    $("#multiple-preview-form").submit();
}

function createFilter () {
    var iconFilter = '<span class="icon-filter-control"><svg aria-hidden="true" class="icon icon-filter"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-filter"></use></svg></span>';
    var iconSearch = '<span class="icon-search-control"><svg aria-hidden="true" class="icon icon-search"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-search"></use></svg></span>';

    $('#table-title').html('<div class="dealnet-large-header">' + translations['MyWorkItems'] + ' <span id="export-all-to-excel"></span> <span class="filter-controls hidden">' + iconFilter + ' ' + iconSearch + '</span></div>');
    $('#export-all-to-excel').html('<button class="btn dealnet-button dealnet-link-button" id="export-all-excel">' + translations['ExportAllToExcel'] + '</button>');

    $('#table-title .icon-search-control').on('click', function () {
        $(this).toggleClass('active');
        $('#work-items-table_filter').slideToggle();
    });
    $('#table-title .icon-filter-control').on('click', function () {
        $(this).toggleClass('active');
        $('#expand-table-filter').slideToggle();
    });
    $('#expand-table-filter').html($('.expand-filter-template').detach());
    $('.table-length-filter').html($('#work-items-table_length').detach());
}

function createTableFooter (row, data, start, end, display) {
    $('.table-footer').html($('.reports-table-footer').detach());
}

function getIntValue (value) {
    return typeof value === 'string' ?
        value.replace(/[\$,]/g, '') * 1 :
        typeof value === 'number' ?
            value : 0;
}

function recalculateGrandTotal (table) {
    var sum = table.column(10, { search: 'applied' }).data().reduce(function (acc, value) {
        return acc + getIntValue(value);
    }, 0);

    $('.table-footer #reports-grand-total').html('$ ' + sum.toFixed(2));
    return sum;
}

function recalculateTotalForSelected (table) {
    var data = table.rows('tr.selected', { search: 'applied' }).data();
    var sum = data.reduce(function (acc, value) {
        return acc + getIntValue(value.Value);
    }, 0);
    $('#selectedTotal').html('$ ' + sum.toFixed(2));
    if (data.length) {
        $('.reports-table-footer').addClass('has-selected-items');
    } else {
        $('.reports-table-footer').removeClass('has-selected-items');
    }
}

function getTotalForSelectedCheckboxes (table) {
    return function (ev) {
        var tr = $(ev.target).parents('tr');
        if (ev.target.checked) {
            tr.addClass('selected');
        } else {
            tr.removeClass('selected');
        }
        recalculateSelectedTotals(table);
    }
}

function recalculateSelectedTotals (table) {
    var sel = table.rows('tr.selected', { search: 'applied' }).data();
    var sum = sel.reduce(function (sum, item) {
        return sum + getIntValue(item.Value);
    }, 0.0);
    $('#selectedTotal').html('$ ' + sum.toFixed(2));
    if (sel.length > 0) {
        $('.reports-table-footer').addClass('has-selected-items');
    } else {
        $('.reports-table-footer').removeClass('has-selected-items');
    }
}

function removeContract () {
    var tr = $(this).parents('tr');
    var id = $(tr)[0].id;
    var data = {
        message: translations['AreYouSureYouWantToRemoveThisApplication'] + '?',
        title: translations['Remove'],
        confirmBtnText: translations['Remove']
    };
    module.require('alertModal').dynamicAlertModal(data);

    $('#confirmAlert').on('click', function () {
        $("#remove-contract").val(id);
        module.require('loader').showLoader();
        $("#remove-contract-form").ajaxSubmit({
            method: 'post',
            success: function (result) {
                if (result.isSuccess) {
                    table.row(tr).remove().draw(false);
                } else if (result.isError) {
                    alert(translations['Error']);
                }
            },
            error: function () {
                alert(translations['Error']);
            },
            complete: function (xhr) {
                module.require('loader').hideLoader();
                module.require('alertModal').hideDynamicAlertModal();
            }
        });
    });
}
