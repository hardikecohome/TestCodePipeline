configInitialized
    .then(function () {

        $('#existing-notes-default').text("").attr("id", "ExistingEquipment_0__Notes");
        sessionStorage.existingEquipmetTemplate = document.getElementById('existing-equipment-base').innerHTML;
        $("#existing-equipment-base").remove();

        var datepickerOptions = {
            yearRange: '1900:2200',
            minDate: new Date()
        };
        $('.date-input').each(function (index, input) {
            assignDatepicker(input, datepickerOptions);
        });
        $.validator.addMethod(
            "date",
            function (value, element) {
                var minDate = new Date("1900-01-01");
                var valueEntered = Date.parseExact(value, "M/d/yyyy");
                if (!valueEntered) {
                    return false;
                }
                if (valueEntered < minDate) {
                    return false;
                }
                return true;
            },
            translations['EnterValidDate']
        );
    });

function addExistingEquipment () {
    var nextNumber = Number(sessionStorage.existingEquipmets) + 1;
    var newDiv = document.createElement('div');
    newDiv.innerHTML = sessionStorage.existingEquipmetTemplate.split("ExistingEquipment[0]").join("ExistingEquipment[" + sessionStorage.existingEquipmets + "]")
        .split("ExistingEquipment_0").join("ExistingEquipment_" + sessionStorage.existingEquipmets)
        .replace("removeExistingEquipment(0)", "removeExistingEquipment(" + sessionStorage.existingEquipmets + ")")
        .replace("existing-equipment-remove-0", "existing-equipment-remove-" + sessionStorage.existingEquipmets)
        .replace("№1", "№" + (nextNumber));
    newDiv.id = "existing-equipment-" + sessionStorage.existingEquipmets;
    document.getElementById('existing-equipments').appendChild(newDiv);
    resetFormValidator("#equipment-form");
    customizeSelect();
    toggleClearInputIcon($(newDiv).find('textarea, input'));
    sessionStorage.existingEquipmets = nextNumber;
    resetPlacehoder($(newDiv).find('textarea, input'));
}

function removeExistingEquipment (id) {
    $('#existing-equipment-' + id).remove();
    var nextNumber = Number(id);
    while (true) {
        nextNumber++;
        var existingEquipment = $('#existing-equipment-' + nextNumber);
        if (!existingEquipment.length) { break; }

        var labels = existingEquipment.find('label');
        labels.each(function () {
            $(this).attr('for', $(this).attr('for').replace('ExistingEquipment_' + nextNumber, 'ExistingEquipment_' + nextNumber - 1));
        });
        var inputs = existingEquipment.find('input, select, textarea');
        inputs.each(function () {
            $(this).attr('id', $(this).attr('id').replace('ExistingEquipment_' + nextNumber, 'ExistingEquipment_' + (nextNumber - 1)));
            $(this).attr('name', $(this).attr('name').replace('ExistingEquipment[' + nextNumber, 'ExistingEquipment[' + (nextNumber - 1)));
        });
        var spans = existingEquipment.find('span');
        spans.each(function () {
            var valFor = $(this).attr('data-valmsg-for');
            if (valFor == null) { return; }
            $(this).attr('data-valmsg-for', valFor.replace('ExistingEquipment[' + nextNumber, 'ExistingEquipment[' + (nextNumber - 1)));
        });
        existingEquipment.find('.equipment-number').text('№' + nextNumber);
        var removeButton = existingEquipment.find('#existing-equipment-remove-' + nextNumber);
        removeButton.attr('onclick', removeButton.attr('onclick').replace('removeExistingEquipment(' + nextNumber, 'removeExistingEquipment(' + (nextNumber - 1)));
        removeButton.attr('id', 'existing-equipment-remove-' + (nextNumber - 1));
        existingEquipment.attr('id', 'existing-equipment-' + (nextNumber - 1));
        resetFormValidator("#equipment-form");
    }
    sessionStorage.existingEquipmets = Number(sessionStorage.existingEquipmets) - 1;
};

$(document).ready(function () {
    $('#equipment-form').submit(function (event) {
        $('#equipment-form').validate();
        if (!$('#equipment-form').valid()) {
            event.preventDefault();
        }
    });
});

function resetFormValidator (formId) {
    $(formId).removeData('validator');
    $(formId).removeData('unobtrusiveValidation');
    $.validator.unobtrusive.parse(formId);
}
