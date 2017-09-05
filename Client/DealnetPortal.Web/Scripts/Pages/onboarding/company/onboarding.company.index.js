module.exports('onboarding.company', function (require) {
    var constants = require('onboarding.state').constants;
    var add = require('onboarding.company.province').add;
    var setRemoveClick = require('onboarding.company.province').setRemoveClick;
    var setters = require('onboarding.company.setters');

    var init = function (company) {
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
        $('#company-postal').on('change', setters.setPostalCode);
        $('#company-type').on('change', setters.setType).change();
        $('#years-in-business').on('change', setters.setYearsInBusiness).change();
        $('#installers').on('change', setters.setInstallers);
        $('#sales').on('change', setters.setSales);
        _setLoadedData(company);
    };

    var initAutocomplete = function () {
        initGoogleServices('company-street', 'company-city', 'company-province', 'company-postal');
    };

    var _setLoadedData = function (company) {
        var $provinceSelect = $('#province-select');
        company.Provinces.forEach(function (item) {
            add.call($provinceSelect, { target: { value: item } });
            $(document).trigger('provinceAdded');
        });
        for (var i = 0;i < constants.companyRequiredFields.length;i++) {
            var item = constants.companyRequiredFields[i];
            if (item === 'work-provinces')
                return;
            $('#' + item).change();
        }
    }

    return {
        initCompany: init,
        initAutocomplete: initAutocomplete
    };
});
