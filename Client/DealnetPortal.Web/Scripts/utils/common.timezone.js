module.exports('common.timezone', function () {
    var settings = {
        cookieName: "timezoneoffset"
    };

    var createTimezoneCookie = function() {

        if (!Cookies.get(settings.cookieName)) {
            Cookies.set(settings.cookieName, new Date().getTimezoneOffset());
        }
        else {
            var storedOffset = parseInt(Cookies.get(settings.cookieName));
            var currentOffset = new Date().getTimezoneOffset();

            if (storedOffset !== currentOffset) {
                Cookies.set(settings.cookieName, currentOffset);
            }
        }
    }

    return {
        createTimezoneCookie: createTimezoneCookie
    }
});