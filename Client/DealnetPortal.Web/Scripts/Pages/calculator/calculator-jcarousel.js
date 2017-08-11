module.exports('calculator-jcarousel', function(require) {
    
    function refreshCarouselItems() {
        var number = $('.jcarousel-pagination').find('.active').text();
        $('.jcarousel-pagination').jcarouselPagination('reloadCarouselItems');
        $('.jcarousel').jcarousel('reload');
        $('.jcarousel-pagination').find('.active').removeClass('active');
        $('.jcarousel-pagination').find('a:contains("' + number + '")').addClass('active');


        if (viewport().width >= 1024) {
            $('.control-add-calc').css('left', $('.rate-card-col').length * $('.rate-card-col').outerWidth() - 5);
        } else {
            $('.control-add-calc').css('left', 'auto');
        }
    }

    function carouselRateCards() {
        var windowWidth = $(window).width();
        var paginationItems;
        var targetSlides;
        if (windowWidth >= 1024) {
            paginationItems = 3;
            targetSlides = 0;
        } else if (windowWidth >= 768) {
            paginationItems = 2;
            targetSlides = 2;
        } else {
            paginationItems = 1;
            targetSlides = 1;
        }

        var jcarousel = $('.rate-cards-container:not(".one-rate-card") .jcarousel');
        var carouselItemsToView = viewport().width >= 768 && viewport().width < 1024 ? 2 : viewport().width < 768 ? 1 : 3;
        jcarousel
            .on('jcarousel:reload jcarousel:create', function () {
                var carousel = $(this),
                    carouselWidth = carousel.innerWidth(),
                    width = carouselWidth / carouselItemsToView;

                carousel.jcarousel('items').css('width', Math.ceil(width) + 'px');
            }).jcarousel();

        if (viewport().width < 1024) {
            jcarousel.swipe({
                //Generic swipe handler for all directions
                swipe: function (event, direction, distance, duration, fingerCount, fingerData) {
                    $('.link-over-notify').each(function () {
                        if ($(this).attr('aria-describedby')) {
                            $(this).click();
                        }
                    });

                    if (direction === "left") {
                        jcarousel.jcarousel('scroll', '+=' + carouselItemsToView);
                    } else if (direction === "right") {
                        jcarousel.jcarousel('scroll', '-=' + carouselItemsToView);
                    } else {
                        event.preventDefault();
                    }
                },
                excludedElements: "button, input, select, textarea, .noSwipe, a",
                threshold: 50,
                allowPageScroll: "auto",
                triggerOnTouchEnd: false
            });
        }


        $('.jcarousel-control-prev')
            .jcarouselControl({
                target: '-=' + targetSlides
            });

        $('.jcarousel-control-next')
            .jcarouselControl({
                target: '+=' + targetSlides
            });

        $('.jcarousel-pagination')
            .on('jcarouselpagination:active', 'a', function () {
                $(this).addClass('active');
                if ($(this).is(':first-child')) {
                    $('.jcarousel-control-prev').addClass('disabled');
                } else {
                    $('.jcarousel-control-prev').removeClass('disabled');
                }
                if ($(this).is(':last-child')) {
                    $('.jcarousel-control-next').addClass('disabled');
                } else {
                    $('.jcarousel-control-next').removeClass('disabled');
                }

            })
            .on('jcarouselpagination:inactive', 'a', function () {
                $(this).removeClass('active');
            })
            .on('click', function (e) {
                e.preventDefault();
            })
            .jcarouselPagination({
                perPage: paginationItems,
                item: function (page) {
                    return '<a href="#' + page + '">' + page + '</a>';
                }
            });
    }

    return {
        refreshCarouselItems: refreshCarouselItems,
        carouselRateCards: carouselRateCards
    }
})