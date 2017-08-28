module.exports('onboarding.company', function (require) {
    var state = require('onboarding.state').state;

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

    var init = function () {
        $('input[id^="province-"]').each(function () {
            var $this = $(this);
            var id = $this.attr('id').split('-')[1];
            state.selectedProvinces.push($this.val());
            setRemoveClick($this.val());
            state.nextProvinceId++;
        });
        initGoogleServices('company-street', 'company-city', 'company-province', 'company-postal');
    };

    var add = function () {
        var value = this.value;
        if (value) {
            if (state.selectedProvinces.indexOf(value) === -1) {
                state.selectedProvinces.push(value);

                $('#province-list').append(provinceTemplate(state.nextProvinceId, value));
                setRemoveClick(value);

                state.nextProvinceId++;
            }
            $(this).val('');
        }
    };

    var remove = function () {
        var oldId = $(this).parent().attr('id');
        var value = $(this).attr('id');
        if (value) {
            var index = state.selectedProvinces.indexOf(value);

            if (index > -1) {
                state.selectedProvinces.splice(index, 1);
                state.nextProvinceId--;
            }

        }
        var substrIndex = Number(oldId.substr(oldId.indexOf('-') + oldId.lastIndexOf('-')));
        $('li#' + oldId).remove();
        rebuildIndex(substrIndex);
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
        initCompany: init,
        addProvince: add
    };
});
