var configInitialized = $.Deferred();
$(document).ready(function () {
    var culture = $(document.documentElement).attr('lang');
    $.when(
        $.getJSON(urlContent + 'Content/cldr/supplemental/likelySubtags.json'),
        $.getJSON(urlContent + 'Content/cldr/supplemental/numberingSystems.json'),
        $.getJSON(urlContent + 'Content/cldr/main/' + culture + '/numbers.json')
    ).then(function() {
        return [].slice.apply(arguments, [0]).map(function(result) {
            return result[0];
        });
    }).then(Globalize.load)
        .then(function() {
            Globalize.locale(culture);

            $.validator.methods.number = function (value, element) {
                if (this.optional(element)) {
                    if (value === '' || typeof value === 'undefined') {
                        return true;
                    }
                }
                return !isNaN(Globalize.parseNumber(value));
            }

            var oldParseFloat = window.parseFloat;

            window.parseFloat = function(number) {
                if (typeof number === 'undefined') {
                    return number;
                }
                if (typeof number === 'number') {
                    return number;
                }
                var res = Globalize.parseNumber(number);
                return isNaN(res) ? oldParseFloat(number) : res;
            };

            window.formatNumber = Globalize.numberFormatter({
                maximumFractionDigits: 2,
                minimumFractionDigits: 2,
                round: 'round',
                useGrouping: false
            });
            configInitialized.resolve(true);
        });
});