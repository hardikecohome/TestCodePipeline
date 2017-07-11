module.exports('equipment-template', function() {
    var equipmentFactory = function (parentNode, options) {
        var template = document.getElementById(options.templateId).innerHTML;
        //var equipmentItemsLength = $(options.equipmentDiv).find('[id^=new-equipment-]').length;
        var equipmentItemsLength = $(options.equipmentDiv).find('[id^=' + options.equipmentIdPattern +']').length;
        //var result = template.split(options.equipmentName + '[0]'"NewEquipment[0]")
        //    .join("NewEquipment[" + options.id + "]")
        //    .split("NewEquipment_0").join("NewEquipment_" + options.id)
        //    .replace("№1", "№" + (equipmentItemsLength + 1));
        
        var result = template.split(options.equipmentName + '[0]')
        .join(options.equipmentName + '[' + options.id + ']')
            .split(options.equipmentName + '_0').join(options.equipmentName + '_' + options.id)
            .replace("№1", "№" + (equipmentItemsLength + 1));

        parentNode.append($.parseHTML(result));
        //$(parentNode).attr('id', 'new-equipment-' + options.id);
        $(parentNode).attr('id', options.equipmentIdPattern + options.id);

        
        if (options.id === 0 && options.equipmentName === 'NewEquipment') {
            $(parentNode).find("div.additional-remove").remove();
        }

        return parentNode;
    };

    return equipmentFactory;
});