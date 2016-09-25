$(document)
    .ready(function() {
        assignDatepicker($("#birth-date"));
        assignDatepicker($("#additional1-birth-date"));
        assignDatepicker($("#additional2-birth-date"));
        assignDatepicker($("#additional3-birth-date"));
        //
        $.validator.addMethod(
            "date",
            function(value, element) {
                var minDate = Date.parse("1900-01-01");
                var maxDate = new Date();
                var valueEntered = Date.parseExact(value, "M/d/yyyy");
                if (!valueEntered) {
                    return false;
                }
                if (valueEntered < minDate || valueEntered > maxDate) {
                    return false;
                }
                return true;
            },
            "Please enter a valid date!"
        );
    });

function assignDatepicker(input) {
    if (input == null) { return; }
    input.datepicker({
        beforeShow: function (i) { if ($(i).attr('readonly')) { return false; } },
        dateFormat: 'mm/dd/yy',
        changeMonth: true,
        changeYear: true,
        yearRange: '1900:2016',
        minDate: Date.parse("1900-01-01"),
        maxDate: new Date()
    });
}

function copyFormData(form1, form2, validate) {
    if (validate) {
        form1.validate();
        if (!form1.valid()) {
            return false;
        };
    }
    $(':input[name]', form2).val(function () {
        return $(':input[name=\'' + this.name + '\']', form1).val();
    });
    $('.text-danger span', form2).remove();
    $(':input[name]', form2).removeClass('input-validation-error');
    return true;
}

function saveChanges(form1, form2, url) {
    if (copyFormData(form1, form2, true)) {
        submitChanges(url);
        return true;
    }
    return false;
};

function submitChanges(url) {
    showLoader();
    $('#credit-check-form').ajaxSubmit({
        type: "POST",
        url: url,
        success: function (json) {
            hideLoader();
            if (json.isError) {
                alert("An error occurred while updating data");
            }
        },
        error: function (xhr, status, p3) {
            hideLoader();
            alert(xhr.responseText);
        }
    });
}

function assignAutocompletes() {
    $(document)
        .ready(function() {
            initGoogleServices("street", "locality", "administrative_area_level_1", "postal_code");
            initGoogleServices("mailing_street", "mailing_locality", "mailing_administrative_area_level_1", "mailing_postal_code");
        });
}