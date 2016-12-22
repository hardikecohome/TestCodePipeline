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
                $("#" + modal.getAttribute('data-bdToFill')).datepicker("setDate", date);
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

function submitUpload(sender, uploadUrl) {
    
    var files = sender.files;
    if (files.length > 0) {
        if (window.FormData !== undefined) {
            var data = new FormData();
            for (var x = 0; x < files.length; x++) {
                data.append("file" + x, files[x]);
            }
            showLoader('Processing image...');
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
                        document.getElementById(modal.getAttribute('data-fnToFill')).value = json.FirstName;
                        document.getElementById(modal.getAttribute('data-lnToFill')).value = json.LastName;
                        var date = new Date(json.DateOfBirthStr);
                        date = new Date(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate());
                        //date = new Date(date.valueOf() + date.getTimezoneOffset() * 60 * 1000);
                        //var date = new Date(parseInt(json.DateOfBirth.substr(6)));
                        $("#" + modal.getAttribute('data-bdToFill')).datepicker("setDate", date);
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
                    alert(p3);
                }
            });
            $("#upload-file").val("");
        } else {
            alert(translations['BrowserNotSupportFileUpload']);
        }
    }
}