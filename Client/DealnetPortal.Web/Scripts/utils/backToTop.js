module.exoprts('backToTop', function () {
    function toggleBackToTopVisibility() {
        if ($('.dealnet-body').height() > 1000) {
            $('.back-to-top-hold').show();
        } else {
            $('.back-to-top-hold').hide();
        }
    }

    function backToTop() {
        $("html,body").animate({ scrollTop: 0 }, 1000);
        return false;
    };

    return {
        toggleBackToTopVisibility: toggleBackToTopVisibility,
        backToTop: backToTop
    };
});
