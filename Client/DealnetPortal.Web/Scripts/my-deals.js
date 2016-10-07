$(document)
            .ready(function () {
                showTable();
                assignDatepicker($(".date-control"));
                $('#deal-status option').each(function () {
                    $(this).val($(this).text());
                });
                $('<option selected value="">- not selected -</option>').prependTo($('#deal-status'));
            });

function assignDatepicker(input) {
    input.datepicker({
        dateFormat: 'mm/dd/yy',
        changeYear: true,
        yearRange: '1900:2016',
        minDate: Date.parse("1900-01-01"),
        maxDate: new Date()
    });
}

function showTable() {
    $.when($.ajax(itemsUrl, { mode: 'GET' }))
        .done(function (data) {
            var table = $('#work-items-table')
                .DataTable({
                    data: data,
                    oLanguage: {
                        "sSearch": '<span class="label-caption">Search</span> <span class="icon-search"><i class="glyphicon glyphicon-search"></i></span>',
                        "oPaginate": {
                            "sNext": '<i class="glyphicon glyphicon-menu-right"></i>',
                            "sPrevious": '<i class="glyphicon glyphicon-menu-left"></i>'
                        }
                    },
                    columns: [
                        { "data": "Id" },
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
                                return '<a href="#" title="Edit"><svg aria-hidden="true" class="icon icon-edit"><use xlink:href="/client/Content/images/sprite/sprite.svg#icon-edit"></use></svg></a>';
                            }
                        }
                    ],
                    dom:
                        "<'row'<'col-md-8''<'#table-title.dealnet-caption'>'><'col-md-4 col-sm-6'f>>" +
                            "<'row'<'col-md-12''<'#expand-table-filter'>'>>" +
                            "<'row'<'col-md-12'l>>" +
                            "<'row'<'col-md-12'tr>>" +
                            "<'row'<'col-md-12'p>>" +
                            "<'row'<'col-md-12'i>>",
                    renderer: 'bootstrap',
                    "fnInitComplete": function(oSettings, json) {
                      customizeSelect();
                    }
                });

            var iconFilter = '<span class="icon-filter-control"><svg aria-hidden="true" class="icon icon-filter"><use xlink:href="/client/Content/images/sprite/sprite.svg#icon-filter"></use></svg></span>';
            var iconSearch = '<span class="icon-search-control"><i class="glyphicon glyphicon-search"></i></span>';
            $('#table-title').html('<div class="dealnet-large-header">My Work Items <div class="filter-controls hidden">' + iconFilter + ' ' + iconSearch + '</div></div>');
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