module.exports('equipment-template', function() {
    var equipmentFactory = function (parentNode, options) {
        var template = document.getElementById(options.templateId).innerHTML;
        var equipmentItemsLength = $(options.equipmentDiv).find('[id^=' + options.equipmentIdPattern +']').length;
        
        var result = template.split(options.equipmentName + '[0]')
        .join(options.equipmentName + '[' + options.id + ']')
            .split(options.equipmentName + '_0').join(options.equipmentName + '_' + options.id)
            .replace("№1", "№" + (equipmentItemsLength + 1));

        parentNode.append($.parseHTML(result));
        $(parentNode).attr('id', options.equipmentIdPattern + options.id);

        // Html.TextAreaFor doesn't contain default value parameter
        if (options.equipmentName === 'ExistingEquipment') {
            $(parentNode).find('#ExistingEquipment_' + options.id + '__Notes').text('');
        }

        if (options.id === 0 && options.equipmentName === 'NewEquipment') {
            $(parentNode).find("div.additional-remove").remove();
        }

        return parentNode;
    };

    return equipmentFactory;
});