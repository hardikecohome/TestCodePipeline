module.exports('onboarding.documents.setters', function (require) {
    var state = require('onboarding.state').state;
    var constants = require('onboarding.state').constants;
    var enableSubmit = require('onboarding.setters').enableSubmit;
    var resetForm = require('onboarding.common').resetFormValidation;
    var stateSection = 'documents';

    var setVoidChequeFile = function (e) {
        var files = e.target.files;

        _uploadFile('voidChequeUploaded', 'cheque-upload-title', 'cheque-container', 'void-cheque-files', files[0]);
    }

    var setInsurenceFile = function (e) {
        var files = e.target.files;

        _uploadFile('insurenceUploaded', 'insurence-upload-title', 'insurence-container', 'insurence-files', files[0]);
    }

    var setLicenseNoExpiry = function (id) {
        var input = $('#' + id + '-license-date');
        return function (e) {
            var checked = e.target.checked;

            var lic = state[stateSection]['addedLicense'].find(function (item) {
                return item.id === id;
            });
            if (checked) {
                lic.noExpiry = true;
                if (!input.is(':disabled')) {
                    input.addClass('control-disabled');
                    input.parents('.form-group')
                        .addClass('group-disabled');
                    input.prop('disabled', true);
                }
            } else {
                lic.noExpiry = false;
                if (input.is(':disabled')) {
                    input.removeClass('control-disabled');
                    input.parents('.form-group')
                        .removeClass('group-disabled');
                    input.prop('disabled', false);
                }
            }
            enableSubmit();
        }
    }

    var setLicenseRegistraionNumber = function (id) {
        return function (e) {
            var value = e.target.value;
            var filtred = state[stateSection]['addedLicense'].filter(function (l) {
                return l.id == id;
            })[0];

            filtred.number = value;
            enableSubmit();
        }
    }

    var setLicenseExpirationDate = function (id, date) {
        var filtred = state[stateSection]['addedLicense'].filter(function (l) {
            return l.id == id;
        })[0];

        filtred.date = date;
        enableSubmit();
    }

    var addLicense = function (e) {
        var filtred = _getDocumentsArray();

        $.grep(filtred, function (license) {
            var addedLicense = state[stateSection]['addedLicense'].map(function (l) { return l.id });

            if (addedLicense.indexOf(license.License.Id) !== -1) {
                return;
            }

            state[stateSection]['addedLicense'].push({ 'id': license.License.Id, 'number': '', 'date': '' });

            var result = $('#licenseDocumentTemplate')
                .tmpl({ 'name': license.License.Name, 'id': license.License.Id });

            addIconsToFields(result.find('input, textarea'));

            result.appendTo('#licenseHolder');

            _rebuildIndex();

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
        resetForm('#onboard-form');
    }

    var removeLicense = function (e) {
        var filtred = _getDocumentsArray();
        var added = state[stateSection]['addedLicense'];

        Array.prototype.diff = function (a) {
            return this.filter(function (i) { return a.indexOf(i) < 0; });
        };

        var diff = added.map(function (i) { return i.id }).diff(filtred.map(function (i) { return i.License.Id }));

        $.grep(diff, function (toDel) {
            var deleteFromState = state[stateSection]['addedLicense'].filter(function (l) {
                return l.id === toDel;
            })[0];

            delete state[stateSection]['addedLicense'][state[stateSection]['addedLicense'].indexOf(deleteFromState)];

            $('#' + toDel + '-license-holder').remove();
        });

        e.stopImmediatePropagation();
        resetForm('#onboard-form');
    }

    function _getDocumentsArray () {
        var selectedProvinces = state['company'].selectedProvinces;
        var selectedEquipments = state['product'].selectedEquipment;

        var filtredByProvince = state[stateSection].license.filter(function (l) {
            return selectedProvinces.indexOf(l.Province.Province) !== -1;
        });

        var filtredByEquipment = filtredByProvince.filter(function (lic) {
            var selectedEquipmentIds = selectedEquipments.map(function (obj) { return +obj });

            return selectedEquipmentIds.indexOf(lic.Equipment.Id) !== -1;
        });

        return filtredByEquipment;
    }

    function _uploadFile (checkSelector, buttonSelector, fileContainerSelector, stateFileSection, file) {
        var extension = file.name.replace(/^.*\./, '');

        if (extension === '' || constants.validFileExtensions.indexOf(extension) === -1) {
            alert('Your file type is not supported');
            return;
        }

        var data = new FormData();
        data.append('File', file);

        var type = checkSelector === 'insurenceUploaded' ? 8 : 4;
        var formId = $('#Id').val();

        if (formId !== 0) {
            data.append('DealerInfoId', formId);
        }

        data.append('DocumentTypeId', type);
        data.append('DocumentName', file.name);

        $.get({
            type: "POST",
            url: uploadDocumentUrl,
            contentType: false,
            processData: false,
            data: data,
            success: function (json) {
                if (json.IsSuccess) {
                    if (+$('#Id').val() === 0) {
                        $('#Id').val(json.DealerInfoId);
                    }

                    if ($('#AccessKey').val() === '') {
                        $('#AccessKey').val(json.AccessKey);
                    }

                    _addFile(checkSelector, buttonSelector, fileContainerSelector, stateFileSection, file, json.ItemId);
                } else {
                    alert(json.AggregatedError);
                }
            },
            error: function (xhr, status, p3) {
                console.log('error');
            }
        });
        enableSubmit();
    }

    function _removeFile(checkSelector, buttonSelector, stateFileSection, filename) {
        return function (e) {
            e.preventDefault();
            var data = new FormData();

            var formId = $('#Id').val();
            var documentId = e.target.getAttribute('hiddenId');

            if (formId !== 0) {
                data.append('DealerInfoId', formId);
            }

            data.append('DocumentId', documentId);

            $.get({
                type: "POST",
                url: removeDocumentUrl,
                contentType: false,
                processData: false,
                data: data,
                success: function (json) {
                    if (json.IsSuccess) {
                        $('#' + documentId + '-file-container').remove();
                        state[stateSection][stateFileSection].splice(state[stateSection][stateFileSection].indexOf(filename), 1);
                        if (!state[stateSection][stateFileSection].length) {
                            if ($('#' + buttonSelector).text() === 'Upload another file') {
                                $('#' + buttonSelector).text('Upload');
                            }

                            if ($('#' + checkSelector).is(':visible')) {
                                $('#' + checkSelector).addClass('hidden');
                            }
                        }
                    } else {
                        alert(json.AggregatedError);
                    }
                },
                error: function (xhr, status, p3) {
                    console.log('error');
                }
            });
            enableSubmit();
        }
    }

    function _addFile (checkSelector, buttonSelector, fileContainerSelector, stateFileSection, file, id) {

        state[stateSection][stateFileSection].push(file.name);

        $('#fileUploadedTemplate').tmpl({ filename: file.name, id: id }).appendTo('#' + fileContainerSelector);
        $('#' + id + '-file-remove').on('click', _removeFile(checkSelector, buttonSelector, stateFileSection, file.name));

        if ($('#' + checkSelector).is(':hidden')) {
            $('#' + checkSelector).removeClass('hidden');
        }

        if ($('#' + buttonSelector).text() === 'Upload') {
            $('#' + buttonSelector).text('Upload another file');
        }
    }

    function _rebuildIndex () {
        var licenseIdArray = state[stateSection]['addedLicense'].filter(function (l) { return l.id });
        var index = 0;
        licenseIdArray.forEach(function (i) {
            var container = $('#' + i.id + '-license-holder');
            if (container.length) {

                container.find('input').each(function () {
                    var curr = $(this).attr('name');
                    var toReplace = 'AdditionalDocuments[' + index + ']' + $(this).attr('name').substring($(this).attr('name').lastIndexOf(']') + 1);

                    $(this).attr('name', $(this).attr('name').replace(curr, toReplace));
                });

                container.find('span').each(function () {
                    var curr = $(this).attr('name');
                    if (curr == null) { return; }
                    var toReplace = 'AdditionalDocuments[' + index + ']' + $(this).attr('name').substring($(this).attr('name').lastIndexOf(']') + 1);

                    var valFor = $(this).attr('data-valmsg-for');
                    if (valFor == null) { return; }

                    $(this).attr('data-valmsg-for', valFor.replace(curr, toReplace));
                });

                index++;
            }
        });


    }
    function _moveTonextSection () {

    }

    return {
        setVoidChequeFile: setVoidChequeFile,
        setInsurenceFile: setInsurenceFile,
        addLicense: addLicense,
        removeLicense: removeLicense,
        setLicenseRegistraionNumber: setLicenseRegistraionNumber,
        setLicenseNoExpiry: setLicenseNoExpiry,
        setLicenseExpirationDate: setLicenseExpirationDate

    }
});