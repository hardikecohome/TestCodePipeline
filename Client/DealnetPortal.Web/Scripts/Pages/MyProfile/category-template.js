module.exports('category-template', function () {
    var newCategory = function (index, type, descritption) {
        var hiddenCategoryId = '<input id="DealerEquipments_' + index + '__Id" name="DealerEquipments[' + index + '].Id" type="hidden" value="' + type + '">';
        //var hiddenCategoryType = '<input id="DealerEquipments_' + index + '__Type" name="DealerEquipments[' + index + '].Type" type="hidden" value="' + type + '">';
        var hiddenCategoryDescription = '<input id="DealerEquipments_' + index + '__Description" name="DealerEquipments[' + index + '].Description" type="hidden" value="' + descritption + '">';
        var template = '<li id="equipment-index-' + index + '">' +
            hiddenCategoryId +
            //hiddenCategoryType +
            hiddenCategoryDescription +
            '<input class="hidden" name="selectedEquipment" value="' + type + '">'
            + descritption +
            ' <span class="icon-remove" id="'+ type + '"><svg aria-hidden="true" class="icon icon-remove-cross"><use xlink:href="' +
            urlContent +
            'Content/images/sprite/sprite.svg#icon-remove-cross"></use></svg></span></li>';

        return template;
    }

    return newCategory;
})