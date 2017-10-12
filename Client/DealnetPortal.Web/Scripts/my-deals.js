var table;

$(document)
    .ready(function () {
        showTable();
        var options = {
            yearRange: '1900:' + new Date().getFullYear(),
            minDate: new Date("1900-01-01"),
            maxDate: new Date()
        }
        $('.date-input').each(function (index, input) {
            assignDatepicker(input, options);
        });

        $('<option selected value="">- ' + translations['NotSelected'] + ' -</option>').prependTo($('.select-filter'));
        $('.select-filter').val($('.select-filter > option:first').val());
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
                if ($.inArray(e["Status"], statusOptions) == -1)
                    if (e["Status"]) {
                        statusOptions.push(e["Status"]);
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
            $.each(statusOptions, function (i, e) {
                $("#deal-status").append($("<option />").val(e).text(e));
            });
            $.each(agrTypeOptions, function (i, e) {
                $("#agreement-type").append($("<option />").val(e).text(e));
            });
            $.each(paymentOptions, function (i, e) {
                $("#payment-type").append($("<option />").val(e).text(e));
            });
            $.each(salesRepOptions, function (i, e) {
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
                        if (data.IsNewlyCreated) {
                            $(row).addClass('unread-deals').find('.contract-cell').prepend('<span class="label-new-deal">' + translations['New'] + '</span>');
                        }
                    },
                    columns: [
                        {
                            "render": function (sdata, type, row) {
                                if (row.Id != 0 && !row.IsInternal) {
                                    return '<label class="custom-checkbox"><input type="checkbox"><span class="checkbox-icon"><svg aria-hidden="true" class="icon icon-checked"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-checked"></use></svg></span></label>';
                                } else {
                                    return '<label class="custom-checkbox"><input type="checkbox" disabled="disabled"><span class="checkbox-icon"><svg aria-hidden="true" class="icon icon-checked"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-checked"></use></svg></span></label>';
                                }
                            },
                            className: 'checkbox-cell',
                            orderable: false
                        },
                        { "data": "TransactionId", className: 'contract-cell' },
                        { "data": "CustomerName", className: 'customer-cell' },
                        {
                            //"data": 'Status',
                            "render": function (sdata, type, row) {
                                var status = 'icon-' + row.Status.trim().toLowerCase().replace(/\s/g, '-').replace(/()/g, '').replace(/\//g, '');
                                return '<div class="status-hold">' +
                                    '<span class="icon-hold"><span class="icon icon-status ' + status + '"></span>' +
                                    '</span>' +
                                    '<div class="status-text-hold"><span class="status-text">' +
                                    row.Status + '</span></div></div>';
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
                                        return '<div class="controls-hold"><a class="icon-link icon-edit"  href=' + editContractUrl + '/' + row.Id + ' title="' + translations['Edit'] + '"><svg aria-hidden="true" class="icon icon-edit"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-edit"></use></svg></a></div>';
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
                        }
                    ],
                    dom:
                    "<'row'<'col-md-8''<'#table-title.dealnet-caption'>'><'col-md-4 col-sm-6'f>>" +
                    "<'row'<'col-md-12''<'#expand-table-filter'>'>>" +
                    "<'length-filter '<'row '<'#export-all-to-excel.col-md-3 col-sm-4 col-xs-12 col-md-push-9 col-sm-push-8'><'col-md-7 col-sm-6 col-xs-12  col-md-pull-3 col-sm-pull-4'l>>>" +
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
                    }
                });

            table.on('draw.dt', function () {
                redrawDataTablesSvgIcons();
                resetDataTablesExpandedRows(table);
            });

            getTotalForSelectedCheckboxes();
            createFilter();
            recalculateGrandTotal();

            table.on('search.dt', function () {
                recalculateGrandTotal();
                recalculateTotalForSelected();
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
        }
    });
};

$.fn.dataTable.ext.search.push(
    function () {
        var statusEl = $("#deal-status");
        var agreementTypeEl = $("#agreement-type");
        var salesRepEl = $("#sales-rep");
        var createdByEl = $("#created-by");
        var dateToEl = $("#date-to");
        var dateFromEl = $("#date-from");
        return function (settings, data, dataIndex) {
            var status = statusEl.val();
            var agreementType = agreementTypeEl.val();
            var salesRep = salesRepEl.val();
            var createdBy = createdByEl.val();

            var dateFrom = Date.parseExact(dateFromEl.val(), "M/d/yyyy");
            var dateTo = Date.parseExact(dateToEl.val(), "M/d/yyyy");
            var valueEntered = Date.parseExact(data[7], "M/d/yyyy");

            if ((!status || status === data[3]) &&
                (!agreementType || agreementType === data[4]) &&
                (!salesRep || salesRep === data[9]) &&
                (!dateTo || valueEntered <= dateTo) &&
                (!dateFrom || valueEntered >= dateFrom) &&
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

    $('#table-title').html('<div class="dealnet-large-header">' + translations['MyWorkItems'] + ' <div class="filter-controls hidden">' + iconFilter + ' ' + iconSearch + '</div></div>');
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

function recalculateGrandTotal () {
    var sum = table.column(10, { search: 'applied' }).data().reduce(function (acc, value) {
        return acc + getIntValue(value);
    }, 0);

    $('.table-footer #reports-grand-total').html('$ ' + sum.toFixed(2));
    return sum;
}

function recalculateTotalForSelected () {
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

function getTotalForSelectedCheckboxes () {
    var selectedSum;

    $('#work-items-table tbody').on('click', ':checkbox', function () {
        var tr = $(this).parents('tr');
        tr.toggleClass('selected');
        selectedSum = $('#selectedTotal').html() !== '' ? parseFloat($('#selectedTotal').html().replace(/[$,]/g, "")) : 0;
        var val = parseFloat(tr.find(':nth-child(11)').html().replace(/[$,]/g, ""));
        if (isNaN(val)) { val = 0; }
        var isSelected = tr.is(".selected");
        selectedSum = isSelected ? selectedSum + val : selectedSum - val;

        $('#selectedTotal').html('$ ' + selectedSum.toFixed(2));
        if (selectedSum !== 0 || isSelected) {
            $('.reports-table-footer').addClass('has-selected-items');
        } else {
            $('.reports-table-footer').removeClass('has-selected-items');
        }
    });
}

function removeContract () {
    var tr = $(this).parents('tr');
    var id = $(tr)[0].id;
    var data = {
        message: translations['AreYouSureYouWantToRemoveThisApplication'] + '?',
        title: translations['Remove'],
        confirmBtnText: translations['Remove']
    };
    dynamicAlertModal(data);

    $('#confirmAlert').on('click', function () {
        $("#remove-contract").val(id);
        showLoader();
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
                hideLoader();
                hideDynamicAlertModal();
            }
        });
    });
}
