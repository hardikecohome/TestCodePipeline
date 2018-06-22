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

    var dynamicAlertModal = require('alertModal').dynamicAlertModal;
    var panelCollapsed = require('panelCollapsed');

    var cameraModule = require('camera-capture');
    var dlScanner = require('dl-scanner');

    var dispatch = clientStore.dispatch;

    // view layer
    var observeClientFormStore = observe(clientStore);

    var initAutocomplete = require('new-client-autocomplete').initAutocomplete;

    var validateDob = require('dob-selecters').validate;

    var basicInfoRequiredFields = ['name', 'lastName', 'birthday'];
    var currentAddressRequiredFields = ['street', 'city', 'province', 'postalCode'];
    var currentAddressPreviousRequiredFields = ['pstreet', 'pcity', 'pprovince', 'ppostalCode'];
    var contactInfoRequiredFields = ['email'];
    var homeImprovmentsRequiredFields = ['improvmentStreet', 'improvmentCity', 'improvmentProvince', 'improvmentPostalCode'];
    var clientConsentsRequiredFields = ['creditAgreement', 'contactAgreement'];
    var trimFieldsIds = ['unit_number', 'sin-number', 'dl-number', 'previous_unit_number', 'businessPhone', 'improvment_unit_number'];

    var getErrors = configGetErrors(basicInfoRequiredFields, currentAddressRequiredFields, currentAddressPreviousRequiredFields, contactInfoRequiredFields, homeImprovmentsRequiredFields, clientConsentsRequiredFields);

    //handlers

    //datepickers

    //license-scan
    function initScanLicense(store) {
        if (document.querySelector('video'))
            cameraModule.init();

        $('#owner-scan-button').on('click', function (e) {
            function success(data) {
                $('#first-name').val(data.FirstName).keyup();
                $('#last-name').val(data.LastName).keyup();
                var bDate = new Date(data.DateOfBirthStr);
                $('#birth-date').val((bDate.getUTCMonth() + 1) + '/' + bDate.getUTCDate() + '/' + bDate.getUTCFullYear()).change();
                $('#dl-number').val(data.Id).keyup();
                $('#locality').val(data.City).keyup();
                $('#province').val(data.State).keyup();
                $('#postal_code').val(data.PostalCode).keyup();
                $('#street').val(data.Street).keyup();
                $('#camera-modal').modal('hide');

                var obj = {
                    firstName: data.FirstName,
                    lastName: data.LastName,
                    birthDate: (bDate.getUTCMonth() + 1) + '/' + bDate.getUTCDate() + '/' + bDate.getUTCFullYear(),
                    street: data.Street,
                    locality: data.City,
                    province: data.State,
                    postalCode: data.PostalCode
                };

                store.dispatch(createAction(clientActions.DRIVER_LICENSE_UPLOADED, obj));
            }

            var submitUpload = function (e) {
                dlScanner.submitUpload(e.target.files, success);
                e.target.value = '';
            };

            var capture = function (e) {
                var data = cameraModule.takePhoto();
                $('#upload-capture').on('click', function () {
                    dlScanner.uploadCaptured(data, success);
                });
                $('#retake').one(function () {
                    $('#upload-capture').off();
                });
            };

            $('#owner-upload-file').one('change', submitUpload);
            $('#upload-file').one('change', submitUpload);
            $('#capture-btn').on('click', capture);
            $('#camera-modal').one('hidden.bs.modal', function () {
                $('#capture-btn').off();
                $('#owner-upload-file').off('change', submitUpload);
                $('#upload-file').off('change', submitUpload);
                $('#upload-capture').off();
                $('#retake').off();
            });
        });
    }

    window.initAutocomplete = initAutocomplete;
    $(document).ready(function () {
        $(window).keydown(function (event) {
            if (event.keyCode == 13 && event.target.nodeName != 'TEXTAREA') {
                event.preventDefault();
                return false;
            }
        });

        // init views
        initBasicInfo(clientStore);
        initAddressInfo(clientStore);
        initContactInfo(clientStore);
        initHomeImprovment(clientStore);
        initClientConsents(clientStore);
        initScanLicense(clientStore);

        $('#home-phone').rules('add', 'required');
        $('#cell-phone').rules('add', 'required');

        //mobile adds required to DL upload input, why???
        if ($('#owner-upload-file').length) {
            $('#owner-upload-file').rules('remove');
        }

        $('.j-personal-data-used-modal').on('click', function (e) {
            var data = {
                message: $('#personal-data-used').html(),
                class: "consents-modal",
                cancelBtnText: "OK"
            };
            dynamicAlertModal(data);
            e.preventDefault();
        });
        $('.accordion-panels-hold .panel-heading').on('click', function () {
            panelCollapsed($(this));
        });
    });

    var form = $('#main-form');

    form.on('submit', function (e) {
        $('#submit').prop('disabled', true);
        if (!form.valid()) {
            e.preventDefault();
            $('#submit').prop('disabled', false);
        } else {
            trimValues();
            $('.dob-input').each(function (index, el) {
                validateDob(el);
            });
        }
    });

    function trimValues() {
        $.grep(trimFieldsIds, function (field) {
            var value = $('#' + field).val();
            if (value !== '') {
                $('#' + field).val($.trim(value));
            }
        });
    }

    $('#submit').one('click', function (e) {
        if (!form.valid()) {
            e.preventDefault();
            $(this).prop('disabled', false);
        }

        dispatch(createAction(clientActions.SUBMIT));

        var errors = getErrors(clientStore.getState());
        if (errors.length > 0 && form.valid()) {
            $(this).prop('disabled', false);
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
            displayImprovmentOtherAddress: state.improvmentOtherAddress,
            displayNewAddress: state.displayNewAddress
        };
    })(function (props) {
        if (props.activePanel === 'basic-information') {
            $('#basic-information').addClass('active-panel');
        } else {
            $('#basic-information').removeClass('active-panel');
        }

        if (props.displayNewAddress) {
            if ($('#new-home-address').is(':hidden')) {
                $('#new-home-address').show();
            }
        } else {
            if (!$('#new-home-address').is(':hidden')) {
                $('#new-home-address').hide();
            }
        }

        $('#IsLiveInCurrentAddress').val(!props.displayImprovmentOtherAddress);

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
            displaySubmitErrors: state.displaySubmitErrors,
            isChanged: state.isChanged,
            emailExists: state.emailExists
        }
    })(function (props) {
        $('#ageErrors').empty();
        if (props.errors.length > 0) {
            props.errors
                .filter(function (error) {
                    return error.type === 'birthday'
                })
                .forEach(function (error) {
                    $('#ageErrors').append(createError(window.translations[error.messageKey]));
                });
        }

        if (props.emailExists) {
            $('#email').addClass('input-validation-error');
            $('#email-exists').removeClass('hidden');
        } else {
            if ($('#email').hasClass('input-validation-error')) {
                $('#email').removeClass('input-validation-error');
            }
            $('#email-exists').addClass('hidden');
        }

        if (props.errors.length) {
            $('#submit').addClass('disabled');
            $('#submit').parent().popover();
        } else {
            if ($('#main-form').valid() && !props.emailExists) {
                $('#submit').removeClass('disabled');
                $('#submit').parent().popover('destroy');
            } else {
                if (!$('#submit').is(':disabled')) {
                    $('#submit').addClass('disabled');
                    $('#submit').parent().popover();
                }
            }
        }
    });

})