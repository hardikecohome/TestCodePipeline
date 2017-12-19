$(function () {
    var settings = {
        cookieName: "timezoneoffset"
    };

    createTimezoneCookie();

    function createTimezoneCookie() {
        if (!Cookies.get(settings.cookieName)) {
            Cookies.set(settings.cookieName, new Date().getTimezoneOffset());
        } else {
            var storedOffset = parseInt(Cookies.get(settings.cookieName));
            var currentOffset = new Date().getTimezoneOffset();

            if (storedOffset !== currentOffset) {
                Cookies.set(settings.cookieName, currentOffset);
            }
        }
    };
});