module.exports('my-profile-index', function(require) {
    var categoryHandlers = require('category-handlers');
    var postalCodeHandlers = require('postalCode-handlers');
    var validateAndSubmit = require('form-handlers');

    //init 
    postalCodeHandlers.initPostalCodeState();
    categoryHandlers.initCategoryState();

    //handlers 
    $('#offered-service').on('change', categoryHandlers.addCategory);
    $('#add-postalCode').on('click', postalCodeHandlers.addPostalCode);
    $('#saveProfileBtn').on('click', validateAndSubmit);
});