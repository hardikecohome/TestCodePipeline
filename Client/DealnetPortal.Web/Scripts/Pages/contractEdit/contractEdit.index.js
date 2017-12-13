module.exports('contract-edit', function (require) {
    var eSign = require('contract-edit.eSignature');
    var navigateToStep = require('navigateToStep');
    var toggleBackToTopVisibility = require('backToTop').toggleBackToTopVisibility;
    var backToTop = require('backToTop').backToTop;

    var init = function (eSignEnabled) {
        if (eSignEnabled === 1) {
            eSign.init();
        }
        $('.editToStep1').on('click', function () {
            if ($(this).attr('href')) {
                navigateToStep($(this));
            }
            return false;
        });

        $(window).on('scroll', toggleBackToTopVisibility)
            .on('resize', function () {
                toggleBackToTopVisibility();
            });

        $('#back-to-top').on('click', function () {
            backToTop();
        });
    }

    return {
        init: init
    };
});