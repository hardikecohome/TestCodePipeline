module.exports('onboarding.product', function (require) {
    var state = require('onboarding.state').state;

    function equipmentTemplate(index, id, description) {
        var template = $('#equipment-template').tmpl({ index: index, id: id, description: description });
        return template;
    };

    function init() {
        $('#equipment-list li').each(function () {
            var $this = $(this);
            var id = $this.attr('id');
            var index = Number(id.substr(id.indexOf('-') + id.lastIndexOf('-')));
            var equipmentId = $this.find('#EquipmentTypes_' + index + '__Id').val();
            var desc = $this.find('#EquipmentTypes_' + index + '__Description').val();
            state.selectedEquipment.push({ id: equipmentId, description: desc });
            setRemoveClick(id);
            state.nextEquipmentId++;
        });
        initRadio();
    };

    function initRadio() {
        function clearSiblings() {
            $('.program-service').prop('checked', false);
        }
        $('.custom-radio').on('click', function () {
            var $this = $(this);
            clearSiblings($this);
            $this.find('input').prop('checked', true);
        })
    }

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
                setRemoveClick(value);

                state.nextEquipmentId++;
            }
            $(this).val('');
        }
    };

    function addNewBrand() {
        var $el = $("#manufacturerBrandTemplate").tmpl({ brandNumber: state.nextBrandNumber });

        $("#add-brand-container").before($el);

        $el.find('.remove-brand-link').on('click', removeBrand);

        state.nextBrandNumber++;
        if (state.nextBrandNumber > 2) {
            $('#add-brand-container').hide();
        }
        return false;

    }

    function removeBrand() {
        $(this).parents('.new-brand-group').remove();
        state.nextBrandNumber--;
        if (state.nextBrandNumber === 2)
            rebuildBrandIndex(state.nextBrandNumber);
        $('#add-brand-container').show();
        return false;
    }
    
    function rebuildBrandIndex(index) {
        var group = $($('.new-brand-group')[0]);
        group.find('#brand-2-display').attr('id', 'brand-1-index').text('2');
        group.find('#Brands_2').attr('id', 'Brands_1').attr('name', 'Brands[1]');
    }

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

    function setRemoveClick(id) {
        $('#equipment-' + id).on('click', remove);
    };

    return {
        initProducts: init,
        addProduct: add,
        addBrand: addNewBrand
    };
});
