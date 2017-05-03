module.exports('new-client-index', function (require) {

    var togglePreviousAddress = require('new-client-ui').togglePreviousAddress;
    var toggleInstallationAddress = require('new-client-ui').toggleInstallationAddress;
    var observe = require('redux').observe;

    var initBasicInfo = require('basic-information-view');
    var initAddressInfo = require('address-information-view');
    var initContactInfo = require('contact-information-view');

    var customerFormStore = require('new-client-store');

    var dispatch = customerFormStore.dispatch;
   
    // view layer
    var observeCustomerFormStore = observe(customerFormStore);

    var initAutocomplete = require('new-client-autocomplete').initAutocomplete;

    var selected = [];

    function removeEquipment() {
        var value = $(this).val();
        if (value) {
            var index = selected.indexOf(value);
            if (index !== -1) {
                selected.splice(index, 1);
            }
        }

        $(this).parent().remove();
    }

    $('span.icon-remove').on('click', 'div.form-group', removeEquipment);

    // action handlers
    $('#improvment-equipment').on('change', function () {
        var equipmentValue = $(this).val();
        var equipmentText = $("#improvment-equipment :selected").text();
        if (equipmentValue && selected.indexOf(equipmentValue) === -1) {
            selected.push(equipmentValue);
            $('#improvement-types').append($('<li><input class="hidden" name="HomeImprovementTypes" value="' + equipmentValue + '">' + equipmentText + ' <span class="icon-remove" onclick="$(this).parent().remove()"><svg aria-hidden="true" class="icon icon-remove-cross"<use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-remove-cross"></use></svg></span></li>'));
        }
    });

    //handlers
  
    //datepickers

    //license-scan
    $('#capture-buttons-1').on('click', takePhoto);
    $('#retake').on('click', retakePhoto);

    $('#living-time-checkbox').on('change', togglePreviousAddress);
    $("input[name$='improvement']").on('click', toggleInstallationAddress);

    window.initAutocomplete = initAutocomplete;

    // init views
    initBasicInfo(customerFormStore);
    initAddressInfo(customerFormStore);
    initContactInfo(customerFormStore);
    //initInstallationAddress(customerFormStore);
    //initContactInfo(customerFormStore);
    //initAgreement(customerFormStore);

    // observers
    observeCustomerFormStore(function (state) {
        return {
            displayAddressInfo: state.displayAddressInfo,
            displayContactInfo: state.displayContactInfo,
            activePanel: state.activePanel
        };
    })(function (props) {
        if (props.activePanel === 'basic-information') {
            $('#basic-information').addClass('active-panel');
        } else {
            $('#basic-information').removeClass('active-panel');
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
    });

    var createError = function (msg) {
        var err = $('<div class="well danger-well over-aged-well" id="age-error-message"><svg aria-hidden="true" class="icon icon-info-well"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-info-well"></use></svg></div>');
        err.append(msg);
        return err;
    };

    observeCustomerFormStore(function (state) {
        return {
            errors: function(state) {},
            displaySubmitErrors: state.displaySubmitErrors
        }
    })(function (props) {
        $('#yourInfoErrors').empty();
        //if (props.errors.length > 0) {
        //    props.errors
        //        .filter(function (error) { return error.type === 'birthday' })
        //        .forEach(function (error) {
        //            $('#yourInfoErrors').append(createError(window.translations[error.messageKey]));
        //        });
        //}

        //var emptyError = props.errors.filter(function (error) {
        //    return error.type === 'empty';
        //});

        //if (emptyError.length) {
        //    $('#submit').addClass('disabled');
        //    $('#submit').parent().popover();
        //} else {
        //    $('#submit').removeClass('disabled');
        //    $('#submit').parent().popover('destroy');
        //}
    });

})