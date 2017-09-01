module.exports('onboarding.documents.setters', function (require) {
    var state = require('onboarding.state').state;
    var constants = require('onboarding.state').constants;
    var stateSection = 'documents';

    var setVoidChequeFile = function (e) {
        var files = e.target.files;

        _addFile('voidChequeUploaded', 'cheque-upload-title', 'cheque-container', 'void-cheque-files', files[0]);
    }

    var setInsurenceFile = function(e) {
        var files = e.target.files;

        _addFile('insurenceUploaded', 'insurence-upload-title', 'insurence-container', 'insurence-files', files[0]);
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
        setInsurenceFile: setInsurenceFile
    }
});