$(document)
            .ready(function () {
                sessionStorage.newEquipmets = 0;
                sessionStorage.newEquipmetTemplate = document.getElementById('new-equipment-base').innerHTML;
                sessionStorage.existingEquipmets = 0;
                sessionStorage.existingEquipmetTemplate = document.getElementById('existing-equipment-base').innerHTML;
                $("#new-equipment-remove-0").hide();
                $("#new-equipment-base .dealnet-middle-header").hide();

                assignDatepicker("#estimated-installation-date-0");
                $.validator.addMethod(
                    "date",
                    function (value, element) {
                        var minDate = Date.parse("1900-01-01");
                        var valueEntered = Date.parseExact(value, "M/d/yyyy");
                        if (!valueEntered) {
                            return false;
                        }
                        if (valueEntered < minDate) {
                            return false;
                        }
                        return true;
                    },
                    "Please enter a valid date!"
                );
            });

function addNewEquipment() {
    sessionStorage.newEquipmets = Number(sessionStorage.newEquipmets) + 1;
    var newDiv = document.createElement('div');
    newDiv.innerHTML = sessionStorage.newEquipmetTemplate.split("NewEquipment[0]").join("NewEquipment[" + sessionStorage.newEquipmets + "]")
        .split("NewEquipment_0").join("NewEquipment_" + sessionStorage.newEquipmets).split("estimated-installation-date-0").join("estimated-installation-date-" + sessionStorage.newEquipmets)
        .replace("#new-equipment-0", "#new-equipment-" + sessionStorage.newEquipmets)
        .replace("№1", "№" + (Number(sessionStorage.newEquipmets) + 1));
    //console.log(newDiv.innerHTML);
    newDiv.id = "new-equipment-" + sessionStorage.newEquipmets;
    document.getElementById('new-equipments').appendChild(newDiv);
    assignDatepicker("#estimated-installation-date-" + sessionStorage.newEquipmets);
    resetFormValidator("#equipment-form");
}

function addExistingEquipment() {
    var newDiv = document.createElement('div');
    newDiv.innerHTML = sessionStorage.existingEquipmetTemplate.split("ExistingEquipment[0]").join("ExistingEquipment[" + sessionStorage.existingEquipmets + "]")
        .split("ExistingEquipment_0").join("ExistingEquipment_" + sessionStorage.existingEquipmets)
        .replace("#existing-equipment-0", "#existing-equipment-" + sessionStorage.existingEquipmets)
        .replace("№1", "№" + (Number(sessionStorage.existingEquipmets) + 1));
    newDiv.id = "existing-equipment-" + sessionStorage.existingEquipmets;
    document.getElementById('existing-equipments').appendChild(newDiv);
    resetFormValidator("#equipment-form");
    sessionStorage.existingEquipmets = Number(sessionStorage.existingEquipmets) + 1;

    customizeSelect();
}

function resetFormValidator(formId) {
    $(formId).removeData('validator');
    $(formId).removeData('unobtrusiveValidation');
    $.validator.unobtrusive.parse(formId);
}

function assignDatepicker(inputId) {
    $(inputId).datepicker({
        dateFormat: 'mm/dd/yy',
        changeMonth: true,
        changeYear: true,
        yearRange: '1900:2116',
        minDate: Date.parse("1900-01-01"),
        maxDate: new Date()
    });
}