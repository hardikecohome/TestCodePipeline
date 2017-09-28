$(document).ready(function () {
    showTable();
    assignDatepicker('.date-control', {
        yearRange: '1900:' + new Date().getFullYear(),
        minDate: new Date("1900-01-01"),
        maxDate: new Date()
    });
    $('.select-filter option').each(function () {
        $(this).val($(this).text());
    });
    $('<option selected value="">- ' + translations['NotSelected'] + ' -</option>').prependTo($('.select-filter'));
    $('.select-filter').val($('.select-filter > option:first').val());
});

function showTable () {
    var table;
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

            table = $('#work-items-table')
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
                        "sSearch": '<span class="label-caption">' + translations['Search'] + '</span> <span class="icon-hold"><svg aria-hidden="true" class="icon icon-search"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-search"></use></svg></span>',
                        "oPaginate": {
                            "sNext": '<i class="glyphicon glyphicon-menu-right"></i>',
                            "sPrevious": '<i class="glyphicon glyphicon-menu-left"></i>'
                        },
                        "sLengthMenu": translations['Show'] + " _MENU_ " + translations['Entries'],
                        "sZeroRecords": translations['NoMatchingRecordsFound'],
                        "sEmptyTable": isCompletedProfile ? translations['NoMatchingRecordsFound'] : translations['LeadsNoMatchingRecordsFound']
                    },
                    columns: [
                        { "data": "Date", className: 'date-cell expanded-cell' },
                        { "data": "PostalCode", className: 'code-cell' },
                        { "data": "PreApprovalAmount", className: 'preapproved-cell' },
                        { "data": "Equipment", className: 'equipment-cell' },
                        { "data": "CustomerComment", className: 'customer-cell' },
                        {// this is Actions Column
                            "render": function (sdata, type, row) {
                                return '<div class="contract-controls text-center"><a class="link-accepted" data-container="body" data-toggle="popover" data-trigger="hover" data-content="' + translations['PreApprovedLoanValueFeeWillBeApplied'] + '" id = "lead' + row.Id + '"  onclick="addLead(' + row.Id + ', ' + row.TransactionId + ')"><svg aria-hidden="true" class="icon icon-accept-lead"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-accept-lead"></use></svg></a></div>';
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
            var iconSearch = '<span class="icon-search-control"><svg aria-hidden="true" class="icon icon-search"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-search"></use></svg></span>';
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

            $('#clear-filters').on('click', clearFilters);
            $('#clear-filters-mobile').on('click', clearFilters);

            $('.dataTables_filter input[type="search"]').attr('placeholder', 'Requested service, customer comment');
            if ($('body').not('.tablet-device').not('.mobile-device').length > 0) {
                $('.link-accepted').popover({
                    placement: 'left',
                    template: '<div class="popover customer-popover accepted-leads-popover" role="tooltip"><div class="popover-inner"><div class="popover-container"><span class="popover-icon"><svg aria-hidden="true" class="icon icon-tooltip-info"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-tooltip-info"></use></svg></span><div class="popover-content text-center"></div></div></div></div>',
                });
            }
        });

    function clearFilters () {
        $('.filter-input').val("");
        table.search('').draw(false);
    }

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

function removeLead (id) {
    var table = $('#work-items-table').DataTable();
    var rowLead = $("#lead" + id).closest('tr');
    table.row(rowLead)
        .remove()
        .draw(false);
};

function addLead (id, transactionId) {
    $('.link-accepted').popover('hide');
    var data = {
        message: "<div class=\"modal-leads-content\"><div>" + translations['AreYouSure'] + "</div><div>" + translations['AcceptanceOfLeadFeeAppliedToYourAccount'] + "</div></div>",
        title: translations['AcceptLead'],
        confirmBtnText: translations['AcceptLead'],
        class: "modal-leads"
    };
    dynamicAlertModal(data);

    $('#confirmAlert').on('click', function () {
        showLoader();
        $.post({
            type: "POST",
            url: 'leads/acceptLead?id=' + id,
            success: function (json) {
                if (json.isError) {
                    hideLoader();
                    $('.success-message').hide();
                    $("#error-msg-text").html(json.Errors);
                    $('#section-before-table').append($('#leads-error-message'));
                    $('#section-before-table #leads-error-message').show();
                } else if (json.isSuccess) {
                    hideLoader();
                    var template = $('#success-message-template').html();
                    $('#lead-msg').html(template.replace('{1}', transactionId));
                    $('#leads-error-message').hide();
                    $('#section-before-table').append($('#msg-lead-accepted'));
                    $('#section-before-table #msg-lead-accepted').show();
                }
                removeLead(id);

                $('.modal').modal('hide');
            }

        });
    });
}