$(document)
            .ready(function () {
                showTable();
                assignDatepicker($(".date-control"));
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
            var agrTypeOptions = [];
            var salesRepOptions = [];
            $.each(data, function (i, e) {
                if ($.inArray(e["Status"], statusOptions) == -1)
                    if (e["Status"]) {
                        statusOptions.push(e["Status"]);
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
            $.each(salesRepOptions, function (i, e) {
                $("#sales-rep").append($("<option />").val(e).text(e));
            });

            var table = $('#work-items-table')
                .DataTable({
                    data: data,
                    responsive: {
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
                        { "data": "TransactionId", className: 'contract-cell' },
                        { "data": "CustomerName", className: 'customer-cell' },
                        { "data": "Status", className: 'status-cell' },
                        { "data": "AgreementType", className: 'type-cell' },
                        { "data": "Email", className: 'email-cell' },
                        { "data": "Phone", className: 'phone-cell' },
                        { "data": "Date", className: 'date-cell' },
                        { "data": "Equipment" },
                        { "data": "SalesRep", className:"sales-rep-cell" },
                        { "data": "Value" },
                        {
                            "data": "RemainingDescription",
                            "visible": false
                        },
                        {
                            // this is Actions Column
                            "render": function (sdata, type, row) {
                                if (row.Id != 0) {
                                    return '<div class="edit-control"><a href=' + editContractUrl + '/' + row.Id + ' title="' + translations['Edit'] + '"><svg aria-hidden="true" class="icon icon-edit"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-edit"></use></svg></a></div>';
                                } else {
                                    return '';
                                }
                            },
                            className: 'edit-cell',
                            orderable: false
                        },
                        {
                            "data": "Id",
                            "visible": false
                        }
                    ],

                    dom:
                        "<'row'<'col-md-8''<'#table-title.dealnet-caption'>'><'col-md-4 col-sm-6'f>>" +
                            "<'row'<'col-md-12''<'#expand-table-filter'>'>>" +
                            "<'row'<'col-md-12 col-sm-6'l>>" +
                            "<'row'<'col-md-12'tr>>" +
                            "<'row'<'col-md-12'p>>" +
                            "<'row'<'col-md-12'i>>",
                    renderer: 'bootstrap',
                    order: []
                });

            var iconFilter = '<span class="icon-filter-control"><svg aria-hidden="true" class="icon icon-filter"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-filter"></use></svg></span>';
            var iconSearch = '<span class="icon-search-control"><i class="glyphicon glyphicon-search"></i></span>';
            $('#table-title').html('<div class="dealnet-large-header">' + translations['MyWorkItems'] + ' <div class="filter-controls hidden">' + iconFilter + ' ' + iconSearch + '</div></div>');
            $('#table-title .icon-search-control').on('click', function () {
                $(this).toggleClass('active');
                $('#work-items-table_filter').slideToggle();
            });
            $('#table-title .icon-filter-control').on('click', function () {
                $(this).toggleClass('active');
                $('#expand-table-filter').slideToggle();
            });
            $('#expand-table-filter').html($('.expand-filter-template').detach());
            $('.filter-button').click(function () {
                table.draw();
            });

            table.on('draw.dt', function(){
              redrawDataTablesSvgIcons();
              resetDataTablesExpandedRows(table);
            });
            $('#clear-filters').click(function () {
                $('.filter-input').val("");
                table.search('').draw();
            });
        });
};

$.fn.dataTable.ext.search.push(
    function (settings, data, dataIndex) {
        var status = $("#deal-status").val();
        var agreementType = $("#agreement-type").val();
        var salesRep = $("#sales-rep").val();
        var dateFrom = Date.parseExact($("#date-from").val(), "M/d/yyyy");
        var dateTo = Date.parseExact($("#date-to").val(), "M/d/yyyy");
        var valueEntered = Date.parseExact(data[6], "M/d/yyyy");
        if ((!status || status === data[2]) &&
            (!agreementType || agreementType === data[3]) &&
            (!salesRep || salesRep === data[8]) &&
            (!dateTo || valueEntered <= dateTo) &&
            (!dateFrom || valueEntered >= dateFrom)) {
            return true;
        }
        return false;
    }
);