﻿var table;

$(document)
    .ready(function () {
        showTable();
        commonDataTablesSettings();
    });

function removeContract() {
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

};

function getSignatureDetails() {
    var tr = $(this).parents('tr');
    var id;
    if ($(tr).hasClass('child')) {
        id = $('tr.parent').attr('id');
    } else {
        id = $(tr).attr('id');
    }
    $.ajax({
        method: "GET",
        cache: false,
        url: contractSignatureStatusUrl + '?contractId=' + id,
    }).done(function (data) {
        $('#signature-body').html(data);
        $('#contract-signature-modal').modal();
    });
}

function showTable() {
    $.when($.ajax(itemsUrl, {
            cache: false,
            mode: 'GET'
        }))
        .done(function (data) {
            table = $('#work-items-table')
                .DataTable({
                    responsive: {
                        details: {
                            display: $.fn.dataTable.Responsive.display.childRow
                        },
                        breakpoints: [{
                                name: 'desktop-lg',
                                width: Infinity
                            },
                            {
                                name: 'desktop',
                                width: 1169
                            },
                            {
                                name: 'tablet-l',
                                width: $('body').is('.tablet-device') ? 1025 : 1023
                            },
                            {
                                name: 'tablet',
                                width: 1023
                            },
                            {
                                name: 'mobile',
                                width: 767
                            },
                            {
                                name: 'mobile-l',
                                width: 767
                            },
                            {
                                name: 'mobile-p',
                                width: 480
                            },
                        ]
                    },
                    autoWidth: false,
                    data: data,
                    rowId: 'Id',
                    oLanguage: {
                        "sSearch": '<span class="label-caption">' + translations['Search'] + '</span> <span class="icon-hold"><svg aria-hidden="true" class="icon icon-search"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-search"></use></svg></span>',
                        "oPaginate": {
                            "sNext": '<i class="glyphicon glyphicon-menu-right"></i>',
                            "sPrevious": '<i class="glyphicon glyphicon-menu-left"></i>'
                        },
                        "sLengthMenu": translations['Show'] + ' _MENU_ ' + translations['Entries'],
                        "sZeroRecords": translations['NoMatchingRecordsFound']
                    },
                    createdRow: function (row, data, dataIndex) {
                        var status = mapStatusToColorClass(data.Status);
                        $(row).find('.icon-status').addClass(status);

                        var signatureStatus = mapSignatureStatusToColorClass(data.SignatureStatus);
                        $(row).find('.icon-esig-hold').addClass(signatureStatus);

                        if (data.IsNewlyCreated) {
                            $(row).addClass('unread-deals').find('.contract-cell').prepend('<span class="label-new-deal">' + translations['New'] + '</span>');
                        }
                    },
                    columns: [{
                            //"data": 'TransactionId',
                            render: function (sdate, type, row) {
                                var content = row.Id === 0 ? row.TransactionId :
                                    '<a href="' + editContractUrl + '/' + row.Id + '" title="' + translations['Edit'] + '">' + row.TransactionId + '</a>';

                                return '<div class="status-hold">' +
                                    '<div class="icon-hold"><span class="icon icon-status"></span></div>' +
                                    '<div class="text-hold"><span class="text">' +
                                    content + '</span></div></div>';
                            },
                            className: 'contract-cell',
                            type: 'html-num',
                            orderData: 10
                        },
                        {
                            "data": 'CustomerName',
                            className: 'customer-cell'
                        },
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
                        {
                            "data": 'Action',
                            className: 'action-cell'
                        },
                        {
                            "data": 'Email',
                            className: 'email-cell'
                        },
                        {
                            "data": 'Phone',
                            className: 'phone-cell'
                        },
                        {
                            "data": 'Date',
                            className: 'date-cell'
                        },
                        {
                            "data": 'RemainingDescription',
                            "visible": false
                        },
                        { // this is Edit Actions Column
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
                                        return '<div class="controls-hold"><a class="icon-link icon-edit" href=' + editContractUrl + '/' + row.Id + ' title="' + translations['Edit'] + '"><svg aria-hidden="true" class="icon icon-edit"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-edit"></use></svg></a>' +
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
                            "data": 'Id',
                            "visible": false
                        },
                        {
                            "render": function (sdata, type, row) {
                                return row.TransactionId.toLowerCase().indexOf(':') > -1 ?
                                    row.Id :
                                    row.TransactionId;
                            },
                            "visible": false
                        }
                    ],
                    dom: "<'row'<'col-md-8''<'#table-title.dealnet-caption'>'><'col-md-4 col-sm-6'f>>" +
                        "<'row'<'col-md-12 col-sm-6'l>>" +
                        "<'row'<'col-md-12'tr>>" +
                        "<'row'<'col-md-12'p>>" +
                        "<'row'<'col-md-12'i>>",
                    renderer: 'bootstrap',
                    order: [],
                    drawCallback: function () {
                        resizeTableStatusCells(this);
                        $('.icon-esignature').off();
                        $('.icon-esignature').on('click', getSignatureDetails);
                    }
                });

            $('#work-items-table th').on('click', function () {
                var el = $(this);
                var attr = el.attr('aria-sort');

                if (el.is('.contract-cell')) {
                    if (el.is('.sorting_asc') && !attr) {
                        attr = 'ascending';
                    }
                    if (el.is('.sorting_desc') && !attr) {
                        attr = 'descending';
                    }
                    el.attr('aria-sort', attr);
                }

                if (attr && attr === 'descending') {
                    el.attr('def-sort', 'true');
                }

                if (attr && attr === 'ascending') {
                    if (el.attr('def-sort') !== undefined) {
                        el.removeAttr('aria-sort')
                            .removeAttr('def-sort')
                            .removeClass('sorting_asc')
                            .addClass('sorting');

                        table.order.neutral().draw();
                    }
                }
            });

            table.on('draw.dt', function () {
                redrawDataTablesSvgIcons();
            });

            table.on('responsive-display', function (e, datatable, row, showHide, update) {
                var status = mapStatusToColorClass(row.data().Status);
                showHide ? $(row.child()).find('.icon-status').addClass(status) : $(row.child()).find('.icon-status').removeClass(status);

                var signatureStatus = mapSignatureStatusToColorClass(row.data().SignatureStatus);
                showHide ? $(row.child()).find('.icon-esig-hold').addClass(signatureStatus) : $(row.child()).find('.icon-status').removeClass(signatureStatus);
                showHide ? $(row.child()).find('.icon-esignature').on('click', getSignatureDetails) : $(row.child()).find('.icon-esignature').off();
            });

            resizeTableStatusCells('#work-items-table');

            var iconSearch = '<span class="icon-search-control"><svg aria-hidden="true" class="icon icon-search"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-search"></use></svg></span>';
            $('#table-title').html(translations['MyWorkItems'] + '  <div class="filter-controls hidden">' + iconSearch + '</div></div>');
            $('#table-title .icon-search-control').on('click', function () {
                $('#work-items-table_filter').slideToggle();
            });
        });

};