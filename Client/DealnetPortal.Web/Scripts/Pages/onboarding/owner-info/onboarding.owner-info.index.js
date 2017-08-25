module.exports('onboarding.owner-info.index', function (require) {
    var setters = require('onboarding.owner-info.setters');

    function _initEventHandlers() {
        $('#owner1-firstname').on('change', setters.setFirstName('owner1'));
        $('#owner1-lastname').on('change', setters.setLastName('owner1'));
        $('#owner1-homephone').on('change', setters.setHomePhone('owner1'));
        $('#owner1-cellphone').on('change', setters.setCellPhone('owner1'));
        $('#owner1-email').on('change', setters.setEmailAddress('owner1'));
        $('#owner1-street').on('change', setters.setStreet('owner1'));
        $('#owner1-unit').on('change', setters.setUnit('owner1'));
        $('#owner1-city').on('change', setters.setCity('owner1'));
        $('#owner1-postalcode').on('change', setters.setPostalCode('owner1'));
        $('#owner1-province').on('change', setters.setProvince('owner1'));
        $('#owner1-percentage').on('change', setters.setOwnershipPercentege('owner1'));

        initGoogleServices('owner1-street', 'owner1-city', 'owner1-province', 'owner1-postalcode');
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
                $('#owner1-birthdate').on('change', setters.setBirthDate('owner1', day));
                $(".div-datepicker").removeClass('opened');
            }
        });
    }

    function init() {
        _initDatepickers();
        _initEventHandlers();
    }

    return {
        init: init
    }
})