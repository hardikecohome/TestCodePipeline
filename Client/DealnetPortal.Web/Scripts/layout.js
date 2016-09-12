$(document)
    .ready(function () {
        $('.dealnet-sidebar-item a[href="' + window.location.pathname + '"]')
            .parents('.dealnet-sidebar-item')
            .addClass('dealnet-sidebar-item-selected');

        $('.dealnet-sidebar-item').click(function () {
            window.location.href = $(this).find('a').attr('href');
        });

        $('.dealnet-page-button').click(function () {
            window.location.href = $(this).find('a').attr('href');
        });

    });

function showLoader() {
    $.loader({
        className: 'loader',
        content: '',
        width: 101,
        height: 100
});
}

function hideLoader() {
    $.loader('close');
}
