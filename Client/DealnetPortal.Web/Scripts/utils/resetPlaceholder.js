module.exports('resetPlaceholder', function () {
    return function resetPlaceholder (elems) {
        elems.removeClass('placeholder');
        setTimeout(function () {
            elems.placeholder();
        }, 0);
    }
});
