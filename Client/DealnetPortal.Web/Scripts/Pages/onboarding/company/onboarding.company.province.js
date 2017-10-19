module.exports('onboarding.company.province', function (require) {
    var state = require('onboarding.state').state;
    var workProvinceSet = require('onboarding.company.setters').workProvinceSet;
    var workProvinceRemoved = require('onboarding.company.setters').workProvinceRemoved;

    var setRemoveClick = function (province) {
        $('#' + province).on('click', _remove);
    };

    var add = function (e) {
        var value = e.target.value;
        if (value) {
            if (workProvinceSet.call(this, e)) {
                $('#province-list').append(_provinceTemplate(state.company.selectedProvinces.length - 1, value));
                $(document).trigger('provinceAdded');
                setRemoveClick(value);
            }
            $(this).val('');
            $('#work-province-error').addClass('hidden');
        }
    };

    function _remove(e) {
        if (workProvinceRemoved('work-provinces', e.target.id)) {
            var oldId = $(e.target).parent().attr('id');
            var substrIndex = Number(oldId.split('-')[1]);
            $('li#' + oldId).remove();
            $(document).trigger('provinceRemoved');
            _rebuildIndex(substrIndex);
        }
    };

    function _rebuildIndex(id) {
        while (true) {
            id++;
            var li = $('li#province-' + id + '-index');
            if (!li.length) { break; }

            li.attr('id', 'province-' + (id - 1) + '-index');

            var input = $('#province-' + id);
            input.attr('id', 'province-' + (id - 1));
            input.attr('name', 'Provinces[' + (id - 1) + ']');

            var span = $('#province-' + id + '-display');
            span.attr('id', 'province-' + (id - 1) + '-display');
        }
    }

    function _provinceTemplate(id, province) {
        var template = $('#province-template').html();
        var result = template.split('province-0')
            .join('province-' + id)
            .split('Provinces[0]')
            .join('Provinces[' + id + ']');
        var $result = $(result);
        $result.find('#province-' + id + '-display').text(province);
        $result.find('#province-' + id).val(province);
        $result.find('.icon-remove').attr('id', province);

        return $result;
    };

    return {
        add: add,
        setRemoveClick: setRemoveClick
    };
});