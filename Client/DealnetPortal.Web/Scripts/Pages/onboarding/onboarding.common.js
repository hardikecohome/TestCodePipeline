module.exports('onboarding.common', function () {
    function resetFormValidation (selector) {
        var $form = $(selector);
        $form.removeData("validator");
        $form.removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse(selector);
        if ($form.find('.dob-input').length) {
            $form.find('.dob-input').rules('add', {
                over18: true
            });
        }
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
        initTooltip: initTooltip,
        removeTooltip: removeTooltip
    };
});
