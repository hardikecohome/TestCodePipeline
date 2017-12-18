module.exports('setEqualHeightRows', function () {
    return function setEqualHeightRows(row) {
        var maxHeight = 0;
        row = $(row);
        row.each(function () {
            if ($(this).children().eq(0).outerHeight(true) > maxHeight) {
                maxHeight = $(this).children().eq(0).outerHeight(true);
            }
        });
        row.height(maxHeight);
    }
});
