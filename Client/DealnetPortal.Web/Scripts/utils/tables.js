function commonDataTablesSettings () {
    if ($('#work-items-table').length) {
        $('#work-items-table, .total-info').hide();
        $.extend(true, $.fn.dataTable.defaults, {
            "fnInitComplete": function () {
                $('#work-items-table, .total-info').show();
                $('#work-items-table_filter input[type="search"], .dataTables_length select').removeClass('input-sm');
                customizeSelect();
            }
        });
        $.fn.dataTable.Api.register('order.neutral()', function () {
            return this.iterator('table', function (s) {
                s.aaSorting.length = 0;
                s.aiDisplay.sort(function (a, b) {
                    return a - b;
                });
                s.aiDisplayMaster.sort(function (a, b) {
                    return a - b;
                });
            });
        });
    }
}

function resetDataTablesExpandedRows (table) {
    table.rows().every(function (i) {
        var child = this.child;
        var row = this.node();
        if (child.isShown()) {
            child.hide();
            $(row).removeClass('parent');
        }
    });
}

function redrawDataTablesSvgIcons () {
    /*Redraw svg icons inside dataTable only for ie browsers*/
    if (module.require('detectIE')()) {
        if ($('.dataTable .edit-control a, .dataTable a.icon-link.icon-edit').length > 0) {
            $('.edit-control a, a.icon-link.icon-edit').html('<svg aria-hidden="true" class="icon icon-edit"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-edit"></use></svg>');
        }
        if ($('.dataTable .checkbox-icon').length > 0) {
            $('.checkbox-icon').html('<svg aria-hidden="true" class="icon icon-checked"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-checked"></use></svg>');
        }
        if ($('.dataTable .contract-controls a.preview-item').length > 0) {
            $('.contract-controls a.preview-item').html('<svg aria-hidden="true" class="icon icon-preview"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-preview"></use></svg>');
        }
        if ($('.dataTable .contract-controls a.export-item').length > 0) {
            $('.contract-controls a.export-item').html('<svg aria-hidden="true" class="icon icon-excel"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-excel"></use></svg>');
        }
        if ($('.dataTable .remove-control a, .dataTable a.icon-link.icon-remove').length > 0) {
            $('.remove-control a, a.icon-link.icon-remove').html('<svg aria-hidden="true" class="icon icon-remove"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-trash"></use></svg>');
        }
        if ($('.dataTable a.link-accepted').length > 0) {
            $('a.link-accepted').html('<svg aria-hidden="true" class="icon icon-accept-lead"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-accept-lead"></use></svg>');
        }
    }
}

// work-around for FF for colored status bars in tables
function resizeTableStatusCells (table) {
    if (navigator.userAgent.toLowerCase().indexOf('firefox') > -1) {
        $(table).find('.status-hold').each(function () {
            var $this = $(this);
            var cellHeight = $this.parents('.status-cell').height();
            var thisHeight = $this.height();
            if (thisHeight < cellHeight)
                $this.height(cellHeight + 5);
        });
    }
}

function mapStatusToColorClass (status) {
    return 'icon-' + status.trim().toLowerCase().replace(/\s/g, '-').replace(/\(/g, '').replace(/\)/g, '').replace(/\//g, '').replace(/\$/g, '');
}

function mapSignatureStatusToColorClass (signatureStatus) {
    return 'icon-esig-' + signatureStatus;
}
