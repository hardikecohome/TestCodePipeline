module.exports('onboarding.product.brand', function (require) {
    var state = require('onboarding.state').state;
    var resetForm = require('onboarding.common').resetFormValidation;
    var initSecondaryBrands = require('onboarding.product.setters').initSecondaryBrands;
    var setSecondaryBrand = require('onboarding.product.setters').setSecondaryBrand;
    var removeSecondayBrand = require('onboarding.product.setters').removeSecondayBrand;
    var setLengthLimitedField = require('onboarding.setters').setLengthLimitedField;

    function addNewBrand () {
        initSecondaryBrands();
        var brandsCount = $('.secondary-brand').length;
        if (brandsCount < 2) {
            var $el = $("#manufacturerBrandTemplate").tmpl({
                brandNumber: brandsCount
            });
            var container = $("#add-brand-container");
            container.before($el);

            $el.find('.remove-brand-link').on('click', removeBrand);

            $el.find('input')
                .on('change', setSecondaryBrand(brandsCount))
                .on('keyup', setLengthLimitedField(50))
                .rules('add', {
                    minLength: 2,
                    maxLength: 50,
                    regex: /^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$/,
                    messages: {
                        minLength: translations.TheFieldMustBeMinimumAndMaximum,
                        maxLength: translations.TheFieldMustBeMinimumAndMaximum,
                        regex: translations.SecondaryBrandIncorrectFormat
                    }
                });

            var inputs = $el.find('input');

            addIconsToFields(inputs);
            toggleClearInputIcon(inputs);

            if ($('.secondary-brand').length > 1) {
                container.hide();
            } else {
                if (!container.is('.col-clear-sm-6'))
                    container.addClass('col-clear-sm-6');
                var label = container.find('.dealnet-label');
                if (!label.is('.hidden-sm'))
                    label.addClass('hidden-sm');
            }
            resetForm('#onboard-form');
        }
        return false;
    }

    function removeBrand () {
        var index = $(this).siblings('input').attr('id');
        var id = index.substr(index.lastIndexOf('_') + 1, 1);
        removeSecondayBrand(id);

        $(this).parents('.new-brand-group').remove();
        var container = $('#add-brand-container');
        if ($('.secondary-brand').length === 1) {
            rebuildBrandIndex();
            if (!container.is('.col-clear-sm-6'))
                container.addClass('col-clear-sm-6');
            var label = container.find('.dealnet-label')
            if (!label.is('.hidden-sm')) {
                label.addClass('hidden-sm');
            }
        } else {
            container.removeClass('col-clear-sm-6');
            container.find('.hidden-sm').removeClass('hidden-sm');
        }
        container.show();

        return false;
    }

    function rebuildBrandIndex () {
        var group = $($('.new-brand-group')[0]);
        group.removeClass('col-clear-sm-6').find('#brands_1').attr('id', 'brands_0').attr('name', 'ProductInfo.Brands[0]');
        group.find('.text-danger').attr('data-valmsg-for', 'ProductInfo.Brands[0]')
    }

    return {
        addBrand: addNewBrand,
        removeBrand: removeBrand
    };
});
