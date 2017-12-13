$(document).ready(function () {
    var saveScrollPosition = module.require('scrollPosition').save;
    var resetScrollPosition = module.require('scrollPosition').reset;

    $(document).on('show.bs.modal', function () {
        saveScrollPosition();
        toggleClearInputIcon();
    }).on('shown.bs.modal', function () {
        $('textarea').each(function () {
            has_scrollbar($(this), 'textarea-has-scroll');
        });
    }).on('hidden.bs.modal', function () {
        if (isMobile.iOS()) {
            $('.div-datepicker').removeClass('opened');
            resetScrollPosition();
            if (viewport().width >= 768) {
                resetModalDialogMarginForIpad();
            }
        } else {
            if ($('.modal:visible').length == 0) {
                resetScrollPosition();
            }
        }
    });

    $('.overlay').click(function () {
        $('.navbar-toggle').trigger('click');
        $('body').removeClass('open-menu');
        resetScrollPosition();
        $(this).hide();
        setTimeout(function () {
            $('body').removeClass('menu-animated');
        }, 400);
    });
});

function resetModalDialogMarginForIpad () {
    $('.modal.in').find('.modal-dialog').css({
        'margin-bottom': 30 + 'px'
    });
}

function setModalMarginForIpad () {
    if (window.innerHeight < window.innerWidth) {
        keyboardHeight = 60
    } else {
        keyboardHeight = 40
    }
    $('.modal.in').find('.modal-dialog').css({
        'margin-bottom': keyboardHeight + 'vh'
    });
}
