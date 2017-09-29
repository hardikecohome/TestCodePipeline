module.exports('onboarding.documents.setters', function (require) {
    var state = require('onboarding.state').state;
    var constants = require('onboarding.state').constants;
    var enableSubmit = require('onboarding.setters').enableSubmit;
    var resetForm = require('onboarding.common').resetFormValidation;
    var stateSection = 'documents';
    var datepickerOptions = {
        yearRange: '1900:2200',
        minDate: new Date(),
    };

    var setVoidChequeFile = function (e) {
        var files = e.target.files;

        _uploadFile('voidChequeUploaded', 'cheque-upload-title', 'cheque-container', 'void-cheque-files', files[0]);

        e.target.value = '';
    }

    var setInsurenceFile = function (e) {
        var files = e.target.files;

        _uploadFile('insurenceUploaded', 'insurence-upload-title', 'insurence-container', 'insurence-files', files[0]);

        e.target.value = '';
    }

    var setLicenseRegistraionNumber = function (id) {
        return function (e) {
            var value = e.target.value;

            var filtred = state[stateSection]['addedLicense'].filter(function (l) {
                return l.id == id;
            })[0];

            filtred.number = value;
            moveTonextSection();
            enableSubmit();
        }
    }

    var setLicenseExpirationDate = function (id, date) {
        var filtred = state[stateSection]['addedLicense'].filter(function (l) {
            return l.id == id;
        })[0];

        filtred.date = date;
        moveTonextSection();
        enableSubmit();
    }

    var setLicenseNoExpiry = function (id) {
        var isIos = $('body').is('.ios-device');
        var input = $('#' + id + '-license-date');
        return function (e) {
            var checked = e.target.checked;
            $("#" + id + "-license-checkbox").val(checked);
            var lic = state[stateSection]['addedLicense'].find(function (item) {
                if (item !== undefined && item !== null)
                    return item.id === id;
            });
            lic.noExpiry = checked;
            if (checked) {
                if (!input.is("disabled")) {
                    input.val(null);
                    setLicenseExpirationDate(id, null);
                    input.addClass('control-disabled');
                    input.parents('.form-group')
                        .addClass('group-disabled');
                    input.prop('disabled', true);
                    isIos && _removeDatepicker(id)
                }
            } else {
                if (input.is(":disabled")) {
                    input.removeClass('control-disabled');
                    input.parents('.form-group')
                        .removeClass('group-disabled');
                    input.prop('disabled', false);
                    isIos && _initDatepicker(id);
                }
            }
            moveTonextSection();
        }
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

            var inputs = result.find('input, textarea')
            addIconsToFields(inputs);
            toggleClearInputIcon(inputs);

            result.appendTo('#licenseHolder');

            _rebuildIndex();

            $('#' + license.License.Id + '-license-number').on('change', setLicenseRegistraionNumber(license.License.Id));

            result.find('.date-group').each(function () {
                $('body').is('.ios-device') && $(this).children('.dealnet-disabled-input').length === 0 ? $('<div/>', {
                    class: 'div-datepicker-value',
                    text: $(this).find('.form-control').val()
                }).appendTo(this) : '';
                $('body').is('.ios-device') ? $('<div/>', {
                    class: 'div-datepicker',
                }).appendTo(this) : '';
            });

            result.find('.div-datepicker-value').on('click', function () {
                $('.div-datepicker').removeClass('opened');
                $(this).siblings('.div-datepicker').toggleClass('opened');
                if (!$('.div-datepicker .ui-datepicker-close').length) {
                    addCloseButtonForInlineDatePicker();
                }
            });

            _initDatepicker(license.License.Id);

            if (state[stateSection]['addedLicense'].length > 0) {
                if ($('#licenseHolder').is(':hidden')) {
                    $('#licenseHolder').removeClass('hidden');
                }
            }

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

            var index = state[stateSection]['addedLicense'].indexOf(deleteFromState);
            state[stateSection]['addedLicense'].splice(index, 1);

            $('#' + toDel + '-license-holder').next('hr').remove();
            $('#' + toDel + '-license-holder').remove();
        });

        if (state[stateSection]['addedLicense'].length === 0) {
            $('#licenseHolder').addClass('hidden');
        }

        e.stopImmediatePropagation();
        resetForm('#onboard-form');
        enableSubmit();
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
        var extension = file.name.replace(/^.*\./, '').toLowerCase();

        if (extension === '' || constants.validFileExtensions.map(function (ext) { return ext.toLowerCase(); }).indexOf(extension) === -1) {
            alert(translations['YourFileTypeIsNotSupported']);
            return;
        }

        if (file.size > constants.maxFileUploadSize) {
            alert(translations['ErrorWhileUploadingFile']);
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
                    moveTonextSection();
                    enableSubmit();
                } else {
                    alert(json.AggregatedError);
                }
            },
            error: function (xhr, status, p3) {
                console.log('error');
            }
        });
    }

    var removeFile = function (checkSelector, buttonSelector, stateFileSection, filename) {
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
                        enableSubmit();
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
        $('#' + id + '-file-remove').on('click', removeFile(checkSelector, buttonSelector, stateFileSection, file.name));

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
                    var curr = $(this).attr('data-valmsg-for');
                    if (curr == null) { return; }
                    var toReplace = 'AdditionalDocuments[' + index + ']' + curr.substring(curr.lastIndexOf(']') + 1);

                    $(this).attr('data-valmsg-for', toReplace);
                });

                index++;
            }
        });


    }

    function _initDatepicker (id) {
        var option = $.extend({}, datepickerOptions, {
            disabled: $('#' + id + '-license-checkbox').attr('checked'),
            onSelect: function (day) {
                $(this).siblings('input.form-control').val(day);
                setLicenseExpirationDate(id, day);
                $(".div-datepicker").removeClass('opened');
            }
        });
        var input = assignDatepicker('#' + id + '-license-date', option);

        var license = state.documents.addedLicense.filter(function (item) { return item.id === id; })[0];
        setDatepickerDate('#' + id + '-license-date', license.date);

        var value = input.siblings('.div-datepicker-value');
        value.off('click');
        value.on('click', function () {
            $('.div-datepicker').removeClass('opened');
            input.toggleClass('opened');
            if (!input.find('.ui-datepicker-close').length) {
                setTimeout(function () {
                    $("<button>", {
                        text: translations['Cancel'],
                        type: 'button',
                        class: "ui-datepicker-close ui-state-default ui-priority-primary ui-corner-all",
                        click: function () {
                            input.removeClass('opened');
                        }
                    }).appendTo(input);
                }, 10);
            }
        });
    }

    function _removeDatepicker (id) {
        var date = $('#' + id + '-license-date');

        var input = $('body').is('.ios-device') ? date.siblings('.div-datepicker') : date;

        var value = input.siblings('.div-datepicker-value');
        value.off('click');
        value.on('click', function (e) {
            e.stopPropagation();
        });
        input.datepicker('destroy');
    }

    var moveTonextSection = function () {
        if (state[stateSection]['void-cheque-files'].length === 0)
            return;

        if (state[stateSection]['insurence-files'].length === 0)
            return;

        var valid = state[stateSection].addedLicense.reduce(function (acc, item) {
            return acc && (item.number !== null && item.number.length > 0) && (item.noExpiry || item.date !== null && item.date.length > 0);
        }, true);

        if (valid) {
            $('#' + stateSection + '-panel')
                .addClass('step-passed')
                .removeClass('active-panel');
            var client = $('#client-consent-section');
            if (!client.is('.step-passed') && !initializing)
                client.removeClass('panel-collapsed')
                    .addClass('active-panel');
        } else {
            $('#' + stateSection + '-panel')
                .removeClass('step-passed')
        }
    }

    return {
        setVoidChequeFile: setVoidChequeFile,
        setInsurenceFile: setInsurenceFile,
        addLicense: addLicense,
        removeLicense: removeLicense,
        setLicenseRegistraionNumber: setLicenseRegistraionNumber,
        setLicenseNoExpiry: setLicenseNoExpiry,
        setLicenseExpirationDate: setLicenseExpirationDate,
        removeFile: removeFile,
        moveTonextSection: moveTonextSection
    }
});