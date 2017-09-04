﻿module.exports('onboarding.documents.setters', function (require) {
    var state = require('onboarding.state').state;
    var constants = require('onboarding.state').constants;
    var stateSection = 'documents';

    var setVoidChequeFile = function (e) {
        var files = e.target.files;

        _uploadFile('voidChequeUploaded', 'cheque-upload-title', 'cheque-container', 'void-cheque-files', files[0]);
    }

    var setInsurenceFile = function(e) {
        var files = e.target.files;

        _uploadFile('insurenceUploaded', 'insurence-upload-title', 'insurence-container', 'insurence-files', files[0]);
    }

    var setLicenseNoExpiry = function(id) {
        return function(e) {
            var checked = e.target.checked;

            if (checked) {
                if (!$('#' + id + '-license-date').is(':disabled')) {
                    $('#' + id + '-license-date').addClass('control-disabled');
                    $('#' + id + '-license-date').prop('disabled', true);
                }
            } else {
                if ($('#' + id + '-license-date').is(':disabled')) {
                    $('#' + id + '-license-date').removeClass('control-disabled');
                    $('#' + id + '-license-date').prop('disabled', false); 
                }
            }
        }
    }

    var setLicenseRegistraionNumber = function (id) {
        return function(e) {
            var value = e.target.value;
            var filtred = state[stateSection]['addedLicense'].filter(function(l) {
                return l.id == id;
            })[0];

            filtred.number = value;
        }
    }

    var setLicenseExpirationDate = function (id, date) {
        var filtred = state[stateSection]['addedLicense'].filter(function (l) {
            return l.id == id;
        })[0];

        filtred.date = date;
    }

    var addLicense = function (e) {
        var selectedProvinces = state['company'].selectedProvinces;
        var selectedEquipments = state.selectedEquipment;

        var filtredLicense = state[stateSection].license.filter(function(l) {
            return selectedProvinces.indexOf(l.Province.Province) !== -1;
        }).filter(function (lic) {
            var selectedEquipmentIds = selectedEquipments.map(function(obj) { return +obj.id });
            return selectedEquipmentIds.indexOf(lic.Equipment.Id) !== -1;
            });

        $.grep(filtredLicense, function(license) {
            state[stateSection]['addedLicense'].push({ 'id': license.License.Id, 'number': '', 'date': '' });

            $('#licenseDocumentTemplate')
                .tmpl({ 'name': license.License.Name, 'id': license.License.Id })
                .appendTo('#licenseHolder');

            $('#' + license.License.Id + '-license-number').on('change', setLicenseRegistraionNumber(license.License.Id));
            var date = $('#' + license.License.Id + '-license-date');

            inputDateFocus(date);
            date.datepicker({
                yearRange: '1900:2200',
                minDate: new Date(),
                onSelect: function (day) {
                    $(this).siblings('input.form-control').val(day);
                    $('#' + license.License.Id + '-birthdate').on('change', setLicenseExpirationDate(license.License.Id, day));
                    $(this).siblings('input.form-control').val(day);
                    $(".div-datepicker").removeClass('opened');
                }
            });

            $('#' + license.License.Id + '-license-checkbox').on('change', setLicenseNoExpiry(license.License.Id));
        });

        e.stopImmediatePropagation();
    }

    var removeLicense = function (e) {

        e.stopImmediatePropagation();
    }


    function _uploadFile(checkSelector, buttonSelector, fileContainerSelector, stateFileSection, file) {
        var extension = file.name.replace(/^.*\./, '');
        
        if (extension === '' || constants.validFileExtensions.indexOf(extension) === -1) {
            alert('Your file type is not supported');
            return;
        }

        var fileData = new FormData();
        fileData.append(file.name, file);

        $.get({
            type: "POST",
            url: uploadDocumentUrl,
            contentType: false, 
            processData: false,
            data: fileData,  
            success: function (json) {
                console.log('success');
                _addFile(checkSelector, buttonSelector, fileContainerSelector, stateFileSection, file);
            },
            error: function (xhr, status, p3) {
                console.log('error');
            }
        });
    }

    function _addFile(checkSelector, buttonSelector, fileContainerSelector, stateFileSection, file) {

        state[stateSection][stateFileSection].push(file.name);

        $('#fileUploadedTemplate').tmpl({ filename: file.name }).appendTo('#' + fileContainerSelector);

        if ($('#' + checkSelector).is(':hidden')) {
            $('#' + checkSelector).removeClass('hidden');
        }

        if ($('#' + buttonSelector).text() === 'Upload') {
            $('#' + buttonSelector).text('Upload another file');
        }
    }

    function _moveTonextSection() {
    }

    return {
        setVoidChequeFile: setVoidChequeFile,
        setInsurenceFile: setInsurenceFile,
        addLicense: addLicense,
        removeLicense: removeLicense
    }
});