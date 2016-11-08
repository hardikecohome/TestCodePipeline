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

                $("#admin-fee").rules("add", "required");
                $("#down-payment").rules("add", "required");
                $("#customer-rate").rules("add", "required");
                $("#amortization-term").rules("add", "required");
                var initAgreementType = $("#agreement-type").find(":selected").val();
                manageAgreementElements(initAgreementType);
                $("#agreement-type").change(function () {
                    manageAgreementElements($(this).find(":selected").val());
                });
                if (initAgreementType === '0') {
                    recalculateTotalCashPrice();
                }
            });

function manageAgreementElements(agreementType) {
    switch (agreementType) {
        case '0':
            $(".equipment-cost").prop("disabled", false);
            $(".equipment-cost").each(function () {
                $(this).rules("add", "required");
            });
            $(".monthly-cost").prop("disabled", true);
            $(".monthly-cost").each(function () {
                var input = $(this);
                input.rules("remove", "required");
                input.removeClass('input-validation-error');
                input.next('.text-danger').empty();
            });
            $('.loan-element').show();
            $('.rental-element').hide();
            $('#total-monthly-payment').rules("remove", "required");
            $('#requested-term-label').text("Loan Term");
            break;
        case '1':
        case '2':
            $(".equipment-cost").prop("disabled", true);
            $(".equipment-cost").each(function() {
                var input = $(this);
                input.rules("remove", "required");
                input.removeClass('input-validation-error');
                input.next('.text-danger').empty();
            });
            $(".monthly-cost").prop("disabled", false);
            $(".monthly-cost").each(function () {
                $(this).rules("add", "required");
            });
            $('.loan-element').hide();
            $('.rental-element').show();
            $('#total-monthly-payment').rules("add", "required");
            $('#requested-term-label').text("Requested Term");
            break;
    }
}

function addNewEquipment() {
    sessionStorage.newEquipmets = Number(sessionStorage.newEquipmets) + 1;
    var newDiv = document.createElement('div');
    newDiv.className = 'new-equipment-wrap';
    newDiv.innerHTML = sessionStorage.newEquipmetTemplate.split("NewEquipment[0]").join("NewEquipment[" + sessionStorage.newEquipmets + "]")
        .split("NewEquipment_0").join("NewEquipment_" + sessionStorage.newEquipmets).split("estimated-installation-date-0").join("estimated-installation-date-" + sessionStorage.newEquipmets)
        .replace("#new-equipment-0", "#new-equipment-" + sessionStorage.newEquipmets)
        .replace("№1", "№" + (Number(sessionStorage.newEquipmets) + 1));
    //console.log(newDiv.innerHTML);
    newDiv.id = "new-equipment-" + sessionStorage.newEquipmets;
    document.getElementById('new-equipments').appendChild(newDiv);
    assignDatepicker("#estimated-installation-date-" + sessionStorage.newEquipmets);
    resetFormValidator("#equipment-form");
    manageAgreementElements($("#agreement-type").find(":selected").val());
    customizeSelect();
}

function addExistingEquipment() {
    sessionStorage.existingEquipmets = Number(sessionStorage.existingEquipmets) + 1;
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

function recalculateTotalMonthlyPayment() {
    var sum = 0;
    $(".monthly-cost").each(function() {
        var numberValue = parseFloat(this.value);
        if (!isNaN(numberValue)) {
            sum += numberValue;
        }
    });
    
    $("#total-monthly-payment").val(sum);
}

function recalculateTotalCashPrice() {
    var sum = 0;
    $(".equipment-cost").each(function () {
        var numberValue = parseFloat(this.value);
        if (!isNaN(numberValue)) {
            sum += numberValue;
        }
    });

    $("#equipment-cash-price").text(sum);
    calculateLoanValues();
}

function resetFormValidator(formId) {
    $(formId).removeData('validator');
    $(formId).removeData('unobtrusiveValidation');
    $.validator.unobtrusive.parse(formId);
}

function assignDatepicker(inputId) {
    var input = $(inputId);
    inputDateFocus(input);
    input.datepicker({
        dateFormat: 'mm/dd/yy',
        changeYear: true,
        yearRange: '1900:2200',
        minDate: new Date(),
        onSelect: function(){
          $(this).removeClass('focus');
        }
    });
}