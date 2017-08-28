module.exports('onboarding.product', function (require) {
    var state = require('onboarding.state').state;

    function equipmentTemplate(index, id, description) {
        var template = $('#equipment-template').html();
        var result = template.split('equipment-0')
            .join('equipment-' + index)
            .split('EquipmentTypes[0]')
            .join('EquipmentTypes[' + index + ']')
            .split("EquipmentTypes_0")
            .join("EquipmentTypes_" + index);
        var $result = $(result);
        $result.find('#equipment-' + index + '-display').text(description);
        $result.find('#EquipmentTypes_' + index + '__Id').val(id);
        $result.find('#EquipmentTypes_' + index + '__Description').val(description);
        $result.find('.icon-remove').attr('id', 'equipment-' + id);
        return $result;
    };

    function addNewBrand() {
        var $el = $("#manufacturerBrandTemplate").tmpl({ brandNumber: state.nextBrandNumber });
        
        $("#add-brand-container").before($el);

        $el.find('.remove-brand-link').on('click', function () {
            $(this).parents('.new-brand-group').remove();
            state.nextBrandNumber--;
            $('#add-brand-container').show();
            return false;
        });

        
        state.nextBrandNumber++;
        if (state.nextBrandNumber > 3) {
            $('#add-brand-container').hide();
        }
        return false;

    }

    function init() {
        $('#equipment-list li').each(function () {
            var $this = $(this);
            var id = $this.attr('id');
            var index = Number(id.substr(id.indexOf('-') + id.lastIndexOf('-')));
            var equipmentId = $this.find('#EquipmentTypes_' + index + '__Id').val();
            var desc = $this.find('#EquipmentTypes_' + index + '__Description').val();
            state.selectedEquipment.push({ id: equipmentId, description: desc });
            setRemoveClick(desc.toLowerCase());
            state.nextEquipmentId++;
        });
    };

    function add() {
        var value = this.value;
        var description = $("#offered-equipment :selected").text();
        if (value) {
            var index = state.selectedEquipment.findIndex(function (item) {
                return item.id === value;
            });
            if (index === -1) {
                state.selectedEquipment.push({ id: value, description: description });

                $('#equipment-list').append(equipmentTemplate(state.nextEquipmentId, value, description));
                setRemoveClick(state.nextEquipmentId);

                state.nextEquipmentId++;
            }
            $(this).val('');
        }
    };

    function remove() {
        var liId = $(this).parent().attr('id');
        var id = $(this).attr('id');
        var value = id.substr(id.indexOf('-') + 1);
        if (value) {
            var index = state.selectedEquipment.findIndex(function (item) {
                return item.id === value;
            });

            if (index > -1) {
                state.selectedEquipment.splice(index, 1);
                state.nextEquipmentId--;
            }

        }
        var substrIndex = Number(liId.substr(liId.indexOf('-') + liId.lastIndexOf('-')));
        $('li#' + liId).remove();
        rebuildIndex(substrIndex);
    };

    function rebuildIndex(id) {
        while (true) {
            id++;
            var li = $('li#equipment-' + id + '-index');
            if (!li.length) { break; }

            li.attr('id', 'equipment-' + (id - 1) + '-index');

            var remove = li.find('#equipment-' + id);
            input.attr('id', 'equipment-' + (id - 1));

            var id = li.find('#EquipmentTypes_' + id + '__Id');
            id.attr('id', 'EquipmentTypes_' + (id - 1) + '__Id');
            id.attr('name', 'EquipmentTypes[' + (id - 1) + '].Id');

            var description = li.find('#EquipmentTypes_' + id + '__Description');
            description.attr('id', 'EquipmentTypes_' + (id - 1) + '__Description');
            description.attr('name', 'EquipmentTypes[' + (id - 1) + '].Description');

            var span = $('#equipment-' + id + '-display');
            span.attr('id', 'equipment-' + (id - 1) + '-display');
        }
    }

    function setRemoveClick(id) {
        $('#equipment' + id).on('click', remove);
    };

    return {
        initProducts: init,
        addProduct: add,
        addBrand: addNewBrand
    };
});
