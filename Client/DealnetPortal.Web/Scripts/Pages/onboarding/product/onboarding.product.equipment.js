module.exports('onboarding.product.equipment', function (require) {
    var state = require('onboarding.state').state;
    var equipmentAdded = require('onboarding.product.setters').equipmentAdded;
    var equipmentRemoved = require('onboarding.product.setters').equipmentRemoved;

    function equipmentTemplate (index, id, description) {
        var template = $('#equipment-template').tmpl({ index: index, id: id, description: description });
        return template;
    };


    function addEquipment (e) {
        var id = e.target.value;
        var description = $('#offered-equipment option[value="' + id + '"]').text();
        if (id) {
            if (equipmentAdded(e)) {
                $('#equipment-list').append(equipmentTemplate(state.product.selectedEquipment.length - 1, id, description));
                setRemoveClick(id);
                $(document).trigger('equipmentAdded');
            }
        }
        e.target.value = '';
        $('#equipment-error').addClass('hidden');
    }

    function removeEquipment (e) {
        var liId = $(this).parent().attr('id');
        var id = $(this).attr('id');
        var value = id.substr(id.indexOf('-') + 1);
        if (equipmentRemoved(value)) {
            var substrIndex = Number(liId.substr(liId.indexOf('-') + liId.lastIndexOf('-')));
            $('li#' + liId).remove();
            $(document).trigger('equipmentRemoved');
            rebuildIndex(substrIndex);
        }
    };

    function rebuildIndex (id) {
        while (true) {
            id++;
            var li = $('li#equipment-' + id + '-index');
            if (!li.length) { break; }

            li.attr('id', 'equipment-' + (id - 1) + '-index');

            var input = li.find('#EquipmentTypes_' + id + '__Id');
            input.attr('id', 'EquipmentTypes_' + (id - 1) + '__Id');
            input.attr('name', 'EquipmentTypes[' + (id - 1) + '].Id');

            var description = li.find('#EquipmentTypes_' + id + '__Description');
            description.attr('id', 'EquipmentTypes_' + (id - 1) + '__Description');
            description.attr('name', 'EquipmentTypes[' + (id - 1) + '].Description');

            var span = $('#equipment-' + id + '-display');
            span.attr('id', 'equipment-' + (id - 1) + '-display');
        }
    }

    function setRemoveClick (id) {
        $('#equipment-' + id).on('click', removeEquipment);
    };

    return {
        addEquipment: addEquipment
    }
});
