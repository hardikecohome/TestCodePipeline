module.exports('logger', function () {
    var ns = [];
    var enabled = [];

    window.logger = {
        enable: function (nameSpace) {
            if (nameSpace === '**') {
                enabled = ns.slice();
            } else {
                enabled.push(nameSpace);
            }
        },
        disable: function (nameSpace) {
            if (nameSpace === '**') {
                enabled = [];
            } else {
                var index = enabled.indexOf(nameSpace);
                if (index !== -1) {
                    enabled.splice(index, 1);
                }
            }
        },
    };

    var log = function (nameSpace) {
        return function () {
            if (enabled.indexOf(nameSpace) !== -1)
                console.log.apply(console, arguments);
        };
    };
    var error = function (nameSpace) {
        return function () {
            if (enabled.indexOf(nameSpace) !== -1)
                console.error.apply(console, arguments);
        };
    };
    var info = function (nameSpace) {
        return function () {
            if (enabled.indexOf(nameSpace) !== -1)
                console.info.apply(console, arguments);
        };
    };

    return function (newNameSpace) {
        if (ns.indexOf(newNameSpace) === -1) {
            ns.push(newNameSpace);
        }

        return {
            log: log(newNameSpace),
            error: error(newNameSpace),
            info: info(newNameSpace),
        };
    };
});