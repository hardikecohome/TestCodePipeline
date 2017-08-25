module.exports('onboarding.owner-info.index', function (require) {
    var autocomplete = require('onboarding.autocomplete');
    var setters = require('onboarding.owner-info.setters');

    function _initEventHandlers() {
        $('#owner1-firstname').on('change', setters.setFirstName('owner1'));
        $('#owner1-lastname').on('change', setters.setLastName('owner1'));
        $('#owner1-homephone').on('change', setters.setHomePhone('owner1'));
        $('#owner1-cellphone').on('change', setters.setCellPhone('owner1'));
        $('#owner1-email').on('change', setters.setEmailAddress('owner1'));
    }

    function _initDatepickers() {
        var birth = $("#owner1-birthdate");

        inputDateFocus(birth);
        birth.datepicker({
            dateFormat: 'mm/dd/yy',
            changeYear: true,
            changeMonth: (viewport().width < 768) ? true : false,
            yearRange: '1900:' + (new Date().getFullYear() - 18),
            minDate: Date.parse("1900-01-01"),
            maxDate: new Date(new Date().setFullYear(new Date().getFullYear() - 18)),
            onSelect: function (day) {
                $(this).siblings('input.form-control').val(day);
                $('#owner1-birthdate').on('change', setters.setBirhDate('owner1'));
                $(".div-datepicker").removeClass('opened');
            }
        });
    }

    function init() {
        _initDatepickers();
        _initEventHandlers();
        //autocomplete.add('Street', 'City');
    }

    return {
        init: init
    }
})