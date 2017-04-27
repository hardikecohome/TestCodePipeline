module.exports('new-customer', function(require) {
    var assignDatepicker = require('new-client-ui').assignDatepicker;
    var togglePreviousAddress = require('new-client-ui').togglePreviousAddress;
    var toggleInstallationAddress = require('new-client-ui').toggleInstallationAddress;
    var basicInfoMandotaryFields = ['#first-name', '#last-name', '#birth-date'];
    var addressInformationMandotaryFields = ['#street', '#locality', '#province', '#postal_code'];

    var basicInfoBehavior = function () {

        if (!$('#additional-infomration').is('.active-panel')) {
            $('#basic-information').removeClass('active-panel');
            $('#additional-infomration').removeClass('panel-collapsed');
            $('#additional-infomration').addClass('active-panel');
        }
    }
    var checkForm = function (isValidBehavior) {
        var isValid;
        basicInfoMandotaryFields.forEach(function(field) {
            isValid = $(field).valid();
        });

        if (isValid) isValidBehavior();
    }


    //handlers
    basicInfoMandotaryFields.forEach(function(i) {
         $(i).on('change', checkForm(basicInfoBehavior));
    });

    addressInformationMandotaryFields.forEach(function(f) {
        
    });
    //datepickers
    assignDatepicker($('#birth-date'));

    //license-scan
    $('#capture-buttons-1').on('click', takePhoto);
    $('#retake').on('click', retakePhoto);

    $('#living-time-checkbox').on('change', togglePreviousAddress);
    $("input[name$='improvement']").on('click', toggleInstallationAddress);
})