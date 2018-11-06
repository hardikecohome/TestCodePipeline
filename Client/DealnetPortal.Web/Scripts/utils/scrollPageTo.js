module.exports('scrollPageTo', function () {
    return function scrollPageTo(elem) {
        if (elem.offset().top < $(window).scrollTop() || elem.offset().top > $(window).scrollTop() + window.innerHeight) {
            $('html, body').animate({
                scrollTop: elem.offset().top - elem.outerHeight(true) - 10
            }, 2000);
        }
    };
});
