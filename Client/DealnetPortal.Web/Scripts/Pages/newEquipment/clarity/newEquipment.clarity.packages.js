module.exports('newEquipment.clarity.packages', function(require) {
    var state = require('state').state;
    var conversion = require('newEquipment.conversion');
    var resetPlaceholder = require('resetPlaceholder');

    var settings = {
        recalculateClarityValuesAndRender: {}
    }

    /**
     * Add new equipment ot list of new equipments
     * Takes template of equipment replace razor generated ids, names with new id index
     * @returns {void} 
     */
    var addPackage = function () {
        var id = $('div#installation-packages').find('[id^=package-]').length;
        var newId = id.toString();
        state.packages[newId] = {
            id: newId,
            description: '',
            monthlyCost: ''
        };

        //create new template with appropiate id and names
        var newTemplate = conversion.createItem(
            {
                id: id,
                templateId: 'package-template-base',
                equipmentName: 'InstallationPackages',
                equipmentIdPattern: 'package-',
                equipmentDiv: 'div#installation-packages'
            });

        newTemplate.find('div.additional-remove').attr('id', 'remove-package-' + id);

        if (id === 2) {
            $('.add-package-link').addClass("hidden");
        }

        state.packages[newId].template = newTemplate;

        // equipment handlers
        newTemplate.find('#remove-package-' + id).on('click', function() {
            var options = {
                name: 'packages',
                equipmentIdPattern: 'package-',
                equipmentName: 'InstallationPackages',
                equipmentRemovePattern: 'remove-package-'
            };
            conversion.removeItem.call(this, options);
            settings.recalculateClarityValuesAndRender();
            $('.add-package-link').removeClass("hidden");
        });

        newTemplate.find('.package-monthly-cost').on('change', updateMonthlyCost);

        customizeSelect();
        toggleClearInputIcon($(newTemplate).find('textarea, input'));
        resetPlaceholder($(newTemplate).find('textarea, input'));

        $('#installation-packages').append(newTemplate);

        resetFormValidator("#equipment-form");
    };

    var init = function(params) {
        if (params.isClarity) {
            settings.recalculateClarityValuesAndRender = params.recalculateClarityValuesAndRender;
        }

        var packages = $('div#installation-packages').find('[id^=package-]').length;
        for (var j = 0;j < packages;j++) {
            _initPackage(j);
        }
    }
    /**
     * update monthly cost of equipment in our global state object
     * on cost changed we recalulate global cost
     * method uses only for Rental/RentalHwt agreement type
     * @returns {void} 
     */
    function updateMonthlyCost () {
        var mvcId = $(this).attr("id");
        var id = mvcId.split('__MonthlyCost')[0].substr(mvcId.split('__MonthlyCost')[0].lastIndexOf('_') + 1);
        state.packages[id].monthlyCost = Globalize.parseNumber($(this).val());
        settings.recalculateClarityValuesAndRender();
    }

    function _initPackage(i) {
        var monthlyCost = Globalize.parseNumber($('#InstallationPackages_' + i + '__MonthlyCost').val());
        state.packages[i] = { id: i.toString(), monthlyCost: monthlyCost };
        $('#remove-package-' + i).on('click', function() {
            var options = {
                name: 'packages',
                equipmentIdPattern: 'package-',
                equipmentName: 'InstallationPackages',
                equipmentRemovePattern: 'remove-package-'
            };
            conversion.removeItem.call(this, options);
            settings.recalculateClarityValuesAndRender();
            $('.add-package-link').removeClass("hidden");
        });

        if (i === 2) {
            $('.add-package-link').addClass("hidden");
        }
    }

    function resetFormValidator (formId) {
        $(formId).removeData('validator');
        $(formId).removeData('unobtrusiveValidation');
        $.validator.unobtrusive.parse(formId);
    }

    return {init: init, addPackage: addPackage}
})