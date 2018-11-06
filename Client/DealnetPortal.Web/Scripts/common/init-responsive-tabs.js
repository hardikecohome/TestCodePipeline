$(document).ready(function () {
    /*Responsive tabs*/
    if ($('.responsive-tabs').length) {
        var uploadErrorIcon = '<span class="error-icon"><svg aria-hidden="true" class="icon icon-error-upload"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-error-upload"></use></svg></span>';
        var uploadSuccess = '<span class="custom-checkbox"><span class="checkbox-icon"><svg aria-hidden="true" class="icon icon-checked"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-checked"></use></svg></span></span>';
        var accordionArrows = '<span class="pill-arrow"><i class="glyphicon"></i></span>';

        var uploadedStatus = $('<span>', {
            class: 'upload-doc-status',
            html: uploadErrorIcon + uploadSuccess
        });
        uploadedStatus.prependTo('a[data-toggle="tab"]');
        $(accordionArrows).appendTo('a[data-toggle="tab"]');

        $('.responsive-tabs a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
            var currentTab = $(this);
            var currentTabParent = currentTab.parents('.documents-pills-item');
            var currentTabId = currentTab.attr('aria-controls');

            $('a[data-toggle="tab"]').parents('.documents-pills-item').removeClass('active');
            $('a[aria-controls="' + currentTabId + '"]').parents('.documents-pills-item').addClass('active');
            currentTab.parents('.documents-pills-item').addClass('active');
        });
    }
});