module.exports('form-handlers', function (require) {
    var postalCodeHandlers = require('postalCode-handlers');
    var showLoader = require('loader').showLoader;
    var hideLoader = require('loader').hideLoader;

    return function (e) {
        var isUniquePostalCodes = postalCodeHandlers.checkCopies();
        var validPostalCode = postalCodeHandlers.validateQuebecProvinces();

        if (!isUniquePostalCodes || !validPostalCode.isValid) {
            e.preventDefault();
            $('#infoErrors').empty();
            if(!isUniquePostalCodes)
                $('#infoErrors').append(createError([translations['SuchPostalCodeAlreadyExist']]));
            if(!validPostalCode.isValid)
                $('#infoErrors').append(createError([translations[validPostalCode.quebecDealer ? 'ServiceAreaInQc' : 'ServiceAreaNotInQc']]));
        } else {
            $('#infoErrors').empty();
            if (!$('#main-form').valid()) {
                e.preventDefault();
            } else {
                showLoader();
                $('#main-form').ajaxSubmit({
                    type: "POST",
                    success: successCallback,
                    error: errorCallback
                });
            }
        }
    }

    function createError (msg) {
        var err = $('<div class="well danger-well over-aged-well" id="age-error-message"><svg aria-hidden="true" class="icon icon-info-well"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-info-well"></use></svg></div>');
        err.append(msg);
        return err;
    };

    function showErrors (errors) {
        $('#infoErrors').empty();
        if (errors && errors.length > 0) {
            errors.forEach(function (er) {
                $('#infoErrors').append(createError(er));
            });
        }
    }

    function successCallback (json) {
        hideLoader();
        if (json.isSuccess) {
            $('#success-message').show();
        } else {
            showErrors(json.Errors);
            $('#success-message').hide();
        }
    }

    function errorCallback (xhr, status, p3) {
        hideLoader();
        alert(translations['ErrorWhileUpdatingData']);
        $('#success-message').hide();
    }

})