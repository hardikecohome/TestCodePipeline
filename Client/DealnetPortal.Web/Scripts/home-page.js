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
    $.when($.ajax('/reports/DealFlowOverview',
                {
                    mode: 'GET',
                    data: {
                        type: $('.dealnet-chart-switch-button-selected').attr('data-type')
                    }
                })
                    )
                    .done(function (data) {
                        var canvas = document.getElementById("data-flow-overview");
                        var chart = new Chart(canvas, {
                            type: 'bar',
                            data: data,
                            options: {
                                legend: {
                                  display:false  
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
    $.when($.ajax('/reports/WorkItems', { mode: 'GET' }))
    .done(function (data) {
        $('#work-items-table').DataTable({
            'data': data,
            'columns': [
                { title: "Contract #" },
                { title: "Customer" },
                { title: "Status" },
                { title: "Action req." },
                { title: "Email" },
                { title: "Phone" },
                { title: "Date" }
            ]
        });
    });
};
