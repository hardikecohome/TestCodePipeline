module.exports('onboarding.documents.setters', function (require) {
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

    var addLicense = function (e) {

        $('#licenseDocumentTemplate')
            .tmpl({ 'name':  'testName'})
            .appendTo('#licenseHolder');

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