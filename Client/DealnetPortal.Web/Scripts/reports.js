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
		    $('<option selected value="">- not selected -</option>').prependTo($('.select-filter'));
        $('.select-filter').val($('.select-filter > option:first').val());
		});
function assignDatepicker(input) {
    inputDateFocus(input);
    input.datepicker({
        dateFormat: 'mm/dd/yy',
        changeYear: true,
        changeMonth: (viewport().width < 768) ? true : false,
        yearRange: '1900:2016',
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
            var table = $('#work-items-table')
                .DataTable({
                    data: data,
                    rowId: 'Id',
                    responsive: {
                        details: {
                            type: 'column',
                            target: 1
                        }
                    },
                    oLanguage: {
                        "sSearch": '<span class="label-caption">Search</span> <span class="icon-search"><i class="glyphicon glyphicon-search"></i></span>',
                        "oPaginate": {
                            "sNext": '<i class="glyphicon glyphicon-menu-right"></i>',
                            "sPrevious": '<i class="glyphicon glyphicon-menu-left"></i>'
                        }
                    },
                    columns: [
                    {
                        "render": function(sdata, type, row) {
                            return '<label class="custom-checkbox"><input type="checkbox"><span class="checkbox-icon"><svg aria-hidden="true" class="icon icon-checked"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-checked"></use></svg></span></label>';
                        },
                        className: 'checkbox-cell'
                    },                    
                    { "data": "TransactionId" },
                    { "data": "CustomerName" },
                    { "data": "Status" },
                    { "data": "Email" },
                    { "data": "Phone" },
                    { "data": "Date" },
                    { "data": "Equipment" },
                    { "data": "Value" },
                    {
                        "data": "RemainingDescription",
                        "visible": false
                    },
                    {
                        "data": "AgreementType",
                        "visible": false
                    },
                    {
                        "data": "PaymentType",
                        "visible": false
                    },
                    {// this is Actions Column
                        "render": function (sdata, type, row) {
                            return '<div class="contract-controls"><a href="#" class="icon-link preview-item"><svg aria-hidden="true" class="icon icon-preview"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-preview"></use></a><a href="#" class="icon-link export-item"><svg aria-hidden="true" class="icon icon-excel"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-excel"></use></a></div>';
                        },
                        className: 'controls-cell'
                    }
            ],
            columnDefs: [
                {
                    className: 'control id-cell',
                    targets: 1
                },
                {
                    className: 'customer-cell',
                    targets: 2
                },
                {
                    targets: [0, -1],
                    orderable: false
                }
            ],
            order: [[1, 'asc']],
            dom:
            "<'row'<'col-md-8''<'#table-title.dealnet-caption'>'><'col-md-4 col-sm-6'f>>" +
            "<'row'<'col-md-12''<'#expand-table-filter'>'>>" +
            "<'length-filter '<'row '<'#export-all-to-excel.col-md-3 col-sm-4 col-xs-12 col-md-push-9 col-sm-push-8'><'col-md-7 col-sm-6 col-xs-12  col-md-pull-3 col-sm-pull-4'l>>>" +
            "<'row'<'col-md-12'tr>>" +
            "<'table-footer'>" +
            "<'row'<'col-md-12'p>>" +
            "<'row'<'col-md-12'i>>",
            renderer: 'bootstrap',
            footerCallback: createTableFooter
        });

        getTotalForSelectedCheckboxes();
        createFilter();
        $(".filter-button").click(function () {
            table.draw();
        });
        $('#clear-filters').click(function () {
            $('.filter-input').val("");
            table.draw();
        });
        $('#export-excel').click(function () {
            var ids = $.map($('#work-items-table tbody tr.selected'), function (tr) {
                return $(tr)[0].id;
            });
            submitExportRequest(ids);
        });
        $('#export-all-excel').click(function () {
            var ids = $.map($('#work-items-table tbody tr'), function (tr) {
                //return $(tr).find(':nth-child(2)').text();
                return $(tr)[0].id;
            });
            submitExportRequest(ids);
        });
        $('.export-item').click(function () {
            var tr = $(this).parents('tr');
            //var id = $(tr).find(':nth-child(2)').text();
            var id = $(tr)[0].id;
            var arr = [];
            arr[0] = id;
            submitExportRequest(arr);
        });
        $('#preview-button').click(function () {
            var ids = $.map($('#work-items-table tbody tr.selected'), function (tr) {
                return $(tr)[0].id;
            });
            if (ids.length > 1) {
                submitMultiplePreviewRequest(ids);
            } else {
                submitSinglePreviewRequest(ids[0]);
            }
        });
        $('.preview-item').click(function () {
            var tr = $(this).parents('tr');                                   
            var id = $(tr)[0].id;
            submitSinglePreviewRequest(id);
        });
    });
};

$.fn.dataTable.ext.search.push(
function (settings, data, dataIndex) {
    var status = $("#deal-status").val();
    var agreementType = $("#agreement-type").val();
    var paymentType = $("#payment-type").val();
    var equipment = $("#equipment-input").val();
    var dateFrom = Date.parseExact($("#date-from").val(), "M/d/yyyy");
    var dateTo = Date.parseExact($("#date-to").val(), "M/d/yyyy");
    var date = Date.parseExact(data[6], "M/d/yyyy");
    var value = parseFloat(data[8].replace(/[\$,]/g, ''));
    var valueOfDeal = parseFloat($("#deal-value").val());
    if ((!status || status === data[3]) &&
        (!dateTo || date <= dateTo) &&
        (!dateFrom || date >= dateFrom) &&
        (!agreementType || agreementType === data[10]) &&
        (!paymentType || paymentType === data[11]) &&
        (!equipment || data[7].match(new RegExp(equipment, "i"))) &&
        (isNaN(valueOfDeal) || !isNaN(value) && value >= valueOfDeal)) {
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

    $('#table-title').html('<div class="dealnet-large-header">Reports <div class="filter-controls hidden">' + iconFilter + ' ' + iconSearch + '</div></div>');
    $('#export-all-to-excel').html('<button class="btn dealnet-button dealnet-success-button block-button" id="export-all-excel">Export All to Excel</button>');

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
    var api = this.api(), data;

    // Remove the formatting to get integer data for summation
    var intVal = function (i) {
        return typeof i === 'string' ?
        i.replace(/[\$,]/g, '') * 1 :
            typeof i === 'number' ?
            i : 0;
    };

    // Total over all pages
    total = api
        .column(8)
        .data()
        .reduce(function (a, b) {
            return intVal(a) + intVal(b);
        }, 0);

    // Total over this page
    pageTotal = api
        .column(8, { page: 'current' })
        .data()
        .reduce(function (a, b) {
            return intVal(a) + intVal(b);
        }, 0);

    // Update footer
    $('.table-footer').html($('.reports-table-footer').detach());
    $('.table-footer #reports-grand-total').html('$ ' + total.toFixed(2));
}

function getTotalForSelectedCheckboxes() {
    var selectedSum;

    $('#work-items-table tbody').on('click', ':checkbox', function () {
        var tr = $(this).parents('tr');
        tr.toggleClass('selected');
        selectedSum = $('#selectedTotal').html() !== '' ? parseFloat($('#selectedTotal').html().replace(/[$,]/g, "")) : 0;
        var val = parseFloat(tr.find(':nth-child(9)').html().replace(/[$,]/g, ""));
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