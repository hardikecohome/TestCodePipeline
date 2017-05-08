module.exports('equipment-template', function() {
    var equipmentFactory = function (parentNode, options) {
        var template = document.getElementById('new-equipment-base').innerHTML;
        var equipmentItemsLength = $('div#new-equipments').find('[id^=new-equipment-]').length;
        var result = template.split("NewEquipment[0]")
            .join("NewEquipment[" + options.id + "]")
            .split("NewEquipment_0").join("NewEquipment_" + options.id)
            .replace("№1", "№" + (equipmentItemsLength + 1));
        
        parentNode.append($.parseHTML(result));

        $(parentNode).attr('id', 'new-equipment-' + options.id);

        return parentNode;
    };

    return equipmentFactory;
});