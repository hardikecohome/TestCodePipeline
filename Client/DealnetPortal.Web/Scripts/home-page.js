var chart = null;
var table;

$(document)
    .ready(function () {
        setTimeout(function () {
            showChart();
        }, 500);
        showTable();
        $('.dealnet-chart-switch-button')
            .click(function () {
                $('.dealnet-chart-switch-button').removeClass('dealnet-chart-switch-button-selected');
                $(this).addClass('dealnet-chart-switch-button-selected');
                setTimeout(function () {
                    showChart();
                }, 500);
            });
    });

function FormatLongNumber(value) {
    if (value == 0) {
        return 0;
    }
    else {
        // for testing
        //value = Math.floor(Math.random()*1001);

        // hundreds
        if (value <= 999) {
            return value;
        }
            // thousands
        else if (value >= 1000 && value <= 999999) {
            return (value / 1000) + 'K';
        }
            // millions
        else if (value >= 1000000 && value <= 999999999) {
            return (value / 1000000) + 'M';
        }
            // billions
        else if (value >= 1000000000 && value <= 999999999999) {
            return (value / 1000000000) + 'B';
        }
        else
            return value;
    }
}

function showChart() {
    var graphsBgColor = $('body').is('.theme-one-dealer') ? 'rgba(235, 151, 0, 0.23)' : 'rgba(221, 243, 213, 1)';
    var maxValueXAxix = $('body').is('.mobile-device') ? '14' : ''
    var fontSize = $('body').is('.mobile-device') ? '10' : '12'
    var maxRotation = $('body').is('.mobile-device') ? '0' : '30'
    $.when($.ajax(chartUrl,
                {
                    mode: 'GET',
                    cache: false,
                    data: {
                        type: $('.dealnet-chart-switch-button-selected').attr('data-type')
                    }
                })
                    )
                    .done(function (data) {

                        if (chart) {
                            chart.destroy();
                        }
                        var canvas = document.getElementById('data-flow-overview');
                        $('.data-flow-container').addClass('data-loaded')
                        if ($('#data-flow-overview').length > 0) {
                            $('body').addClass('body-scrolled');
                        } else {
                            $('body').removeClass('body-scrolled');
                        }
                        chart = new Chart(canvas,
                        {
                            type: 'bar',
                            data: data,
                            options: {
                                tooltips: {
                                    backgroundColor: '#f2f1f1',
                                    titleColor: '#1f1f1f',
                                    bodyColor: '#1f1f1f',
                                    footerColor: '#1f1f1f',
                                    callbacks: {
                                        label: function (tooltipItems) {
                                            return '$ ' + tooltipItems.yLabel;
                                        },
                                        title: function (tooltipItems) {
                                            return tooltipItems[0].xLabel + ':';
                                        }
                                    },
                                },
                                legend: {
                                    display: false,
                                },
                                elements: {
                                    rectangle: {
                                        backgroundColor: graphsBgColor
                                    }
                                },
                                scales: {
                                    yAxes: [{
                                        ticks: {
                                            beginAtZero: true,
                                            userCallback: function (value, index, values) {
                                                return FormatLongNumber(value);
                                            },
                                            min: 0,
                                            suggestedMax: 10000,
                                        },
                                        gridLines:
                                            {
                                                lineWidth: 1
                                            }
                                    }],
                                    xAxes: [{
                                        ticks: {
                                            fontSize: fontSize,
                                            maxRotation: maxRotation,
                                            /*userCallback: function(value, index, values) {
                                              if(values.length <= 13 && $('body').is('.mobile-device')){
                                                value = value.slice(0, 4);
                                              }
                                              return value;
                                            }*/
                                        },
                                        scaleLabel: {
                                            display: true,
                                            labelString: " ",
                                            fontSize: 5,
                                        },
                                        gridLines:
                                            {
                                                lineWidth: 1
                                            }
                                    }]
                                }
                            }
                        });
                    });
};

function removeContract() {
    var tr = $(this).parents('tr');
    var id = $(tr)[0].id;
    var data = {
        message: translations['AreYouSureYouWantToRemoveThisApplication']+'?',
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
    
};

function showTable() {
    $.when($.ajax(itemsUrl, { cache: false, mode: 'GET' }))
    .done(function (data) {
        table = $('#work-items-table')
              .DataTable({
                  responsive: {
                      details: {
                          display: $.fn.dataTable.Responsive.display.childRow
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
                      if (data.IsNewlyCreated) {
                          $(row).addClass('unread-deals').find('.contract-cell').prepend('<span class="label-new-deal">' + translations['New'] + '</span>');
					  }
					  $(row).find('.contract-cell').wrapInner('<a href="'+ editItemUrl + '/' + data.Id + '" title="' + translations['Edit'] +'"></a>');					  
                  },
                  columns: [
                        { "data": 'TransactionId', className: 'contract-cell' },
                        { "data": 'CustomerName', className: 'customer-cell' },
                        { "data": 'Status', className: 'status-cell' },
                        { "data": 'Action', className: 'action-cell' },
                        { "data": 'Email', className: 'email-cell' },
                        { "data": 'Phone', className: 'phone-cell' },
                        { "data": 'Date', className: 'date-cell' },
                        {
                            "data": 'RemainingDescription',
                            "visible": false
                        },
                        {// this is Edit Actions Column
                            "render": function (sdata, type, row) {
                                if (row.Id != 0) {
                                    if (row.IsInternal) {
                                        return '<div class="controls-hold">' +
                                            '<a class="icon-link icon-edit" href=' +
                                            editItemUrl +
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
                                        return '<div class="controls-hold"><a class="icon-link icon-edit" href=' + editItemUrl + '/' + row.Id + ' title="' + translations['Edit'] + '"><svg aria-hidden="true" class="icon icon-edit"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-edit"></use></svg></a></div>';
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
                        }

                  ],
                  dom:
                  "<'row'<'col-md-8''<'#table-title.dealnet-caption'>'><'col-md-4 col-sm-6'f>>" +
                  "<'row'<'col-md-12 col-sm-6'l>>" +
                  "<'row'<'col-md-12'tr>>" +
                  "<'row'<'col-md-12'p>>" +
                  "<'row'<'col-md-12'i>>",
                  renderer: 'bootstrap',
                  order: []
              });

        table.on('draw.dt', function () {
            redrawDataTablesSvgIcons();
        });

        var iconSearch = '<span class="icon-search-control"><svg aria-hidden="true" class="icon icon-search"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-search"></use></svg></span>';
        $('#table-title').html(translations['MyWorkItems'] + '  <div class="filter-controls hidden">' + iconSearch + '</div></div>');
        $('#table-title .icon-search-control').on('click', function () {
            $('#work-items-table_filter').slideToggle();
        });
    });

};
