$(function () {
    if (customerDealsCountUrl) {
        $.ajax({
            cache: false,
            type: "GET",
            url: customerDealsCountUrl,
            success: function (json) {
                if (json && json.dealsCount && json.dealsCount !== 0) {
                    $('.new-deals-number').text(json.dealsCount + ' ' + translations['new']);
                } else {
                    $('.new-deals-number').parent().hide();
                }
            }
        });
    }
});