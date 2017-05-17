module.exports('my-profile-index', function(require) {
    var categoryHandlers = require('category-handlers');
    var postalCodeHandlers = require('postalCode-handlers');

    //init 
    postalCodeHandlers.initPostalCodeState();
    categoryHandlers.initCategoryState();

    //handlers 
    $('#offered-service').on('change', categoryHandlers.addCategory);
    $('#add-postalCode').on('click', postalCodeHandlers.addPostalCode);
    $('#saveProfileBtn').on('click', function () {
        showLoader();
        $('#main-form').ajaxSubmit({
            type: "POST",
            success: function(json) {
                hideLoader();
                if (json.isSuccess) {
                    $('#success-message').show();
                } else {
                    $('#success-message').hide();
                }
            },
            error: function (xhr, status, p3) {
                hideLoader();
                $('#success-message').hide();
            }
        });
    });
});