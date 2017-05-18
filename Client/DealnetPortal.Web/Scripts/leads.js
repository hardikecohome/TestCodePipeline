$(document).ready(function () {
    showTable();
    assignDatepicker($(".date-control"));
    $('.select-filter option').each(function () {
        $(this).val($(this).text());
    });
    $('<option selected value="">- ' + translations['NotSelected'] + ' -</option>').prependTo($('.select-filter'));
    $('.select-filter').val($('.select-filter > option:first').val());
});

function showTable() {
    $.when($.ajax(itemsUrl, { cache: false, mode: 'GET' }))
        .done(function (data) {
            var postalCodeOptions = [];
            var preApprovedOptions = [];
            $.each(data, function (i, e) {
                if ($.inArray(e["PostalCode"], postalCodeOptions) == -1)
                    if (e["PostalCode"]) {
                        postalCodeOptions.push(e["PostalCode"]);
                    }
            });
            $.each(data, function (i, e) {
                if ($.inArray(e["PreApprovalAmount"], preApprovedOptions) == -1)
                    if (e["PreApprovalAmount"]) {
                        preApprovedOptions.push(e["PreApprovalAmount"]);
                    }
            });
            $.each(postalCodeOptions, function (i, e) {
                $("#postal-code").append($("<option />").val(e).text(e));
            });
            $.each(preApprovedOptions, function (i, e) {
                $("#pre-approved-for").append($("<option />").val(e).text(e));
            });

            var table = $('#work-items-table')
                .DataTable({
                    data: data,
                    responsive: {
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
                        "sSearch": '<span class="label-caption">' + translations['Search'] + '</span> <span class="icon-search"><i class="glyphicon glyphicon-search"></i></span>',
                        "oPaginate": {
                            "sNext": '<i class="glyphicon glyphicon-menu-right"></i>',
                            "sPrevious": '<i class="glyphicon glyphicon-menu-left"></i>'
                        },
                        "sLengthMenu": translations['Show'] + " _MENU_ " + translations['Entries'],
                        "sZeroRecords": translations['NoMatchingRecordsFound']
                    },
                    columns: [
					    { "data": "Date", className: 'date-cell expanded-cell' },
					    { "data": "PostalCode", className: 'code-cell' },
					    { "data": "PreApprovalAmount", className: 'preapproved-cell' },
					    { "data": "Equipment", className: 'equipment-cell' },
					    { "data": "CustomerComment", className: 'customer-cell' },
					    {// this is Actions Column
					        "render": function (sdata, type, row) {
                                return '<div class="contract-controls text-center"><a class="link-accepted-link" data-container="body" data-toggle="popover" data-trigger="hover" data-content="$50.00 fee will be applied to your account" onclick="addLead(' + row.Id + ', '+ row.TransactionId + ')"><svg aria-hidden="true" class="icon icon-accept-lead"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-accept-lead"></use></svg></a></div>';
					        },
					        className: 'controls-cell accept-cell',
					        orderable: false
					    },
                    ],
                    dom:
				        "<'row'<'col-md-8''<'#table-title.dealnet-caption'>'><'col-md-4 col-sm-6'f>>" +
				        "<'row'<'col-md-12''<'#expand-table-filter'>'>>" +
				        "<'row'<'col-md-12 col-sm-6'l>>" +
				        "<'row'<'col-md-12''<'#section-before-table'>'>>" +
				        "<'row'<'col-md-12'tr>>" +
				        "<'row'<'col-md-12'p>>" +
				        "<'row'<'col-md-12'i>>",
                    renderer: 'bootstrap',
                    order: []
                });

            var iconFilter = '<span class="icon-filter-control"><svg aria-hidden="true" class="icon icon-filter"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-filter"></use></svg></span>';
            var iconSearch = '<span class="icon-search-control"><i class="glyphicon glyphicon-search"></i></span>';
            $('#table-title').html('<div class="dealnet-large-header">' + translations['Leads'] + '<div class="filter-controls hidden">' + iconFilter + ' ' + iconSearch + '</div></div>');
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
            table.on('draw.dt', function () {
                redrawDataTablesSvgIcons();
                resetDataTablesExpandedRows(table);
            });
            $('#clear-filters').click(function () {
                $('.filter-input').val("");
                table.search('').draw();
            });
          $('.dataTables_filter input[type="search"]').attr('placeholder','Requested service, customer comment');
          $('.link-accepted-link').popover({
            placement : 'left',
            template: '<div class="popover customer-popover accepted-leads-popover" role="tooltip"><div class="popover-inner"><div class="popover-container"><span class="popover-icon"><svg aria-hidden="true" class="icon icon-tooltip-info"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-tooltip-info"></use></svg></span><div class="popover-content text-center"></div></div></div></div>',
          });
        });
};

$.fn.dataTable.ext.search.push(
    function (settings, data, dataIndex) {
        var postalCode = $("#postal-code").val();
        var preApprovedFor = $("#pre-approved-for").val();
        var dateFrom = Date.parseExact($("#date-from").val(), "M/d/yyyy");
        var dateTo = Date.parseExact($("#date-to").val(), "M/d/yyyy");
        var date = Date.parseExact(data[0], "M/d/yyyy");
        if ((!postalCode || postalCode === data[1]) &&
            (!preApprovedFor || preApprovedFor === data[2]) &&
            (!dateTo || date <= dateTo) &&
            (!dateFrom || date >= dateFrom)) {
            return true;
        }
        return false;
    }
);

function assignDatepicker(input) {
    inputDateFocus(input);
    input.datepicker({
        dateFormat: 'mm/dd/yy',
        changeYear: true,
        changeMonth: (viewport().width < 768) ? true : false,
        yearRange: '1900:' + new Date().getFullYear(),
        minDate: Date.parse("1900-01-01"),
        maxDate: new Date(),
        onClose: function () {
            onDateSelect($(this));
        }
    });
}

function addLead(id, transactionId) {
    var data = {
        message: translations['YouSureYouWantToAcceptLeadThenYouPay'],
        title: translations['AcceptLead'],
        confirmBtnText: translations['AcceptLead']
    };
    dynamicAlertModal(data);
    $('#confirmAlert').on('click', function () {
        var replacedText = $('#lead-msg').html().replace('{1}', transactionId);
        $('#lead-msg').html(replacedText);
        showLoader();
        $.post({
            type: "POST",
            url: 'leads/acceptLead?id=' + id,
            success: function(json) {
                if (json.isError) {
                    hideLoader();
                    $('.success-message').hide();
                    alert(translations['ErrorWhileUpdatingData']);
                } else if (json.isSuccess) {
                    hideLoader();
                    $('#section-before-table').append($('#msg-lead-accepted'));
                    $('#section-before-table #msg-lead-accepted').show();
                    window.location.href = redirectUrl;
                }

                $('.modal').modal('hide');
            }

        });
    });
}