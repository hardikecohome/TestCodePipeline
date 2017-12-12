$(document).ready(function () {

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

    $('#alertModal').on('hidden.bs.modal', function () {
        $('#confirmAlert').off('click');
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

// function updateModalHeightIpad () {

//     //If focus on input inside modal in new added blocks (which were display none when modal appears)

//     if ($('body').is('.ios-device.tablet-device') && viewport().width >= 768) {
//         $('input, textarea, [contenteditable=true], select').on({
//             focus: function () {
//                 setModalMarginForIpad();
//             },
//             blur: function () {
//                 resetModalDialogMarginForIpad();
//             }
//         });
//     }
// }

// function fixedOnKeyboardShownIos (fixedElem) {
//     var $fixedElement = fixedElem;
//     var topPadding = 10;

//     function fixFixedPosition () {
//         var absoluteTopCoord = ($(window).scrollTop() - fixedElem.parent().offset().top) + topPadding;

//         $fixedElement.addClass('absoluted-div').css({
//             top: absoluteTopCoord + 'px',
//         }).fadeIn('fast')
//     }

//     function resetFixedPosition () {
//         $fixedElement.removeClass('absoluted-div').css({
//             top: 60
//         });
//         $(document).off('scroll', updateScrollTop);
//         resetModalDialogMarginForIpad();
//     }

//     function updateScrollTop () {
//         var absoluteTopCoord = ($(window).scrollTop() - fixedElem.parent().offset().top) + topPadding;
//         $fixedElement.css('top', absoluteTopCoord + 'px');
//     }

//     $('input, textarea, [contenteditable=true], select').on({
//         focus: function () {
//             if ($(this).parents('.modal.in').length === 1) {
//                 setModalMarginForIpad();
//             } else {
//                 setTimeout(fixFixedPosition, 100);
//             }
//             $(document).scroll(updateScrollTop);
//         },
//         blur: resetFixedPosition
//     });
// }
