(function () {
    var modules = {};
    var require = function require (name) {
        if (!modules.hasOwnProperty(name)) {
            throw new Error('Module ' + name + 'is not in scope.');
        }

        if (modules[name].initialized) {
            if (__DEBUG__) console.log('module: ' + name + ' required.');
            return modules[name].cached;
        } else {
            if (__DEBUG__) console.log('module: ' + name + ' initialized.');
            modules[name].cached = modules[name].module(require);
            modules[name].initialized = true;
            return modules[name].cached;
        }
    };

    window.module = {
        exports: function (name, module) {
            if (modules.hasOwnProperty(name)) {
                throw new Error('Module ' + name + 'was already registered.');
            }

            modules[name] = {
                module: module,
                cached: {},
                initialized: false,
            };
        },
        require: require,
    };
})();