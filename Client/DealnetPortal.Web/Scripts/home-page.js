﻿var chart = null;

$(document)
    .ready(function () {
        showChart();
        showTable();
        $('.dealnet-chart-switch-button')
            .click(function () {
                $('.dealnet-chart-switch-button').removeClass('dealnet-chart-switch-button-selected');
                $(this).addClass('dealnet-chart-switch-button-selected');
                showChart();
            });
    });




function showChart() {
    $.when($.ajax(chartUrl,
                {
                    mode: 'GET',
                    data: {
                        type: $('.dealnet-chart-switch-button-selected').attr('data-type')
                    }
                })
                    )
                    .done(function (data) {

                        if (chart) {
                            chart.destroy();
                        }
                        var canvas = document.getElementById("data-flow-overview");
                        chart = new Chart(canvas,
                        {
                            type: 'bar',
                            data: data,
                            options: {
                                tooltips: {
                                    backgroundColor: 'rgba(0, 0, 0, 1)',
                                    titleFontColor: '#1f1f1f',
                                    bodyFontColor: '#1f1f1f',
                                    cornerRadius: 4
                                },
                                legend: {
                                    display: false
                                },
                                elements: {
                                    rectangle: {
                                        backgroundColor: 'rgba(221, 243, 213, 1)'
                                    }
                                },
                                scales: {
                                    yAxes: [{
                                        ticks: {
                                            beginAtZero: true
                                        }
                                    }]
                                }
                            }
                        });
                    });
};

function showTable() {
    $.when($.ajax(itemsUrl, { mode: 'GET' }))
    .done(function (data) {
        $('#work-items-table')
            .DataTable({
                responsive: {
                    details: {
                        display: $.fn.dataTable.Responsive.display.childRowImmediate
                    }
                },
                data: data,
                columns: [
                    { "data" : "Id" },
                    { "data": "CustomerName" },
                    { "data": "Status" },
                    { "data": "Action" },
                    { "data": "Email" },
                    { "data": "Phone" },
                    { "data": "Date" },
                    {// this is Actions Column 
                        "render": function (sdata, type, row) {
                            return '<a href=' + editItemUrl + '/' + row.Id + ' title="Edit"><svg aria-hidden="true" class="icon-svg icon-edit"><use xlink:href="#icon-edit"></use></svg></a>';
                        }
                    }
                ]
            });
        $('.paginate_button.previous a').html('<i class="glyphicon glyphicon-menu-left"></i>');
        $('.paginate_button.next a').html('<i class="glyphicon glyphicon-menu-right"></i>');
    });
};
