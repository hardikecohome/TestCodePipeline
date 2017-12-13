var isMobile = {
    Android: function () {
        return navigator.userAgent.match(/Android/i);
    },
    BlackBerry: function () {
        return navigator.userAgent.match(/BlackBerry/i);
    },
    iOS: function () {
        return navigator.userAgent.match(/iPhone|iPad|iPod/i);
    },
    Opera: function () {
        return navigator.userAgent.match(/Opera Mini/i);
    },
    Windows: function () {
        return navigator.userAgent.match(/IEMobile/i);
    },
    any: function () {
        return (isMobile.Android() || isMobile.BlackBerry() || isMobile.iOS() || isMobile.Opera() || isMobile.Windows());
    }
};

$(document)
    .ready(function () {
        setDeviceClasses();

        if (detectIE()) {
            $('body').addClass('ie');
        }

        if (layotSettingsUrl) {
            $.ajax({
                cache: false,
                type: "GET",
                url: layotSettingsUrl,
                success: function (json) {
                    if (json.aboutAvailability) {
                        $('#sidebar-item-about').show();
                    }
                }
            });
        }

        if (customerDealsCountUrl) {
            $.ajax({
                cache: false,
                type: "GET",
                url: customerDealsCountUrl,
                success: function (json) {
                    if (json.dealsCount && json.dealsCount !== 0) {
                        $('#new-deals-number').text(json.dealsCount);
                        $('#new-deals-number').show();
                    }
                }
            });
        }

        $('.chosen-language-link').on('click', function () {
            $(this).parents('.lang-switcher').toggleClass('open');
            return false;
        });

        //If opened switch language dropdown, hide it when click anywhere accept opened dropdown
        $('html').on('click touchstart', function (event) {
            if ($('.navbar-header .lang-switcher.open').length > 0 && $(event.target).parents('.lang-switcher').length == 0) {
                $('.lang-switcher').removeClass('open')
            }
        });
        $('html').on('click', function (event) {
            if (isMobile.iOS() &&
                $(event.target).parents('.div-datepicker').length === 0 &&
                $(event.target).parents('.ui-datepicker-header').length == 0 &&
                $(event.target).not('.div-datepicker-value').length &&
                $('.div-datepicker.opened').length
            ) {
                $('.div-datepicker').removeClass('opened');
            }
        });

        $('.dealnet-sidebar-item a[href="' + window.location.pathname + '"]')
            .parents('.dealnet-sidebar-item')
            .addClass('dealnet-sidebar-item-selected');

        // NewApplication has multiple steps with different window.location.pathname,
        // but New Application navigation should be active on each step.
        if (window.location.pathname.indexOf('NewApplication') !== -1) {
            $('#sidebar-item-newrental').addClass('dealnet-sidebar-item-selected');
        }

        $('.navbar-toggle').click(function () {
            if ($('.navbar-collapse').attr('aria-expanded') === 'false') {
                saveScrollPosition();
                $('body').addClass('open-menu');
                $('body').addClass('menu-animated');
                $('.overlay').show();
            } else {
                $('body').removeClass('open-menu');
                resetScrollPosition();
                $('.overlay').hide();
                setTimeout(function () {
                    $('body').removeClass('menu-animated');
                }, 400);
            }
        });


        $('.credit-check-info-hold.fit-to-next-grid').each(function () {

            if ($(this).find('.grid-column').length % 2 !== 0) {
                $(this).parents('.grid-parent').next('.credit-check-info-hold').addClass('shift-to-basic-info');
                $(this).parents('.grid-parent').next('.grid-parent:not(.main-parent)').find('.credit-check-info-hold').addClass('shift-to-basic-info');
            }

        });

        $(window).on('scroll', function () {
            detectPageHeight();
        }).on('resize', function () {
            setDeviceClasses();
            if (isMobile.iOS() && viewport().width >= 768) {
                if ($('.modal.in').length === 1) {
                    setModalMarginForIpad();
                }
            }
            detectPageHeight();
            documentsColHeight();

            if ($(".dataTable").length !== 0) {
                $('.dataTable td.dataTables_empty').attr('colspan', $('.dataTable th').length);
            }
        });

        setTimeout(function () {
            documentsColHeight();
            $('.credit-check-info-hold .dealnet-credit-check-section').each(function () {
                var col = $(this).parents('.col-md-6');
                if (col.not('.col-md-push-6')) {
                    var colOffset = col.position().left;
                    if (colOffset == 0 && col.next('.col-md-6').length) {
                        col.addClass('has-right-border');
                    }
                }
                if (col.is('.col-md-push-6')) {
                    var colOffset = col.next('.col-md-pull-6').position().left;
                    if (colOffset == 0 && col.next('.col-md-pull-6').length) {
                        col.next('.col-md-pull-6').addClass('has-right-border');
                    }
                }
            });
        }, 500);

        /*Settings for propper work of datepicker inside bootstrap modal*/

        $.fn.modal.Constructor.prototype.enforceFocus = function () { };

        /*END Settings for propper work of datepicker inside bootstrap modal*/

        $('[data-toggle="popover"]').popover({
            template: '<div class="popover customer-loan-popover" role="tooltip"><h3 class="popover-title"></h3><div class="popover-content"></div></div>',
        });
    });

