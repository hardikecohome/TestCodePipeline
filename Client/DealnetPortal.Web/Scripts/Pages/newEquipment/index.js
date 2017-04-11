module.exports('new-equipment',
    function(require) {
        var equipmentTemplateFactory = require('equipment-template');
        var parseFloat = require('numberUtils');

        var recalculateValues = function() {

        };

        var equipments = {
            '0': {
                type: '',
                description: '',
                cost: '',
            },
        };

        var updateCost = function(id) {
            return function(e) {
                if (!equipments.hasOwnProperty(id)) {
                    return;
                }

                equipments[id].cost = parseFloat(e.target.value);
            };

            recalculateValues();
        };

        var id = 1;
        var addEquipment = function() {
            equipments[id.toString()] = {
                id: id.toString(),
                type: '',
                description: '',
                cost: '',
            };

            var index = Object.keys(equipments).length;
            var newTemplate = equipmentTemplateFactory($('<div></div>'), { index: index });

            newTemplate.find('.equipment-cost').on('change', updateCost(id));
            $('#new-equipments').append(newTemplate);

        };

        $('#addEquipment').on('click', addEquipment);


    });