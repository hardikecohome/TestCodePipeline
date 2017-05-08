module.exports('category-handlers', function() {

    var add = function() {
        var equipmentValue = $(this).val();
        var equipmentText = $("#offered-service :selected").text();
        if (equipmentValue) {
            $('#selected-categories').append($('<li><input class="hidden" name="Categories" value="' + equipmentValue + '">' + equipmentText + ' <span class="icon-remove" onclick="$(this).parent().remove()"><svg aria-hidden="true" class="icon icon-remove-cross"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-remove-cross"></use></svg></span></li>'));
        }
    }

    var remove = function () { };

    return {
        addCategory: add,
        removeCategory: remove
    }
})