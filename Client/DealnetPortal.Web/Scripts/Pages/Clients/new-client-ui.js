module.exports('new-client-ui', function (require) {
    // view layer

    var assignDatepicker = function (input) {

        inputDateFocus(input);

        input.datepicker({
            dateFormat: 'mm/dd/yy',
            changeYear: true,
            changeMonth: (viewport().width < 768) ? true : false,
            yearRange: '1900:' + (new Date().getFullYear() - 18),
            minDate: Date.parse("1900-01-01"),
            maxDate: new Date(new Date().setFullYear(new Date().getFullYear() - 18)),
            onClose: function () {
                onDateSelect($(this));
            }
        });
    };

    var togglePreviousAddress = function () {
        if ($(this).is(':checked')) {
            $('#previous-address').show();
        } else {
            $('#previous-address').hide();
        }
    };

    var toggleInstallationAddress = function () {
        if ($(this).is("#houseCustomerChosen")) {
            $('#installation-address').show();
        } else {
            $('#installation-address').hide();
        }
    };

    return {
        assignDatepicker: assignDatepicker,
        togglePreviousAddress: togglePreviousAddress,
        toggleInstallationAddress: toggleInstallationAddress
    }
});