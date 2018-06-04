module.exports('dl-scanner', function (require) {
    var showLoader = require('loader').showLoader;
    var hideLoader = require('loader').hideLoader;

    var uploadCaptured = function (data, callback) {
        showLoader(translations.ProcessingImage);
        $.post(uploadCaptureUrl, {
                imgBase64: data
            })
            .done(function (json) {
                if (json.isError) {
                    alert(translations.CannotRecognizeDriverLicense);
                } else {
                    callback(json);
                }
            })
            .fail(function (xhr, status, p3) {
                alert(xhr.responseText);
            }).always(function () {
                hideLoader();
            });
    };

    var submitUpload = function (files, callback) {
        showLoader(translations.ProcessingImage);
        if (files.length > 0) {
            if (window.FormData !== undefined) {
                var data = new FormData();
                for (var x = 0; x < files.length; x++) {
                    data.append("file" + x, files[x]);
                }
                $.ajax({
                        type: "POST",
                        contentType: false,
                        processData: false,
                        url: recognizeDLPhotoUrl,
                        data: data
                    }).done(function (json) {
                        if (json.isError) {
                            alert(translations.CannotRecognizeDriverLicense);
                        } else {
                            callback(json);
                        }
                    })
                    .fail(function (xhr, status, p3) {
                        alert(xhr.responseText);
                    }).always(function () {
                        hideLoader();
                    });
            } else {
                alert(translations.BrowserNotSupportFileUpload);
            }
        }
    };

    return {
        uploadCaptured: uploadCaptured,
        submitUpload: submitUpload
    };
});