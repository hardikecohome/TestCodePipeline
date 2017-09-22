module.exports('onboarding.product', function (require) {
    var setters = require('onboarding.product.setters');
    var constants = require('onboarding.state').constants;
    var resetForm = require('onboarding.common').resetFormValidation;
    var addEquipment = require('onboarding.product.equipment').addEquipment;
    var addBrand = require('onboarding.product.brand').addBrand;
    var removeBrand = require('onboarding.product.brand').removeBrand;
    var setLengthLimitedField = require('onboarding.setters').setLengthLimitedField;

    function init (product) {
        $('#primary-brand')
            .on('change', setters.setPrimaryBrand)
            .on('keyup', setLengthLimitedField(50));
        $('#annual-sales-volume')
            .on('change', setters.setAnnualSales)
            .on('keyup', setLengthLimitedField(10));
        $('#av-transaction-size')
            .on('change', setters.setTransactionSize)
            .on('keyup', setLengthLimitedField(10));
        $('.sales-approach').on('change', setters.setSalesApproach);
        $('.lead-gen').on('change', setters.setLeadGen);
        $('#relationship').on('change', setters.setRelationship);
        $('#oem')
            .on('change', setters.setOem)
            .on('keyup', setLengthLimitedField(50));
        $('#WithCurrentProvider').on('change', setters.setWithCurrentProvider);
        $('#finance-provider-name')
            .on('change', setters.setFinanceProviderName)
            .on('keyup', setLengthLimitedField(50));
        $('#monthly-financed-value')
            .on('change', setters.setMonthFinancedValue)
            .on('keyup', setLengthLimitedField(10));
        $('#OfferMonthlyDeferrals').on('change', setters.setOfferDeferrals);
        $('#percent-month-deferrals')
            .on('change', setters.setPercentMonthDeferrals)
            .on('keyup', setLengthLimitedField(3));
        initRadio();
        $('#WithCurrentProvider')
            .on('change', toggleCheckGroup('.hidden-current-provider'));
        $('#OfferMonthlyDeferrals')
            .on('change', toggleCheckGroup('.hidden-monthly-deferrals'));
        $('#relationship')
            .on('change', toggleRelationship);
        $('#offered-equipment')
            .on('change', addEquipment);
        $('.add-new-brand-link')
            .on('click', addBrand);
        $('#reason-for-interest')
            .on('change', setters.setReasonForInterest);
        $('.remove-brand-link').on('click', removeBrand);

        _setLoadedData(product);
    };

    //function _addEquipment(e) {
    //    addEquipment(e);
    //    //fix for switchers on product page
    //    if ($('#WithCurrentProvider').prop('checked')) {
    //        showFormGroup('.hidden-current-provider');
    //    } else {
    //        hideFormGroup('.hidden-current-provider');
    //    }
    //    if ($('#OfferMonthlyDeferrals').prop('checked')) {
    //        showFormGroup('.hidden-monthly-deferrals');
    //    } else {
    //        hideFormGroup('.hidden-monthly-deferrals');
    //    }
    //    var value = Number($('#relationship').val());
    //    if (value === 1) {
    //        showFormGroup('.hidden-relationship');
    //    } else {
    //        hideFormGroup('.hidden-relationship');
    //    }
    //}

    function initRadio () {
        function clearSiblings () {
            $('.program-service').prop('checked', false);
        }
        $('.custom-radio').on('click', function (e) {
            var $this = $(this);
            clearSiblings();
            var $input = $this.find('input');
            setters.setProgramService($input.attr('id'));
            toggleAdditionalInsurenceTest($input.attr('id'));

            $input.prop('checked', true);
        });
    }

    function toggleCheckGroup (selector) {
        return function () {
            if ($(this).prop('checked')) {
                showFormGroup(selector);
            } else {
                hideFormGroup(selector);
            }
        }
    }

    function toggleRelationship () {
        var value = Number(this.value);
        if (value === 1) {
            showFormGroup('.hidden-relationship');
        } else {
            hideFormGroup('.hidden-relationship');
        }
    }

    function showFormGroup (selector) {
        $(selector)
            .removeClass('hidden')
            .find('input')
            .each(function () {
                $(this).prop('disabled', false)
                    .rules('add', {
                        required: true,
                        messages: {
                            required: translations.ThisFieldIsRequired
                        }
                    });
            });
    }

    function hideFormGroup (selector) {
        $(selector)
            .addClass('hidden')
            .find('input')
            .each(function () {
                $(this)
                    .prop('disabled', true)
                    .rules('remove', 'required');
            });
    }

    function toggleAdditionalInsurenceTest (id) {
        if (id === 'loan') {
            $('#additional-insurence-text').addClass('hidden');
            $('#additional-insurence-text').siblings('.placeholder-text').css('margin-top', 12);
        } else {
            $('#additional-insurence-text').removeClass('hidden');
            $('#additional-insurence-text').siblings('.placeholder-text').css('margin-top', 0);

        }
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
            toggleAdditionalInsurenceTest(this.id);
            setters.setProgramService(this.id);
        });
        constants.productRequiredFields.forEach(function (item) {
            if (item === 'equipment' || item === 'sales-approach' || item === 'lead-gen')
                return;
            var $item = $('#' + item);

            if ($item.val())
                $item.change();
        });
        if ($('#oem').val())
            $('#oem').change();

        $('.secondary-brand').each(function () {
            if (state.product.brands === undefined)
                state.product.brands = [];
            state.product.brands.push(this.value);
        });
    }

    return {
        initProducts: init
    };
});
