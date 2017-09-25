module.exports('onboarding.company', function (require) {
    var constants = require('onboarding.state').constants;
    var add = require('onboarding.company.province').add;
    var setRemoveClick = require('onboarding.company.province').setRemoveClick;
    var setters = require('onboarding.company.setters');
    var setLengthLimitedField = require('onboarding.setters').setLengthLimitedField;

    var init = function (company) {
        $('#province-select').on('change', add);

        $('#full-legal-name')
            .on('change', setters.setLegalName)
            .on('keyup', setLengthLimitedField(50));
        $('#operating-name')
            .on('change', setters.setOperatingName)
            .on('keyup', setLengthLimitedField(50));
        $('#company-phone')
            .on('change', setters.setPhone)
            .on('keyup', setLengthLimitedField(10));
        $('#company-website')
            .on('change', setters.setWebsite)
            .on('keyup', setLengthLimitedField(50));
        $('#company-email-address')
            .on('change', setters.setEmail)
            .on('keyup', setLengthLimitedField(50));
        $('#company-street')
            .on('change', setters.setStreet)
            .on('keyup', setLengthLimitedField(100));
        $('#company-unit')
            .on('change', setters.setUnit)
            .on('keyup', setLengthLimitedField(10));
        $('#company-city')
            .on('change', setters.setCity)
            .on('keyup', setLengthLimitedField(50));
        $('#company-postal')
            .on('change', setters.setPostalCode)
            .on('keyup', setLengthLimitedField(6));
        $('#company-province').on('change', setters.setProvince);
        $('#company-type').on('change', setters.setType);
        $('#years-in-business').on('change', setters.setYearsInBusiness);
        $('#installers').on('change', setters.setInstallers);
        $('#sales').on('change', setters.setSales);

        _setLoadedData(company);
    };

    var initAutocomplete = function () {
        initGoogleServices('company-street', 'company-city', 'company-province', 'company-postal');
        $('#company-street').attr('placeholder', '');
        $('#company-city').attr('placeholder', '');
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
