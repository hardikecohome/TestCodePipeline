module.exports('onboarding.common', function () {
    function resetFormValidation (selector) {
        var $form = $(selector);
        $form.removeData("validator");
        $form.removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse(selector);
    }

    /**
     * detect IE
     * returns version of IE or false, if browser is not Internet Explorer
     */
    function detectIe () {
        var ua = window.navigator.userAgent;

        var msie = ua.indexOf('MSIE ');
        if (msie > 0) {
            // IE 10 or older => return version number
            return parseInt(ua.substring(msie + 5, ua.indexOf('.', msie)), 10);
        }

        var trident = ua.indexOf('Trident/');
        if (trident > 0) {
            // IE 11 => return version number
            var rv = ua.indexOf('rv:');
            return parseInt(ua.substring(rv + 3, ua.indexOf('.', rv)), 10);
        }

        var edge = ua.indexOf('Edge/');
        if (edge > 0) {
            // Edge (IE 12+) => return version number
            return parseInt(ua.substring(edge + 5, ua.indexOf('.', edge)), 10);
        }

        // other browser
        return false;
    }

    var initTooltip = function () {
        var toggle = $('#submitBtn').parent('[data-toggle="popover"]');
        var body = $('body');
        return function () {
            var isMobile = body.is('.tablet-device') || body.is('.mobile-device');
            if (toggle.data('bs.popover')) {
                toggle.off().popover('destroy');
            }
            toggle.popover({
                template: '<div class="popover customer-loan-popover" role="tooltip"><h3 class="popover-title"></h3><div class="popover-content"></div></div>',
                placement: 'top',
                trigger: isMobile ? 'click' : 'hover'
            }).on('shown.bs.popover', function () {
                if (isMobile) {
                    $(this).parents('div[class*="equal-height-row"]').addClass('row-auto-height');
                }
            }).on('hide.bs.popover', function () {
                if (isMobile) {
                    $(this).parents('div[class*="equal-height-row"]').removeClass('row-auto-height');
                }
            });
            if (isMobile) {
                toggle.on('touchstart click', function () {
                    toggle.popover('show');
                    setTimeout(function () {
                        toggle.popover('hide');
                    }, 2000);
                });
            }
            toggle.data('bs.popover').tip().addClass('onboard-popover');
        }
    }();

    var removeTooltip = function () {
        var toggle = $('#submitBtn').parent('[data-toggle="popover"]');
        return function () {
            toggle.off().popover('destroy');
        }
    }();

    return {
        resetFormValidation: resetFormValidation,
        detectIe: detectIe,
        initTooltip: initTooltip,
        removeTooltip: removeTooltip
    };
});
