module.exports('clearAddress', function () {
    return function clearAddress() {
            $(this).parents('.address-container').find('input, select').each(function () {
                if ($(this).not('.placeholder')) {
                    $(this).val("").change();
                }
            });
            return false;
    }
});
