modules.exports('bill59', function (require) {

    var state = require('state').state;

    var enableForAll = function () {
        if (state.isOntario) {
            var newEquipment = $('[id^="new-equipment-"]');
            $.each(newEquipment, function (i, el) {
                var $el = $(el);
                var selectedEquipmentId = $el.find('.equipment-select').val();
                var index = state.bill59Equipment.indexOf(function (item) {
                    return item == selectedEquipmentId;
                });
                if (index > -1) {
                    $el.find('.description-col').removeClass('col-md-6').addClass('col-md-5');
                    $el.find('.monthly-cost-col').removeClass('col-md-3').addClass('col-md-2');
                    $el.find('.estimated-retail-col').removeClass('hidden')
                    var input = $el.find('.estimated-retail');
                    input.prop('disabled', false);
                    input[0].form && input.rules('add', 'required');
                }
            });
        }
    };

    var disableForAll = function () {
        if (state.isOntario) {
            var newEquipment = $('[id^="new-equipment-"]');
            $.each(newEquipment, function (i, el) {
                var $el = $(el);

                $el.find('.description-col').removeClass('col-md-5').addClass('col-md-6');
                $el.find('.monthly-cost-col').removeClass('col-md-2').addClass('col-md-3');
                $el.find('.estimated-retail-col').addClass('hidden');

                var input = $el.find('.estimated-retail');
                input.prop('disabled', true);
                input[0].form && input.rules('remove', required);
            });
        }
    };

    var enableForOne = function (e) {
        if (state.isOntario) {

        }
    };

    var disableForOne = function (e) {
        if (state.isOntario) {

        }
    };

    return {
        enableForAll: enableForAll,
        disableForAll: disableForAll
    };
});