function documentsColHeight () {
    var columns = $('.report-documents-list .document-col');
    columns.find('.dealnet-credit-check-section').css('min-height', columns.find('.documents-inner').height());
}

function detectPageHeight () {
    if ($('.dealnet-body').height() > 1000) {
        $('.back-to-top-hold').show();
    } else {
        $('.back-to-top-hold').hide();
    }
}

function backToTop () {
    $("html,body").animate({ scrollTop: 0 }, 1000);
    return false;
};

function resetPlacehoder (elems) {
    elems.removeClass('placeholder');
    setTimeout(function () {
        elems.placeholder();
    }, 0);
}

function saveScrollPosition () {
    var $body = $('body');
    //if open one modal right after other one
    var topOffset = $(window).scrollTop();
    $body.css('top', -topOffset);
}

function resetScrollPosition () {
    var $body = $('body');
    var bodyOffset = Math.abs(parseInt($body.css('top')));

    $body.css({
        'top': 'auto'
    });

    $('html, body').scrollTop(bodyOffset);
}

$.prototype.disableTab = function () {
    this.each(function () {
        $(this).attr('tabindex', '500');
    });
};

String.prototype.toDash = function () {
    return this.replace(/([A-Z])/g, function ($1) { return "-" + $1.toLowerCase(); });
};

/**
 * detect IE
 * returns version of IE or false, if browser is not Internet Explorer
 */
function detectIE () {
    var ua = window.navigator.userAgent;

    // Test values; Uncomment to check result …

    // IE 10
    // ua = 'Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)';

    // IE 11
    // ua = 'Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko';

    // Edge 12 (Spartan)
    // ua = 'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.71 Safari/537.36 Edge/12.0';

    // Edge 13
    // ua = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586';

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

function setDeviceClasses () {
    if (isMobile.any()) {
        if (viewport().width < 768) {
            $('body').addClass('mobile-device').removeClass('tablet-device')
        } else {
            $('body').addClass('tablet-device').removeClass('mobile-device')
        }
    } else {
        $('body').removeClass('mobile-device').removeClass('tablet-device')
    }
    if (isMobile.iOS()) {
        $('body').addClass('ios-device');
        $('.modal').removeClass('fade').addClass('fade-on-ios');
    } else {
        $('body').removeClass('ios-device')
    }
}

function viewport () {
    var e = window, a = 'inner';
    if (!('innerWidth' in window)) {
        a = 'client';
        e = document.documentElement || document.body;
    }
    return { width: e[a + 'Width'], height: e[a + 'Height'] };
}
