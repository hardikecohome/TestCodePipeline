var configInitialized = $.Deferred();
$(document).ready(function () {
    var culture = $(document.documentElement).attr('lang');
    $.when(
        $.getJSON(urlContent + 'Content/cldr/supplemental/likelySubtags.json'),
        $.getJSON(urlContent + 'Content/cldr/supplemental/numberingSystems.json'),
        $.getJSON(urlContent + 'Content/cldr/supplemental/currencyData.json'),
        $.getJSON(urlContent + 'Content/cldr/main/' + culture + '/numbers.json'),
        $.getJSON(urlContent + 'Content/cldr/main/' + culture + '/currencies.json')
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

            var parseFloat = window.parseFloat
            window.parseFloat = function(number) {
                if (typeof number === 'undefined') {
                    return number;
                }
                if (typeof number === 'number') {
                    return number;
                }
                if (typeof number === 'string' && !/^[0-9]*$/.test(number)) {
                    return parseFloat(number);
                }

                return Globalize.parseNumber(number);
            };

            window.formatNumber = Globalize.numberFormatter({
                maximumFractionDigits: 2,
                minimumFractionDigits: 2,
                round: 'round',
                useGrouping: false
            });

            window.formatCurrency = Globalize.currencyFormatter('CAD');
            configInitialized.resolve(true);
        });
});