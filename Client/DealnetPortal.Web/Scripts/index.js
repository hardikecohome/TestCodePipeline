﻿module.exports('index', function (require) {
    var saveScrollPosition = require('scrollPosition').save;
    var resetScrollPosition = require('scrollPosition').reset;
    var detectIe = require('detectIE');
    var mobileNavDropdown;
    var darkOverlay;

    function init() {
        setDeviceClasses();

        $.fn.modal.Constructor.prototype.enforceFocus = function () { };

        $(window).on('resize', function () {
            setDeviceClasses();
            if (isMobile.iOS() && viewport().width >= 768) {
                if ($('.modal.in').length === 1) {
                    setModalMarginForIpad();
                }
            }

            if ($(".dataTable").length !== 0) {
                $('.dataTable td.dataTables_empty').attr('colspan', $('.dataTable th').length);
            }

        });

        //If opened switch language dropdown, hide it when click anywhere accept opened dropdown
        $('html').on('click touchstart', function (event) {
            if ($('.navbar-header .lang-switcher.open').length > 0 && $(event.target).parents('.lang-switcher').length == 0) {
                $('.lang-switcher').removeClass('open');
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

        $('.header__navigation a[href="' + window.location.pathname + '"]')
            .addClass('active');

        // NewApplication has multiple steps with different window.location.pathname,
        // but New Application navigation should be active on each step.
        if (window.location.pathname.toLowerCase().indexOf('newapplication') !== -1) {
            $('#sidebar-item-newrental').addClass('dealnet-sidebar-item-selected');
        }

        if (detectIe()) {
            $('body').addClass('ie');
        }

        $('.credit-check-info-hold.fit-to-next-grid').each(function () {

            if ($(this).find('.grid-column').length % 2 !== 0) {
                $(this).parents('.grid-parent').next('.credit-check-info-hold').addClass('shift-to-basic-info');
                $(this).parents('.grid-parent').next('.grid-parent:not(.main-parent)').find('.credit-check-info-hold').addClass('shift-to-basic-info');
            }

        });

        setTimeout(function () {
            $('.credit-check-info-hold .dealnet-credit-check-section').each(function () {
                var col = $(this).parents('.col-md-6');
                var colOffset;
                if (col.not('.col-md-push-6')) {
                    colOffset = col.position().left;
                    if (colOffset == 0 && col.next('.col-md-6').length) {
                        col.addClass('has-right-border');
                    }
                }
                if (col.is('.col-md-push-6')) {
                    colOffset = col.next('.col-md-pull-6').position().left;
                    if (colOffset == 0 && col.next('.col-md-pull-6').length) {
                        col.next('.col-md-pull-6').addClass('has-right-border');
                    }
                }
            });
        }, 500);

        var mobileNav = $("#mobile-navigation");
        darkOverlay = $(".body-dark-overlay")
        mobileNav.find(".mobile-navigation__active-item").click(handleMobileNavigation);
        mobileNavDropdown = mobileNav.find(".mobile-navigation__dropdown");
        darkOverlay.click(handleMobileNavigation);

        $('#language').on('change', function (e) {
            window.location = e.target.value === 'en' ?
                changeToEnglishLink : changeToFrenchLink;
        });
    }

    function handleMobileNavigation() {
        if (mobileNavDropdown.hasClass("active")) {
            mobileNavDropdown.slideUp(300, function () {
                mobileNavDropdown.removeClass("active");
            });
            darkOverlay.fadeOut(300);

        } else {
            mobileNavDropdown.slideDown(300, function () {
                mobileNavDropdown.addClass("active");

            });
            darkOverlay.show().fadeTo(300, 0.3);
        }
    }

    function setDeviceClasses() {
        if (isMobile.any()) {
            if (viewport().width < 768) {
                $('body').addClass('mobile-device').removeClass('tablet-device');
            } else {
                $('body').addClass('tablet-device').removeClass('mobile-device');
            }
        } else {
            $('body').removeClass('mobile-device').removeClass('tablet-device');
        }
        if (isMobile.iOS()) {
            $('body').addClass('ios-device');
            $('.modal').removeClass('fade').addClass('fade-on-ios');
        } else {
            $('body').removeClass('ios-device');
        }
    }

    return init;
});

$.prototype.disableTab = function () {
    this.each(function () {
        $(this).attr('tabindex', '500');
    });
};

String.prototype.toDash = function () {
    return this.replace(/([A-Z])/g, function ($1) { return '-' + $1.toLowerCase(); });
};

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

function viewport() {
    var e = window, a = 'inner';
    if (!('innerWidth' in window)) {
        a = 'client';
        e = document.documentElement || document.body;
    }
    return { width: e[a + 'Width'], height: e[a + 'Height'] };
}

//Next two lines for IE
if (!window.console) window.console = {};
if (!window.console.log) window.console.log = function () { };
// global formatNumber function Todo: remove global variables
window.formatNumber = function (num) {
    if (typeof num === 'number') {
        return num.toFixed(2);
    }
    return num;
};

$(document).ready(function () {
    module.require('index')();
})