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
        $("#check-credit-button").click(function (event) {
            var check1 = document.getElementById('home-owner-agrees');
            var check2 = document.getElementById('additional1-agrees');
            var check3 = document.getElementById('additional2-agrees');
            var check4 = document.getElementById('additional3-agrees');
            if (!(check1.checked &&
                (check2 == null || check2.checked) &&
                (check3 == null || check3.checked) &&
                (check4 == null || check4.checked))) {
                event.preventDefault();
                $("#proceed-error-message").show();
            }
        });
    });

function assignDatepicker(input) {
    if (input == null) { return; }
    inputDateFocus(input);
    input.datepicker({
        beforeShow: function (i) { if ($(i).attr('readonly')) { return false; } },
        dateFormat: 'mm/dd/yy',
        changeYear: true,
        changeMonth: (viewport().width < 768) ? true : false,
        yearRange: '1900:2016',
        minDate: Date.parse("1900-01-01"),
        maxDate: new Date(),
        onClose: function(){
          onDateSelect($(this));
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