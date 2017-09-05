module.exports('onboarding.company', function (require) {
    var state = require('onboarding.state').state;
    var add = require('onboarding.company.province').add;
    var setRemoveClick = require('onboarding.company.province').setRemoveClick;
    var setters = require('onboarding.company.setters');

    var init = function() {
        $('input[id^="province-"]')
            .each(function() {
                var $this = $(this);
                var id = $this.attr('id').split('-')[1];
                setRemoveClick($this.val());
            });
        $('#province-select').on('change', add);

        $('#full-legal-name').on('change', setters.setLegalName);
        $('#operating-name').on('change', setters.setOperatingName);
        $('#company-phone').on('change', setters.setPhone);
        $('#company-email-address').on('change', setters.setEmail);
        $('#company-website').on('change', setters.setWebsite);
        $('#company-street').on('change', setters.setStreet);
        $('#company-unit').on('change', setters.setUnit);
        $('#company-city').on('change', setters.setCity);
        $('#company-province').on('change', setters.setProvince);
        $('#company-postal').on('change',function (e) {
                    e.target.value = e.target.value.toUpperCase();
                    setters.setPostalCode();
            });
        $('#company-type').on('change', setters.setType);
        $('#years-in-business').on('change', setters.setYearsInBusiness);
        $('#installers').on('change', setters.setInstallers);
        $('#sales').on('change', setters.setSales);
    };

    var initAutocomplete = function () {
        initGoogleServices('company-street', 'company-city', 'company-province', 'company-postal');
    };

    return {
        initCompany: init,
        initAutocomplete: initAutocomplete
    };
});
