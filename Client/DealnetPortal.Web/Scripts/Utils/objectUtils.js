module.exports('objectUtils', function () {
    var shallowDiff = function (oldState, newState) {
        return Object.keys(oldState).some(function (key) {
            return oldState[key] !== newState[key];
        });
    };

    var concatObj = function (acc, next) {
        return next ? $.extend(acc, next) : acc;
    };

    var mapObj = function (obj) {
        return function(key) {
            return obj.hasOwnProperty(key) ? obj[key] : null;
        };
    };

    var destructObj = function(obj) {
        return Object.keys(obj).map(function(key) {
            var tmpObj = {};
            tmpObj[key] = obj[key];
        });
    };

    var filterObj = function(filter) {
        return function(obj) {
            return Object.keys(obj)
                .filter(function(key) {
                    return filter(key, obj);
                });
        };
    };

    return {
        shallowDiff: shallowDiff,
        concatObj: concatObj,
        mapObj: mapObj,
        destructObj: destructObj,
        filterObj: filterObj,
    };
});