module.exports('my-profile-index', function(require) {
    var categoryHandlers = require('category-handlers');
    var postalCodeHandlers = require('postalCode-handlers');

    //init 
    postalCodeHandlers.initPostalCodeState();
    categoryHandlers.initCategoryState();
    var createError = function (msg) {
        var err = $('<div class="well danger-well over-aged-well" id="age-error-message"><svg aria-hidden="true" class="icon icon-info-well"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-info-well"></use></svg></div>');
        err.append(msg);
        return err;
    };
    var showErrors = function (errors) {
        if (errors.length > 0) {
            errors.forEach(function (er) {
                $('#infoErrors').append(createError(er));
            });
        }
    };
    //handlers 
    $('#offered-service').on('change', categoryHandlers.addCategory);
    $('#add-postalCode').on('click', postalCodeHandlers.addPostalCode);
    $('#saveProfileBtn').on('click', function () {
        $('#infoErrors').empty();
        showLoader();
        $('#main-form').ajaxSubmit({
            type: "POST",
            success: function(json) {
                hideLoader();
                if (json.isSuccess) {
                    $('#success-message').show();
                } else {
                    showErrors(json.Errors);
                    $('#success-message').hide();
                }
            },
            error: function (xhr, status, p3) {
                hideLoader();
                alert(translations['ErrorWhileUpdatingData']);
                $('#success-message').hide();
            }
        });
    });
});