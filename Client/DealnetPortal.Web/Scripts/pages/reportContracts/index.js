module.exports('report-contracts', function (require) {
    var toggleBackToTopVisibility = require('backToTop').toggleBackToTopVisibility;
    var backToTop = require('backToTop').backToTop;

    function init () {
        $('.reports-contract-item').each(function () {
            $('.contract-hidden-info').hide();
        });

        $('.show-full-conract-link').on('click', function () {
            $(this).parents('.reports-contract-item').find('.contract-hidden-info').show();
            $(this).hide();
            $('.hide-full-conract-link').show();
            return false;
        });

        $('.hide-full-conract-link').on('click', function () {
            $(this).parents('.reports-contract-item').find('.contract-hidden-info').hide();
            $(this).hide();
            $('.show-full-conract-link').show();
            return false;
        });

        $(window).on('scroll', toggleBackToTopVisibility)
            .on('resize', toggleBackToTopVisibility);

        $('#back-to-top').on('click', backToTop);
    }

    return {
        init: init
    };
});