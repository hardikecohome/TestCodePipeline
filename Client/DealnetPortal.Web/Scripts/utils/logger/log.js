module.exports('logger', function () {
    var noop = function () { };
    return function () {
        return {
            log: noop,
            info: noop,
            error: noop,
        };
    };
});