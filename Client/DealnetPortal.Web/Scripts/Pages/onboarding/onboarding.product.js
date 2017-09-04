module.exports('onboarding.product', function (require) {
    var state = require('onboarding.state').state;
    var resetForm = require('onboarding.common').resetFormValidation;

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
            $(document).trigger('equipmentAdded');
            setRemoveClick(id);
            state.nextEquipmentId++;
        });
        initRadio();
        $('#WithCurrentProvider').on('change', toggleCheckGroup('.hidden-current-provider'));
        $('#OfferMonthlyDeferrals').on('change', toggleCheckGroup('.hidden-monthly-deferrals'));
        $('#relationship').on('change', toggleRelationship);
        $('#offered-equipment').on('change', addEquipment);
        $('.add-new-brand-link').on('click', addNewBrand);
    };

    function initRadio() {
        function clearSiblings() {
            $('.program-service').prop('checked', false);
        }
        $('.custom-radio').on('click', function () {
            var $this = $(this);
            clearSiblings();
            $this.find('input').prop('checked', true);
        })
    }

    function toggleCheckGroup(selector) {
        return function () {
            if ($(this).prop('checked')) {
                hideFormGroup(selector);
            } else {
                showFormGroup(selector);
            }
        }
    }

    function toggleRelationship() {
        var value = Number(this.value);
        if (value === 1) {
            hideFormGroup('.hidden-relationship');
        } else {
            showFormGroup('.hidden-relationship');
        }
    }

    function hideFormGroup(selector) {
        $(selector).removeClass('hidden').find('input').removeProp('disabled');
    }

    function showFormGroup(selector) {
        $(selector).addClass('hidden').find('input').prop('disabled', true);
    }

    function addEquipment() {
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
                $(document).trigger('equipmentAdded');
                state.nextEquipmentId++;
            }
            $(this).val('');
        }
        $('#equipment-error').removeClass('field-validation-error').text('');
    };

    function addNewBrand() {
        var $el = $("#manufacturerBrandTemplate").tmpl({ brandNumber: state.nextBrandNumber });

        $("#add-brand-container").before($el);

        $el.find('.remove-brand-link').on('click', removeBrand);

        $el.find('input').rules('add', {
            minLength: 2,
            maxLength: 50,
            regex: /^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$/,
            messages: {
                minLength: translations.TheFieldMustBeMinimumAndMaximum,
                maxLength: translations.TheFieldMustBeMinimumAndMaximum,
                regex: translations.SecondaryBrandIncorrectFormat
            }
        });

        state.nextBrandNumber++;
        if (state.nextBrandNumber > 1) {
            $('#add-brand-container').hide();
        }
        resetForm('#onboard-form');
        return false;
    }

    function removeBrand() {
        $(this).parents('.new-brand-group').remove();
        state.nextBrandNumber--;
        if (state.nextBrandNumber === 1)
            rebuildBrandIndex(state.nextBrandNumber);
        $('#add-brand-container').show();
        return false;
    }

    function rebuildBrandIndex(index) {
        var group = $($('.new-brand-group')[0]);
        group.find('#ProductInfo_Brands_1').attr('id', 'ProductInfo_Brands_0').attr('name', 'ProductInfo.Brands[0]');
    }

    function removeEquipment() {
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
        $('#equipment-' + id).on('click', removeEquipment);
    };

    return {
        initProducts: init,
        addProduct: addEquipment,
        addBrand: addNewBrand
    };
});
