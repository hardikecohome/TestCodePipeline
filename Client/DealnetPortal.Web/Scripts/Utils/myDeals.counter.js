$(function () {
    if (customerDealsCountUrl) {
        $.ajax({
            cache: false,
            type: "GET",
            url: customerDealsCountUrl,
            success: function (json) {
                var number = $('#new-deals-number');
                if (json && json.dealsCount && json.dealsCount !== 0) {
                    number.text(json.dealsCount + ' ' + translations['new']);
                    number.show();
                } else {
                    number.hide();
                }
            }
        });
    }
});