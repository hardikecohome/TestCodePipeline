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
        $('#company-street').on('change', setters.setStreet);
        $('#company-city').on('change', setters.setCity);
        $('#company-province').on('change', setters.setProvince);
        $('#company-postal').on('change', setters.setPostalCode);
        $('#company-type').on('change', setters.setType);
        $('#years-in-business').on('change', setters.setYearsInBusiness);
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
        constants.companyRequiredFields.forEach(function (item) {
            if (item === 'work-provinces')
                return;
            var $item = $('#' + item);
            if ($item.val())
                $item.change();
        });
    }

    return {
        initCompany: init,
        initAutocomplete: initAutocomplete
    };
});
