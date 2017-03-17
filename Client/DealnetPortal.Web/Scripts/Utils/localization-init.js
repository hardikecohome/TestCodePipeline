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

            $.validator.methods.number = function(value, element) {
                return this.optional(element) || jQuery.isNumeric(Globalize.parseFloat(value));
            }
        });
});