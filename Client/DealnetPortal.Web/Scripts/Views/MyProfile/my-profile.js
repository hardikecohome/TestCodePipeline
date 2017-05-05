module.exports('my-profile-view', function (require) {
    var profileActions = require('my-profile-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;
    var observe = require('redux').observe;

    return function (store) {
        var dispatch = store.dispatch;

        $('#offered-service').on('change', function (e) {
            var equipmentValue = $(this).val();
            dispatch(createAction(profileActions.SET_CATEGORY, equipmentValue));
            var equipmentText = $("#offered-service :selected").text();
            if (equipmentValue) {
                $('#selected-categories').append($('<li><input class="hidden" name="Categories" value="' + equipmentValue + '">' + equipmentText + ' <span class="icon-remove" onclick="$(this).parent().remove()"><svg aria-hidden="true" class="icon icon-remove-cross"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-remove-cross"></use></svg></span></li>'));
            }
        });
    }
});