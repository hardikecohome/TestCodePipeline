module.exports('onboarding.documents.index', function (require) {
    var datepickerOptions = {
        yearRange: '1900:2200',
        minDate: new Date()
    };
    var setters = require('onboarding.documents.setters');
    var state = require('onboarding.state').state;

    function _setInputHandlersForExistedLicense (licenseId) {
        $('#' + licenseId + '-license-number').on('change', setters.setLicenseRegistraionNumber(licenseId));
        $('#' + licenseId + '-license-checkbox').on('change', setters.setLicenseNoExpiry(licenseId));

        var date = $('#' + licenseId + '-license-date');

        var options = $.extend({}, datepickerOptions, {
            onSelect: function (day) {
                $(this).siblings('input.form-control').val(day);
                $('#' + licenseId + '-birthdate').on('change', setters.setLicenseExpirationDate(licenseId, day));
                $(this).siblings('input.form-control').val(day);
                $(".div-datepicker").removeClass('opened');
            },
            disabled: $('#' + licenseId + '-license-checkbox').attr('checked')
        })

        var input = assignDatepicker('#' + licenseId + '-license-date', options);
    }

    function _setInputHandlers () {
        document.getElementById('void-cheque-upload').addEventListener('change', setters.setVoidChequeFile, false);
        document.getElementById('insurence-upload').addEventListener('change', setters.setInsurenceFile, false);

        $(document).bind('provinceAdded', setters.addLicense);
        $(document).bind('equipmentAdded', setters.addLicense);

        $(document).bind('provinceRemoved', setters.removeLicense);
        $(document).bind('equipmentRemoved', setters.removeLicense);
    }

    var init = function (license, existedLicense, existedDocuments) {
        state['documents'].license = license;

        existedDocuments.forEach(function (doc) {
            if (doc.DocumentTypeId === 4) {
                $('#voidChequeUploaded').removeClass('hidden');
                $('#cheque-upload-title').text('Upload another file');
                $('#' + doc.Id + '-file-remove').on('click', setters.removeFile('voidChequeUploaded', 'cheque-upload-title', 'void-cheque-files', doc.Name));
                state.documents['void-cheque-files'].push(doc.Name);
            } else {
                $('#insurenceUploaded').removeClass('hidden');
                $('#insurence-upload-title').text('Upload another file');
                $('#' + doc.Id + '-file-remove').on('click', setters.removeFile('insurenceUploaded', 'insurence-upload-title', 'insurence-files', doc.Name));
                state.documents['insurence-files'].push(doc.Name)
            }
        });

        existedLicense.forEach(function (l) {
            state['documents'].addedLicense.push({ 'id': l.LicenseTypeId, 'number': l.Number, 'date': l.ExpiredDate, noExpiry: l.NotExpired });
            var name = state['documents'].license.filter(function (x) {
                return x.License.Id === l.LicenseTypeId;
            })[0].License.Name;

            _setInputHandlersForExistedLicense(l.LicenseTypeId);

            if (l.NotExpired) {
                var date = $('#' + l.LicenseTypeId + '-license-date');
                date.addClass('control-disabled');

                date.parents('.form-group').addClass('group-disabled');

                if ($('body').is('.ios-device')) {
                    var input = $('body').is('.ios-device') ?
                        date.siblings('.div-datepicker') :
                        date;
                    input.prop('disabled', true);
                    input.datepicker('option', 'disabled', true);
                } else {
                    date.attr('disabled', true);
                }


            }

            $('#' + l.LicenseTypeId + '-license-title').text(name);
        });

        if (existedLicense.length > 0) {
            if ($('#licenseHolder').is(':hidden')) {
                $('#licenseHolder').removeClass('hidden');
            }
        }

        setters.moveTonextSection();
        _setInputHandlers();
    }

    return {
        init: init
    }
})