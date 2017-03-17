var configInitialized = $.Deferred();
$(document).ready(function() {
    var culture = $(document.documentElement).attr('lang');
    $.when(
        $.getJSON('/Content/cldr/supplemental/likelySubtags.json'),
        $.getJSON('/Content/cldr/supplemental/numberingSystems.json'),
        $.getJSON('/Content/cldr/main/' + culture + '/numbers.json')
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
                return !Number.isNaN(Globalize.parseNumber(value));
            }

            window.parseFloat = Globalize.parseNumber.bind(Globalize);
            configInitialized.resolve(true);
        });
});