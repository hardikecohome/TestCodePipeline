module.exports('new-client-index', function (require) {

    var observe = require('redux').observe;
    var createAction = require('redux').createAction;
    var clientActions = require('new-client-actions');

    var initBasicInfo = require('basic-information-view');
    var initAddressInfo = require('address-information-view');
    var initContactInfo = require('contact-information-view');
    var initHomeImprovment = require('home-improvments-view');
    var initClientConsents = require('client-consents-view');

    var clientStore = require('new-client-store');

    var configGetErrors = require('new-client-selectors').getErrors;

    var dispatch = clientStore.dispatch;

    // view layer
    var observeClientFormStore = observe(clientStore);

    var initAutocomplete = require('new-client-autocomplete').initAutocomplete;

    var basicInfoRequiredFields = ['name', 'lastName', 'birthday'];
    var currentAddressRequiredFields = ['street', 'city', 'province', 'postalCode'];
    var currentAddressPreviousRequiredFields = ['pstreet', 'pcity', 'pprovince', 'ppostalCode'];
    var contactInfoRequiredFields = ['phone', 'cellPhone', 'email', 'contactMethod'];
    var homeImprovmentsRequiredFields = ['improvmentStreet', 'improvmentCity', 'improvmentProvince', 'improvmentPostalCode', 'improvmentMoveInDate'];
    var clientConsentsRequiredFields = ['creditAgreement', 'contactAgreement'];

    var getErrors = configGetErrors(basicInfoRequiredFields, currentAddressRequiredFields, currentAddressPreviousRequiredFields, contactInfoRequiredFields, homeImprovmentsRequiredFields, clientConsentsRequiredFields);

    //handlers
  
    //datepickers

    //license-scan
    $('#capture-buttons-1').on('click', takePhoto);
    $('#retake').on('click', retakePhoto);

    window.initAutocomplete = initAutocomplete;

    // init views
    initBasicInfo(clientStore);
    initAddressInfo(clientStore);
    initContactInfo(clientStore);
    initHomeImprovment(clientStore);
    initClientConsents(clientStore);

    var form = $('#main-form');
    $('#submit').on('click', function (e) {
        dispatch(createAction(clientActions.SUBMIT));
        var errors = getErrors(clientStore.getState());
        if (errors.length > 0 && form.valid()) {
            e.preventDefault();
        }
    });

    // observers
    observeClientFormStore(function (state) {
        return {
            displayAddressInfo: state.displayAddressInfo,
            displayContactInfo: state.displayContactInfo,
            displayPreviousAddress: state.lessThanSix,
            activePanel: state.activePanel,
            displayImprovmentOtherAddress: state.improvmentOtherAddress
        };
    })(function (props) {
        if (props.activePanel === 'basic-information') {
            $('#basic-information').addClass('active-panel');
        } else {
            $('#basic-information').removeClass('active-panel');
        }

        if (props.displayImprovmentOtherAddress) {
            $('#installation-address').show();
        } else {
            $('#installation-address').hide();
        }

        if (props.displayPreviousAddress) {
            $('#previous-address').show();
        } else {
            $('#previous-address').hide();
        }

        if (props.displayAddressInfo) {
            $('#installationAddressForm').slideDown();
        }

        if (props.activePanel === 'additional-infomration') {
            $('#additional-infomration').addClass('active-panel');
            $('#additional-infomration').removeClass('panel-collapsed');
        } else {
            $('#additional-infomration').removeClass('active-panel');
        }

        if (props.displayContactInfo) {
            $('#contactInfoForm').slideDown();

        }

        if (props.activePanel === 'contact-information') {
            $('#contact-information').addClass('active-panel');
            $('#contact-information').removeClass('panel-collapsed');
        } else {
            $('#contact-information').removeClass('active-panel');
        }

        if (props.activePanel === 'home-improvments') {
            $('#home-improvments').addClass('active-panel');
            $('#home-improvments').removeClass('panel-collapsed');
        } else {
            $('#home-improvments').removeClass('active-panel');
        }

        if (props.activePanel === 'client-consents') {
            $('#client-consents').addClass('active-panel');
            $('#client-consents').removeClass('panel-collapsed');
        } else {
            $('#client-consents').removeClass('active-panel');
        }
    });

    var createError = function (msg) {
        var err = $('<div class="well danger-well over-aged-well" id="age-error-message"><svg aria-hidden="true" class="icon icon-info-well"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-info-well"></use></svg></div>');
        err.append(msg);
        return err;
    };

    observeClientFormStore(function (state) {
        return {
            errors: getErrors(state),
            displaySubmitErrors: state.displaySubmitErrors
        }
    })(function (props) {
        $('#formErrors').empty();
        if (props.errors.length > 0) {
            props.errors
                .filter(function (error) { return error.type === 'birthday' })
                .forEach(function (error) {
                    $('#formErrors').append(createError(window.translations[error.messageKey]));
                });
        }

        var emptyError = props.errors.filter(function (error) {
            return error.type === 'empty';
        });

        if (emptyError.length) {
            $('#submit').addClass('disabled');
            $('#submit').parent().popover();
        } else {
            $('#submit').removeClass('disabled');
            $('#submit').parent().popover('destroy');
        }
    });

})