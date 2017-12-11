$(document).ready(function () {
    initDatepicker();
});

function inputDateFocus (input) {
    input.on('focus', function () {
        setTimeout(customDPSelect, 0);
        if (!navigator.userAgent.match(/(iPod|iPhone|iPad)/)) {
            $(this).blur()
                .addClass('focus');
        }
    });
}

function onDateSelect (input) {
    input.removeClass('focus');
    $('body').removeClass('bodyHasDatepicker');
}

function initDatepicker () {
    $('.date-group').each(function () {
        if ($('body').is('.ios-device') && $(this).children('.dealnet-disabled-input').length === 0) {
            $('<div/>', {
                class: 'div-datepicker-value',
                text: $(this).find('.form-control').val()
            }).appendTo(this);
        }
        if ($('body').is('.ios-device')) {
            $('<div/>', {
                class: 'div-datepicker',
            }).appendTo(this);
        }
    });

    $('.div-datepicker-value').on('click', function () {
        $('.div-datepicker').removeClass('opened');
        $(this).siblings('.div-datepicker').toggleClass('opened');
        if (!$('.div-datepicker .ui-datepicker-close').length) {
            addCloseButtonForInlineDatePicker();
        }
    });

    $.datepicker.setDefaults({
        dateFormat: 'mm/dd/yy',
        changeYear: true,
        changeMonth: (viewport().width < 768) ? true : false,
        showButtonPanel: true,
        closeText: translations['Cancel'],
        onSelect: function (value) {
            $(this).siblings('input.form-control').val(value).blur();
            $(".div-datepicker").removeClass('opened');
        },
        onChangeMonthYear: function () {
            $('.div-datepicker select').each(function () {
                $(this).blur();
            });
        },
        onClose: function () {
            onDateSelect($(this));
        }
    });
}

function assignDatepicker (selector, options) {
    var isIOS = $('body').is('.ios-device');
    var selected = $(selector);
    var input = isIOS ? selected.siblings('.div-datepicker') : selected;

    if (isIOS) {
        var dateGroup = selected.parents('.date-group');
        if (dateGroup.children('.dealnet-disabled-input').length === 0 && dateGroup.children('.div-datepicker-value').length === 0) {
            dateGroup.append($('<div/>', {
                class: 'div-datepicker-value',
                text: $(this).find('.form-control').val()
            }));
        }
        if (dateGroup.children('.div-datepicker').length === 0) {
            dateGroup.append($('<div/>', {
                class: 'div-datepicker',
            }));
        }

        dateGroup.children('.div-datepicker-value').on('click', function () {
            $('.div-datepicker').removeClass('opened');
            $(this).siblings('.div-datepicker').toggleClass('opened');
            if (!$('.div-datepicker .ui-datepicker-close').length) {
                addCloseButtonForInlineDatePicker();
            }
        });
    }

    inputDateFocus(input);

    input.datepicker(options);
    return input;
}

function getDatepickerDate (selector) {
    var input = $('body').is('.ios-device') ? $(selector).siblings('.div-datepicker') : $(selector);
    return input.datepicker('getDate');
}

function setDatepickerDate (selector, date) {
    var d = new Date(date);
    if (!isNaN(d.getTime())) {
        var input = $('body').is('.ios-device') ? $(selector).siblings('.div-datepicker') : $(selector);

        input.datepicker('setDate', date);
        if (!input.is('input')) {
            var formated = !date ? '' : (date.getMonth() + 1) + '/' + date.getDate() + '/' + date.getFullYear();
            input.siblings('input.form-control').val(formated).blur();
        }
        var formated = !date ? '' : (date.getMonth() + 1) + '/' + date.getDate() + '/' + date.getFullYear();
        input.siblings('input.form-control').val(formated).blur();
    }
}

function addCloseButtonForInlineDatePicker () {
    setTimeout(function () {
        $("<button>", {
            text: translations['Cancel'],
            type: 'button',
            class: "ui-datepicker-close ui-state-default ui-priority-primary ui-corner-all"
        }).appendTo($('.div-datepicker'));
        $('body').on('click', '.ui-datepicker-close', function () {
            $(".div-datepicker.opened").removeClass('opened');
        })
    }, 100);
}
