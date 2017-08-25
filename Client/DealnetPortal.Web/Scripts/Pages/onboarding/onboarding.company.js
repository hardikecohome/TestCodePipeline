module.exports('onboarding.company',function (require) {
    var state = require('onboarding.state').state;

    var provinceTemplate = function (province) {
        var template = $('#province-template').innerHTML();
        debugger
    };

    var init = function () {
        $('input[id^="province-"]').each(function () {
            var $this = $(this);
            var id = $this.attr('id').split('-')[1];
            state.selectedProvinces.push({ id: id, province: $this.val() });
            state.nextProvinceId++;
            $('#province-' + id + '-remove').on('click', remove);
        });
    };

    var remove = function () {

    };

    return {
        initCompany: init
    };
});
