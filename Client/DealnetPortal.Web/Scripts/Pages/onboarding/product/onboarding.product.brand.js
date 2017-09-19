module.exports('onboarding.product.brand', function (require) {
    var state = require('onboarding.state').state;
    var resetForm = require('onboarding.common').resetFormValidation;
    var addSecondaryBrand = require('onboarding.product.setters').addSecondaryBrand;
    var setSecondaryBrand = require('onboarding.product.setters').setSecondaryBrand;
    var removeSecondayBrand = require('onboarding.product.setters').removeSecondayBrand;
    var setLengthLimitedField = require('onboarding.setters').setLengthLimitedField;

    function addNewBrand () {
        if (addSecondaryBrand('')) {
            var brandsCount = $('.secondary-brand').length;
            var $el = $("#manufacturerBrandTemplate").tmpl({ brandNumber: brandsCount});
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
            addIconsToFields($el.find('input'));
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
        group.removeClass('col-clear-sm-6').find('#ProductInfo_Brands_1').attr('id', 'ProductInfo_Brands_0').attr('name', 'ProductInfo.Brands[0]');
    }

    return {
        addBrand: addNewBrand,
        removeBrand: removeBrand
    };
});
