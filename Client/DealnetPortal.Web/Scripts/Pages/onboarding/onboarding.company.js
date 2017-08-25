module.exports('onboarding.company', function (require) {
    var state = require('onboarding.state').state;

    var provinceTemplate = function (id, province) {
        var template = $('#province-template').html();
        var result = template.split('province-0')
            .join('province-' + id)
            .split('Province[0]')
            .join('Province[' + id + ']');
        var $result = $(result);
        $result.find('#province-' + id + '-display').text(province);
        $result.find('#province-' + id).val(province);
        return $result;
    };

    var init = function () {
        $('input[id^="province-"]').each(function () {
            var $this = $(this);
            var id = $this.attr('id').split('-')[1];
            state.selectedProvinces.push({ id: id, province: $this.val() });
            setRemoveClick(id);
            state.nextProvinceId++;
        });
    };

    var add = function () {
        var value = this.value;
        if (value) {
            if (state.selectedProvinces.indexOf(value) === -1) {
                state.selectedProvinces.push({ id: state.nextProvinceId, province: value });

                $('#province-list').append(provinceTemplate(state.nextProvinceId, value));
                setRemoveClick(state.nextProvinceId);

                state.nextProvinceId++;
            }
            $(this).val('');
        }
    };

    var remove = function () {

    };

    var setRemoveClick = function (id) {
        $('#province-' + id + '-remove').on('click', remove);
    };

    return {
        initCompany: init,
        addProvince: add
    };
});
