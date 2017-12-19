module.exports('basicInfo.index', function (require) {
    var EmploymentInformation = require('employmentInformation');
    var AddressInformation = require('addressInformation');
    var dob = require('dob-selecters');

    function init(model) {
        debugger
        $('.dob-group').each(function (index, el) {
            dob.initDobGroup(el);
        });

        $('.dob-input').on('change', function () {
            $(this).valid();
        });
    }

    return {
        init: init
    };
});
