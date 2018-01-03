module.exports('basicInfo.homeOwner.employment', function () {
    var status;
    var incomeType;
    var annual;
    var hourly;
    var years;
    var months;
    var empType;
    var jobTitle;
    var name;
    var phone;
    var street;
    var unit;
    var city;
    var province;
    var postal;

    var requiredObj = {
        required: true,
        messages: {
            required: translations.ThisFieldIsRequired
        }
    };

    function init(disabled) {
        status = $('#ho-employment-status');
        status.on('change',
            function (e) {
                changeStatus(e.target.value);
            });
        status.prop('disabled', disabled);

        var statusVal = status.val();

        incomeType = $('#ho-employment-income-type');
        incomeType.on('change',
            function (e) {
                changeIncome(e.target.value);
            }).rules('add', requiredObj);
        incomeType.prop('disabled', disabled || statusVal === '0');

        annual = $('#ho-employment-annual');
        annual.rules('add', requiredObj);
        annual.prop('disabled', disabled || (statusVal === '0' && incomeType.val() === '0'));

        hourly = $('#ho-employment-hourly');
        hourly.rules('add', requiredObj);
        hourly.prop('disabled', disabled || (statusVal === '0' && incomeType.val() === '1'));

        years = $('#ho-employment-years');
        years.on('change',
            function (e) {
                changeYears(e.target.value);
            }).rules('add', requiredObj);
        years.prop('disabled', disabled || statusVal === '0' || statusVal === '2');

        months = $('#ho-employment-months');
        months.rules('add', requiredObj);
        months.prop('disabled', disabled || statusVal === '0' || statusVal === '2');

        empType = $('#ho-employment-emp-type');
        empType.rules('add', requiredObj);
        empType.prop('disabled', disabled || statusVal === '0');

        jobTitle = $('#ho-employment-job-title');
        jobTitle.rules('add', requiredObj);
        jobTitle.prop('disabled', disabled || statusVal === '0' || statusVal === '2');

        name = $('#ho-employment-company-name');
        name.rules('add', requiredObj);
        name.prop('disabled', disabled || statusVal === '0' || statusVal === '2');

        phone = $('#ho-employment-company-phone');
        phone.rules('add', requiredObj);
        phone.prop('disabled', disabled || statusVal === '0' || statusVal === '2');

        street = $('#ho-employment-street');
        street.prop('disabled', disabled || statusVal === '0' || statusVal === '2');

        unit = $('#ho-employment-unit');
        unit.prop('disabled', disabled || statusVal === '0' || statusVal === '2');

        city = $('#ho-employment-city');
        city.prop('disabled', disabled || statusVal === '0' || statusVal === '2');

        province = $('#ho-employment-province');
        province.prop('disabled', disabled || statusVal === '0' || statusVal === '2');

        postal = $('#ho-employment-postal-code');
        postal.prop('disabled', disabled || statusVal === '0' || statusVal === '2');

        if ($('#administrative_area_level_1').val().toLowerCase() === 'qc') {
            status.change();
        }
    }

    function changeStatus(value) {
        switch (value) {
            case '0':
                changeToEmployed();
                break;
            case '1':
            case '3':
                changeToUnemployedOrRetired();
                break;
            case '2':
                changeToSelfEmployed();
                break;
        }
    }

    function changeToEmployed() {
        incomeType.prop('disabled', false).parents('.form-group').removeClass('hidden');
        annual.parents('.form-group').addClass('hidden');
        hourly.parents('.form-group').addClass('hidden');
        years.prop('disabled', false).parents('.form-group').removeClass('hidden');
        months.prop('disabled', false);
        empType.prop('disabled', false).parents('.form-group').removeClass('hidden');
        $('#ho-company-info-hold').removeClass('hidden');
        jobTitle.prop('disabled', false);
        name.prop('disabled', false);
        phone.prop('disabled', false);
        $('#ho-company-address-hold').removeClass('hidden');
        street.prop('disabled', false);
        unit.prop('disabled', false);
        city.prop('disabled', false);
        province.prop('disabled', false);
        postal.prop('disabled', false);
    }

    function changeToUnemployedOrRetired() {
        annual.prop('disabled', false).parents('.form-group').removeClass('hidden');

        incomeType.prop('disabled', true).parents('.form-group').addClass('hidden');
        hourly.prop('disabled', true).parents('.form-group').addClass('hidden');
        empType.prop('disabled', true).parents('.form-group').addClass('hidden');
        years.prop('disabled', true).parents('.form-group').addClass('hidden');
        months.prop('disabled', true);
        $('#ho-company-info-hold').addClass('hidden');
        jobTitle.prop('disabled', true);
        name.prop('disabled', true);
        phone.prop('disabled', true);
        $('#ho-company-address-hold').addClass('hidden');
        street.prop('disabled', true);
        unit.prop('disabled', true);
        city.prop('disabled', true);
        province.prop('disabled', true);
        postal.prop('disabled', true);
    }

    function changeToSelfEmployed() {
        annual.prop('disabled', false).parents('.form-group').removeClass('hidden');
        years.prop('disabled', false).parents('.form-group').removeClass('hidden');
        months.prop('disabled', false);
        $('#ho-company-info-hold').removeClass('hidden');
        jobTitle.prop('disabled', false);
        name.prop('disabled', false);
        phone.prop('disabled', false);
        $('#ho-company-address-hold').removeClass('hidden');
        street.prop('disabled', false);
        unit.prop('disabled', false);
        city.prop('disabled', false);
        province.prop('disabled', false);
        postal.prop('disabled', false);

        incomeType.prop('disabled', true).parents('.form-group').addClass('hidden');
        hourly.prop('disabled', true).parents('.form-group').addClass('hidden');
        empType.prop('disabled', true).parents('.form-group').addClass('hidden');
    }

    function changeIncome(value) {
        if (value === '') {
            annual.prop('disabled', true).parents('.form-group').addClass('hidden');
            hourly.prop('disabled', true).parents('.form-group').addClass('hidden');
        }
        if (value === '0') {
            annual.prop('disabled', false).parents('.form-group').removeClass('hidden');
            hourly.prop('disabled', true).parents('.form-group').addClass('hidden');
        }
        if (value === '1') {
            hourly.prop('disabled', false).parents('.form-group').removeClass('hidden');
            annual.prop('disabled', true).parents('.form-group').addClass('hidden');
        }
    }

    function changeYears(value) {
        if (value === '10+') {
            months.prop('disabled', true).val('');
        } else {
            months.prop('disabled', false);
        }
    }

    function enable() {
        $('#ho-employment-info').removeClass('hidden');
        status.prop('disabled', false).change();
    }

    function disable() {
        var section = $('#ho-employment-info');
        section.addClass('hidden');
        section.find('input, select').each(function (index, elem) {
            $(elem).prop('disable', true);
        });
    }

    return {
        initEmployment: init,
        enableEmployment: enable,
        disableEmployment: disable
    };
});
