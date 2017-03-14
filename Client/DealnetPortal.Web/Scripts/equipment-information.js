
$(document)
            .ready(function () {
                $('#equipment-form').submit(function (event) {
                    var agreementType = $("#agreement-type").find(":selected").val();
                    if (agreementType === "0") {                        
                        isCalculationValid = false;
                        recalculateTotalCashPrice();
                        if (!isCalculationValid) {
                            event.preventDefault();
                            $('#new-equipment-validation-message').text(translations['TotalMonthlyPaymentMustBeGreaterZero']);
                        }
                    } else {
                        var monthPayment = $("#total-monthly-payment").val();
                        if (isNaN(monthPayment) || (monthPayment == 0)) {
                            event.preventDefault();
                            $('#new-equipment-validation-message').text(translations['TotalMonthlyPaymentMustBeGreaterZero']);
                        }
                    }
                });                

                $('#existing-notes-default').text("").attr("id", "ExistingEquipment_0__Notes");
                sessionStorage.newEquipmetTemplate = document.getElementById('new-equipment-base').innerHTML;
                sessionStorage.existingEquipmetTemplate = document.getElementById('existing-equipment-base').innerHTML;
                $("#new-equipment-base").remove();
                $("#existing-equipment-base").remove();

                $('.date-input').each(assignDatepicker);
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

                $("#customer-rate").rules("add", "required");
                $("#amortization-term").rules("add", "required");
                $("#requested-term").rules("add", "required");
                $("#loan-term").rules("add", "required");
                var initAgreementType = $("#agreement-type").find(":selected").val();
                manageAgreementElements(initAgreementType);
                $("#agreement-type").on('change', function () {
                    manageAgreementElements($(this).find(":selected").val());
                });
                if (initAgreementType === '0') {
                    recalculateTotalCashPrice();
                } else {
                    recalculateTotalMonthlyPaymentHst();
                }
            });

function manageAgreementElements(agreementType) {
    switch (agreementType) {
        case '0':
            $(".equipment-cost").prop("disabled", false).parents('.equipment-cost-col').show();
            $(".equipment-cost").each(function () {
                $(this).rules("add", "required");
            });
            $(".monthly-cost").prop("disabled", true).parents('.monthly-cost-col').hide();
            $(".monthly-cost").each(function () {
                var input = $(this);
                input.rules("remove", "required");
                input.removeClass('input-validation-error');
                input.next('.text-danger').empty();
            });
            $('.loan-element').show();
            $('.equipment-form-container').addClass('has-loan-calc');
            $('.rental-element').hide();
            $('#total-monthly-payment').rules("remove", "required");
            $("#total-monthly-payment").prop("disabled", true);
            break;
        case '1':
        case '2':
            $(".equipment-cost").prop("disabled", true).parents('.equipment-cost-col').hide();
            $(".equipment-cost").each(function() {
                var input = $(this);
                input.rules("remove", "required");
                input.removeClass('input-validation-error');
                input.next('.text-danger').empty();
            });
            $(".monthly-cost").prop("disabled", false).parents('.monthly-cost-col').show();
            $(".monthly-cost").each(function () {
                $(this).rules("add", "required");
            });
            $('.loan-element').hide();
            $('.equipment-form-container').removeClass('has-loan-calc');
            $('.rental-element').show();
            $("#total-monthly-payment").prop("disabled", false);
            $('#total-monthly-payment').rules("add", "required");
            break;
    }
}

function addNewEquipment() {
    var nextNumber = Number(sessionStorage.newEquipmets) + 1;
    var newDiv = document.createElement('div');
    newDiv.className = 'new-equipment-wrap';
    newDiv.innerHTML = sessionStorage.newEquipmetTemplate.split("NewEquipment[0]").join("NewEquipment[" + sessionStorage.newEquipmets + "]")
        .split("NewEquipment_0").join("NewEquipment_" + sessionStorage.newEquipmets).split("estimated-installation-date-0").join("estimated-installation-date-" + sessionStorage.newEquipmets)
        .replace("removeNewEquipment(0)", "removeNewEquipment(" + sessionStorage.newEquipmets + ")")
        .replace("new-equipment-remove-0", "new-equipment-remove-" + sessionStorage.newEquipmets)
        .replace("№1", "№" + (nextNumber));
    //console.log(newDiv.innerHTML);
    newDiv.id = "new-equipment-" + sessionStorage.newEquipmets;
    document.getElementById('new-equipments').appendChild(newDiv);
    assignDatepicker.call($("#estimated-installation-date-" + sessionStorage.newEquipmets));
    resetFormValidator("#equipment-form");
    manageAgreementElements($("#agreement-type").find(":selected").val());
    customizeSelect();
    sessionStorage.newEquipmets = nextNumber;
    resetPlacehoder($(newDiv).find('textarea, input'));
}

