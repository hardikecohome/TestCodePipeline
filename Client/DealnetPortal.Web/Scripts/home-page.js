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
    $.when($.ajax('/client/reports/DealFlowOverview',
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
    $.when($.ajax('/client/reports/WorkItems', { mode: 'GET' }))
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
                    { title: "Contract #" },
                    { title: "Customer" },
                    { title: "Status" },
                    { title: "Action req." },
                    { title: "Email" },
                    { title: "Phone" },
                    { title: "Date" }
                ]
            });
        $('.paginate_button.previous a').text('<');
        $('.paginate_button.next a').text('>');
    });
};
