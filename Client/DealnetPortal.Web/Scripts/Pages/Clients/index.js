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

    var selected = [];

    function removeEquipment() {
        var value = $(this).val();
        if (value) {
            var index = selected.indexOf(value);
            if (index !== -1) {
                selected.splice(index, 1);
            }
        }

        $(this).parent().remove();
    }

    $('span.icon-remove').on('click', 'div.form-group', removeEquipment);

    // action handlers
    $('#improvment-equipment').on('change', function () {
        var equipmentValue = $(this).val();
        var equipmentText = $("#improvment-equipment :selected").text();
        if (equipmentValue && selected.indexOf(equipmentValue) === -1) {
            selected.push(equipmentValue);
            $('#improvement-types').append($('<li><input class="hidden" name="HomeImprovementTypes" value="' + equipmentValue + '">' + equipmentText + ' <span class="icon-remove" onclick="$(this).parent().remove()"><svg aria-hidden="true" class="icon icon-remove-cross"<use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-remove-cross"></use></svg></span></li>'));
        }
    });

    //handlers
    //basicInfoMandotaryFields.forEach(function(i) {
    //     $(i).on('change', checkForm(basicInfoBehavior));
    //});

    //addressInformationMandotaryFields.forEach(function(f) {
        
    //});
    //datepickers
    assignDatepicker($('#birth-date'));
    assignDatepicker($('#impvoment-date'));

    //license-scan
    $('#capture-buttons-1').on('click', takePhoto);
    $('#retake').on('click', retakePhoto);

    $('#living-time-checkbox').on('change', togglePreviousAddress);
    $("input[name$='improvement']").on('click', toggleInstallationAddress);
})