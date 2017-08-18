function uploadCaptured(uploadUrl) {
    var dataUrl = bigCanvas.toDataURL();
    showLoader(translations['ProcessingImage']);
    $.ajax({
        type: "POST",
        url: uploadUrl,
        data: {
            imgBase64: dataUrl
        },
        success: function (json) {
            hideLoader();
            if (json.isError) {
                alert(translations['CannotRecognizeDriverLicense']);
            } else {
                var modal = document.getElementById('camera-modal');
                document.getElementById(modal.getAttribute('data-fnToFill')).value = json.FirstName;
                document.getElementById(modal.getAttribute('data-lnToFill')).value = json.LastName;
                var date = new Date(json.DateOfBirthStr);
                date = new Date(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate());
                var dateInput = $("#" + modal.getAttribute('data-bdToFill'));
                dateInput.datepicker("setDate", date);
                dateInput.change();
                document.getElementById(modal.getAttribute('data-dlToFill')).value = json.Id;
                document.getElementById(modal.getAttribute('data-stToFill')).value = json.Street;
                document.getElementById(modal.getAttribute('data-ctToFill')).value = json.City;
                document.getElementById(modal.getAttribute('data-prToFill')).value = json.State;
                document.getElementById(modal.getAttribute('data-pcToFill')).value = json.PostalCode;
                $('#camera-modal').modal('hide');
            }
        },
        error: function (xhr, status, p3) {
            hideLoader();
        alert(xhr.responseText);
    }
    });
}

function submitUpload(sender, uploadUrl, fn, ln, bd, dl, st, ct, pr, pc) {
    
    var files = sender.files;
    if (files.length > 0) {
        if (window.FormData !== undefined) {
            var data = new FormData();
            for (var x = 0; x < files.length; x++) {
                data.append("file" + x, files[x]);
            }
            showLoader(translations['ProcessingImage']);
            $.ajax({
                type: "POST",
                url: uploadUrl,
                contentType: false,
                processData: false,
                data: data,
                success: function (json) {
                    hideLoader();
                    if (json.isError) {
                        alert(translations['CannotRecognizeDriverLicense']);
                    } else {
                        var modal = document.getElementById('camera-modal');
                        document.getElementById(fn || modal.getAttribute('data-fnToFill')).value = json.FirstName;
                        document.getElementById(ln || modal.getAttribute('data-lnToFill')).value = json.LastName;
                        var date = new Date(json.DateOfBirthStr);
                        date = new Date(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate());
                        var id = bd || modal.getAttribute('data-bdToFill');
                        var dateInput = $("#" + id);
                        dateInput.datepicker("setDate", date);
                        dateInput.change();
                        document.getElementById(dl || modal.getAttribute('data-dlToFill')).value = json.Id;
                        document.getElementById(st || modal.getAttribute('data-stToFill')).value = json.Street;
                        document.getElementById(ct || modal.getAttribute('data-ctToFill')).value = json.City;
                        document.getElementById(pr || modal.getAttribute('data-prToFill')).value = json.State;
                        document.getElementById(pc || modal.getAttribute('data-pcToFill')).value = json.PostalCode;
                        $('#camera-modal').modal('hide');
                        $('#' + fn).trigger('uploadSuccess');
                    }
                },
                error: function (xhr, status, p3) {
                    hideLoader();
                    alert(p3);
                }
            });
            $("#upload-file").val("");
        } else {
            alert(translations['BrowserNotSupportFileUpload']);
        }
    }
}