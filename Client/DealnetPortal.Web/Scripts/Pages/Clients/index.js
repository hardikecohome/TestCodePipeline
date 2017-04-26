module.exports('new-customer', function(require) {
    var assignDatepicker = require('new-customer-ui').assignDatepicker;
    var togglePreviousAddress = require('new-customer-ui').togglePreviousAddress;
    var toggleInstallationAddress = require('new-customer-ui').toggleInstallationAddress;

    //handlers

    //datepickers
    assignDatepicker($('#birth-date'));

    //license-scan
    $('#capture-buttons-1').on('click', takePhoto);
    $('#retake').on('click', retakePhoto);

    $('#living-time-checkbox').on('change', togglePreviousAddress);
    $("input[name$='improvement']").on('click', toggleInstallationAddress);
})