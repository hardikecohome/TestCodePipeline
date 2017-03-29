module.exports('objectUtils', function () {
    var shallowDiff = function (oldState, newState) {
        return Object.keys(oldState).some(function (key) {
            return oldState[key] !== newState[key];
        });
    };

    return {
        shallowDiff: shallowDiff,
    };
});