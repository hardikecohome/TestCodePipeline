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

    var init = function (license, existedLicense) {
        state['documents'].license = license;

        existedLicense.forEach(function(l) {
            state['documents'].addedLicense.push({ 'id': l.LicenseTypeId, 'number': l.Number, 'date': l.ExpiredDate });

            _setInputHandlersForExistedLicense(l.LicenseTypeId);
        });

        _setInputHandlers();
    }

    return {
        init: init
    }
})