module.exports('onboarding.product', function (require) {
    var setters = require('onboarding.product.setters');
    var resetForm = require('onboarding.common').resetFormValidation;
    var addEquipment = require('onboarding.product.equipment').addEquipment;
    var addBrand = require('onboarding.product.brand').addBrand;

    function init () {
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
        $('#primary-brand').on('change', setters.setPrimaryBrand);
        $('#annual-sales-volume').on('change', setters.setAnnualSates);
        $('#av-transaction-size').on('change', setters.setTransactionSize);
        $('.sales-approach').on('change', setters.setSalesApproach);
        $('.lead-gen').on('change', setters.setLeadGen);
        $('#relationship').on('change', setters.setRelationship);
        $('#oem').on('change', setters.setOem);
        $('#WithCurrentProvider').on('change', setters.setWithCurrentProvider);
        $('#finance-provider-name').on('change', setters.setFinancaProviderName);
        $('#monthly-financed-value').on('change', setters.setMonthFinancedValue);
        $('#OfferMonthlyDeferrals').on('change', setters.setOfferDeferrals);
        $('#percent-month-deferrals').on('change', setters.setPercentMonthDeferrals);
        initRadio();
        $('#WithCurrentProvider').on('change', toggleCheckGroup('.hidden-current-provider'));
        $('#OfferMonthlyDeferrals').on('change', toggleCheckGroup('.hidden-monthly-deferrals'));
        $('#relationship').on('change', toggleRelationship);
        $('#offered-equipment').on('change', addEquipment);
        $('.add-new-brand-link').on('click', addBrand);
    };

    function initRadio () {
        function clearSiblings () {
            $('.program-service').prop('checked', false);
        }
        $('.custom-radio').on('click', function (e) {
            var $this = $(this);
            clearSiblings();
            var $input = $this.find('input');
            setters.setProgramService(input.attr('id'));
            input.prop('checked', true);
        });
    }

    function toggleCheckGroup (selector) {
        return function () {
            if ($(this).prop('checked')) {
                hideFormGroup(selector);
            } else {
                showFormGroup(selector);
            }
        }
    }

    function toggleRelationship () {
        var value = Number(this.value);
        if (value === 1) {
            hideFormGroup('.hidden-relationship');
        } else {
            showFormGroup('.hidden-relationship');
        }
    }

    function hideFormGroup (selector) {
        $(selector).removeClass('hidden').find('input').removeProp('disabled');
    }

    function showFormGroup (selector) {
        $(selector).addClass('hidden').find('input').prop('disabled', true);
    }

    return {
        initProducts: init
    };
});
