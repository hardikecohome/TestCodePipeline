module.exports('my-profile-index', function(require) {
    var categoryHandlers = require('category-handlers');
    var postalCodeHandlers = require('postalCode-handlers');

    //init 
    postalCodeHandlers.initPostalCodeState();
    categoryHandlers.initCategoryState();

    //handlers 
    $('#offered-service').on('change', categoryHandlers.addCategory);
    $('#add-postalCode').on('click', postalCodeHandlers.addPostalCode);

    $('#saveBtn').on('click', function () {
        showLoader();
        $('#mainForm').ajaxSubmit({
            type: "POST",
            success: function (json) {
                hideLoader();
                if (json.isError) {
                    $('.success-message').hide();
                    alert(translations['ErrorWhileUpdatingData']);
                } else if (json.isSuccess) {
                    $('#success-message-enabled').show();
                }
            },
            error: function (xhr, status, p3) {
                hideLoader();
                $('.success-message').hide();
                alert(translations['ErrorWhileUpdatingData']);
            }
        });
    });
});