module.exports('onboarding.documents.index', function (require) {
    var setters = require('onboarding.documents.setters');
    var state = require('onboarding.state').state;

    function _setInputHandlersForExistedLicense(licenseId) {
        $('#' + licenseId + '-license-number').on('change', setters.setLicenseRegistraionNumber(licenseId));
        $('#' + licenseId + '-license-checkbox').on('change', setters.setLicenseNoExpiry(licenseId));

        var date = $('#' + licenseId + '-license-date');

        inputDateFocus(date);
        date.datepicker({
            yearRange: '1900:2200',
            minDate: new Date(),
            onSelect: function (day) {
                $(this).siblings('input.form-control').val(day);
                $('#' + license.License.Id + '-birthdate').on('change', setters.setLicenseExpirationDate(licenseId, day));
                $(this).siblings('input.form-control').val(day);
                $(".div-datepicker").removeClass('opened');
            }
        });
    }

    function _setInputHandlers() {
        document.getElementById('void-cheque-upload').addEventListener('change', setters.setVoidChequeFile, false);
        document.getElementById('insurence-upload').addEventListener('change', setters.setInsurenceFile, false);

        $(document).bind('provinceAdded', setters.addLicense);
        $(document).bind('equipmentAdded', setters.addLicense);

        $(document).bind('provinceRemoved', setters.removeLicense);
        $(document).bind('equipmentRemoved', setters.removeLicense);
    }

    var init = function (license, existedLicense, existedDocuments) {
        state['documents'].license = license;

        existedDocuments.forEach(function(doc) {
            if (doc.Name.includes('cheque')) {
                $('#voidChequeUploaded').removeClass('hidden');
                $('#cheque-upload-title').text('Upload another file');
            } else {
                $('#insurenceUploaded').removeClass('hidden');
                $('#insurence-upload-title').text('Upload another file');
            }
        });

        existedLicense.forEach(function(l) {
            state['documents'].addedLicense.push({ 'id': l.LicenseTypeId, 'number': l.Number, 'date': l.ExpiredDate });
            var name = state['documents'].license.filter(function (x) { return x.License.Id === l.LicenseTypeId })[0].License.Name;

            if (l.NotExpired) {
                $('#' + l.LicenseTypeId + '-license-date').addClass('control-disabled');
                $('#' + l.LicenseTypeId + '-license-date').prop('disabled', true);
            }

            $('#' + l.LicenseTypeId + '-license-title').text(name);

            _setInputHandlersForExistedLicense(l.LicenseTypeId);
        });

        _setInputHandlers();
    }

    return {
        init: init
    }
})