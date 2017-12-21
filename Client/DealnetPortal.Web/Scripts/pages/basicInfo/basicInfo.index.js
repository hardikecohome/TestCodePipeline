module.exports('basicInfo.index', function (require) {
    var BasicInfo = require('basicInfo.component');

    var dob = require('dob-selecters');

    function init (model) {
        $('.dob-group').each(function (index, el) {
            dob.initDobGroup(el);
        });

        $('.dob-input').on('change', function () {
            $(this).valid();
        });

        var vm = new BasicInfo(model);

        $('#add-additional-applicant').on('click', function () {
            vm.hasAdditionalApplicant(true);
        });
        $('#additonal1-remove').on('click', function () {
            vm.hasAdditionalApplicant(false);
        })

        ko.applyBindings(vm, document.getElementById('main-form'));
    }

    return {
        init: init
    };
});
