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

        ko.applyBindings(vm, document.getElementById('main-form'));
    }

    return {
        init: init
    };
});
