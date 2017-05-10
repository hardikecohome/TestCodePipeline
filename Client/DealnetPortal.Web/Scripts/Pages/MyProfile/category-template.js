module.exports('category-template', function () {
    var newCategory = function (index, type, descritption) {
        var hiddenCategoryType = '<input id="Categories_' + index + '__Type" name="Categories[' + index + '].Type" type="hidden" value="' + type + '">';
        var hiddenCategoryDescription = '<input id="Categories_' + index + '__Description" name="Categories[' + index + '].Description" type="hidden" value="' + descritption + '">';
        var template = '<li id="category-index-' + index + '">' +
            hiddenCategoryType +
            hiddenCategoryDescription +
            '<input class="hidden" name="selectedCategory" value="' + type + '">'
            + descritption +
            ' <span class="icon-remove" id="'+ type + '"><svg aria-hidden="true" class="icon icon-remove-cross"><use xlink:href="' +
            urlContent +
            'Content/images/sprite/sprite.svg#icon-remove-cross"></use></svg></span></li>';

        return template;
    }

    return newCategory;
})