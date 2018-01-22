module.exports('newEquipment.clarity.packages', function(require) {
    var state = require('state').state;
    var conversion = require('newEquipment.conversion');
    var resetPlaceholder = require('resetPlaceholder');

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

            $('.add-package-link').removeClass("hidden");
        });

        //ewTemplate.find('.package-monthly-cost').on('change', updateMonthlyCost);

        customizeSelect();
        toggleClearInputIcon($(newTemplate).find('textarea, input'));
        resetPlaceholder($(newTemplate).find('textarea, input'));

        $('#installation-packages').append(newTemplate);

        resetFormValidator("#equipment-form");
    };

    var init = function() {
        var packages = $('div#packages').find('[id^=package-]').length;
        for (var j = 0;j < packages;j++) {
            _initPackage(j);
        }
    }

    function _initPackage(i) {
        state.packages[i] = { id: i.toString() };
        $('#package-remove-' + i).on('click', function() {
            conversion.removeItem(
                {
                    itemId: i,
                    name: 'packages',
                    equipmentIdPattern: 'package-',
                    equipmentName: 'InstallationPackages',
                    equipmentRemovePattern: 'package-remove-'
                });

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