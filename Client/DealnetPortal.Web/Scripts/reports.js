var table;
$(document)
		.ready(function () {
		    showTable();
		    assignDatepicker($(".date-control"));
		    setTimeout(function () {
		        $(window).trigger('resize');
		    }, 300);
		    $('.select-filter option').each(function () {
		        $(this).val($(this).text());
		    });
		    $('<option selected value="">- ' + translations['NotSelected'] + ' -</option>').prependTo($('.select-filter'));
            $('.select-filter').val($('.select-filter > option:first').val());
		});
function assignDatepicker(input) {
    inputDateFocus(input);
    input.datepicker({
        dateFormat: 'mm/dd/yy',
        changeYear: true,
        changeMonth: (viewport().width < 768) ? true : false,
        yearRange: '1900:' + new Date().getFullYear(),
        minDate: Date.parse("1900-01-01"),
        maxDate: new Date(),
        onClose: function(){
          onDateSelect($(this));
        }
    });
}

function showTable() {
    $.when($.ajax(itemsUrl, { mode: 'GET' }))
    .done(function (data) {
        var statusOptions = [];
        var paymentOptions = [];
        var agrTypeOptions = [];
        var salesRepOptions = [];
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
        });
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
                            { name: 'desktop',  width: 1169 },
                            { name: 'tablet-l',  width: $('body').is('.tablet-device') ? 1025 : 1023 },
                            { name: 'tablet',  width: 1023 },
                            { name: 'mobile',   width: 767 },
                            { name: 'mobile-l',   width: 767 },
                            { name: 'mobile-p',   width: 480 },
                        ]
                    },
                    oLanguage: {
                        "sSearch": '<span class="label-caption">' + translations['Search'] + '</span> <span class="icon-search"><i class="glyphicon glyphicon-search"></i></span>',
                        "oPaginate": {
                            "sNext": '<i class="glyphicon glyphicon-menu-right"></i>',
                            "sPrevious": '<i class="glyphicon glyphicon-menu-left"></i>'
                        },
                        "sLengthMenu": translations['Show'] + " _MENU_ " + translations['Entries'],
                        "sZeroRecords": translations['NoMatchingRecordsFound']
                    },
                    columns: [
                    {
                        "render": function (sdata, type, row) {
                            if (row.Id != 0) {
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
                    { "data": "Status", className: 'status-cell' },
                    { "data": "AgreementType", className: 'type-cell'},
                    { "data": "Email", className: 'email-cell' },
                    { "data": "Phone", className: 'phone-cell' },
                    { "data": "Date", className: 'date-cell' },
                    { "data": "Equipment", className: 'equipment-cell' },
                    { "data": "SalesRep", className: 'sales-rep-cell' },
                    { "data": "Value", className: 'value-cell' },
                    {
                        "data": "RemainingDescription",
                        "visible": false
                    },
                    {
                        "data": "PaymentType",
                        "visible": false
                    },
                    {// this is Actions Column
                        "render": function (sdata, type, row) {
                            if (row.Id != 0) {
                                return '<div class="contract-controls"><a class="icon-link preview-item" onclick="previewItem.call(this);"><svg aria-hidden="true" class="icon icon-preview"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-preview"></use></a><a class="icon-link export-item" onclick="exportItem.call(this);"><svg aria-hidden="true" class="icon icon-excel"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-excel"></use></a></div>';
                            } else {
                                return '';
                            }
                        },
                        className: 'controls-cell',
                        orderable: false
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
            order: []
        });

        table.on('draw.dt', function(){
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
        $(".filter-button").click(function () {
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
                //if (tr.id != '') {
                //    return tr.id;
                //} else {
                //    return "t" + $(tr).find(':nth-child(2)').text();
                //}
                //return $(tr).find(':nth-child(1)').text();
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
    });
};

function exportItem() {
    var tr = $(this).parents('tr');
    //var id = $(tr).find(':nth-child(2)').text();
    var id = $(tr)[0].id;
    var arr = [];
    arr[0] = id;
    submitExportRequest(arr);
};


function previewItem() {
    var tr = $(this).parents('tr');
    var id = $(tr)[0].id;
    submitSinglePreviewRequest(id);
};

$.fn.dataTable.ext.search.push(
function (settings, data, dataIndex) {
    var status = $("#deal-status").val();
    var agreementType = $("#agreement-type").val();
    var paymentType = $("#payment-type").val();
    var salesRep = $("#sales-rep").val();
    var equipment = $("#equipment-input").val();
    var dateFrom = Date.parseExact($("#date-from").val(), "M/d/yyyy");
    var dateTo = Date.parseExact($("#date-to").val(), "M/d/yyyy");
    var date = Date.parseExact(data[7], "M/d/yyyy");
    var value = parseFloat(data[10].replace(/[\$,]/g, ''));
    var valueOfDealFrom = parseFloat($("#deal-value-from").val());
    var valueOfDealTo = parseFloat($("#deal-value-to").val());
    if ((!status || status === data[3]) &&
        (!dateTo || date <= dateTo) &&
        (!dateFrom || date >= dateFrom) &&
        (!agreementType || agreementType === data[4]) &&
        (!salesRep || salesRep === data[9]) &&
        (!paymentType || paymentType === data[12]) &&
        (!equipment || data[8].match(new RegExp(equipment, "i"))) &&
        (isNaN(valueOfDealFrom) || !isNaN(value) && value >= valueOfDealFrom) &&
        (isNaN(valueOfDealTo) || !isNaN(value) && value <= valueOfDealTo)) {
        return true;
    }
    return false;
});

function submitExportRequest(ids) {
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

function submitSinglePreviewRequest(id) {
    window.location.href = contractPreviewUrl + id;
}

function submitMultiplePreviewRequest(ids) {
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

function createFilter() {
    var iconFilter = '<span class="icon-filter-control"><svg aria-hidden="true" class="icon icon-filter"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-filter"></use></svg></span>';
    var iconSearch = '<span class="icon-search-control"><i class="glyphicon glyphicon-search"></i></span>';

    $('#table-title').html('<div class="dealnet-large-header">' + translations['Reports'] + ' <div class="filter-controls hidden">' + iconFilter + ' ' + iconSearch + '</div></div>');
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

function createTableFooter(row, data, start, end, display) {    
    $('.table-footer').html($('.reports-table-footer').detach());
}

function getIntValue(value) {
    return typeof value === 'string' ?
        value.replace(/[\$,]/g, '') * 1 :
            typeof value === 'number' ?
        value : 0;
}

function recalculateGrandTotal() {
    var sum = 0;
    table.column(10, { search: 'applied' }).data().each(function (value, index) {
        sum += getIntValue(value);
    });

    $('.table-footer #reports-grand-total').html('$ ' + sum.toFixed(2));
    return sum;
}

function recalculateTotalForSelected() {
    var data = table.rows('tr.selected', { search: 'applied' }).data();
    var sum = 0;
    data.each(function (value, index) {
        sum += getIntValue(value.Value);
    });
    $('#selectedTotal').html('$ ' + sum.toFixed(2));
    if (data.length) {
        $('.reports-table-footer').addClass('has-selected-items');
    } else {
        $('.reports-table-footer').removeClass('has-selected-items');
    }
}

function getTotalForSelectedCheckboxes() {
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