module.exports('loader', function () {
    function showLoader (loadingText) {
        var classes = loadingText ? 'hasText loader' : 'loader';
        $.loader({
            className: classes,
            content: loadingText,
            width: 101,
            height: 100
        });
    }

    function hideLoader () {
        $.loader('close');
    }
    return {
        showLoader: showLoader,
        hideLoader: hideLoader
    };
});
