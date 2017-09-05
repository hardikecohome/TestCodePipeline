module.exports('onboarding.company.setters', function (require) {
    var state = require('onboarding.state').state;

    var configSetField = require('onboarding.setters').configSetField;
    var moveToNextSection = require('onboarding.setters').moveToNextSection;

    var stateSection = 'company';

    var setFormField = configSetField(stateSection);

    var setLegalName = setFormField('full-legal-name');

    var setOperatingName = setFormField('operating-name');

    var setPhone = setFormField('company-phone');

    var setEmail = setFormField('company-email-address');

    var setWebsite = setFormField('company-website');

    var setStreet = setFormField('company-street');

    var setUnit = setFormField('company-unit')

    var setCity = setFormField('company-city');

    var setProvince = setFormField('company-province');

    var setPostalCode = setFormField('company-postal');

    var setType = setFormField('company-type');

    var setYearsInBusiness = setFormField('years-in-business');

    var setInstallers = setFormField('installers');

    var setSales = setFormField('sales');

    function workProvinceSet (e) {
        var province = e.target.value;
        var provIndex = state[stateSection].selectedProvinces.indexOf(province);

        if (provIndex === -1) {

            state[stateSection].selectedProvinces.push(province);
            state[stateSection].nextProvinceId++;

            var index = state[stateSection].requiredFields.indexOf('work-provinces');

            if (index > -1) {
                state[stateSection].requiredFields.splice(index, 1);
            }
            moveToNextSection.call(this, stateSection);
            return true;
        }
        return false;
    }

    function workProvinceRemoved (field, province) {
        var index = state[stateSection].selectedProvinces.indexOf(province);

        if (index > -1) {
            state[stateSection].selectedProvinces.splice(index, 1);
            state[stateSection].nextProvinceId--;
            if (state[stateSection].selectedProvinces.length < 1) {
                var requiredIndex = state[stateSection].requiredFields.indexOf(field);
                if (requiredIndex === -1) {
                    state[stateSection].requiredFields.push(field);
                }
                $('#work-province-error').removeClass('hidden');
            }
            return true;
        }
        return false;
    }

    return {
        setLegalName: setLegalName,
        setOperatingName: setOperatingName,
        setPhone: setPhone,
        setEmail: setEmail,
        setWebsite: setWebsite,
        setStreet: setStreet,
        setUnit: setUnit,
        setCity: setCity,
        setProvince: setProvince,
        setPostalCode: setPostalCode,
        setType: setType,
        setYearsInBusiness: setYearsInBusiness,
        setInstallers: setInstallers,
        setSales: setSales,
        workProvinceSet: workProvinceSet,
        workProvinceRemoved: workProvinceRemoved
    };
});
