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
                        display: $.fn.dataTable.Responsive.display.childRow
                    }
                },
                data: data,
                oLanguage: {
                  "sSearch": '<span class="label-caption">Search</span> <span class="icon-search"><i class="glyphicon glyphicon-search"></i></span>',
                  "oPaginate": {
                    "sNext": '<i class="glyphicon glyphicon-menu-right"></i>',
                    "sPrevious": '<i class="glyphicon glyphicon-menu-left"></i>'
                  }
                },
                columns: [
                      { "data" : "Id" },
                      { "data": "CustomerName" },
                      { "data": "Status" },
                      { "data": "Action" },
                      { "data": "Email" },
                      { "data": "Phone" },
                      { "data": "Date"},
                      {// this is Actions Column
                          "render": function (sdata, type, row) {
                              return '<a href=' + editItemUrl + '/' + row.Id + ' title="Edit"><svg aria-hidden="true" class="icon-svg icon-edit"><use xlink:href="#icon-edit"></use></svg></a>';
                          }
                      }
                  ],

                columnDefs: [
                  { responsivePriority: 8, targets: -1 },
                  { width: "40px", targets: -1 }
                ],
                dom:
                "<'row'<'col-md-6''<'#table-title.dealnet-caption'>'><'col-md-6'f>>" +
                "<'row'<'col-md-12'l>>" +
                "<'row'<'col-md-12'tr>>" +
                "<'row'<'col-md-12'p>>" +
                "<'row'<'col-md-12'i>>",
                renderer: 'bootstrap'
            });
        $('#table-title').html('My Work Items <span class="icon-search-control hidden"><i class="glyphicon glyphicon-search"></i></span>');
        $('#table-title .icon-search-control').on('click', function(){
          $('#work-items-table_filter').slideToggle();
        });
        /*$('.paginate_button.previous a').html('<i class="glyphicon glyphicon-menu-left"></i>');
        $('.paginate_button.next a').html('<i class="glyphicon glyphicon-menu-right"></i>');*/

        /*$('#work-items-table_filter .icon-search').on('click', function(){
          $(this).siblings('input').toggleClass('active');
        });*/
    });
};
