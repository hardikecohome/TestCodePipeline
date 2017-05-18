module.exports('category-handlers', function (require) {
    var template = require('category-template');
    var state = require('my-profile-state');

    var init = function() {
        $('input[name="selectedEquipment"]').next().each(function () {
            if (this.id !== undefined) {
                if (state.categories.indexOf(this.id) === -1) {
                    state.categories.push(this.id);
                    $('#' + this.id).on('click', remove);
                    state.categorySecondId++;
                }
            }
        });
    }

    var add = function() {
        var value = $(this).val();
        var description = $("#offered-service :selected").text();
        if (value) {
            var lowerCaseValue = value.toLowerCase();
            if (state.categories.indexOf(lowerCaseValue) === -1) {
                state.categories.push(lowerCaseValue);

                $('#selected-categories').append($(template(state.categorySecondId, lowerCaseValue, description)));
                $('#' + lowerCaseValue).on('click', remove);

                state.categorySecondId++;
            }
        }
    }

    function remove() {
        var oldId = $(this).parent().attr('id');
        var value = $(this).attr('id');

        if (value) {
            var index = state.categories.indexOf(value);

            if (index !== -1) {
                state.categories.splice(index, 1);
                state.categorySecondId--;
            }
        }

        var substringIndex = Number(oldId.substr(oldId.lastIndexOf("-") + 1));

        $('li#' + oldId).remove();

        rebuildCategoryIndex(substringIndex);
    };

    function rebuildCategoryIndex(id) {
        while (true) {
            id++;
            var div = $('li#equipment-index-' + id);

            if (!div.length) { break; }

            div.attr('id', 'equipment-index-' + (id - 1));
            div.find('input[name*="Id"]').each(function () {
                $(this).attr('id', $(this).attr('id').replace('DealerEquipments_' + id, 'DealerEquipments' + (id - 1)));
                $(this).attr('name', $(this).attr('name').replace('DealerEquipments[' + id, 'DealerEquipments[' + (id - 1)));
            });

            div.find('input[name*="Type"]').each(function () {
                $(this).attr('id', $(this).attr('id').replace('DealerEquipments_' + id, 'DealerEquipments' + (id - 1)));
                $(this).attr('name', $(this).attr('name').replace('DealerEquipments[' + id, 'DealerEquipments[' + (id - 1)));
            });

            div.find('input[name*="Description"]').each(function () {
                $(this).attr('id', $(this).attr('id').replace('DealerEquipments_' + id, 'DealerEquipments' + (id - 1)));
                $(this).attr('name', $(this).attr('name').replace('DealerEquipments[' + id, 'DealerEquipments[' + (id - 1)));
            });
        }
    }

    return {
        initCategoryState: init,
        addCategory: add
    }
})