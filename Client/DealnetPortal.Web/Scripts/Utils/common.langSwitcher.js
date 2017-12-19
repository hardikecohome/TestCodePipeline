$(function() {
    $('.chosen-language-link').on('click', function () {
        $(this).parents('.lang-switcher').toggleClass('open');
        return false;
    });

    //If opened switch language dropdown, hide it when click anywhere accept opened dropdown
    $('html').on('click touchstart', function (event) {
        if ($('.navbar-header .lang-switcher.open').length > 0 && $(event.target).parents('.lang-switcher').length == 0) {
            $('.lang-switcher').removeClass('open');
        }
    });

})