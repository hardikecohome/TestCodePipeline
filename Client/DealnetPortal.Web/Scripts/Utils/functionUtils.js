module.exports('functionUtils', function () {
    var compose = function () {
        var partials = Array.prototype.slice.call(arguments);
        var last = partials.length - 1;
        return function () {
            return partials.reduceRight(function (acc, partial, index) {
                if (index === last) return partial.apply(null, acc);
                return partial(acc);
            }, arguments);
        }
    };

    return {
        compose: compose,
    };
});