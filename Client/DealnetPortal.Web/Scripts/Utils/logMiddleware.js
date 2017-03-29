module.exports('logMiddleware', function (require) {
    return function logMiddleware (ns) {
        var logger = require('logger')(ns);
        return function (store) {
            return function (next) {
                return function (action) {
                    var nextAction = next(action);
                    if (window.__DEBUG__) {
                        var prevState = store.getState();
                        var state = store.getState()
                        logger.log('prev: ', prevState);
                        logger.log('action: ', action);
                        logger.log('next: ', state);
                    }
                    return nextAction;
                };
            };
        };
    };
});