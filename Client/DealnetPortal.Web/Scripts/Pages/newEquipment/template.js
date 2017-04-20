module.exports('equipment-template', function() {
    var equipmentFactory = function (parentNode, options) {
        var template = document.getElementById('new-equipment-base').innerHTML;

        var result = template.split("NewEquipment[0]")
            .join("NewEquipment[" + options.id + "]")
            .split("NewEquipment_0").join("NewEquipment_" + options.id)
            .replace("№1", "№" + (options.index));

        parentNode.append($.parseHTML(result));

        return parentNode;
    };

    return equipmentFactory;
});