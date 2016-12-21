$(document)
            .ready(function () {
                showTable();
                assignDatepicker($(".date-control"));
                $('#deal-status option').each(function () {
                    $(this).val($(this).text());
                });
                $('<option selected value="">- ' + translations['NotSelected'] + ' -</option>').prependTo($('#deal-status'));
                $('#deal-status').val($('#deal-status > option:first').val());
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
            $.each(data, function (i, e) {
                if ($.inArray(e["Status"], statusOptions) == -1)
                    if (e["Status"]) {
                        statusOptions.push(e["Status"]);
                    }
            });
            $.each(statusOptions, function (i, e) {
                $("#deal-status").append($("<option />").val(e).text(e));
            });

            var table = $('#work-items-table')
                .DataTable({
                    data: data,
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
                            // this is Actions Column
                            "render": function (sdata, type, row) {
                                if (row.Id != 0) {
                                    return '<div class="edit-control"><a href=' + editContractUrl + '/' + row.Id + ' title="' + translations['Edit'] + '"><svg aria-hidden="true" class="icon icon-edit"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-edit"></use></svg></a></div>';
                                } else {
                                    return '';
                                }
                            }
                        },
                        {
                            "data": "Id",
                            "visible": false
                        }
                    ],
                    columnDefs: [
                      { targets  : [-1], orderable: false},
                      { className: 'customer-cell', targets: 1},
                      { className: 'id-cell', targets: 9},
                      { className: 'edit-cell', targets: -1}
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
            $("#filter-button").click(function () {
                table.draw();
            });

            table.on('draw.dt', function(){
              redrawDataTablesSvgIcons();
              resetDataTablesExpandedRows(table);
            });
        });
};

$.fn.dataTable.ext.search.push(
    function (settings, data, dataIndex) {
        var status = $("#deal-status").val();
        var dateFrom = Date.parseExact($("#date-from").val(), "M/d/yyyy");
        var dateTo = Date.parseExact($("#date-to").val(), "M/d/yyyy");
        var valueEntered = Date.parseExact(data[5], "M/d/yyyy");
        if ((!status || status === data[2]) && (!dateTo || valueEntered <= dateTo) && (!dateFrom || valueEntered >= dateFrom)) {
            return true;
        }
        return false;
    }
);