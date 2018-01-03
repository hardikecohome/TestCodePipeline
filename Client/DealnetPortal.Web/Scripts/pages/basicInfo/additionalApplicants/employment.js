module.exports('basicInfo.additionalApplicants.employment', function () {
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
    var postal

    var requiredObj = {
        required: true,
        messages: {
            required: translations.ThisFieldIsRequired
        }
    };

    function init() {
        status = $('#add1-employment-status');
        status.on('change',
            function (e) {
                changeStatus(e.target.value);
            });

        incomeType = $('#add1-employment-income-type');
        incomeType.on('change',
            function (e) {
                changeIncome(e.target.value);
            }).rules('add', requiredObj);

        annual = $('#add1-employment-annual');
        annual.rules('add', requiredObj);

        hourly = $('#add1-employment-hourly');
        hourly.rules('add', requiredObj);

        years = $('#add1-employment-years');
        years.on('change',
            function (e) {
                changeYears(e.target.value);
            }).rules('add', requiredObj);

        months = $('#add1-employment-months');
        months.rules('add', requiredObj);

        empType = $('#add1-employment-emp-type');
        empType.rules('add', requiredObj);

        jobTitle = $('#add1-employment-job-title');
        jobTitle.rules('add', requiredObj);

        name = $('#add1-employment-company-name');
        name.rules('add', requiredObj);

        phone = $('#add1-employment-company-phone');
        phone.rules('add', requiredObj);

        street = $('#add1-employment-street');

        unit = $('#add1-employment-unit');

        city = $('#add1-employment-city');

        province = $('#add1-employment-province');

        postal = $('#add1-employment-postal-code');

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
        $('#add1-company-info-hold').removeClass('hidden');
        jobTitle.prop('disabled', false);
        name.prop('disabled', false);
        phone.prop('disabled', false);
        $('#add1-company-address-hold').removeClass('hidden');
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
        $('#add1-company-info-hold').addClass('hidden');
        jobTitle.prop('disabled', true);
        name.prop('disabled', true);
        phone.prop('disabled', true);
        $('#add1-company-address-hold').addClass('hidden');
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
        $('#add1-company-info-hold').removeClass('hidden');
        jobTitle.prop('disabled', false);
        name.prop('disabled', false);
        phone.prop('disabled', false);
        $('#add1-company-address-hold').removeClass('hidden');
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
        $('#add1-employment-info').removeClass('hidden');
        status.prop('disabled', false).change();
    }

    function disable() {
        var section = $('#add1-employment-info');
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
