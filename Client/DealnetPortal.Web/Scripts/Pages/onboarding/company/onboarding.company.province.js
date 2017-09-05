module.exports('onboarding.company.province', function (require) {
    var state = require('onboarding.state').state;
    var workProvinceSet = require('onboarding.company.setters').workProvinceSet;
    var workProvinceRemoved = require('onboarding.company.setters').workProvinceRemoved;

    var provinceTemplate = function (id, province) {
        var template = $('#province-template').html();
        var result = template.split('province-0')
            .join('province-' + id)
            .split('Provinces[0]')
            .join('Provinces[' + id + ']');
        var $result = $(result);
        $result.find('#province-' + id + '-display').text(province);
        $result.find('#province-' + id).val(province);
        $result.find('.icon-remove').attr('id', province)
        return $result;
    };

    var add = function (e) {
        var value = e.target.value;
        if (value) {
            if (workProvinceSet.call(this, e)) {

                $('#province-list').append(provinceTemplate(state.company.selectedProvinces.length - 1, value));
                setRemoveClick(value);
            }
            $(this).val('');
            $('#work-province-error').addClass('hidden');
        }
    };

    var remove = function (e) {
        if (workProvinceRemoved('work-provinces', e.target.id)) {
            var oldId = $(e.target).parent().attr('id');
            var substrIndex = Number(oldId.substr(oldId.indexOf('-') + oldId.lastIndexOf('-')));
            $('li#' + oldId).remove();
            rebuildIndex(substrIndex);
        }
    };

    var rebuildIndex = function (id) {
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

    var setRemoveClick = function (province) {
        $('#' + province).on('click', remove);
    };

    return {
        add: add,
        setRemoveClick: setRemoveClick
    };
});
