var chart = null;

$(document)
    .ready(function () {
      setTimeout(function(){
        showChart();
      }, 500);
        showTable();
        $('.dealnet-chart-switch-button')
            .click(function () {
                $('.dealnet-chart-switch-button').removeClass('dealnet-chart-switch-button-selected');
                $(this).addClass('dealnet-chart-switch-button-selected');
                setTimeout(function(){
                  showChart();
                }, 500);
            });
    });




function showChart() {
  var graphsBgColor = $('body').is('.theme-one-dealer') ? 'rgba(235, 151, 0, 0.23)' : 'rgba(221, 243, 213, 1)';
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
                                    cornerRadius: 4,
                                    callbacks: {
                                      label: function(tooltipItems) {
                                        return tooltipItems.yLabel;
                                      },
                                      title: function (tooltipItems) {
                                        return tooltipItems[0].xLabel + ':';
                                      }
                                    },
                                },
                                legend: {
                                    display: false
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
                                        },
                                        scaleLabel: {
                                            display: true,
                                            labelString: '$',
                                            fontSize: 14,
                                            fontStyle: 'bold'
                                        },
                                        gridLines:
                                            {
                                                display: false,
                                                lineWidth : 2
                                            }                                        
                                    }],
                                    xAxes: [{                                        
                                        scaleLabel: {
                                            display: true,
                                            labelString: 'Time',
                                            fontSize: 14,
                                            fontStyle: 'bold'
                                        },
                                        gridLines: 
                                            {
                                                display : false,
                                                lineWidth : 2
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
      var table = $('#work-items-table')
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
                      { "data" : "TransactionId" },
                      { "data": "CustomerName" },
                      { "data": "Status" },
                      { "data": "Action" },
                      { "data": "Email" },
                      { "data": "Phone" },
                      { "data": "Date"},
                      {// this is Actions Column
                          "render": function (sdata, type, row) {
                              return '<div class="edit-control"><a href=' + editItemUrl + '/' + row.Id + ' title="Edit"><svg aria-hidden="true" class="icon icon-edit"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-edit"></use></svg></a></div>';
                          }
                      },
                      {
                          "data": "Id",
                          "visible": false
                      }
                  ],

                columnDefs: [                   
                  { responsivePriority: 8, targets: -1 },
                  { width: "40px", targets: -1 },
                  { className: 'customer-cell', targets: 1},
                  { className: 'id-cell', targets: 8},
                  { className: 'edit-cell', targets: -1},
                  { targets  : [-1], orderable: false}
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

        table.on('draw.dt', function(){
          redrawDataTablesSvgIcons();
        });

        var iconSearch = '<span class="icon-search-control"><i class="glyphicon glyphicon-search"></i></span>';
        $('#table-title').html('My Work Items  <div class="filter-controls hidden">'+ iconSearch +'</div></div>');
        $('#table-title .icon-search-control').on('click', function(){
          $('#work-items-table_filter').slideToggle();
        });
    });
};
