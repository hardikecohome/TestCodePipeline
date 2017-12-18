module.exports('scrollPosition', function () {

    function saveScrollPosition() {
        var $body = $('body');
        //if open one modal right after other one
        var topOffset = $(window).scrollTop();
        $body.css('top', -topOffset);
    }

    function resetScrollPosition() {
        var $body = $('body');
        var bodyOffset = Math.abs(parseInt($body.css('top')));

        $body.css({
            'top': 'auto'
        });

        $('html, body').scrollTop(bodyOffset);
    }

    return {
        save: saveScrollPosition,
        reset: resetScrollPosition
    };
});
