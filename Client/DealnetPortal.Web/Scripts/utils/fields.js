$(document).ready(function () {
    addIconsToFields();
    toggleClearInputIcon();
    customizeSelect();

    $('body').on('click', '.recover-pass-link', recoverPassword);

    $('.dealnet-disabled-input, input.control-disabled, textarea.control-disabled, select.control-disabled').each(function () {
        $(this).attr('type', 'hidden');
        var inpValue = $(this).is('select') ? $(this).find("option:selected").text() : $(this).is('textarea') ? $(this).text() : $(this).val();
        if ($(this).is('.control-disabled')) {
            $(this).after($('<div/>', {
                class: "control-disabled",
                text: inpValue
            }));
        } else {
            $(this).after($('<div/>', {
                class: "dealnet-disabled-input dealnet-disabled-input-value",
                html: inpValue.replace(/\r?\n/g, '<br />')
            }));
        }
    });

    var resizeInt = null;
    $('textarea').each(function () {
        var textField = $(this);
        setTimeout(function () {
            has_scrollbar(textField, 'textarea-has-scroll');
        }, 100);

        textField.on("mousedown", function (e) {
            resizeInt = setInterval(function () {
                has_scrollbar(textField, 'textarea-has-scroll');
            }, 1000 / 15);
        });
    });

    $('textarea').on('keyup', function () {
        has_scrollbar($(this), 'textarea-has-scroll');
    });

    $('select').each(function (i, el) {
        if (!el.value) {
            $(el).addClass('not-selected');
        }
    });
    $('body').on('change', 'select', function () {
        if (this.value) {
            $(this).removeClass('not-selected');
        } else {
            $(this).addClass('not-selected');
        }
    });

    $(window).on("mouseup", function (e) {
        if (resizeInt !== null) {
            clearInterval(resizeInt);
        }
    });

    //Apply function placeholder for ie browsers
    $("input, textarea").placeholder();
});

function addIconsToFields (fields) {
    var localFields = fields || ($('.control-group input, .control-group textarea'));
    var fieldDateParent = localFields.parent('.control-group.date-group');
    var fieldPassParent = localFields.parent('.control-group.control-group-pass, .control-group.control-hidden-value');
    var iconCalendar = '<svg aria-hidden="true" class="icon icon-calendar"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-calendar"></use></svg>';
    var iconClearField = '<a class="clear-input"><svg aria-hidden="true" class="icon icon-remove"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-remove"></use></svg></a>';
    var iconPassField = '<a class="recover-pass-link"><svg aria-hidden="true" class="icon icon-eye"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-eye"></use></svg></a>';

    if (fieldDateParent.length && fieldDateParent.children('.icon-calendar').length === 0) {
        fieldDateParent.append(iconCalendar);
    }

    localFields.each(function () {
        var $this = $(this);
        var fieldParent = $this.parent('.control-group').not(fieldDateParent).not(fieldPassParent);
        if (!$this.is(".dealnet-disabled-input") && !$this.is(".form-control-not-clear") && $this.attr("type") !== "hidden") {
            if (fieldParent.children('.clear-input').length === 0) {
                fieldParent.append(iconClearField);
            }
        }
    });

    if (fieldPassParent.length && fieldPassParent.children('.recover-pass-link').length === 0) {
        fieldPassParent.append(iconPassField);
    }

    setTimeout(function () {
        localFields.each(function () {
            toggleClickInp($(this));
        });
    }, 100);
}

function toggleClearInputIcon (fields) {
    var localFields = fields || $('.control-group input, .control-group textarea');
    var fieldParent = localFields.parent('.control-group:not(.date-group):not(.control-group-pass)');
    localFields.each(function () {
        toggleClickInp($(this));
    });
    localFields.on('keyup', function () {
        toggleClickInp($(this));
    });
    fieldParent.find('.clear-input').on('click', function () {
        $(this).siblings('input, textarea').val('').change().keyup();
        $(this).hide();
    });
}

function toggleClickInp (inp) {
    if (inp.val().length !== 0) {
        inp.siblings('.clear-input').css('display', 'block');
    } else {
        inp.siblings('.clear-input').hide();
    }
}

function customDPSelect (elem) {
    var selectClasses = "custom-select datepicker-select";
    if ($('select.ui-datepicker-month').length && !$('.ui-datepicker-month').parents('.custom-select').length) {
        $('.ui-datepicker-month')
            .wrap($('<div>', {
                class: selectClasses
            })).after('<span class="caret">');
    }
    if ($('select.ui-datepicker-year').length && !$('.ui-datepicker-year').parents('.custom-select').length) {
        $('.ui-datepicker-year')
            .wrap($('<div>', {
                class: selectClasses
            })).after('<span class="caret">');
    }
    if ($('select.ui-datepicker-month').length) {
        $('.ui-datepicker-prev, .ui-datepicker-next').hide();
    }
}

function customizeSelect () {
    setTimeout(function () {
        $('select').each(function () {
            //Added opt group to each select to fix long value inside option for IOS.
            if ($('body').is('.ios-device') && $(this).find('optgroup').length === 0) {
                $('<optgroup label=""></optgroup>').appendTo($(this));
            }
            var selectClasses = $(this).hasClass("dealnet-disabled-input") || $(this).hasClass("control-disabled") ? "custom-select-disabled" : "custom-select";
            if (!$(this).parents(".ui-datepicker").length && !$(this).parents(".custom-select").length && !$(this).parents(".custom-select-disabled").length) {
                $(this).wrap('<div class=' + selectClasses + '>');
                if (detectIE() === false) {
                    $(this).after('<span class="caret">');
                }
            }
        });

        $('select.dealnet-disabled-input').disableTab();
    }, 300);
}

function recoverPassword (e) {
    var pass = $(e.target).parents('.control-group').find('input');
    if (pass.prop('type') == "password") {
        pass.prop('type', 'text');
    } else {
        pass.prop('type', 'password');
    }
    return false;
}

function has_scrollbar (elem, className) {
    elem_id = elem.attr('id');
    if (elem[0].clientHeight < elem[0].scrollHeight)
        elem.parents('.control-group').addClass(className);
    else
        elem.parents('.control-group').removeClass(className);
}