function addExistingEquipment() {
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

function removeNewEquipment(id) {
    $('#new-equipment-' + id).remove();
    var nextNumber = Number(id);
    while (true) {
        nextNumber++;
        var nextEquipment = $('#new-equipment-' + nextNumber);
        if (!nextEquipment.length) { break; }
        
        var labels = nextEquipment.find('label');
        labels.each(function() {
            $(this).attr('for', $(this).attr('for').replace('NewEquipment_' + nextNumber, 'NewEquipment_' + nextNumber - 1));
        });
        var inputs = nextEquipment.find('input, select, textarea');
        inputs.each(function () {
            $(this).attr('id', $(this).attr('id').replace('NewEquipment_' + nextNumber, 'NewEquipment_' + (nextNumber - 1)));
            $(this).attr('name', $(this).attr('name').replace('NewEquipment[' + nextNumber, 'NewEquipment[' + (nextNumber - 1)));
        });
        var spans = nextEquipment.find('span');
        spans.each(function () {
            var valFor = $(this).attr('data-valmsg-for');
            if (valFor == null){ return; }
            $(this).attr('data-valmsg-for', valFor.replace('NewEquipment[' + nextNumber, 'NewEquipment[' + (nextNumber - 1)));
        });
        nextEquipment.find('.equipment-number').text('№' + nextNumber);
        var removeButton = nextEquipment.find('#new-equipment-remove-' + nextNumber);
        removeButton.attr('onclick', removeButton.attr('onclick').replace('removeNewEquipment(' + nextNumber, 'removeNewEquipment(' + (nextNumber - 1)));
        removeButton.attr('id', 'new-equipment-remove-' + (nextNumber - 1));
        nextEquipment.attr('id', 'new-equipment-' + (nextNumber - 1));
        resetFormValidator("#equipment-form");
    }
    sessionStorage.newEquipmets = Number(sessionStorage.newEquipmets) - 1;
}

function removeExistingEquipment(id) {
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
}

function recalculateTotalMonthlyPayment() {
    var agreementType = $("#agreement-type").find(":selected").val();
    if (agreementType === "0") {
        return;
    }
    var sum = 0;
    $(".monthly-cost").each(function() {
        var numberValue = parseFloat(this.value);
        if (!isNaN(numberValue)) {
            sum += numberValue;
        }
    });
    
    $("#total-monthly-payment").val(sum.toFixed(2));
    recalculateTotalMonthlyPaymentHst();
}

function recalculateTotalMonthlyPaymentHst() {
    var sum = $("#total-monthly-payment").val();
    var totalHst = sum * taxRate / 100;
    var totalMp = sum * 1 + totalHst;
    $("#total-hst").text(totalHst.toFixed(2));
    $("#total-monthly-payment-hst").text(totalMp.toFixed(2));
}

function recalculateTotalCashPrice() {
    var agreementType = $("#agreement-type").find(":selected").val();
    if (agreementType !== "0") {
        return;
    }
    var sum;
    $(".equipment-cost").each(function () {
        var numberValue = parseFloat(this.value);
        if (!isNaN(numberValue)) {
            if (!sum) { sum = 0; }
            sum += numberValue;
        }
    });

    if (!sum) { return; }
    $("#equipment-cash-price").text(sum.toFixed(2));
    calculateLoanValues();
}

function resetFormValidator(formId) {
    $(formId).removeData('validator');
    $(formId).removeData('unobtrusiveValidation');
    $.validator.unobtrusive.parse(formId);
    $("#customer-rate").rules("add", "required");
    $("#amortization-term").rules("add", "required");
    $("#requested-term").rules("add", "required");
    $("#loan-term").rules("add", "required");
}

function assignDatepicker() {
    var input = $(this);
    inputDateFocus(input);
    input.datepicker({
        dateFormat: 'mm/dd/yy',
        changeYear: true,
        changeMonth: (viewport().width < 768) ? true : false,
        yearRange: '1900:2200',
        minDate: new Date(),
        onClose: function(){
            onDateSelect($(this));
        }
    });
}