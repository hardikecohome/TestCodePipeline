module.exports('onboarding.product', function (require) {
    var setters = require('onboarding.product.setters');
    var constants = require('onboarding.state').constants;
    var resetForm = require('onboarding.common').resetFormValidation;
    var addEquipment = require('onboarding.product.equipment').addEquipment;
    var addBrand = require('onboarding.product.brand').addBrand;


    function init (product) {
        $('#primary-brand').on('change', setters.setPrimaryBrand);
        $('#annual-sales-volume').on('change', setters.setAnnualSales);
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
        $('#WithCurrentProvider').on('change', toggleCheckGroup('.hidden-current-provider')).change();
        $('#OfferMonthlyDeferrals').on('change', toggleCheckGroup('.hidden-monthly-deferrals')).change();
        $('#relationship').on('change', toggleRelationship);
        $('#offered-equipment').on('change', addEquipment);
        $('.add-new-brand-link').on('click', addBrand);
        $('#reason-for-interest').on('change', setters.setReasonForInterest).change();
        _setLoadedData(product);
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
        $(selector).removeClass('hidden').find('input').prop('disabled', false);
    }

    function showFormGroup (selector) {
        $(selector).addClass('hidden').find('input').prop('disabled', true);
    }

    function _setLoadedData (product) {
        var equipmentSelect = $('#offered-equipment');
        product.EquipmentTypes.forEach(function (element) {
            addEquipment.call(equipmentSelect, { target: { value: element.Id } })
            $(document).trigger('equipmentAdded');
        }, this);
        $('.sales-approach:checked').each(function () {
            setters.setSalesApproach({ target: this });
        });
        $('.lead-gen:checked').each(function () {
            setters.setLeadGen({ target: this });
        });
        $('.program-service:checked').each(function () {
            setters.setProgramService(this.id);
        });
        constants.productRequiredFields.forEach(function (item) {
            if (item === 'equipment' || item === 'sales-approach' || item === 'lead-gen')
                return;
            $('#' + item).change();
        });
    }

    return {
        initProducts: init
    };
});
